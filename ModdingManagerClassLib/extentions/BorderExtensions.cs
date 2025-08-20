using System.Windows.Controls;

namespace ModdingManager.classes.extentions
{
    public static class BorderExtensions
    {
        public static System.Windows.Controls.Image GetImage(this Border border)
        {
            if (border.Child is Canvas canvas)
            {
                return canvas.Children.OfType<System.Windows.Controls.Image>().FirstOrDefault();
            }
            return null;
        }
    }
}
