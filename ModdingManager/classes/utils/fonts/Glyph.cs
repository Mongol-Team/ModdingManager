
namespace ModdingManager.classes.utils.fonts
{
    public class Glyph
    {
        public char Char { get; }
        public SixLabors.ImageSharp.Size Size { get; }
        public int X { get; set; }
        public int Y { get; set; }
        public Glyph(char c, SixLabors.ImageSharp.Size size) => (Char, Size) = (c, size);
    }
}
