using ModdingManager.classes.utils;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Types;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class IdeologyComposer : IComposer
    {
        public IdeologyComposer() { }
        public static List<IConfig> Parse()
        {
            string[] possiblePaths = {
                ModPathes.IdeologyPath,
                GamePathes.IdeologyPath
            };
            foreach (string path in possiblePaths)
            {
                List<string> ideologyFiles = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories).ToList();
                if (!File.Exists(path))
                    throw new FileNotFoundException($"File not found: {path}");

                string content = File.ReadAllText(path);

                var parser = new TxtParser(new TxtPattern());

                var parsedFile = (HoiFuncFile)parser.Parse(content);
                if (parsedFile == null)
                    throw new InvalidOperationException("Failed to parse the ideology file.");

                var ideologiesBracket = parsedFile.Brackets.FirstOrDefault(b => b.Name == "ideologies");
                if (ideologiesBracket == null)
                    return new List<IConfig>();

                var configs = new List<IConfig>();
                foreach (var ideologyBracket in ideologiesBracket.SubBrackets)
                {
                    var config = ParseIdeologyConfig(ideologyBracket.Name, ideologyBracket);
                    if (config != null)
                        configs.Add(config);
                }

                return configs;
            }
            return new List<IConfig>();

        }
        private static IdeologyConfig ParseIdeologyConfig(string name, Bracket bracket)
        {
            var config = new IdeologyConfig
            {
                Id = new Identifier(name),
                SubTypes = new List<IdeologyType>(),
                Rules = new Dictionary<RuleConfig, bool>(),
                Modifiers = new Dictionary<ModifierDefinitionConfig, object>(),
                FactionModifiers = new Dictionary<ModifierDefinitionConfig, object>(),
                DynamicFactionNames = new List<string>()
            };

            var typesBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "types");
            if (typesBracket != null)
            {
                foreach (var typeBracket in typesBracket.SubBrackets)
                {
                    var type = new IdeologyType
                    {
                        Name = typeBracket.Name,
                        Parrent = name
                    };

                    Var canBeRandom = typeBracket.SubVars.FirstOrDefault(v => v.Name == "can_be_randomly_selected");
                    if (canBeRandom != null && canBeRandom.Value is bool canBeRandomval)
                    {
                        type.CanBeRandomlySelected = canBeRandomval;
                    }

                    var colorValue = typeBracket.SubVars.FirstOrDefault(v => v.Name == "color");
                    if (colorValue != null && colorValue.Value is Color colorobj)
                    {
                        type.Color = colorobj;
                    }

                    config.SubTypes.Add(type);
                }
            }

            // Обработка dynamic_faction_names
            var namesArr = bracket.Arrays.FirstOrDefault(b => b.Name == "dynamic_faction_names");
            if (namesArr != null)
            {
                config.DynamicFactionNames = namesArr.Values
                    .SelectMany(line => line.ToString().ParseQuotedStrings())
                    .ToList();
            }

            // Обработка color
            var color = bracket.SubVars.FirstOrDefault(b => b.Name == "color");
            if (color != null)
            {
                config.Color = (Color)color.Value;
            }

            // Обработка rules
            var rulesBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "rules");
            if (rulesBracket != null)
            {
                foreach (var varItem in rulesBracket.SubVars)
                {
                    var rule = ModConfig.Instance.Rules.Where(r => r.Id == (varItem.Value as HoiReference).Value).FirstOrDefault();

                    if (!config.Rules.TryAdd(rule, (bool)varItem.Value))
                    {
                        Logger.AddLog($"Не удалось добавить правило с Id = {(varItem.Value as HoiReference).Value}");
                    }

                }
            }

            // Обработка modifiers
            var modsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "modifiers");
            if (modsBracket != null)
            {
                foreach (var varItem in modsBracket.SubVars)
                {
                    var mod = ModConfig.Instance.ModifierDefenitions.Where(r => r.Id.AsString() == (varItem.Value as HoiReference).Value).FirstOrDefault();
                    if (!config.Modifiers.TryAdd(mod, varItem.Value))
                    {
                        Logger.AddLog($"Не удалось добавить модифер с Id = {(varItem.Value as HoiReference).Value}");
                    }
                }
            }

            // Обработка faction_modifiers
            var factionModsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "faction_modifiers");
            if (factionModsBracket != null)
            {
                foreach (var varItem in factionModsBracket.SubVars)
                {
                    var mod = ModConfig.Instance.ModifierDefenitions.Where(r => r.Id.AsString() == (varItem.Value as HoiReference).Value).FirstOrDefault();
                    if (!config.Modifiers.TryAdd(mod, varItem.Value))
                    {
                        Logger.AddLog($"Не удалось добавить модифер для фракции с Id = {(varItem.Value as HoiReference).Value}");
                    }
                }
            }

            // Обработка остальных переменных
            foreach (var varItem in bracket.SubVars)
            {
                if (varItem.Name.StartsWith("ai_"))
                {
                    string aiName = varItem.Name.Substring(3);
                    IdeologyAIType ideologyAIType;
                    switch (aiName)
                    {
                        case "neutrality":
                            ideologyAIType = IdeologyAIType.Neutrality;
                            break;
                        case "democracy":
                            ideologyAIType = IdeologyAIType.Democracy;
                            break;
                        case "fascism":
                            ideologyAIType = IdeologyAIType.Fascism;
                            break;
                        case "communism":
                            ideologyAIType = IdeologyAIType.Communism;
                            break;
                        default:
                            ideologyAIType = IdeologyAIType.None; // Keep original if not matched
                            break;
                    }
                    config.AiIdeologyName = ideologyAIType;
                }

                switch (varItem.Name)
                {
                    case "can_host_government_in_exile":
                        config.CanFormExileGoverment = (bool)varItem.Value;
                        break;
                    case "war_impact_on_world_tension":
                        config.WarImpactOnTension = (double)varItem.Value;
                        break;
                    case "faction_impact_on_world_tension":
                        config.FactionImpactOnTension = (double)varItem.Value;
                        break;
                    case "can_be_boosted":
                        config.CanBeBoosted = (bool)varItem.Value;
                        break;
                    case "can_collaborate":
                        config.CanColaborate = (bool)varItem.Value;
                        break;
                    default:

                        break;
                }
            }

            return config;
        }
    }
}
