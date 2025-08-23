using ModdingManager.classes.cache.data;
using ModdingManager.classes.managers.gfx;
using ModdingManager.classes.utils.search;
using ModdingManager.managers.@base;
using ModdingManagerClassLib;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerModels;
using ModdingManagerModels.Types.ObectCacheData;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Media;

namespace ModdingManager.classes.utils
{
    public class Registry
    {
        private Registry() { }
        private static Registry _instance = new();
        public static Registry Instance => _instance ??= new Registry();
        public LocalisationCache LocCache { get; set; }
        public MapCache MapCache { get; set; }
        public List<RegimentConfig> Regiments { get; set; }
        public List<CountryConfig> Countries { get; set; }
        public List<IdeaConfig> Ideas { get; set; }
        public List<string> Modifiers { get; set; }
        public MapConfig Map { get; set; }
        public List<CountryCharacterConfig> Characters { get; set; }
        public List<IdeologyConfig> Ideologies { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Ideologies)
            {
                sb.AppendLine(item.ToString());
            }
            return sb.ToString();
        }
        public static void LoadInstance()
        {
            using LogScope scope = new LogScope("Loading Registry", ConsoleColor.Magenta);
            
                LoadCache();
                LoadRegiemts();
                LoadCountries();
                LoadIdeas();
                LoadModifiers();
                LoadCharacters();
                LoadIdeologies();
                LoadMap();
            
        }
        #region Load Methods
        private static void LoadCache()
        {
            _instance.LocCache = new();
            _instance.MapCache = new();
        }
        private static void LoadCharacters()
        {

        }
        private static void LoadIdeologies()
        {
            _instance.Ideologies = new List<IdeologyConfig>();
            var ideologyFiles = new List<string>();

            string modDir = Path.Combine(ModManager.ModDirectory, "common", "ideologies");
            if (Directory.Exists(modDir))
                ideologyFiles.AddRange(Directory.GetFiles(modDir, "*.*", SearchOption.AllDirectories));

            string gameDir = Path.Combine(ModManager.GameDirectory, "common", "ideologies");
            if (Directory.Exists(gameDir) && ideologyFiles.Count < 1)
                ideologyFiles.AddRange(Directory.GetFiles(gameDir, "*.*", SearchOption.AllDirectories));

            foreach (var file in ideologyFiles)
            {
                string fileContent = File.ReadAllText(file);
                var searcher = new BracketSearcher
                {
                    CurrentString = fileContent.ToCharArray(),
                    OpenBracketChar = '{',
                    CloseBracketChar = '}'
                };

                var ideologyBrackets = searcher.FindBracketsByName("ideologies");
                if (ideologyBrackets.Count == 0) continue;

                var ideologyMainBracket = ideologyBrackets[0];
                foreach (var subBracket in ideologyMainBracket.SubBrackets)
                {
                    var config = ParseIdeologyConfig(subBracket.Header, subBracket);
                    if (config != null)
                        _instance.Ideologies.Add(config);
                }

                _instance.Ideologies.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            }
        }
        private static void LoadRegiemts()
        {
            var result = new List<RegimentConfig>();
            var unitDefs = CollectUnitDefinitions();

            foreach (var (name, categories) in unitDefs)
            {
                var icon = ImageManager.FindUnitIcon(name);
                result.Add(new RegimentConfig
                {
                    Name = name,
                    Categories = categories,
                    Icon = icon
                });
            }

            _instance.Regiments = result;
        }
        private static void LoadCountries()
        {

        }
        private static void LoadIdeas()
        {

        }
        private static void LoadModifiers()
        {

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
        #region Parse Methods
        public static MapConfig ParseProvinceMap(string[] lines)
        {
            MapConfig res = new MapConfig();
            var seaProvinces = new List<ProvinceConfig>();
            var otherProvinces = new List<ProvinceConfig>();

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
            res.Provinces = seaProvinces.Concat(otherProvinces).ToList();
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

        public static List<StateConfig> ParseStateMap(MapConfig map)
        {
            var stateMap = new Dictionary<int, StateConfig>();

            string[] priorityFolders = {
                Path.Combine(ModManager.ModDirectory, "history", "states"),
                Path.Combine(ModManager.GameDirectory, "history", "states")
            };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder)) continue;
                string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    var searcher = new BracketSearcher { CurrentString = content.ToCharArray() };
                    var stateBrackets = searcher.FindBracketsByName("state");

                    foreach (var stateBracket in stateBrackets)
                    {
                        try
                        {
                            var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
                            if (idVar == null || !int.TryParse(idVar.Value as string, out int id) || stateMap.ContainsKey(id))
                                continue;
                            if (idVar.Value.ToString() == "2")
                            {
                            }
                            var nameVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "name");
                            string internalName = nameVar?.Value?.ToString().Trim('"');
                            if (string.IsNullOrEmpty(internalName)) continue;

                            var provincesBracket = stateBracket.SubBrackets.FirstOrDefault(b => b.Header == "provinces");
                            if (provincesBracket == null) continue;
                            var historyBracket = stateBracket.SubBrackets.FirstOrDefault(b => b.Header == "history");
                            var provinceIds = provincesBracket.Content
                                .SelectMany(line => line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                                .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                                .Where(x => x.HasValue)
                                .Select(x => x.Value)
                                .ToHashSet();

                            var matchedProvinces = map.Provinces
                                .Where(p => provinceIds.Contains(p.Id))
                                .ToList();
                            int.TryParse(stateBracket.SubVars.FirstOrDefault(v => v.Name == "manpower").Value as string, out int manpower);
                            var buildings = historyBracket.SubBrackets.FirstOrDefault(b => b.Header == "buildings")?.SubVars;
                            double.TryParse(stateBracket.SubVars.FirstOrDefault(v => v.Name == "local_supplies")?.Value as string, NumberStyles.Float, CultureInfo.InvariantCulture, out double localSupply);
                            var cathegory = stateBracket.SubVars.FirstOrDefault(v => v.Name == "state_category")?.Value.ToString();

                            if (buildings == null) throw new Exception("[⚠️] Buildings equals null.");


                            stateMap[id] = new StateConfig
                            {
                                Id = id,
                                Color = ModManager.GenerateColorFromId(id),
                                FilePath = file,
                                Provinces = matchedProvinces,
                                LocalizationKey = nameVar?.Value as string,
                                Manpower = manpower,
                                Buildings = buildings,
                                LocalSupply = localSupply,
                                Cathegory = cathegory,
                            };
                        }
                        catch (Exception ex)
                        {
                            Logger.AddLog(ex.Message + $"\n {stateBracket.ToString()}");
                        }
                    }
                }
            }

            var result = stateMap.Values.ToList();
            GetStateNames(result);
            return result;
        }

        public static List<CountryOnMapConfig> ParseCountryMap(MapConfig map)
        {
            var tagVars = new List<Var>();
            string[] tagFolders = {
                Path.Combine(ModManager.ModDirectory, "common", "country_tags"),
                Path.Combine(ModManager.GameDirectory, "common", "country_tags")
            };

            foreach (var folder in tagFolders)
            {
                if (Directory.Exists(folder))
                {
                    tagVars.AddRange(VarSearcher.TryGetCountryTags(folder));
                }
            }

            var tagLookup = tagVars
    .GroupBy(v => v.Name)
    .Select(g => g.First())
    .ToDictionary(v => v.Name, v => v.Value);

            var countries = new List<CountryOnMapConfig>();
            var visitedIds = new HashSet<int>();

            string[] stateFolders = {
        Path.Combine(ModManager.ModDirectory, "history", "states"),
        Path.Combine(ModManager.GameDirectory, "history", "states")
    };

            foreach (var folder in stateFolders)
            {
                if (!Directory.Exists(folder)) continue;

                foreach (var file in Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories))
                {
                    string content = File.ReadAllText(file);
                    var searcher = new BracketSearcher { CurrentString = content.ToCharArray() };
                    var stateBrackets = searcher.FindBracketsByName("state");

                    foreach (var stateBracket in stateBrackets)
                    {
                        var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
                        var ownerVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "owner");

                        if (idVar == null || !int.TryParse(idVar.Value as string, out int id) ||
                            ownerVar == null || visitedIds.Contains(id))
                            continue;

                        var state = map.States?.FirstOrDefault(s => s.Id == id);
                        if (state == null) continue;

                        string ownerTag = ownerVar.Value as string;
                        if (!tagLookup.TryGetValue(ownerTag, out var countryPath))
                            continue;

                        var country = countries.FirstOrDefault(c => c.Tag == ownerTag);
                        if (country == null)
                        {
                            var color = GetCountryColor(ownerTag, countryPath.ToString());
                            country = new CountryOnMapConfig
                            {
                                Tag = ownerTag,
                                Color = color.ToDrawingColor(),
                                States = new List<StateConfig>()
                            };
                            countries.Add(country);
                        }

                        country.States.Add(state);
                        visitedIds.Add(id);
                    }
                }
            }

            map.Countries = countries;
            return countries;
        }
        public static IdeologyConfig ParseIdeologyConfig(string name, Bracket bracket)
        {
            var config = new IdeologyConfig
            {
                Id = name,
                SubTypes = new List<IdeologyType>(),
                Rules = new Dictionary<string, bool>(),
                Modifiers = new Dictionary<string, double>(),
                FactionModifiers = new Dictionary<string, double>(),
                DynamicFactionNames = new List<string>()
            };

            // Обработка types
            var typesBracket = bracket.SubBrackets.FirstOrDefault(b => b.Header == "types");
            if (typesBracket != null)
            {
                foreach (var typeBracket in typesBracket.SubBrackets)
                {
                    var type = new IdeologyType
                    {
                        Name = typeBracket.Header,
                        Parrent = name
                    };

                    var canBeRandom = typeBracket.SubVars.FirstOrDefault(v => v.Name == "can_be_randomly_selected");
                    if (canBeRandom != null)
                    {
                        type.CanBeRandomlySelected = VarSearcher.ParseBool(canBeRandom.Value as string);
                    }

                    var colorValue = typeBracket.SubVars.FirstOrDefault(v => v.Name == "color");
                    if (colorValue != null)
                    {
                        type.Color = VarSearcher.ParseColor(colorValue.Value as string);
                    }

                    config.SubTypes.Add(type);
                }
            }

            // Обработка dynamic_faction_names
            var namesBracket = bracket.SubBrackets.FirstOrDefault(b => b.Header == "dynamic_faction_names");
            if (namesBracket != null)
            {
                config.DynamicFactionNames = namesBracket.Content
                    .SelectMany(line => VarSearcher.ParseQuotedStrings(line))
                    .ToList();
            }

            // Обработка color
            var colorBracket = bracket.SubBrackets.FirstOrDefault(b => b.Header == "color");
            if (colorBracket != null && colorBracket.Content.Count > 0)
            {
                config.Color = VarSearcher.ParseColor(colorBracket.Content[0]);
            }

            // Обработка rules
            var rulesBracket = bracket.SubBrackets.FirstOrDefault(b => b.Header == "rules");
            if (rulesBracket != null)
            {
                foreach (var var in rulesBracket.SubVars)
                {
                    config.Rules[var.Name] = VarSearcher.ParseBool(var.Value as string);
                }
            }

            // Обработка modifiers
            var modsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Header == "modifiers");
            if (modsBracket != null)
            {
                foreach (var var in modsBracket.SubVars)
                {
                    if (double.TryParse((var.Value as string), out double dVal))
                        config.Modifiers[var.Name] = dVal;
                }
            }

            // Обработка faction_modifiers
            var factionModsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Header == "faction_modifiers");
            if (factionModsBracket != null)
            {
                foreach (var var in factionModsBracket.SubVars)
                {
                    if (double.TryParse(var.Value as string, out double dVal))
                        config.FactionModifiers[var.Name] = dVal;
                }
            }

            // Обработка остальных переменных
            foreach (var var in bracket.SubVars)
            {
                switch (var.Name)
                {
                    case "can_host_government_in_exile":
                        config.CanFormExileGoverment = VarSearcher.ParseBool(var.Value as string);
                        break;
                    case "war_impact_on_world_tension":
                        if (double.TryParse((var.Value as string).Replace(".", ","), out double warTension))
                            config.WarImpactOnTension = warTension;
                        break;
                    case "faction_impact_on_world_tension":
                        if (double.TryParse((var.Value as string).Replace(".", ","), out double factionTension))
                            config.FactionImpactOnTension = factionTension;
                        break;
                    case "can_be_boosted":
                        config.CanBeBoosted = VarSearcher.ParseBool(var.Value as string);
                        break;
                    case "can_collaborate":
                        config.CanColaborate = VarSearcher.ParseBool(var.Value as string);
                        break;
                    case string s when s.StartsWith("ai_"):
                        config.AiIdeologyName = s.Substring(3);
                        break;
                }
            }

            return config;
        }
        #endregion
        #region Helper Methods
        public static void GetProvinceVictoryPoints(List<ProvinceConfig> provinces)
        {
            if (provinces == null || !provinces.Any() || Registry.Instance.MapCache.GetStateFiles().Count == 0)
                return;
            provinces = provinces
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .ToList();

            var provinceDict = provinces.ToDictionary(p => p.Id, p => p);
            foreach (var stateFile in Registry.Instance.MapCache.GetStateFiles())
            {
                var stateBracket = stateFile.Value.StateBracket;
                if (stateBracket == null)
                {
                    continue;
                }
                var victoryPointsBrackets = stateBracket.SubBrackets.Where(b => b.Header == "victory_points");

                foreach (var bracket in victoryPointsBrackets)
                {
                    foreach (var line in bracket.Content)
                    {
                        var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 2 &&
                            int.TryParse(parts[0], out int provinceId) &&
                            int.TryParse(parts[1], out int victoryPoints))
                        {
                            if (provinceDict.TryGetValue(provinceId, out var province))
                            {
                                if (province.VictoryPoints != victoryPoints)
                                {
                                    province.VictoryPoints = victoryPoints;

                                }
                            }
                        }
                    }
                }
            }

            // Инициализация нулевых значений (избыточно, можно удалить)
            foreach (var province in provinces.Where(p => p.VictoryPoints == 0))
            {
                province.VictoryPoints = 0;
            }
        }
        public IdeologyConfig? GetIdeology(string id)
        {
            return this.Ideologies.FirstOrDefault(i => i.Id == id);
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
            string pathClean = countryPath.Trim('"').Replace('/', Path.DirectorySeparatorChar);
            string[] possiblePaths = {
                Path.Combine(ModManager.ModDirectory, "common", pathClean),
                Path.Combine(ModManager.GameDirectory, "common", pathClean)
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path);
                    var searcher = new BracketSearcher { CurrentString = content.ToCharArray() };
                    var colorBrackets = searcher.FindBracketsByName("color");

                    if (colorBrackets.Count > 0)
                    {
                        return VarSearcher.ParseColor(colorBrackets[0].Content.FirstOrDefault()).ToMediaColor();
                    }
                }
            }

            return Colors.Gray;
        }
        private static void GetStateNames(List<StateConfig> states)
        {
            List<Var> stateLocals = new(_instance.LocCache.StateLocalisation);
            List<Var> allCache = new(_instance.LocCache.AllCache);

            var stateLookup = stateLocals.ToDictionary(v => v.Name, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);
            var allLookup = allCache.ToDictionary(v => v.Name, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            foreach (var state in states)
            {
                string key = state.LocalizationKey.Trim('"');

                if (stateLookup.TryGetValue(key, out var name))
                {
                    state.Name = name.Trim('"');
                }
                else if (allLookup.TryGetValue(key, out var altName))
                {
                    state.Name = altName.Trim('"');
                }
                else
                {
                    state.Name = $"[Unnamed {state.Id}]";
                }
            }
        }

        public static List<(string Name, List<string> Categories)> CollectUnitDefinitions()
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var rootPath in new[] { ModManager.GameDirectory, ModManager.ModDirectory })
            {
                if (!Directory.Exists(rootPath)) continue;

                var unitDirs = Directory.GetDirectories(rootPath, "units", SearchOption.AllDirectories)
                                      .Where(path => Path.GetFullPath(path)
                                          .Contains(Path.Combine("common", "units")));

                foreach (var unitDir in unitDirs)
                {
                    foreach (var file in Directory.GetFiles(unitDir, "*.txt", SearchOption.TopDirectoryOnly))
                    {
                        var fileContent = File.ReadAllText(file);
                        var fileSearcher = new BracketSearcher
                        {
                            CurrentString = fileContent.ToCharArray()
                        };

                        // Находим все блоки sub_units
                        var subUnitsBrackets = fileSearcher.FindBracketsByName("sub_units");

                        foreach (var subUnitsBracket in subUnitsBrackets)
                        {
                            // Обрабатываем все определения юнитов внутри sub_units
                            foreach (var unitBracket in subUnitsBracket.SubBrackets)
                            {
                                // Ищем блок categories внутри каждого юнита
                                var categoriesBracket = unitBracket.SubBrackets
                                    .FirstOrDefault(b => b.Header.Equals("categories", StringComparison.OrdinalIgnoreCase));

                                if (categoriesBracket != null)
                                {
                                    var cleanCategories = categoriesBracket.Content
                                        .SelectMany(line => line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                                        .Where(category => !string.IsNullOrWhiteSpace(category) &&
                                                          !category.Contains("{") &&
                                                          !category.Contains("}"))
                                        .ToList();

                                    if (cleanCategories.Count > 0)
                                    {
                                        var unitName = unitBracket.Header.Trim();
                                        if (!result.ContainsKey(unitName))
                                        {
                                            result[unitName] = cleanCategories;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        #endregion
    }
}
