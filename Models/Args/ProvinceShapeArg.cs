using System.Drawing;

namespace Models.Args
{

    public class ProvinceShapeArg
    {
        public Point[] ContourPoints { get; set; }  // точки в оригинальном масштабе
        public Point Pos { get; set; }
        public Color FillColor { get; set; }
    }
}
