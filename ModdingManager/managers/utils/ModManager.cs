using ModdingManager.classes.extentions;
using ModdingManager.classes.gfx;
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
using TeximpNet;
using TeximpNet.Compression;
using TeximpNet.DDS;
namespace ModdingManager.managers.utils
{
    public class ModManager
    {
        public static string Directory;
        public static bool IsDebugRuning;
        public static string GameDirectory;
        public static void SaveCountryFlag(System.Drawing.Image fascismImage,
                                   System.Drawing.Image neutralityImage,
                                   System.Drawing.Image communismImage,
                                   System.Drawing.Image democraticImage,
                                   string modPath,
                                   string countryTag)
        {
            try
            {
                string flagsDir = modPath;

                System.IO.Directory.CreateDirectory(flagsDir);
                System.IO.Directory.CreateDirectory(Path.Combine(flagsDir, "small"));
                System.IO.Directory.CreateDirectory(Path.Combine(flagsDir, "medium"));

                SaveFlagSet(neutralityImage, flagsDir, countryTag, "neutrality");
                SaveFlagSet(fascismImage, flagsDir, countryTag, "fascism");
                SaveFlagSet(communismImage, flagsDir, countryTag, "communism");
                SaveFlagSet(democraticImage, flagsDir, countryTag, "democratic");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка создания флагов: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void SaveFlagSet(System.Drawing.Image image, string flagsDir,
                                      string countryTag, string ideology)
        {
            using (var imageSharp = ImageManager.ConvertToImageSharp(image))
            {
                using (var resized = ImageManager.ResizeStretch(imageSharp,82, 52))
                {
                    resized.SaveAsTGA(Path.Combine(flagsDir, $"{countryTag}_{ideology}.tga"));
                }

                using (var resized = ImageManager.ResizeStretch(imageSharp, 41, 26))
                {
                    resized.SaveAsTGA(Path.Combine(flagsDir, "medium", $"{countryTag}_{ideology}.tga"));
                }

                using (var resized = ImageManager.ResizeStretch(imageSharp, 10, 7))
                {
                    resized.SaveAsTGA(Path.Combine(flagsDir, "small", $"{countryTag}_{ideology}.tga"));
                }
            }
        }
    }
}
