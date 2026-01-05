using Application.Settings;

namespace Application.utils.Pathes
{
    public static class GamePathes
    {
        private static string BaseDirectory => ModManagerSettings.GameDirectory;
        private static string CurrentLanguage => ModManagerSettings.CurrentLanguage.ToString();

        private static string Combine(params string[] paths)
        {
            var relativePath = Path.Combine(paths);
            return Path.Combine(BaseDirectory, relativePath);
        }

        public static readonly string IdeologyPath = Combine("common", "ideologies");
        public static readonly string IdeasPath = Combine("common", "ideas");
        public static readonly string TexturesPath = Combine("gfx");
        public static readonly string RootPath = BaseDirectory;
        public static readonly string InterfacePath = Combine("interface");
        public static readonly string StrategicRegionPath = Combine("map", "strategicregions");
        public static readonly string ModifierDefFirstPath = Combine("common", "modifier_definitions");
        public static readonly string ModifierDefSecondPath = Combine("documentation", "modifiers_documentation.html");
        public static readonly string StateCathegoryPath = Combine("common", "state_category");
        public static readonly string RegimentsPath = Combine("common", "units");
        public static readonly string RulesPath = Combine("common", "game_rules");
        public static readonly string StaticModifiersPath = Combine("common", "modifiers");
        public static readonly string OpinionModifiersPath = Combine("common", "opinion_modifiers");
        public static readonly string DynamicModifiersPath = Combine("common", "dynamic_modifiers");
        public static readonly string CountryTagsPath = Combine("common", "country_tags");
        public static readonly string CommonCountriesPath = Combine("common", "countries");
        public static readonly string HistoryCountriesPath = Combine("history", "countries");
        public static readonly string HistoryPath = Combine("history");
        public static readonly string IdeaTagsPath = Combine("common", "idea_tags");
        public static readonly string StatesPath = Combine("history", "states");
        public static readonly string LocalisationPath = Combine("localisation", CurrentLanguage);
        public static readonly string LocalisationReplacePath = Combine("localisation", CurrentLanguage, "replace");
        public static readonly string DefinitionPath = Combine("map", "definition.csv");
        public static readonly string ProvinceImagePath = Combine("map", "provinces.bmp");
        public static readonly string CommonCharacterPath = Combine("common", "characters");
        public static readonly string TechTreePath = Combine("common", "technologies");
        public static readonly string TechDefPath = Combine("common", "technology_tags");
        public static readonly string TraitsPath = Combine("common", "country_leader");
        public static readonly string BuildingsPath = Combine("common", "buildings");
    }
}
