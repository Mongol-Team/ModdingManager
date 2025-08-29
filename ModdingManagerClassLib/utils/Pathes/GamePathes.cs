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
        public readonly static string IdeologyGamePath = Path.Combine(ModManager.GameDirectory, "common", "ideologies");
        public readonly static string CountryTagsGamePath = Path.Combine(ModManager.GameDirectory, "common", "country_tags");
        public readonly static string CommonCountriesGamePath = Path.Combine(ModManager.GameDirectory, "common", "countries");
        public readonly static string HistoryCountriesGamePath = Path.Combine(ModManager.GameDirectory, "history", "countries");
        public readonly static string StatesGamePath = Path.Combine(ModManager.GameDirectory, "history", "states");
        public readonly static string LocalisationGamePath = Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage);
        public readonly static string LocalisationReplaceGamePath = Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace");
        public readonly static string GameDefinitionPath = Path.Combine(ModManager.GameDirectory, "map", "definition.csv");
        public readonly static string GameProvinceImagePath = Path.Combine(ModManager.GameDirectory, "map", "provinces.bmp");
    }
}
