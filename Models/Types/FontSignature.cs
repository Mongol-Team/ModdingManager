using System.Drawing;

namespace Models.Types
{
    public class FontSignature
    {
        public string Family { get; }
        public int Size { get; }
        public string Path { get; set; }
        public Color Color { get; }
    }
}
