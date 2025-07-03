
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using ModdingManager.classes.gfx;
using ModdingManager.classes.utils.fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TeximpNet;
using TeximpNet.DDS;

namespace ModdingManager.classes.extentions
{
    public static class ImageExtensions
    {
        public static void SaveAsDDS(this System.Drawing.Image image, string directory, string filename, int width, int height)
        {
            Directory.CreateDirectory(directory);

            using (var imageSharp = ConvertToImageSharp(image))
            using (var resized = ImageManager.ResizeStretch(imageSharp, width, height))
            {
                string outputPath = Path.Combine(directory, $"{filename}.dds");

                byte[] pixelData = new byte[resized.Width * resized.Height * 4];
                resized.CopyPixelDataTo(pixelData);

                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    byte r = pixelData[i];
                    byte b = pixelData[i + 2];
                    pixelData[i] = b;
                    pixelData[i + 2] = r;
                }

                using (var surface = new Surface(resized.Width, resized.Height))
                {
                    Marshal.Copy(pixelData, 0, surface.DataPtr, pixelData.Length);
                    DDSFile.Write(outputPath, surface, TextureDimension.Two, DDSFlags.None);
                }
            }
        }

        

        public static void SaveAsDDS(this Image<Rgba32> image, string fullPath)
        {
            string directory = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(directory);

            // 1. Подготавливаем данные пикселей
            byte[] pixelData = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelData);

            // 2. Создаем энкодер BC3
            var encoder = new BcEncoder(CompressionFormat.Bc3)
            {
                OutputOptions =
                {
                    GenerateMipMaps = false,
                    Quality = CompressionQuality.Balanced,
                    FileFormat = OutputFileFormat.Dds,
                    DdsPreferDxt10Header = false
                }
            };

            // 3. Кодируем и сохраняем
            using (var ms = new MemoryStream())
            {
                encoder.EncodeToStream(pixelData, image.Width, image.Height, BCnEncoder.Encoder.PixelFormat.Rgba32, ms);
                File.WriteAllBytes(fullPath, ms.ToArray());
            }
        }

        public static void SaveAsDDS(this Bitmap image, string fullPath)
        {
            string directory = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(directory);

            using (var resized = new Bitmap(image, image.Width, image.Height))
            {
                var pixelData = new byte[resized.Width * resized.Height * 4];

                int index = 0;
                for (int y = 0; y < resized.Height; y++)
                {
                    for (int x = 0; x < resized.Width; x++)
                    {
                        System.Drawing.Color pixel = resized.GetPixel(x, y);
                        // Swap R and B
                        pixelData[index++] = pixel.B; // R <- B
                        pixelData[index++] = pixel.G;
                        pixelData[index++] = pixel.R; // B <- R
                        pixelData[index++] = pixel.A;
                    }
                }

                using (var surface = new Surface(resized.Width, resized.Height))
                {
                    Marshal.Copy(pixelData, 0, surface.DataPtr, pixelData.Length);
                    DDSFile.Write(fullPath, surface, TextureDimension.Two, DDSFlags.None);
                }
            }
        }


        public static void SaveAsDDS(this Image<Rgba32> image, string directory, string filename, int width, int height)
        {
            Directory.CreateDirectory(directory);
            using (var resized = ImageManager.ResizeStretch(image, width, height))
            {
                string outputPath = Path.Combine(directory, $"{filename}.dds");

                byte[] pixelData = new byte[resized.Width * resized.Height * 4];
                resized.CopyPixelDataTo(pixelData);

                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    byte r = pixelData[i];
                    byte b = pixelData[i + 2];
                    pixelData[i] = b;
                    pixelData[i + 2] = r;
                }

                using (var surface = new Surface(resized.Width, resized.Height))
                {
                    Marshal.Copy(pixelData, 0, surface.DataPtr, pixelData.Length);
                    DDSFile.Write(outputPath, surface, TextureDimension.Two, DDSFlags.None);
                }
            }
        }
        public static void SaveAsTGA(this Image<Rgba32> image, string path)
        {
            var encoder = new TgaEncoder
            {
                BitsPerPixel = TgaBitsPerPixel.Pixel32,
                Compression = TgaCompression.None
            };

            image.Save(path, encoder);
        }
        public static Image<Rgba32> ConvertToImageSharp(this System.Drawing.Image systemDrawingImage)
        {
            if (systemDrawingImage == null)
            {
                var emptyFlag = new Image<Rgba32>(82, 52);
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
    }
}
