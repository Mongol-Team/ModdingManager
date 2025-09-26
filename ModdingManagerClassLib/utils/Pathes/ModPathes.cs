using ModdingManager.managers.@base;

namespace ModdingManagerClassLib.utils.Pathes
{
    public static class ModPathes
    {
        public static readonly string IdeologyPath = Path.Combine(ModManager.ModDirectory, "common", "ideologies");
        public static readonly string CommonCountriesPath = Path.Combine(ModManager.ModDirectory, "common", "countries");
        public static readonly string HistoryCountriesPath = Path.Combine(ModManager.ModDirectory, "history", "countries");
        public static readonly string StatesPath = Path.Combine(ModManager.ModDirectory, "history", "states");
        public static readonly string ModifierDefFirstPath = Path.Combine(ModManager.ModDirectory, "common", "modifier_definitions");
        public static readonly string ModifierDefSecondPath = Path.Combine(ModManager.ModDirectory, "documentation", "modifiers_documentation.html");
        public static readonly string IdeasPath = Path.Combine(ModManager.ModDirectory, "common", "ideas");
        public static readonly string InterfacePath = Path.Combine(ModManager.ModDirectory, "interface");
        public static readonly string StateCathegoryPath = Path.Combine(ModManager.ModDirectory, "common", "state_categories");
        public static readonly string LocalisationReplacePath = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage.ToString(), "replace");
        public static readonly string LocalisationPath = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage.ToString());
        public static readonly string CountryTagsPath = Path.Combine(ModManager.ModDirectory, "common", "country_tags");
        public static readonly string DefinitionPath = Path.Combine(ModManager.ModDirectory, "map", "definition.csv");
        public static readonly string ProvinceImagePath = Path.Combine(ModManager.ModDirectory, "map", "provinces.bmp");
        public static readonly string StrategicRegionPath = Path.Combine(ModManager.ModDirectory, "map", "strategicregions");


        public static readonly string RegimentsPath = Path.Combine(ModManager.ModDirectory, "common", "units");
        public static readonly string RulesPath = Path.Combine(ModManager.ModDirectory, "common", "game_rules");
        public static readonly string StaticModifiersPath = Path.Combine(ModManager.ModDirectory, "common", "modifiers");
        public static readonly string OpinionModifiersPath = Path.Combine(ModManager.ModDirectory, "common", "opinion_modifiers");
        public static readonly string DynamicModifiersPath = Path.Combine(ModManager.ModDirectory, "common", "dynamic_modifiers");
    }
}
