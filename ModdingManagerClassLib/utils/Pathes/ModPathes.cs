using ModdingManager.managers.@base;
using ModdingManagerClassLib.Settings;

namespace ModdingManagerClassLib.utils.Pathes
{
    public static class ModPathes
    {
        public static readonly string IdeologyPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "ideologies");
        public static readonly string CommonCountriesPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "countries");
        public static readonly string HistoryCountriesPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "history", "countries");
        public static readonly string StatesPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "history", "states");
        public static readonly string ModifierDefFirstPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "modifier_definitions");
        public static readonly string ModifierDefSecondPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "documentation", "modifiers_documentation.html");
        public static readonly string IdeasPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "ideas");
        public static readonly string InterfacePath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "interface");
        public static readonly string StateCathegoryPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "state_category");
        public static readonly string LocalisationReplacePath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "localisation", ModManagerSettings.Instance.CurrentLanguage.ToString(), "replace");
        public static readonly string LocalisationPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "localisation", ModManagerSettings.Instance.CurrentLanguage.ToString());
        public static readonly string CountryTagsPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "country_tags");
        public static readonly string DefinitionPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "map", "definition.csv");
        public static readonly string ProvinceImagePath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "map", "provinces.bmp");
        public static readonly string StrategicRegionPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "map", "strategicregions");
        public static readonly string IdeaTagsPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "idea_tags");
        public static readonly string TexturesPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "gfx");
        public static readonly string RootPath = ModManagerSettings.Instance.ModDirectory;
        public static readonly string RegimentsPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "units");
        public static readonly string RulesPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "game_rules");
        public static readonly string StaticModifiersPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "modifiers");
        public static readonly string OpinionModifiersPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "opinion_modifiers");
        public static readonly string DynamicModifiersPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "dynamic_modifiers");

        public static readonly string TechTreePath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "technologies");
        public static readonly string TechDefPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "technology_tags");
    }
}
