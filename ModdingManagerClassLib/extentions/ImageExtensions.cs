
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TeximpNet;
using TeximpNet.DDS;

namespace ModdingManagerClassLib.Extentions
{
    public static class ImageExtensions
    {
        public static void SaveAsDDS(this System.Drawing.Image image, string directory, string filename, int width, int height)
        {
            Directory.CreateDirectory(directory);

            using (var imageSharp = ConvertToImageSharp(image))
            using (var resized = ResizeStretch(imageSharp, width, height))
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

            byte[] pixelData = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelData);

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

            using (var ms = new MemoryStream())
            {
                encoder.EncodeToStream(pixelData, image.Width, image.Height, BCnEncoder.Encoder.PixelFormat.Rgba32, ms);
                File.WriteAllBytes(fullPath, ms.ToArray());
            }
        }
        public static ImageSource ToImageSource(this System.Drawing.Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        public static Bitmap ToBitmap(this System.Drawing.Image image)
        {
            // Если уже Bitmap — просто приводим тип
            if (image is Bitmap bitmap)
                return bitmap;

            // Иначе создаём новый Bitmap и рисуем исходное изображение на нём
            var newBitmap = new Bitmap(image.Width, image.Height);
            using (var g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(image, 0, 0, image.Width, image.Height);
            }
            return newBitmap;
        }
       

        public static void SaveFlagSet(this System.Drawing.Image image, string flagsDir, string countryTag, string ideology)
        {
            using (var imageSharp = image.ConvertToImageSharp())
            {
                string pathLarge = Path.Combine(flagsDir, $"{countryTag}_{ideology}.tga");
                Directory.CreateDirectory(Path.GetDirectoryName(pathLarge)!);
                using (var resized = ResizeStretch(imageSharp, 82, 52))
                {
                    resized.SaveAsTGA(pathLarge);
                }
                string mediumDir = Path.Combine(flagsDir, "medium");
                Directory.CreateDirectory(mediumDir);
                using (var resized = ResizeStretch(imageSharp, 41, 26))
                {
                    resized.SaveAsTGA(Path.Combine(mediumDir, $"{countryTag}_{ideology}.tga"));
                }

                // Маленький флаг
                string smallDir = Path.Combine(flagsDir, "small");
                Directory.CreateDirectory(smallDir);
                using (var resized = ResizeStretch(imageSharp, 10, 7))
                {
                    resized.SaveAsTGA(Path.Combine(smallDir, $"{countryTag}_{ideology}.tga"));
                }
            }
        }


        public static void SaveAsDDS(this Image<Rgba32> image, string directory, string filename, int width, int height)
        {
            Directory.CreateDirectory(directory);
            using (var resized = ResizeStretch(image, width, height))
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

        public static Image<Rgba32> ResizeStretch(this Image<Rgba32> image, int width, int height)
        {
            return image.Clone(x => x.Resize(width, height));
        }

        public static BitmapSource ToBitmapSource(this System.Drawing.Image image)
        {
            if (image == null)
                return null;

            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
