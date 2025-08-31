using ModdingManager.managers.@base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.utils.Pathes
{
    public static class GamePathes
    {
        public readonly static string IdeologyPath = Path.Combine(ModManager.GameDirectory, "common", "ideologies");
        public readonly static string IdeasPath = Path.Combine(ModManager.GameDirectory, "common", "ideas");
        public readonly static string StrategicRegionPath = Path.Combine(ModManager.GameDirectory, "map", "strategicregions");
        public readonly static string CountryTagsPath = Path.Combine(ModManager.GameDirectory, "common", "country_tags");
        public readonly static string CommonCountriesPath = Path.Combine(ModManager.GameDirectory, "common", "countries");
        public readonly static string HistoryCountriesPath = Path.Combine(ModManager.GameDirectory, "history", "countries");
        public readonly static string StatesPath = Path.Combine(ModManager.GameDirectory, "history", "states");
        public readonly static string LocalisationPath = Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage);
        public readonly static string LocalisationReplacePath = Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace");
        public readonly static string DefinitionPath = Path.Combine(ModManager.GameDirectory, "map", "definition.csv");
        public readonly static string ProvinceImagePath = Path.Combine(ModManager.GameDirectory, "map", "provinces.bmp");

        public static string StrategicRegionsPath { get; internal set; }
    }
}
