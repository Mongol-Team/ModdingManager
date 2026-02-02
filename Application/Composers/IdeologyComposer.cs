using Application.Debugging;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.Enums;
using Models.GfxTypes;
using Models.Types;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System.Drawing;

namespace Application.Composers
{
    public class IdeologyComposer : IComposer
    {
        public IdeologyComposer() { }
        public static List<IConfig> Parse()
        {
            List<IConfig> res = new();
            string[] possiblePaths = {
                ModPathes.IdeologyPath,
                GamePathes.IdeologyPath
            };
            foreach (string path in possiblePaths)
            {
                if (!Directory.Exists(path))
                    throw new FileNotFoundException($"Dir not found: {path}");
                List<string> ideologyFiles = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories).ToList();
                foreach (string file in ideologyFiles)
                {
                    string content = File.ReadAllText(file);
                    var parser = new TxtParser(new TxtPattern());
                    var parsedFile = (HoiFuncFile)parser.Parse(content);
                    if (parsedFile == null)
                    {
                        Logger.AddDbgLog("Failed to parse the ideology file:" + file, "IdeologyComposer");
                        continue;
                    }
                    var fileConfigs = ParseFile(parsedFile, file);
                    foreach (var cfg in fileConfigs)
                    { 
                        cfg.FileFullPath = file;
                    }
                    res.AddRange(fileConfigs);
                }
            }
            if (res.Count > 0)
            {
                ParseDynamicModifierDefinitions(res.OfType<ModifierDefinitionConfig>().ToList());
            }
            return res;
        }

        public static List<IConfig> ParseFile(HoiFuncFile file, string FileFullPath)
        {
            List<IConfig> res = new();
            Bracket ideologiesBracket = file.Brackets.FirstOrDefault(b => b.Name == "ideologies");
            if (ideologiesBracket == null)
            {
                Logger.AddDbgLog("Failed to search ideologies in file:" + FileFullPath, "IdeologyComposer");
                return res;
            }
            foreach (var ideologyBracket in ideologiesBracket.SubBrackets)
            {
                var config = ParseIdeologyConfig(ideologyBracket.Name, ideologyBracket);
                if (config != null)
                    res.Add(config);
            }
            return res;
        }
        public static IdeologyConfig ParseIdeologyConfig(string name, Bracket bracket)
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
                    var rule = ModDataStorage.Mod.Rules.Where(r => r.Id.ToString() == varItem.Name).FirstOrDefault();
                    if (rule == null)
                    {
                        Logger.AddDbgLog($"Не удалось найти правило с Id = {varItem.Name}", "IdeologyComposer");
                        continue;
                    }
                    if (!config.Rules.TryAdd(rule, (bool)varItem.Value))
                    {
                        Logger.AddDbgLog($"Не удалось добавить правило с Id = {(varItem.Value as HoiReference).Value}", "IdeologyComposer");
                    }

                }
            }

            // Обработка modifiers
            var modsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "modifiers");
            if (modsBracket != null)
            {
                foreach (var varItem in modsBracket.SubVars)
                {
                    var mod = ModDataStorage.Mod.ModifierDefinitions.Where(r => r.Id.ToString() == varItem.Name).FirstOrDefault();
                    if (mod == null || varItem.Value == null)
                    {
                        Logger.AddDbgLog($"Ошибка, либо модификатор либо его значение не найдено!", "IdeologyComposer");
                        continue;
                    }
                    if (!config.Modifiers.TryAdd(mod, varItem.Value))
                    {
                        Logger.AddDbgLog($"Не удалось добавить модифер с Id = {(varItem.Value as HoiReference).Value}", "IdeologyComposer");
                    }
                }
            }

            // Обработка faction_modifiers
            var factionModsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "faction_modifiers");
            if (factionModsBracket != null)
            {
                foreach (var varItem in factionModsBracket.SubVars)
                {
                    var mod = ModDataStorage.Mod.ModifierDefinitions.Where(r => r.Id.ToString() == varItem.Name).FirstOrDefault();
                    if (mod == null || varItem.Value == null)
                    {
                        Logger.AddDbgLog($"Ошибка, либо модификатор либо его значение не найдено!", "IdeologyComposer");
                        continue;
                    }
                    if (!config.Modifiers.TryAdd(mod, varItem.Value))
                    {
                        Logger.AddDbgLog($"Не удалось добавить модифер для фракции с Id = {(varItem.Value as HoiReference).Value}", "IdeologyComposer");
                    }
                }
            }

            // Обработка остальных переменных
            foreach (var varItem in bracket.SubVars)
            {
                if (varItem.Name.StartsWith("ai_"))
                {
                    string aiName = varItem.Name.Substring(3);
                    var ideologyAIType = aiName switch
                    {
                        "neutrality" => IdeologyAIType.Neutrality,
                        "democracy" => IdeologyAIType.Democracy,
                        "fascism" => IdeologyAIType.Fascism,
                        "communism" => IdeologyAIType.Communism,
                        _ => IdeologyAIType.None,// Keep original if not matched
                    };
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

        public static void ParseDynamicModifierDefinitions(List<ModifierDefinitionConfig> defs)
        {
            foreach (var ideology in defs)
            {
                ModifierDefinitionConfig dynDriftMod = new();
                dynDriftMod.Id = new Identifier($"{ideology.Id}_drift");
                dynDriftMod.IsCore = true;
                dynDriftMod.Cathegory = ModifierDefinitionCathegoryType.Country;
                dynDriftMod.ValueType = ModifierDefenitionValueType.Number;
                dynDriftMod.ScopeType = ScopeTypes.Country;
                dynDriftMod.ColorType = ModifierDefenitionColorType.Good;
                dynDriftMod.Precision = 2;
                dynDriftMod.FileFullPath = DataDefaultValues.ItemCreatedDynamically;
                dynDriftMod.Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx);
                ModDataStorage.Mod.ModifierDefinitions.Add(dynDriftMod);
                dynDriftMod.Localisation = new ConfigLocalisation()
                {
                    Language = ModManagerSettings.CurrentLanguage,
                };
                dynDriftMod.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(dynDriftMod.Id.ToString()));

                ModifierDefinitionConfig dynAcceptanceMod = dynDriftMod;

                dynAcceptanceMod.Id = new Identifier($"{ideology.Id}_acceptance");
                dynAcceptanceMod.Localisation = new ConfigLocalisation()
                {
                    Language = ModManagerSettings.CurrentLanguage,
                };
                dynAcceptanceMod.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(dynAcceptanceMod.Id.ToString()));

                ModDataStorage.Mod.ModifierDefinitions.Add(dynAcceptanceMod);
                return;
            }
        }
    }
}
