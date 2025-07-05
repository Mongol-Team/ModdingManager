using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModdingManager.classes.extentions
{
    public static class BitmapSourceExtensions
    {

        public static BitmapSource CreateIndependentBitmapCopy(this BitmapSource source)
        {
            if (source == null)
                return null;
            return new WriteableBitmap(source);
        }

        public static byte[] ConvertToDdsBC3(this BitmapSource bitmapSource)
        {
            var width = bitmapSource.PixelWidth;
            var height = bitmapSource.PixelHeight;
            var stride = width * 4;
            var pixelData = new byte[height * stride];
            bitmapSource.CopyPixels(pixelData, stride, 0);

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte temp = pixelData[i];
                pixelData[i] = pixelData[i + 2];
                pixelData[i + 2] = temp;
            }

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
            using var ms = new MemoryStream();
            encoder.EncodeToStream(pixelData, width, height, BCnEncoder.Encoder.PixelFormat.Rgba32, ms);
            return ms.ToArray();
        }

        public static BitmapSource ToBitmapSource(this UIElement element, int width, int height)
        {
            var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(System.Windows.Media.Brushes.Transparent, null, new Rect(0, 0, width, height));

                var vb = new VisualBrush(element);
                ctx.DrawRectangle(vb, null, new Rect(0, 0, width, height));
            }

            renderBitmap.Render(dv);
            return renderBitmap;
        }
    }
}
