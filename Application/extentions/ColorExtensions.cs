namespace Application.Extentions
{
    public static class ColorExtensions
    {
        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static System.Drawing.Color? ToDrawingColor(this System.Windows.Media.Color? color)
        {
            if (color == null) return null;

            var c = color.Value;
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static System.Windows.Media.Color? ToMediaColor(this System.Drawing.Color? color)
        {
            if (color == null) return null;

            var c = color.Value;
            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}
