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
        public readonly static string IdeologyModPath = Path.Combine(ModManager.ModDirectory, "common", "ideologies");
        public readonly static string CommonCountriesModPath = Path.Combine(ModManager.ModDirectory, "common", "countries");
        public readonly static string HistoryCountriesModPath = Path.Combine(ModManager.ModDirectory, "history", "countries");
        public readonly static string StatesModPath = Path.Combine(ModManager.ModDirectory, "history", "states");
        public readonly static string LocalisationReplaceModPath = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace");
        public readonly static string LocalisationModPath = Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage);
        public readonly static string CountryTagsModPath = Path.Combine(ModManager.ModDirectory, "common", "country_tags");
        public readonly static string ModDefinitionPath = Path.Combine(ModManager.ModDirectory, "map", "definition.csv");
        public readonly static string ModProvinceImagePath = Path.Combine(ModManager.ModDirectory, "map", "provinces.bmp");
    }
}
