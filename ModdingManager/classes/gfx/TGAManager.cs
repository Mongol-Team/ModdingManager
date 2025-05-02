using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
namespace ModdingManager.classes.gfx
{
    public class TGAManager : ImageManager
    {
        public TGAManager() { }

        public static void SaveTga(Image<Rgba32> image, string path)
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
