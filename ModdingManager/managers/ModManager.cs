using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.IO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Net;
using TeximpNet;
using TeximpNet.Compression;
using TeximpNet.DDS;
using System.Runtime.InteropServices;
using ModdingManager.managers.gfx;
namespace ModdingManager.managers
{
    public class ModManager
    {
        public static string Directory;
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
                MessageBox.Show($"Ошибка создания флагов: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void SaveFlagSet(System.Drawing.Image image, string flagsDir,
                                      string countryTag, string ideology)
        {
            using (var imageSharp = ImageManager.ConvertToImageSharp(image))
            {
                using (var resized = ImageManager.ResizeStretch(imageSharp, 82, 52))
                {
                    TGAManager.SaveTga(resized, Path.Combine(flagsDir, $"{countryTag}_{ideology}.tga"));
                }

                using (var resized = ImageManager.ResizeStretch(imageSharp, 41, 26))
                {
                    TGAManager.SaveTga(resized, Path.Combine(flagsDir, "medium", $"{countryTag}_{ideology}.tga"));
                }

                using (var resized = ImageManager.ResizeStretch(imageSharp, 10, 7))
                {
                    TGAManager.SaveTga(resized, Path.Combine(flagsDir, "small", $"{countryTag}_{ideology}.tga"));
                }
            }
        }

    }
}
