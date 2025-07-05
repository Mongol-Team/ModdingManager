using ModdingManager.classes.utils.search;
using ModdingManager.configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.managers.utils
{
    public class RegistryManager
    {
        private RegistryManager() { }
        private static RegistryManager _instance = new();
        public static RegistryManager Instance => _instance ??= new RegistryManager();
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
            RegistryManager._instance.Ideologies = new List<IdeologyConfig>();
            var ideologyFiles = new List<string>();

            string modDir = Path.Combine(ModManager.Directory, "common", "ideologies");
            if (Directory.Exists(modDir))
                ideologyFiles.AddRange(Directory.GetFiles(modDir, "*.*", SearchOption.AllDirectories));
            string gameDir = Path.Combine(ModManager.GameDirectory, "common", "ideologies");
            if (Directory.Exists(gameDir) || ideologyFiles.Count < 1)
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
            }
        }
    }
}
