using ModdingManager.classes.args;
using ModdingManager.classes.configs;
using OpenCvSharp;

namespace ModdingManager.classes.extentions
{
    public static class MatExtentions
    {
        public static Point2f[] GetPointsFromMat(this Mat mat)
        {
            if (mat.Empty())
                return Array.Empty<Point2f>();

            var points = new Point2f[mat.Rows * mat.Cols];
            var index = 0;

            for (int i = 0; i < mat.Rows; i++)
            {
                for (int j = 0; j < mat.Cols; j++)
                {
                    points[index++] = mat.At<Point2f>(i, j);
                }
            }

            return points;
        }

        public static ProvinceShapeArg CreateSinglePixelShape(this Mat mask, ProvinceConfig province)
        {
            // Находим координаты единственного пикселя
            var index = mask.FindNonZero();
            var pt = index.At<OpenCvSharp.Point>(0);

            // Создаем квадратный контур 1x1 пиксель
            var contour = new[]
            {
        new System.Windows.Point(pt.X, pt.Y),
        new System.Windows.Point(pt.X+1, pt.Y),
        new System.Windows.Point(pt.X+1, pt.Y+1),
        new System.Windows.Point(pt.X, pt.Y+1)
    };

            return new ProvinceShapeArg
            {
                ContourPoints = contour,
                Pos = new System.Windows.Point(pt.X + 0.5, pt.Y - 0.5),
                FillColor = System.Windows.Media.Color.FromArgb(255, province.Color.R, province.Color.G, province.Color.B)
            };
        }
    }
}