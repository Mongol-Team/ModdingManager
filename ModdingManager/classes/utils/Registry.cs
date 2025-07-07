using ModdingManager.classes.gfx;
using ModdingManager.classes.utils.search;
using ModdingManager.configs;
using ModdingManager.managers.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.utils
{
    public class Registry
    {
        private Registry() { }
        private static Registry _instance = new();
        public static Registry Instance => _instance ??= new Registry();
        public  List<RegimentConfig> Regiments { get; set; }
        public  List<CountryConfig> Countries { get; set; }
        public  List<IdeaConfig> Ideas { get; set; }
        public  List<string> Modifiers { get; set; }
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
            LoadRegiemts();
            LoadCountries();
            LoadIdeas();
            LoadModifiers();
            LoadCharacters();
            LoadIdeologies();
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
