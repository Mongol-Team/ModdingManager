namespace ModdingManagerModels.Args
{
    public class ProvinceShapeArg
    {
        public System.Windows.Point[] ContourPoints { get; set; }  // точки в оригинальном масштабе
        public System.Windows.Point Pos { get; set; }
        public System.Windows.Media.Color FillColor { get; set; }
    }
}
