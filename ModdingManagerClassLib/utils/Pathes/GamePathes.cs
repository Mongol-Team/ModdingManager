using ModdingManager.managers.@base;

namespace ModdingManagerClassLib.utils.Pathes
{
    public static class GamePathes
    {
        public static readonly string IdeologyPath = Path.Combine(ModManager.GameDirectory, "common", "ideologies");
        public static readonly string IdeasPath = Path.Combine(ModManager.GameDirectory, "common", "ideas");
        public static readonly string InterfacePath = Path.Combine(ModManager.GameDirectory, "interface");
        public static readonly string StrategicRegionPath = Path.Combine(ModManager.GameDirectory, "map", "strategicregions");
        public static readonly string ModifierDefFirstPath = Path.Combine(ModManager.GameDirectory, "common", "modifier_definitions");
        public static readonly string ModifierDefSecondPath = Path.Combine(ModManager.GameDirectory, "documentation", "modifiers_documentation.html");
        public static readonly string StateCathegoryPath = Path.Combine(ModManager.GameDirectory, "common", "state_category");
        public static readonly string RegimentsPath = Path.Combine(ModManager.GameDirectory, "common", "units");
        public static readonly string RulesPath = Path.Combine(ModManager.GameDirectory, "common", "game_rules");
        public static readonly string StaticModifiersPath = Path.Combine(ModManager.GameDirectory, "common", "modifiers");
        public static readonly string OpinionModifiersPath = Path.Combine(ModManager.GameDirectory, "common", "opinion_modifiers");
        public static readonly string DynamicModifiersPath = Path.Combine(ModManager.GameDirectory, "common", "dynamic_modifiers");
        public static readonly string CountryTagsPath = Path.Combine(ModManager.GameDirectory, "common", "country_tags");
        public static readonly string CommonCountriesPath = Path.Combine(ModManager.GameDirectory, "common", "countries");
        public static readonly string HistoryCountriesPath = Path.Combine(ModManager.GameDirectory, "history", "countries");
        public static readonly string StatesPath = Path.Combine(ModManager.GameDirectory, "history", "states");
        public static readonly string LocalisationPath = Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage.ToString());
        public static readonly string LocalisationReplacePath = Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage.ToString(), "replace");
        public static readonly string DefinitionPath = Path.Combine(ModManager.GameDirectory, "map", "definition.csv");
        public static readonly string ProvinceImagePath = Path.Combine(ModManager.GameDirectory, "map", "provinces.bmp");
    }
}
