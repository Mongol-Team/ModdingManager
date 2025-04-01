using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
namespace ModdingManager
{
    public class ModManager
    {
        public static string directory;
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

                Directory.CreateDirectory(flagsDir);
                Directory.CreateDirectory(Path.Combine(flagsDir, "small"));
                Directory.CreateDirectory(Path.Combine(flagsDir, "medium"));

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
            using (var imageSharp = ConvertToImageSharp(image))
            {
                // Основной флаг (82x52)
                using (var resized = ResizeStretch(imageSharp, 82, 52))
                {
                    SaveTga(resized, Path.Combine(flagsDir, $"{countryTag}_{ideology}.tga"));
                }

                // Средний флаг (41x26)
                using (var resized = ResizeStretch(imageSharp, 41, 26))
                {
                    SaveTga(resized, Path.Combine(flagsDir, "medium", $"{countryTag}_{ideology}.tga"));
                }

                // Малый флаг (10x7)
                using (var resized = ResizeStretch(imageSharp, 10, 7))
                {
                    SaveTga(resized, Path.Combine(flagsDir, "small", $"{countryTag}_{ideology}.tga"));
                }
            }
        }

        private static SixLabors.ImageSharp.Image<Rgba32> ConvertToImageSharp(System.Drawing.Image systemDrawingImage)
        {
            if (systemDrawingImage == null)
            {
                var emptyFlag = new SixLabors.ImageSharp.Image<Rgba32>(82, 52);
                emptyFlag.Mutate(x => x.BackgroundColor(new Rgba32(255, 0, 255, 255)));
                return emptyFlag;
            }

            using (var ms = new MemoryStream())
            {
                systemDrawingImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                return SixLabors.ImageSharp.Image.Load<Rgba32>(ms);
            }
        }

        private static SixLabors.ImageSharp.Image<Rgba32> ResizeStretch(SixLabors.ImageSharp.Image<Rgba32> image, int width, int height)
        {
            // Просто растягиваем изображение на весь размер без сохранения пропорций
            return image.Clone(x => x.Resize(width, height));
        }

        private static void SaveTga(SixLabors.ImageSharp.Image<Rgba32> image, string path)
        {
            var encoder = new TgaEncoder
            {
                BitsPerPixel = TgaBitsPerPixel.Pixel32,
                Compression = TgaCompression.None
            };

            image.Save(path, encoder);
        }
    }
}
