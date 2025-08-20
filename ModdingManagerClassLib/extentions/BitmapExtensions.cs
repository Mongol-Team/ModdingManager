using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModdingManager.classes.extentions
{
    public static class BitmapExtensions
    {

        public static void SaveAsDds(this Bitmap bitmap, string path)
        {
            var encoder = new BcEncoder(CompressionFormat.Bc3);
            encoder.OutputOptions.Quality = CompressionQuality.Balanced;
            encoder.OutputOptions.GenerateMipMaps = false;

            // Конвертируем Bitmap в массив RGBA
            var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                byte[] bytes = new byte[data.Stride * data.Height];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

                // BCnEncoder ожидает RGBA, а WPF дает BGRA, поэтому меняем каналы
                for (int i = 0; i < bytes.Length; i += 4)
                {
                    byte temp = bytes[i];
                    bytes[i] = bytes[i + 2];
                    bytes[i + 2] = temp;
                }

                using (var ms = new MemoryStream())
                {
                    encoder.EncodeToStream(bytes, bitmap.Width, bitmap.Height, BCnEncoder.Encoder.PixelFormat.Rgba32, ms);
                    File.WriteAllBytes(path, ms.ToArray());
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
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

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
        }
    }
}
