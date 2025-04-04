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
namespace ModdingManager
{
    public class ModManager
    {
        public static string Directory;
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
     


        public static void SaveIdeaGFXAsDDS(System.Drawing.Image image, string dir, string id, string tag)
        {
            var path = Path.Combine(dir, "gfx", "interface", "ideas", tag);

            // Создаем директорию, если её нет
            System.IO.Directory.CreateDirectory(path);

            using (var imageSharp = ConvertToImageSharp(image))
            using (var resized = ResizeStretch(imageSharp, 64, 64))
            {
                string outputPath = Path.Combine(path, $"{id}.dds");

                // Получаем raw-данные изображения в формате R8G8B8A8
                byte[] pixelData = new byte[resized.Width * resized.Height * 4];
                resized.CopyPixelDataTo(pixelData);

                // Конвертируем R8G8B8A8 → B8G8R8A8 (меняем порядок каналов)
                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    byte r = pixelData[i];
                    byte b = pixelData[i + 2];
                    pixelData[i] = b;     // B
                    pixelData[i + 2] = r; // R
                }

                // Создаем Surface из данных
                using (var surface = new Surface(resized.Width, resized.Height))
                {
                    // Копируем данные в Surface
                    var surfacePtr = surface.DataPtr;
                    Marshal.Copy(pixelData, 0, surfacePtr, pixelData.Length);

                    // Сохраняем в DDS (формат B8G8R8A8)
                    DDSFile.Write(outputPath, surface, TextureDimension.Two, DDSFlags.None);
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
