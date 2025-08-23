namespace ModdingManager.WPFExtensions
{
    public static class PointExtentions
    {
        public static Point ToDrawingPoint(this System.Windows.Point p)
        => new Point((int)p.X, (int)p.Y);

        // System.Drawing.Point → System.Windows.Point
        public static System.Windows.Point ToWindowsPoint(this Point p)
            => new System.Windows.Point(p.X, p.Y);

        // IEnumerable<System.Windows.Point> → IEnumerable<System.Drawing.Point>
        public static IEnumerable<Point> ToDrawingPoints(this IEnumerable<System.Windows.Point> points)
            => points.Select(p => p.ToDrawingPoint());

        // IEnumerable<System.Drawing.Point> → IEnumerable<System.Windows.Point>
        public static IEnumerable<System.Windows.Point> ToWindowsPoints(this IEnumerable<Point> points)
            => points.Select(p => p.ToWindowsPoint());
    }
}
