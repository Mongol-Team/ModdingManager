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
using ModdingManagerModels.Types.ObectCacheData;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.TableCacheData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ModdingManagerClassLib.utils.Pathes
{
    public class ConfigComposer
    {
        public static List<IdeologyConfig> ParseIdeologyConfigs(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            string content = File.ReadAllText(path);

            var parser = new TxtParser(new TxtPattern());
          
            var parsedFile = (HoiFuncFile)parser.Parse(content);
            if (parsedFile == null)
                throw new InvalidOperationException("Failed to parse the ideology file.");

            var ideologiesBracket = parsedFile.Brackets.FirstOrDefault(b => b.Name == "ideologies");
            if (ideologiesBracket == null)
                return new List<IdeologyConfig>();

            var configs = new List<IdeologyConfig>();
            foreach (var ideologyBracket in ideologiesBracket.SubBrackets)
            {
                var config = ParseIdeologyConfig(ideologyBracket.Name, ideologyBracket);
                if (config != null)
                    configs.Add(config);
            }

            return configs;
        }

        private static IdeologyConfig ParseIdeologyConfig(string name, Bracket bracket)
        {
            var config = new IdeologyConfig
            {
                Id = name,
                SubTypes = new List<IdeologyType>(),
                Rules = new Dictionary<RuleConfig, bool>(),
                Modifiers = new Dictionary<ModifierDefenitionConfig, object>(),
                FactionModifiers = new Dictionary<ModifierDefenitionConfig, object>(),
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
                    .SelectMany(line => ParseQuotedStrings(line.ToString()))
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
                    var rule = ConfigRegistry.Instance.Rules.Where(r => r.Id == (varItem.Value as HoiReference).Value).FirstOrDefault();

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
                    var mod = ConfigRegistry.Instance.ModifierDefenitions.Where(r => r.Name == (varItem.Value as HoiReference).Value).FirstOrDefault();
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
                    var mod = ConfigRegistry.Instance.ModifierDefenitions.Where(r => r.Name == (varItem.Value as HoiReference).Value).FirstOrDefault();
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
        public static List<ProvinceConfig> ParseProvinceConfigs()
        {
            List<ProvinceConfig> res = new List<ProvinceConfig>();
            List<ProvinceConfig> seaProvinces = new List<ProvinceConfig>();
            List<ProvinceConfig> otherProvinces = new List<ProvinceConfig>();
            CsvParser csvParser = new CsvParser(new CsvDefinitionsPattern());
            var defFile = csvParser.Parse(ModPathes.DefinitionPath) as HoiTable;
            foreach (var line in defFile.Values)
            {
                if (line == null || line.Count < 8)
                    continue;
                try
                {
                    var province = new ProvinceConfig
                    {
                        Id = (int)line[0],
                        Color = (Color)line[1],
                        Type = (ProvinceType)line[2],
                        IsCoastal = (bool)line[3],
                        Terrain = (string)line[4],
                        ContinentId = (int)line[5],
                    };

                    if (province.Type == ProvinceType.sea)
                        seaProvinces.Add(province);
                    else
                        otherProvinces.Add(province);
                }
                catch
                {
                    continue;
                }
            }

            res = seaProvinces.Concat(otherProvinces).ToList();
            return res;
        }
        #region Fimoz

        public static List<StrategicRegionConfig> ParseStrategicMap()
        {
            var strategicMap = new Dictionary<int, StrategicRegionConfig>();

            string[] priorityFolders = {
                ModPathes.StrategicRegionPath,
                GamePathes.StrategicRegionsPath,
            };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder))
                    continue;

                string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);

                foreach (string filePath in files)
                {
                    HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(filePath) as HoiFuncFile;

                    foreach (var regionBracket in file.Brackets)
                    {
                        var idVar = regionBracket.SubVars.FirstOrDefault(v => v.Name == "id");
                        if (idVar == null || !int.TryParse(idVar.Value as string, out int id) || strategicMap.ContainsKey(id))
                            continue;

                        HoiArray provincesBracket = regionBracket.Arrays.FirstOrDefault(b => b.Name == "provinces");
                        if (provincesBracket == null) continue;

                        var matchedProvinces = ConfigRegistry.Instance.Map.Provinces
                            .Where(p => provincesBracket.Values.Contains(p.Id))
                            .ToList();

                        strategicMap[id] = new StrategicRegionConfig
                        {
                            Id = id,
                            Provinces = matchedProvinces,
                            FilePath = file.FilePath,
                            Color = ModManager.GenerateColorFromId(id)
                        };
                    }
                }
            }

            return strategicMap.Values.ToList();
        }
        public static List<ProvinceConfig> ParseAllStateProvinces()
        {
            var allProvinces = new List<ProvinceConfig>();

            string[] priorityFolders = {
                ModPathes.StatesPath,
                GamePathes.StatesPath,
            };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder))
                    continue;

                string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);

                foreach (string filePath in files)
                {
                    var state = ParseSingleState(filePath);
                    if (state == null || state.Provinces == null)
                        continue;

                    allProvinces.AddRange(state.Provinces);
                }
            }

            return allProvinces;
        }

        public static StateConfig ParseSingleState(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Logger.AddLog($"Файл не найден: {filePath}");
                return null;
            }

            HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(filePath) as HoiFuncFile;
            if (file == null || file.Brackets.Count == 0)
            {
                Logger.AddLog($"Не удалось распарсить файл: {filePath}");
                return null;
            }

            var stateBracket = file.Brackets.First();

            var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
            if (idVar == null || !int.TryParse(idVar.Value as string, out int id))
            {
                Logger.AddLog($"Не удалось извлечь ID из файла: {filePath}");
                return null;
            }

            var provincesArray = stateBracket.Arrays.FirstOrDefault(a => a.Name == "provinces");
            var provinceIds = provincesArray?.Values
                .OfType<object>()
                .Select(v => v is HoiReference hr ? hr.Value : v)
                .OfType<int>()
                .ToList() ?? new List<int>();
            var 
            var matchedProvinces = ConfigRegistry.Instance.Map.Provinces
                .Where(p => provinceIds.Contains(p.Id))
                .ToList();

            return new StateConfig
            {
                Id = id,
                Provinces = matchedProvinces,
                FilePath = file.FilePath,
                Color = ModManager.GenerateColorFromId(id),
                Cathegory = stateBracket.SubVars.FirstOrDefault(v => v.Name == "category")?.Value as string,
            };
        }

        public static void PopulateCountryStates(CountryConfig countryConfig, MapConfig map)
        {
            if (string.IsNullOrEmpty(countryConfig.Tag))
                throw new ArgumentException("CountryConfig.Tag cannot be null or empty.");

            var visitedIds = new HashSet<int>();
            countryConfig.States = countryConfig.States ?? new List<StateConfig>();

            // Define folder sets for mod and vanilla paths
            var folders = new[]
            {
                new
                {
                    TagFolder = ModPathes.CountryTagsPath,
                    StateFolder = ModPathes.StatesPath
                },
                new
                {
                    TagFolder = GamePathes.CountryTagsPath,
                    StateFolder = GamePathes.StatesPath
                }
            };

            
            var pattern = new TxtPattern();
            var parser = new TxtParser(pattern);
            Dictionary<string, object> tagLookup = new Dictionary<string, object>();
            foreach (var set in folders)
            {
                if (!Directory.Exists(set.TagFolder) || !Directory.Exists(set.StateFolder))
                    continue;
                foreach (var file in Directory.GetFiles(set.TagFolder))
                {
                    HoiFuncFile parsedfile = parser.Parse(file) as HoiFuncFile; // Assuming VarSearcher is still used for tags
                    tagLookup = parsedfile.Vars
                        .GroupBy(v => v.Name)
                        .Select(g => g.First())
                        .ToDictionary(v => v.Name, v => v.Value);

                    if (!tagLookup.ContainsKey(countryConfig.Tag))
                        continue;
                }
              

                foreach (var file in Directory.GetFiles(set.StateFolder, "*.txt", SearchOption.AllDirectories))
                {
                    string content = File.ReadAllText(file);
                    var parsedFile = (HoiFuncFile)parser.Parse(content);
                    if (parsedFile == null)
                    {
                        Logger.AddLog($"[⚠️] Failed to parse state file: {file}");
                        continue;
                    }

                    var stateBracket = parsedFile.Brackets.FirstOrDefault(b => b.Name == "state");
                    if (stateBracket == null)
                    {
                        Logger.AddLog($"[⚠️] No 'state' bracket found in file: {file}");
                        continue;
                    }

                    var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
                    if (idVar == null || !int.TryParse(idVar.Value as string, out int id) || visitedIds.Contains(id))
                        continue;

                    var historyBracket = stateBracket.SubBrackets.FirstOrDefault(b => b.Name == "history");
                    if (historyBracket == null)
                    {
                        Logger.AddLog($"[⚠️] No 'history' bracket found in state {id} in file: {file}");
                        continue;
                    }

                    var ownerVar = historyBracket.SubVars.FirstOrDefault(v => v.Name == "owner");
                    if (ownerVar == null || ownerVar.Value as string != countryConfig.Tag)
                        continue;

                    var state = map.States?.FirstOrDefault(s => s.Id == id);
                    if (state == null)
                    {
                        Logger.AddLog($"[⚠️] State {id} not found in map.States for file: {file}");
                        continue;
                    }

                    if (!countryConfig.States.Any(s => s.Id == id))
                    {
                        countryConfig.States.Add(state);
                        visitedIds.Add(id);
                    }
                }

                // Stop processing further folders if states are found (mimics original behavior)
                if (countryConfig.States.Any())
                    break;
            }

            // Update country color if not set
            if (countryConfig.Color == null && tagLookup.TryGetValue(countryConfig.Tag, out var countryPath))
            {
                countryConfig.Color = GetCountryColor(countryConfig.Tag);
            }
        }
        
        private static System.Drawing.Color GetCountryColor(string tag)
        {
           
            string[] possiblePaths = {
                GamePathes.CommonCountriesPath,
                ModPathes.CommonCountriesPath
            };
            System.Drawing.Color col = Color.FromArgb(128, 128, 128);
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    TxtPattern pattern = new TxtPattern();
                    TxtParser parser = new TxtParser(pattern);
                    HoiFuncFile file = parser.Parse(File.ReadAllText(path)) as HoiFuncFile;
                    Var colorVar = file.Vars.Where(v => v.Name == "collor" || v.PossibleCsType is Color).ToList().First();
                    
                    if (colorVar != null)
                    {
                        return (Color)colorVar.Value;
                    }
                    else
                    {
                        Logger.AddLog($"[⚠️] Common countries file not found at: {path}");
                    }
                }

                
            }
            return col;
        }

        #endregion
        #region Helper Methods
        private static List<string> ParseQuotedStrings(string line)
        {
            var results = new List<string>();

            // Регулярное выражение для поиска строк в кавычках с учётом экранирования
            var pattern = @"(?<!\\)([""'])(.*?)(?<!\\)\1";

            foreach (Match match in Regex.Matches(line, pattern))
            {
                results.Add(match.Groups[2].Value);
            }

            return results;
        }
       

        #endregion
    }
}
