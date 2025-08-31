using ModdingManager.classes.cache.data;
using ModdingManager.classes.managers.gfx;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerModels;
using System.Drawing;
using System.Text;
using System.Windows.Media;

namespace ModdingManager.classes.utils
{
    public class ConfigRegistry
    {
        private ConfigRegistry() { }
        private static ConfigRegistry _instance = new();
        public static ConfigRegistry Instance => _instance ??= new ConfigRegistry();
        public List<RuleConfig> Rules { get; set; }
        public List<RegimentConfig> Regiments { get; set; }
        public List<CountryConfig> Countries { get; set; }
        public List<IdeaConfig> Ideas { get; set; }
        public List<StaticModifierConfig> StaticModifiers { get; set; }
        public List<OpinionModifierConfig> OpinionModifiers { get; set; }
        public List<DynamicModifierConfig> DynamicModifiers { get; set; }
        public List<ModifierDefenitionConfig> ModifierDefenitions { get; set; }
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

        #endregion
        #region Parse Methods





        #endregion
        #region Helper Methods
        public static void GetProvinceVictoryPoints(List<ProvinceConfig> provinces)
        {
            if (provinces == null || !provinces.Any() || ConfigRegistry.Instance.MapCache.GetStateFiles().Count == 0)
                return;
            provinces = provinces
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .ToList();

            var provinceDict = provinces.ToDictionary(p => p.Id, p => p);
            foreach (var stateFile in ConfigRegistry.Instance.MapCache.GetStateFiles())
            {
                var stateBracket = stateFile.Value.StateBracket;
                if (stateBracket == null)
                {
                    continue;
                }
                var victoryPointsBrackets = stateBracket.SubBrackets.Where(b => b.Name == "victory_points");

                foreach (var bracket in victoryPointsBrackets)
                {
                    //foreach (var line in bracket.Content)
                    //{
                    //    var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //    if (parts.Length == 2 &&
                    //        int.TryParse(parts[0], out int provinceId) &&
                    //        int.TryParse(parts[1], out int victoryPoints))
                    //    {
                    //        if (provinceDict.TryGetValue(provinceId, out var province))
                    //        {
                    //            if (province.VictoryPoints != victoryPoints)
                    //            {
                    //                province.VictoryPoints = victoryPoints;

                    //            }
                    //        }
                    //    }
                    //}
                }
            }

            // Инициализация нулевых значений (избыточно, можно удалить)
            foreach (var province in provinces.Where(p => p.VictoryPoints == 0))
            {
                province.VictoryPoints = 0;
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
            string pathClean = countryPath.Trim('"').Replace('/', Path.DirectorySeparatorChar);
            string[] possiblePaths = {
                Path.Combine(ModManager.ModDirectory, "common", pathClean),
                Path.Combine(ModManager.GameDirectory, "common", pathClean)
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    //var content = File.ReadAllText(path);
                    //var searcher = new BracketSearcher { CurrentString = content.ToCharArray() };
                    //var colorBrackets = searcher.FindBracketsByName("color", "rgb");

                    //if (colorBrackets.Count > 0)
                    //{
                    //    return VarSearcher.ParseColor(colorBrackets[0].Content.FirstOrDefault()).ToMediaColor();
                    //}
                }
            }

            return Colors.Gray;
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
                        //var fileContent = File.ReadAllText(file);
                        //var fileSearcher = new BracketSearcher
                        //{
                        //    CurrentString = fileContent.ToCharArray()
                        //};

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
