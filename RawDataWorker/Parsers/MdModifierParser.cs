using Models.Configs;
using Models.Enums;
using Models.Interfaces;
using Models.Types.HtmlFilesData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class MdModifierParser : Parser
{
    public MdModifierParser() : base() { }

    protected override void Normalize(ref string content) { }

    protected override IHoiData ParseRealization(string content)
    {
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var modifierToScope = new Dictionary<string, ScopeTypes>(StringComparer.OrdinalIgnoreCase);

        static bool TryParseScope(string raw, out ScopeTypes scope)
        {
            string norm(string s) => new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

            var target = norm(raw);

            if (Enum.TryParse<ScopeTypes>(target, out scope))
            {
                return true;
            }
            scope = default;
            return false;
        }

        var reScopeHeader = new Regex(@"^##\s+Modifiers\s+for\s+scope\s+(?<scope>\w+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var reModLink = new Regex(@"^\*\s+\[(?<mod>[^\]]+)\]\(#(?<id>[^\)]+)\)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // First pass: collect scopes and their modifiers
        ScopeTypes currentScope = ScopeTypes.Any;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            var mScope = reScopeHeader.Match(line);
            if (mScope.Success)
            {
                var scopeSlug = mScope.Groups["scope"].Value;
                if (TryParseScope(scopeSlug, out var scopeType))
                {
                    currentScope = scopeType;
                }
                continue;
            }

            if (currentScope != ScopeTypes.Any)
            {
                var mMod = reModLink.Match(lines[i]); // use lines[i] without Trim
                if (mMod.Success)
                {
                    var mod = mMod.Groups["mod"].Value;
                    if (!modifierToScope.ContainsKey(mod))
                    {
                        modifierToScope[mod] = currentScope;
                    }
                }
            }
        }

        var reNumberSpec = new Regex(@"\s*Number\s+with\s+(?<n>\d+)\s+decimal\s+places\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var reBoolean = new Regex(@"\s*Boolean\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var reCategories = new Regex(@"\s*Categories\s*:\s*(?<v>.+?)\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var reModHeader = new Regex(@"^##\s+(?<key>[^\s]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        var configs = new List<ModifierDefinitionConfig>();
        string currentKey = null;
        var currentDetails = new List<string>();

        // Second pass: collect details for each modifier
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            var mHeader = reModHeader.Match(line);
            if (mHeader.Success)
            {
                // Process previous if any
                if (!string.IsNullOrEmpty(currentKey))
                {
                    ProcessCurrentModifier(currentKey, currentDetails, modifierToScope, configs);
                }

                currentKey = mHeader.Groups["key"].Value;
                currentDetails.Clear();
                continue;
            }

            if (!string.IsNullOrEmpty(currentKey))
            {
                if (line.StartsWith("* "))
                {
                    currentDetails.Add(line);
                }
                else if (line.StartsWith("#") || !string.IsNullOrWhiteSpace(line) && !line.StartsWith("*"))
                {
                    // If not starting with *, and not empty, skip this modifier
                    currentKey = null;
                    currentDetails.Clear();
                }
            }
        }

        // Process the last one
        if (!string.IsNullOrEmpty(currentKey))
        {
            ProcessCurrentModifier(currentKey, currentDetails, modifierToScope, configs);
        }

        var result = new ModifierDefinitionFile() { ModifierDefinitions = configs };
        return result;
    }

    private void ProcessCurrentModifier(string key, List<string> details, Dictionary<string, ScopeTypes> modifierToScope, List<ModifierDefinitionConfig> configs)
    {
        var cfg = new ModifierDefinitionConfig
        {
            Id = new Identifier(key),
            ScopeType = modifierToScope.TryGetValue(key, out var st) ? st : ScopeTypes.Country
        };

        bool hasPrecision = false;
        bool hasCategories = false;

        foreach (var detailLine in details)
        {
            if (!detailLine.StartsWith("* ")) continue;

            // Убираем "* " и лишние пробелы
            var text = detailLine.Substring(2).Trim();

            // 1. Проверяем точность (Number with N decimal places)
            if (text.Contains("Number", StringComparison.OrdinalIgnoreCase) &&
                text.Contains("decimal", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(text, @"Number\s+with\s+(\d+)\s+decimal\s+places", RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int precision))
                {
                    cfg.Precision = precision;
                    hasPrecision = true;
                    continue;
                }
            }

            // 2. Проверяем Boolean
            if (text.Contains("Boolean", StringComparison.OrdinalIgnoreCase))
            {
                cfg.Precision = 0;
                hasPrecision = true;
                continue;
            }

            // 3. Проверяем категории
            if (text.Contains("Categories", StringComparison.OrdinalIgnoreCase) &&
                text.Contains(":", StringComparison.OrdinalIgnoreCase))
            {
                // Берём всё после "Categories:"
                var afterColon = text.Split(new[] { ':' }, 2)[1].Trim();

                // Разделяем по запятым и берём первую категорию
                var categories = afterColon
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                if (categories.Count > 0)
                {
                    var firstCategory = categories[0];

                    if (Enum.TryParse<ModifierDefinitionCathegoryType>(firstCategory, true, out var cat))
                    {
                        cfg.Cathegory = cat;
                        hasCategories = true;
                    }
                }
                continue;
            }
        }

        // Добавляем только те модификаторы, у которых есть хотя бы одна из двух обязательных характеристик
        // (можно убрать это условие, если хотите собирать все модификаторы независимо от наличия precision/categories)
        if (hasPrecision || hasCategories)
        {
            configs.Add(cfg);
        }
    }
}