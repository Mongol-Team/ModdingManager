using ModdingManager.managers.@base;
using Application.Settings;

namespace Application.utils.Pathes
{
    public static class ModPathes
    {
        public static readonly string IdeologyPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "ideologies");
        public static readonly string CommonCountriesPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "countries");
        public static readonly string HistoryCountriesPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "history", "countries");
        public static readonly string StatesPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "history", "states");
        public static readonly string ModifierDefFirstPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "modifier_definitions");
        public static readonly string ModifierDefSecondPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "documentation", "modifiers_documentation.html");
        public static readonly string IdeasPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "ideas");
        public static readonly string InterfacePath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "interface");
        public static readonly string StateCathegoryPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "state_category");
        public static readonly string LocalisationReplacePath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "localisation", ModdingManagerSettings.Instance.CurrentLanguage.ToString(), "replace");
        public static readonly string LocalisationPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "localisation", ModdingManagerSettings.Instance.CurrentLanguage.ToString());
        public static readonly string CountryTagsPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "country_tags");
        public static readonly string DefinitionPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "map", "definition.csv");
        public static readonly string ProvinceImagePath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "map", "provinces.bmp");
        public static readonly string StrategicRegionPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "map", "strategicregions");
        public static readonly string IdeaTagsPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "idea_tags");
        public static readonly string TexturesPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "gfx");
        public static readonly string RootPath = ModdingManagerSettings.Instance.ModDirectory;
        public static readonly string HistoryPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "history");
        public static readonly string RegimentsPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "units");
        public static readonly string RulesPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "game_rules");
        public static readonly string StaticModifiersPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "modifiers");
        public static readonly string OpinionModifiersPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "opinion_modifiers");
        public static readonly string DynamicModifiersPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "dynamic_modifiers");
        public static readonly string CommonCharacterPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "characters");
        public static readonly string TechTreePath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "technologies");
        public static readonly string TechDefPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "technology_tags");

        public static readonly string TraitsPath = Path.Combine(ModdingManagerSettings.Instance.ModDirectory, "common", "country_leader");
    }
}

