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
        public static readonly string LocalisationReplacePath = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace");
        public static readonly string LocalisationPath = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage);
        public static readonly string CountryTagsPath = Path.Combine(ModManager.ModDirectory, "common", "country_tags");
        public static readonly string DefinitionPath = Path.Combine(ModManager.ModDirectory, "map", "definition.csv");
        public static readonly string ProvinceImagePath = Path.Combine(ModManager.ModDirectory, "map", "provinces.bmp");
        public static readonly string StrategicRegionPath = Path.Combine(ModManager.ModDirectory, "map", "strategicregions");
    }
}
