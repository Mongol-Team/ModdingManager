using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using System.IO;
using System.Windows.Media.Imaging;

namespace ModdingManager.classes.extentions
{
    public static class BitmapSourceExtensions
    {
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
            encoder.EncodeToStream(pixelData, width, height, PixelFormat.Rgba32, ms);
            return ms.ToArray();
        }
    }
}
