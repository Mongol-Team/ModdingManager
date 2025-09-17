using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TeximpNet;
using TeximpNet.DDS;
using Pfim;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ModdingManagerClassLib.Extentions
{
    public static class BitmapExtensions
    {

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

        public static ImageSource ToImageSource(this Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // важно для освобождения потока
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // делает потокобезопасным

                return bitmapImage;
            }
        }

        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            var hBitmap = bitmap.GetHbitmap(); // Получаем дескриптор HBitmap

            try
            {
                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                bitmapSource.Freeze(); // Делаем потокобезопасным
                return bitmapSource;
            }
            finally
            {
                // Освобождаем ресурсы GDI
                NativeMethods.DeleteObject(hBitmap);
            }
        }
        public static Mat ToMat(this Bitmap bitmap)
        {
            return BitmapConverter.ToMat(bitmap);
        }
        public static System.Drawing.Image ToDrawingImage(this BitmapSource bitmapSource)
        {
            if (bitmapSource == null)
                return null;

            using (var outStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder(); // <-- используем PNG, не BMP
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(outStream);
                outStream.Position = 0;

                return System.Drawing.Image.FromStream(outStream, true, true);
            }
        }
        public static Bitmap LoadFromDDS(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new ArgumentException("Invalid or non-existent DDS file path.", nameof(path));

            // 1) Загружаем и декодируем DDS
            using Pfim.IImage dds = Pfim.Pfimage.FromFile(path);

            int width = dds.Width;
            int height = dds.Height;
            byte[] raw = dds.Data;

            // 2) Мапим формат Pfim → System.Drawing.Imaging.PixelFormat
            PixelFormat sysFmt = dds.Format switch
            {
                Pfim.ImageFormat.Rgba32 => PixelFormat.Format32bppArgb,
                Pfim.ImageFormat.Rgb24 => PixelFormat.Format24bppRgb,
                Pfim.ImageFormat.R5g5b5 => PixelFormat.Format16bppRgb555,
                Pfim.ImageFormat.R5g6b5 => PixelFormat.Format16bppRgb565,
                Pfim.ImageFormat.Rgba16 => PixelFormat.Format16bppArgb1555,
                _ => throw new NotSupportedException($"DDS формат {dds.Format} не поддерживается")
            };

            // 3) Создаём Bitmap и копируем данные
            var bmp = new Bitmap(width, height, sysFmt);
            var rect = new System.Drawing.Rectangle(0, 0, width, height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, sysFmt);

            // Проверяем, совпадают ли stride
            if (bmpData.Stride == dds.Stride)
            {
                Marshal.Copy(raw, 0, bmpData.Scan0, raw.Length);
            }
            else
            {
                // Копируем по строкам, если stride отличаются
                int bytesPerPixel = dds.Format switch
                {
                    Pfim.ImageFormat.Rgba32 => 4,
                    Pfim.ImageFormat.Rgb24 => 3,
                    _ => 2 // для 16-битных форматов
                };
                int rowBytes = width * bytesPerPixel;
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(raw, y * dds.Stride, bmpData.Scan0 + y * bmpData.Stride, rowBytes);
                }
            }

            bmp.UnlockBits(bmpData);
            bmp.SetResolution(96, 96);  // DPI по умолчанию для WPF

            return bmp;
        }
        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
        }
    }
}
