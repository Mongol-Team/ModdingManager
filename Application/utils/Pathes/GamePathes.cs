using ModdingManager.managers.@base;
using Application.Settings;

namespace Application.utils.Pathes
{
    public static class GamePathes
    {
        public static readonly string IdeologyPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "ideologies");
        public static readonly string IdeasPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "ideas");
        public static readonly string TexturesPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "gfx");
        public static readonly string RootPath = ModdingManagerSettings.Instance.GameDirectory;
        public static readonly string InterfacePath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "interface");
        public static readonly string StrategicRegionPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "map", "strategicregions");
        public static readonly string ModifierDefFirstPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "modifier_definitions");
        public static readonly string ModifierDefSecondPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "documentation", "modifiers_documentation.html");
        public static readonly string StateCathegoryPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "state_category");
        public static readonly string RegimentsPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "units");
        public static readonly string RulesPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "game_rules");
        public static readonly string StaticModifiersPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "modifiers");
        public static readonly string OpinionModifiersPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "opinion_modifiers");
        public static readonly string DynamicModifiersPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "dynamic_modifiers");
        public static readonly string CountryTagsPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "country_tags");
        public static readonly string CommonCountriesPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "countries");
        public static readonly string HistoryCountriesPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "history", "countries");
        public static readonly string IdeaTagsPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "idea_tags");
        public static readonly string StatesPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "history", "states");
        public static readonly string LocalisationPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "localisation", ModdingManagerSettings.Instance.CurrentLanguage.ToString());
        public static readonly string LocalisationReplacePath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "localisation", ModdingManagerSettings.Instance.CurrentLanguage.ToString(), "replace");
        public static readonly string DefinitionPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "map", "definition.csv");
        public static readonly string ProvinceImagePath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "map", "provinces.bmp");
        public static readonly string CommonCharacterPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "characters");
        public static readonly string TechTreePath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "technologies");
        public static readonly string TechDefPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "technology_tags");
        public static readonly string HistoryPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "history");

        public static readonly string TraitsPath = Path.Combine(ModdingManagerSettings.Instance.GameDirectory, "common", "country_leader");
    }
}
