using ModdingManager.classes.utils;
using Application;
using Application.Composers;
using Application.Debugging;
using Application.Extentions;
using Application.Loaders;
using Application.utils;
using Models;
using Models.Enums;
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