using ModdingManager.classes.configs;
using ModdingManager.classes.extentions;
using ModdingManager.classes.utils.search;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TeximpNet;
using TeximpNet.Compression;
using TeximpNet.DDS;
namespace ModdingManager.managers.@base
{
    public class ModManager
    {
        public static string Directory;
        public static bool IsDebugRuning;
        public static string GameDirectory;
        public static string CurrentLanguage = "russian";
        public static List<string> LoadCountryFileNames()
        {
            string countriesDir = Path.Combine(ModManager.Directory, "common", "country_tags");

            if (!System.IO.Directory.Exists(countriesDir))
            {
                System.Windows.MessageBox.Show("Папка 'countries' не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var filePaths = System.IO.Directory.GetFiles(countriesDir);
            var fileNamesLines = filePaths.Select(path => Path.GetFileName(path)).ToList();
            return fileNamesLines;
        }
        public static System.Windows.Media.Color GenerateColorFromId(int id)
        {
            byte r = (byte)((id * 53) % 255);
            byte g = (byte)((id * 97) % 255);
            byte b = (byte)((id * 151) % 255);
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }
        public static System.Windows.Media.Color ParseColor(string content)
        {
            string[] parts = content.Trim('{', '}').Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return Colors.Black;

            return System.Windows.Media.Color.FromRgb(
                byte.Parse(parts[0]),
                byte.Parse(parts[1]),
                byte.Parse(parts[2])
            );
        }


    }

}