using ModdingManager.classes.cache;
using ModdingManager.classes.configs;
using ModdingManager.classes.gfx;
using ModdingManager.classes.utils.search;
using ModdingManager.classes.utils.types;
using ModdingManager.configs;
using ModdingManager.managers.utils;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ModdingManager.classes.utils
{
    public class Registry
    {
        private Registry() { }
        private static Registry _instance = new();
        public static Registry Instance => _instance ??= new Registry();
        public LocalisationCache Cache { get; set; } = new LocalisationCache();
        public  List<RegimentConfig> Regiments { get; set; }
        public  List<CountryConfig> Countries { get; set; }
        public  List<IdeaConfig> Ideas { get; set; }
        public  List<string> Modifiers { get; set; }
        public  MapConfig Map { get; set; }
        public  List<CountryCharacterConfig> Characters { get; set; }
        public  List<IdeologyConfig> Ideologies { get; set; }
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
            LoadCache();
            LoadRegiemts();
            LoadCountries();
            LoadIdeas();
            LoadModifiers();
            LoadCharacters();
            LoadIdeologies();
            LoadBaseProvinceMap();
        }
        private static void LoadCache()
        {
            _instance.Cache = new LocalisationCache();
            _instance.Cache.LoadLocalisation();
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
        public static void LoadBaseProvinceMap()
        {
            string definitionPath = System.IO.Path.Combine(ModManager.Directory, "map", "definition.csv");
            string provinceImagePath = System.IO.Path.Combine(ModManager.Directory, "map", "provinces.bmp");

            if (!File.Exists(definitionPath))
                throw new FileNotFoundException("Не найден файл definition.csv", definitionPath);
            if (!File.Exists(provinceImagePath))
                throw new FileNotFoundException("Не найден файл provinces.bmp", provinceImagePath);

            string[] lines = File.ReadAllLines(definitionPath);
            Instance.Map = ParseProvinceMap(lines);
            Instance.Map.States = ParseStateMap(Instance.Map);
            Instance.Map.StrategicRegions = ParseStrategicMap(Instance.Map);
            Instance.Map.Countries = ParseCountryMap(Instance.Map);
            Instance.Map.Bitmap = new Bitmap(provinceImagePath);
        }

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
            res.Provinces = seaProvinces.Concat(otherProvinces).ToList();
            return res;
        }

        private static void GetProvinceNames(List<ProvinceConfig> list)
        {
            var victoryPoints = _instance.Cache.VictoryPointsLocalisation;
            var allCache = _instance.Cache.AllCache;

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

        public static List<StateConfig> ParseStateMap(MapConfig map)
        {
            var stateMap = new Dictionary<int, StateConfig>();

            string[] priorityFolders = {
                Path.Combine(ModManager.Directory, "history", "states"),
                Path.Combine(ModManager.GameDirectory, "history", "states")
            };

            foreach (string folder in priorityFolders)
            {
                if (!System.IO.Directory.Exists(folder)) continue;

                var files = System.IO.Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    int? id = VarSearcher.SearchInt(lines, "id");
                    if (id == null || stateMap.ContainsKey(id.Value)) continue;

                    string internalName = VarSearcher.SearchString(lines, "name")?.Trim('"');
                    if (string.IsNullOrEmpty(internalName)) continue;

                    var bracketSearcher = new BracketSearcher
                    {
                        CurrentString = content.ToCharArray()
                    };
                    string key = VarSearcher.SearchString(lines, "name");
                    var provinceBlock = bracketSearcher.GetBracketContentByHeaderName("provinces".ToCharArray())
                        .FirstOrDefault();

                    if (provinceBlock == null) continue;

                    var provinceIds = provinceBlock
                        .Split(new[] { '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                        .ToHashSet();

                    var matchedProvinces = map.Provinces
                        .Where(p => provinceIds.Contains(p.Id))
                        .ToList();

                    var color = ModManager.GenerateColorFromId(id.Value);

                    stateMap[id.Value] = new StateConfig
                    {
                        Id = id.Value,
                        Color = color,
                        FilePath = file,
                        Provinces = matchedProvinces,
                        LocalizationKey = key,
                    };
                }
            }
            var result = stateMap.Values.ToList();
            GetStateNames(result);
            return result;
        }

        private static void GetStateNames(List<StateConfig> states)
        {
            var stateLocals = _instance.Cache.StateLocalisation;
            var allCache = _instance.Cache.AllCache;

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


        public static List<StrategicRegionConfig> ParseStrategicMap(MapConfig map)
        {
            var strategicMap = new Dictionary<int, StrategicRegionConfig>();

            string[] priorityFolders = {
        Path.Combine(ModManager.Directory, "map", "strategicregions"),
        Path.Combine(ModManager.GameDirectory, "map", "strategicregions")
    };

            foreach (string folder in priorityFolders)
            {
                if (!System.IO.Directory.Exists(folder)) continue;

                var files = System.IO.Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    int? id = VarSearcher.SearchInt(lines, "id");
                    if (id == null || strategicMap.ContainsKey(id.Value)) continue;

                    var bracketSearcher = new BracketSearcher
                    {
                        CurrentString = content.ToCharArray()
                    };

                    var provinceBlock = bracketSearcher.GetBracketContentByHeaderName("provinces".ToCharArray())
                        .FirstOrDefault();

                    if (provinceBlock == null) continue;

                    var provinceIds = provinceBlock
                        .Split(new[] { '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s, out int value) ? value : (int?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                        .ToHashSet();

                    var matchedProvinces = map.Provinces
                        .Where(p => provinceIds.Contains(p.Id))
                        .ToList();

                    var region = new StrategicRegionConfig
                    {
                        Id = id.Value,
                        Provinces = matchedProvinces,
                        FilePath = file,
                        Color = ModManager.GenerateColorFromId(id.Value)
                    };

                    strategicMap[id.Value] = region;
                }
            }

            return strategicMap.Values.ToList();
        }
        public static List<CountryOnMapConfig> ParseCountryMap(MapConfig map)
        {
            List<Var> tagVars = VarSearcher.TryGetCountryTags(Path.Combine(ModManager.Directory, "common", "country_tags"));
            if (tagVars.Count == 0)
                tagVars = VarSearcher.TryGetCountryTags(Path.Combine(ModManager.GameDirectory, "common", "country_tags"));

            var tagLookup = tagVars.ToDictionary(v => v.Name, v => v.Value);

            var countries = new List<CountryOnMapConfig>();
            var visitedIds = new HashSet<int>();

            string[] stateFolders = {
        Path.Combine(ModManager.Directory, "history", "states"),
        Path.Combine(ModManager.GameDirectory, "history", "states")
    };

            foreach (var folder in stateFolders)
            {
                if (!Directory.Exists(folder)) continue;

                foreach (var file in Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories))
                {
                    string content = File.ReadAllText(file);
                    var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    int? id = VarSearcher.SearchInt(lines, "id");
                    string ownerTag = VarSearcher.SearchString(lines, "owner");

                    if (id == null || string.IsNullOrWhiteSpace(ownerTag)) continue;
                    if (visitedIds.Contains(id.Value)) continue;

                    var state = map.States?.FirstOrDefault(s => s.Id == id.Value);
                    if (state == null) continue;

                    string countryKey = tagLookup.ContainsKey(ownerTag) ? ownerTag : null;
                    string countryPath = tagLookup.TryGetValue(ownerTag, out var pathVal) ? pathVal as string : null;

                    var country = countries.FirstOrDefault(c => c.Tag == countryKey);
                    if (country == null)
                    {
                        System.Windows.Media.Color color = Colors.Gray;

                        if (countryPath != null)
                        {
                            string pathClean = countryPath.Trim('"').Replace('/', Path.DirectorySeparatorChar);
                            string fullPathDir = Path.Combine(ModManager.Directory, "common", pathClean);
                            string fullPathGame = Path.Combine(ModManager.GameDirectory, "common", pathClean);

                            string pathToUse = File.Exists(fullPathDir) ? fullPathDir :
                                               File.Exists(fullPathGame) ? fullPathGame : null;

                            if (pathToUse != null)
                            {
                                var cContent = File.ReadAllText(pathToUse);
                                var bSearcher = new BracketSearcher { CurrentString = cContent.ToCharArray() };
                                string colorBlock = bSearcher.GetBracketContentByHeaderName("color".ToCharArray()).FirstOrDefault();
                                if (!string.IsNullOrWhiteSpace(colorBlock))
                                    color = VarSearcher.ParseColor(colorBlock).ToMediaColor();
                            }
                        }

                        country = new CountryOnMapConfig
                        {
                            Tag = countryKey,
                            Color = color,
                            States = new List<StateConfig>()
                        };
                        countries.Add(country);
                    }

                    country.States.Add(state);
                    visitedIds.Add(id.Value);
                }
            }

            map.Countries = countries;
            return countries;
        }
        private static void LoadCharacters()
        {

        }
        private static void LoadIdeologies()
        {
            _instance.Ideologies = new List<IdeologyConfig>();
            var ideologyFiles = new List<string>();

            string modDir = Path.Combine(ModManager.Directory, "common", "ideologies");
            if (Directory.Exists(modDir))
                ideologyFiles.AddRange(Directory.GetFiles(modDir, "*.*", SearchOption.AllDirectories));
            string gameDir = Path.Combine(ModManager.GameDirectory, "common", "ideologies");
            if (Directory.Exists(gameDir) && ideologyFiles.Count < 1)
                ideologyFiles.AddRange(Directory.GetFiles(gameDir, "*.*", SearchOption.AllDirectories));



            foreach (var file in ideologyFiles)
            {
                string fileContent = File.ReadAllText(file);
                BracketSearcher searcher = new BracketSearcher
                {
                    CurrentString = fileContent.ToCharArray(),
                    OpenBracketChar = '{',
                    CloseBracketChar = '}'
                };

                // Ищем основной блок ideologies
                var ideologiesContent = searcher.GetBracketContentByHeaderName("ideologies".ToCharArray());
                if (ideologiesContent.Count == 0) continue;

                // Обрабатываем каждую найденную идеологию
                var ideologySearcher = new BracketSearcher
                {
                    CurrentString = ideologiesContent[0].ToCharArray(),
                    OpenBracketChar = '{',
                    CloseBracketChar = '}'
                };

                var ideologyNames = ideologySearcher.GetAllBracketSubbracketsNames(1);
                foreach (var name in ideologyNames)
                {
                    var ideologyBlocks = ideologySearcher.GetBracketContentByHeaderName(name.ToCharArray());
                    if (ideologyBlocks.Count == 0) continue;

                    IdeologyConfig config = IdeologyConfig.ParseIdeologyConfig(name, ideologyBlocks[0]);
                    if (config != null)
                        _instance.Ideologies.Add(config);
                }
                _instance.Ideologies.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

               
            }
        }
        
        public IdeologyConfig? GetIdeology(string id)
        {
            return this.Ideologies.FirstOrDefault(i => i.Id == id);
        }

        #region Helper Methods
        public static List<(string Name, List<string> Categories)> CollectUnitDefinitions()
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var rootPath in new[] { ModManager.GameDirectory, ModManager.Directory })
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
                            OpenBracketChar = '{',
                            CloseBracketChar = '}',
                            CurrentString = fileContent.ToCharArray()
                        };

                        var subUnitsContents = fileSearcher.GetBracketContentByHeaderName("sub_units".ToCharArray());

                        foreach (var subUnitsContent in subUnitsContents)
                        {
                            var subUnitsSearcher = new BracketSearcher
                            {
                                OpenBracketChar = '{',
                                CloseBracketChar = '}',
                                CurrentString = subUnitsContent.ToCharArray()
                            };

                            var unitDefinitions = subUnitsSearcher.GetAllBracketSubbracketsNames(1);

                            foreach (var unitDef in unitDefinitions)
                            {
                                var unitContents = subUnitsSearcher.GetBracketContentByHeaderName(unitDef.ToCharArray());

                                foreach (var unitContent in unitContents)
                                {
                                    var unitSearcher = new BracketSearcher
                                    {
                                        OpenBracketChar = '{',
                                        CloseBracketChar = '}',
                                        CurrentString = unitContent.ToCharArray()
                                    };
                                    var categories = unitSearcher.GetBracketContentByHeaderName("categories".ToCharArray());

                                    if (categories.Count > 0)
                                    {
                                        var cleanCategories = categories.SelectMany(c => c.Split('\n'))
                                            .Select(line => line.Trim())
                                            .Where(line => !string.IsNullOrEmpty(line) &&
                                                          !line.Contains("{") &&
                                                          !line.Contains("}"))
                                            .ToList();

                                        if (cleanCategories.Count > 0)
                                        {
                                            var unitName = unitDef.Trim();
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
            }

            return result.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        #endregion
    }
}
