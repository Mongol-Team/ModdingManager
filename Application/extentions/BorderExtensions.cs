using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Application.Extentions
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
