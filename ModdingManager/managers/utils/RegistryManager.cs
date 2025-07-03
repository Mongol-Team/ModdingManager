using ModdingManager.configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.managers.utils
{
    public class RegistryManager
    {
        private RegistryManager() { }
        private static RegistryManager _instance;
        public static RegistryManager Instance => _instance ??= new RegistryManager();
        public static List<RegimentConfig> Regiments { get; set; }
        public static List<CountryConfig> Countries { get; set; }
        public static List<IdeaConfig> Ideas { get; set; }
        public static List<string> Modifiers { get; set; }
        public static List<CountryCharacterConfig> Characters { get; set; }
        public static List<IdeologyConfig> Ideologies { get; set; }
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

        }
    }
}
