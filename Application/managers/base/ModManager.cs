using ModdingManager.classes.utils;
using ModdingManagerClassLib;
using ModdingManagerClassLib.Composers;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.Loaders;
using ModdingManagerClassLib.utils;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using SixLabors.ImageSharp;
using System.Text.Json;
using System.Text.RegularExpressions;

using System.Windows;

namespace ModdingManager.managers.@base
{
    public class ModManager
    {
        public static bool IsDebugRuning { get; internal set; }

        //public ModSettings ModSettings { get; } = settings;
        //public  ModDataStorage ModDataStorage { get;  } = storage;

        public static System.Drawing.Color GenerateColorFromId(int id)
        {
            byte r = (byte)((id * 53) % 255);
            byte g = (byte)((id * 97) % 255);
            byte b = (byte)((id * 151) % 255);
            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }
}