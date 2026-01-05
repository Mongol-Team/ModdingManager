using Application.Settings;
using System.IO;

namespace Application.utils.Pathes
{
    public static class ModPathes
    {
        private static string BaseDirectory => ModManagerSettings.ModDirectory;
        private static string CurrentLanguage => ModManagerSettings.CurrentLanguage.ToString();

        private static string Combine(params string[] paths)
        {
            var relativePath = Path.Combine(paths);
            return Path.Combine(BaseDirectory, relativePath);
        }

        public static string IdeologyPath => Combine("common", "ideologies");
        public static string CommonCountriesPath => Combine("common", "countries");
        public static string HistoryCountriesPath => Combine("history", "countries");
        public static string HistoryPath => Combine("history");
        public static string StatesPath => Combine("history", "states");
        public static string ModifierDefFirstPath => Combine("common", "modifier_definitions");
        public static string ModifierDefSecondPath => Combine("documentation", "modifiers_documentation.html");
        public static string IdeasPath => Combine("common", "ideas");
        public static string InterfacePath => Combine("interface");
        public static string StateCathegoryPath => Combine("common", "state_category");
        public static string LocalisationReplacePath => Combine("localisation", CurrentLanguage, "replace");
        public static string LocalisationPath => Combine("localisation", CurrentLanguage);
        public static string CountryTagsPath => Combine("common", "country_tags");
        public static string DefinitionPath => Combine("map", "definition.csv");
        public static string ProvinceImagePath => Combine("map", "provinces.bmp");
        public static string StrategicRegionPath => Combine("map", "strategicregions");
        public static string IdeaTagsPath => Combine("common", "idea_tags");
        public static string TexturesPath => Combine("gfx");
        public static string RootPath => BaseDirectory;
        public static string RegimentsPath => Combine("common", "units");
        public static string RulesPath => Combine("common", "game_rules");
        public static string StaticModifiersPath => Combine("common", "modifiers");
        public static string OpinionModifiersPath => Combine("common", "opinion_modifiers");
        public static string DynamicModifiersPath => Combine("common", "dynamic_modifiers");
        public static string CommonCharacterPath => Combine("common", "characters");
        public static string TechTreePath => Combine("common", "technologies");
        public static string TechDefPath => Combine("common", "technology_tags");
        public static string TraitsPath => Combine("common", "country_leader");
        public static string BuildingsPath => Combine("common", "buildings");
    }
}

