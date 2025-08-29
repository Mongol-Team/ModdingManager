using ModdingManager.classes.utils;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Interfaces;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types.ObectCacheData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
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

            var parser = new FunkFileParser();
          
            var parsedFile = (HoiFunkFile)parser.Parse(content, new TXTPattern());
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
                Rules = new List<Var>(),
                Modifiers = new List<Var>(),
                FactionModifiers = new List<Var>(),
                DynamicFactionNames = new List<string>()
            };

            // Обработка types
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
                    if (colorValue != null && colorValue.Value is Color color)
                    {
                        type.Color = color;
                    }

                    config.SubTypes.Add(type);
                }
            }

            // Обработка dynamic_faction_names
            var namesBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "dynamic_faction_names");
            if (namesBracket != null)
            {
                config.DynamicFactionNames = namesBracket.Content
                    .SelectMany(line => ParseQuotedStrings(line))
                    .ToList();
            }

            // Обработка color
            var colorBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "color");
            if (colorBracket != null && colorBracket.Content.Count > 0)
            {
                config.Color = ParseColor(colorBracket.Content[0]);
            }

            // Обработка rules
            var rulesBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "rules");
            if (rulesBracket != null)
            {
                foreach (var varItem in rulesBracket.SubVars)
                {
                    config.Rules.Add(varItem);
                }
            }

            // Обработка modifiers
            var modsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "modifiers");
            if (modsBracket != null)
            {
                foreach (var varItem in modsBracket.SubVars)
                {
                    config.Modifiers.Add(varItem);
                }
            }

            // Обработка faction_modifiers
            var factionModsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "faction_modifiers");
            if (factionModsBracket != null)
            {
                foreach (var varItem in factionModsBracket.SubVars)
                {
                    config.FactionModifiers.Add(varItem);
                }
            }

            // Обработка остальных переменных
            foreach (var varItem in bracket.SubVars)
            {
                if (varItem.Name.StartsWith("ai_"))
                {

                    config.AiIdeologyName = varItem;
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
        
        #region Fimoz
        public static List<ProvinceConfig> ParseProvinceConfigs(string[] lines)
        {
            List<ProvinceConfig> res = new List<ProvinceConfig>();
            List<ProvinceConfig> seaProvinces = new List<ProvinceConfig>();
            List<ProvinceConfig> otherProvinces = new List<ProvinceConfig>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Trim().Split(';');
                if (parts.Length < 8)
                    continue;

                try
                {
                    var province = new ProvinceConfig
                    {
                        Id = int.Parse(parts[0]),
                        Color = System.Drawing.Color.FromArgb(
                            byte.Parse(parts[1]),
                            byte.Parse(parts[2]),
                            byte.Parse(parts[3])),
                        Type = parts[4],
                        IsCoastal = bool.Parse(parts[5]),
                        Terrain = parts[6],
                        ContinentId = int.Parse(parts[7])
                    };

                    if (province.Type.Equals("sea", StringComparison.OrdinalIgnoreCase))
                        seaProvinces.Add(province);
                    else
                        otherProvinces.Add(province);
                }
                catch
                {
                    continue;
                }
            }
            GetProvinceNames(otherProvinces);
            GetProvinceVictoryPoints(otherProvinces);
            res = seaProvinces.Concat(otherProvinces).ToList();
            return res;
        }
        public static List<StrategicRegionConfig> ParseStrategicMap(MapConfig map)
        {
            var strategicMap = new Dictionary<int, StrategicRegionConfig>();

            string[] priorityFolders = {
                Path.Combine(ModManager.ModDirectory, "map", "strategicregions"),
                Path.Combine(ModManager.GameDirectory, "map", "strategicregions")
            };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder))
                    continue;

                string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    var searcher = new BracketSearcher { CurrentString = content.ToCharArray() };
                    var regionBrackets = searcher.FindBracketsByName("strategic_region");

                    foreach (var regionBracket in regionBrackets)
                    {
                        var idVar = regionBracket.SubVars.FirstOrDefault(v => v.Name == "id");
                        if (idVar == null || !int.TryParse(idVar.Value as string, out int id) || strategicMap.ContainsKey(id))
                            continue;

                        var provincesBracket = regionBracket.SubBrackets.FirstOrDefault(b => b.Header == "provinces");
                        if (provincesBracket == null) continue;

                        var provinceIds = provincesBracket.Content
                            .SelectMany(line => line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                            .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                            .Where(x => x.HasValue)
                            .Select(x => x.Value)
                            .ToHashSet();

                        var matchedProvinces = map.Provinces
                            .Where(p => provinceIds.Contains(p.Id))
                            .ToList();

                        strategicMap[id] = new StrategicRegionConfig
                        {
                            Id = id,
                            Provinces = matchedProvinces,
                            FilePath = file,
                            Color = ModManager.GenerateColorFromId(id)
                        };
                    }
                }
            }

            return strategicMap.Values.ToList();
        }
        public static StateConfig ParseStateConfig(string path, MapConfig map)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"State file not found: {path}");

            string content = File.ReadAllText(path);

            var parser = new FunkFileParser();
            var pattern = new TXTPattern();

            var parsedFile = (HoiFunkFile)parser.Parse(content, pattern);
            if (parsedFile == null)
                throw new InvalidOperationException($"Failed to parse state file: {path}");

            var stateBracket = parsedFile.Brackets.FirstOrDefault(b => b.Name == "state");
            if (stateBracket == null)
            {
                Logger.AddLog($"[❌] No 'state' bracket found in file: {path}");
                return null;
            }

            try
            {
                // Extract state ID
                var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
                if (idVar == null || !int.TryParse(idVar.Value as string, out int id))
                {
                    Logger.AddLog($"[❌] Invalid or missing 'id' in file: {path}");
                    return null;
                }

                // Extract state name
                var nameVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "name");
                string internalName = nameVar?.Value?.ToString().Trim('"');
                if (string.IsNullOrEmpty(internalName))
                {
                    Logger.AddLog($"[❌] Missing or empty 'name' in file: {path}");
                    return null;
                }

                // Extract provinces
                var provincesBracket = stateBracket.SubBrackets.FirstOrDefault(b => b.Name == "provinces");
                if (provincesBracket == null)
                {
                    Logger.AddLog($"[❌] No 'provinces' bracket found in state {id} ({internalName}) in file: {path}");
                    return null;
                }

                var provinceIds = provincesBracket.Content
                    .SelectMany(line => line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToHashSet();

                var matchedProvinces = map.Provinces
                    .Where(p => provinceIds.Contains(p.Id))
                    .ToList();
                if (matchedProvinces == null || !matchedProvinces.Any())
                {
                    Logger.AddLog($"[❌] No matched provinces for state {id} in file: {path}");
                    return null;
                }

                // Extract history and buildings
                var historyBracket = stateBracket.SubBrackets.FirstOrDefault(b => b.Name == "history");
                if (historyBracket == null)
                {
                    Logger.AddLog($"[❌] No 'history' bracket found in state {id} ({internalName}) in file: {path}");
                    return null;
                }

                var buildings = historyBracket.SubBrackets.FirstOrDefault(b => b.Name == "buildings")?.SubVars;
                if (buildings == null)
                {
                    Logger.AddLog($"[⚠️] No 'buildings' bracket found in state {id} ({internalName}) in file: {path}");
                }

                // Extract manpower
                int? manpower = null;
                var manpowerVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "manpower");
                if (manpowerVar != null && int.TryParse(manpowerVar.Value as string, out int mp))
                {
                    manpower = mp;
                }

                // Extract local supplies
                double? localSupply = null;
                var localSuppliesVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "local_supplies");
                if (localSuppliesVar != null && double.TryParse(localSuppliesVar.Value as string, NumberStyles.Float, CultureInfo.InvariantCulture, out double ls))
                {
                    localSupply = ls;
                }

                // Extract state category
                var category = stateBracket.SubVars.FirstOrDefault(v => v.Name == "state_category")?.Value?.ToString();

                // Create StateConfig
                var stateConfig = new StateConfig
                {
                    Id = id,
                    Color = ModManager.GenerateColorFromId(id),
                    FilePath = path,
                    Provinces = matchedProvinces,
                    LocalizationKey = nameVar?.Value as string,
                    Manpower = manpower,
                    Buildings = buildings ?? new List<Var>(),
                    LocalSupply = localSupply,
                    Cathegory = category
                };

                stateConfig.Name = GetStateName(stateConfig); // Assuming GetStateNames is available in ConfigComposer

                return stateConfig;
            }
            catch (Exception ex)
            {
                Logger.AddLog($"[❌] Error parsing state in file {path}: {ex.Message}\n{stateBracket}");
                return null;
            }
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
                    TagFolder = ModPathes.CountryTagsModPath,
                    StateFolder = ModPathes.StatesModPath
                },
                new
                {
                    TagFolder = GamePathes.CountryTagsGamePath,
                    StateFolder = GamePathes.StatesGamePath
                }
            };

            var parser = new FunkFileParser();
            var pattern = new TXTPattern();
            Dictionary<string, object> tagLookup = new Dictionary<string, object>();
            foreach (var set in folders)
            {
                if (!Directory.Exists(set.TagFolder) || !Directory.Exists(set.StateFolder))
                    continue;
                foreach (var file in Directory.GetFiles(set.TagFolder))
                {
                    HoiFunkFile parsedfile = parser.Parse(file, pattern) as HoiFunkFile; // Assuming VarSearcher is still used for tags
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
                    var parsedFile = (HoiFunkFile)parser.Parse(content, pattern);
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
                countryConfig.Color = GetCountryColor(countryConfig.Tag, countryPath.ToString()).ToDrawingColor();
            }
        }
        private static void GetProvinceNames(List<ProvinceConfig> list)
        {
            var victoryPoints = _instance.LocCache.VictoryPointsLocalisation;
            var allCache = _instance.LocCache.AllCache;

            var vpLookup = victoryPoints.ToDictionary(v => v.Name, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);
            var allLookup = allCache.ToDictionary(v => v.Name, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            foreach (var province in list)
            {
                string key = $"VICTORY_POINTS_{province.Id}";

                if (vpLookup.TryGetValue(key, out var name))
                {
                    province.Name = name.Trim('"');
                }
                else if (allLookup.TryGetValue(key, out var altName))
                {
                    province.Name = altName.Trim('"');
                }
                else
                {
                    province.Name = $"[Unnamed {province.Id}]";
                }
            }
        }
        private static System.Windows.Media.Color GetCountryColor(string tag, string countryPath)
        {
           
            string[] possiblePaths = {
                GamePathes.CommonCountriesGamePath,
                ModPathes.CommonCountriesModPath
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path);
                    var searcher = new BracketSearcher { CurrentString = content.ToCharArray() };
                    var colorBrackets = searcher.FindBracketsByName("color", "rgb");

                    if (colorBrackets.Count > 0)
                    {
                        return VarSearcher.ParseColor(colorBrackets[0].Content.FirstOrDefault()).ToMediaColor();
                    }
                }
            }
        }
        public static void LoadMap()
        {
            string definitionPath = System.IO.Path.Combine(ModManager.ModDirectory, "map", "definition.csv");
            string provinceImagePath = System.IO.Path.Combine(ModManager.ModDirectory, "map", "provinces.bmp");

            if (!File.Exists(definitionPath))
                throw new FileNotFoundException("[❌] Не найден файл definition.csv", definitionPath);
            if (!File.Exists(provinceImagePath))
                throw new FileNotFoundException("[❌] Не найден файл provinces.bmp", provinceImagePath);

            string[] lines = File.ReadAllLines(definitionPath);
            Instance.Map = ParseProvinceMap(lines);
            Instance.Map.States = ParseStateMap(Instance.Map);
            Instance.Map.StrategicRegions = ParseStrategicMap(Instance.Map);
            Instance.Map.Countries = ParseCountryMap(Instance.Map);
            Instance.Map.Bitmap = new Bitmap(provinceImagePath);
            
        }
        #endregion
        #region Helper Methods
        private static string GetStateName(StateConfig state)
        {
            List<Var> stateLocals = new(ConfigRegistry.Instance.LocCache.StateLocalisation);
            List<Var> allCache = new(ConfigRegistry.Instance.LocCache.AllCache);

            var stateLookup = stateLocals.ToDictionary(v => v.Name, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);
            var allLookup = allCache.ToDictionary(v => v.Name, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            string key = state.LocalizationKey.Trim('"');

            if (stateLookup.TryGetValue(key, out var name))
            {
                return name.Trim('"');
            }
            else if (allLookup.TryGetValue(key, out var altName))
            {
                return altName.Trim('"');
            }
            else
            {
                return $"[Unnamed {state.Id}]";
            }
        }

        #endregion
    }
}
