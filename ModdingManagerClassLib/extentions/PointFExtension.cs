using System.Drawing;

namespace ModdingManagerClassLib.Extentions
{
    public static class PointFExtension
    {
        public static Point ToDrawingPoint(this System.Windows.Point p)
        => new Point((int)p.X, (int)p.Y);

        public static System.Windows.Point ToWindowsPoint(this Point p)
            => new System.Windows.Point(p.X, p.Y);

        public static IEnumerable<Point> ToDrawingPoints(this IEnumerable<System.Windows.Point> points)
            => points.Select(p => p.ToDrawingPoint());

        public static IEnumerable<System.Windows.Point> ToWindowsPoints(this IEnumerable<Point> points)
            => points.Select(p => p.ToWindowsPoint());
    }
}
