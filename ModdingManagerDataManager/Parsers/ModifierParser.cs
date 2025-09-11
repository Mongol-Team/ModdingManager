using HtmlAgilityPack;
using ModdingManagerDataManager.Parsers;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using System.Text.RegularExpressions;

public class ModifierParser : HtmlParser
{
    public ModifierParser() : base() { }

    protected override void Normalize(ref string content) { }

    protected override List<IConfig> ParseRealization(string content)
    {
        var doc = new HtmlDocument { OptionFixNestedTags = true };
        doc.LoadHtml(content);

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

        // ищем все <h2 id="modifiers-for-scope-XXX">
        var h2Scopes = doc.DocumentNode.SelectNodes("//h2[starts-with(@id,'modifiers-for-scope-')]");
        if (h2Scopes != null)
        {
            foreach (var h2 in h2Scopes)
            {
                var id = h2.GetAttributeValue("id", "");
                var scopeSlug = id.Substring("modifiers-for-scope-".Length); // например "politics"

                if (!TryParseScope(scopeSlug, out var scopeType))
                    continue;

                // ближайший следующий <ul> перечисляет модификаторы этого scope
                var ul = h2.SelectSingleNode("following-sibling::ul[1]");
                if (ul == null) continue;

                foreach (var a in ul.SelectNodes(".//a[@href]") ?? Enumerable.Empty<HtmlNode>())
                {
                    var href = a.GetAttributeValue("href", ""); // "#modifier_name"
                    if (string.IsNullOrWhiteSpace(href) || href[0] != '#') continue;

                    var mod = href.Substring(1);               // "modifier_name"
                    if (!modifierToScope.ContainsKey(mod))
                        modifierToScope[mod] = scopeType;
                }
            }
        }

        var reNumberSpec = new Regex(@"^\s*Number\s+with\s+(?<n>\d+)\s+decimal\s+places\s*$",
                                     RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var reCategories = new Regex(@"^\s*Categories\s*:\s*(?<v>.+?)\s*$",
                                     RegexOptions.IgnoreCase | RegexOptions.Compiled);

        var result = new List<IConfig>();
        var h3Nodes = doc.DocumentNode.SelectNodes("//h3[@id]");

        if (h3Nodes != null)
        {
            foreach (var h3 in h3Nodes)
            {
                var key = h3.GetAttributeValue("id", null);
                if (string.IsNullOrWhiteSpace(key)) continue;

                var cfg = new ModifierDefenitionConfig
                {
                    Name = key,
                    ScopeType = modifierToScope.TryGetValue(key, out var st) ? st : ScopeTypes.country
                };

                var ul = h3.SelectSingleNode("following-sibling::ul[1]");
                if (ul != null)
                {
                    foreach (var li in ul.SelectNodes("./li") ?? Enumerable.Empty<HtmlNode>())
                    {
                        var text = HtmlEntity.DeEntitize(li.InnerText).Trim();
                        if (text.Length == 0) continue;

                        // Precision
                        var mNum = reNumberSpec.Match(text);
                        if (mNum.Success && int.TryParse(mNum.Groups["n"].Value, out var n))
                        {
                            cfg.Precision = n;
                            continue;
                        }

                        // Category
                        var mCat = reCategories.Match(text);
                        if (mCat.Success)
                        {
                            var first = mCat.Groups["v"].Value
                                            .Split(',')
                                            .Select(s => s.Trim())
                                            .FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(first) &&
                                Enum.TryParse<ModifierDefinitionCathegoryType>(first, true, out var cat))
                            {
                                cfg.Cathegory = cat;
                            }
                            continue;
                        }

                        // (при необходимости тут можно распознавать ValueType/ColorType из текста)
                    }
                }

                result.Add(cfg);
            }
        }

        return result;
    }

}
