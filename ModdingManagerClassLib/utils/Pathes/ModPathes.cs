using ModdingManager.managers.@base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.utils.Pathes
{
    public static class ModPathes
    {
        public readonly static string IdeologyPath = Path.Combine(ModManager.ModDirectory, "common", "ideologies");
        public readonly static string CommonCountriesPath = Path.Combine(ModManager.ModDirectory, "common", "countries");
        public readonly static string HistoryCountriesPath = Path.Combine(ModManager.ModDirectory, "history", "countries");
        public readonly static string StatesPath = Path.Combine(ModManager.ModDirectory, "history", "states");
        public readonly static string LocalisationReplacePath = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace");
        public readonly static string LocalisationPath = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage);
        public readonly static string CountryTagsPath = Path.Combine(ModManager.ModDirectory, "common", "country_tags");
        public readonly static string DefinitionPath = Path.Combine(ModManager.ModDirectory, "map", "definition.csv");
        public readonly static string ProvinceImagePath = Path.Combine(ModManager.ModDirectory, "map", "provinces.bmp");
        public readonly static string StrategicRegionPath = Path.Combine(ModManager.ModDirectory, "map", "strategicregions");
    }
}
