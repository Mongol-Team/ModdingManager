using System.Drawing;

namespace ModdingManagerModels.Args

{
    public class ImageSourceArg
    {
        public Bitmap Source { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public double ScaleY { get; set; } = 1.0;
        public double ScaleX { get; set; } = 1.0;
        public bool IsCompresed { get; set; } = false;
    }
}
