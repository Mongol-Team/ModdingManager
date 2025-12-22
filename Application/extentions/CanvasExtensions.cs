using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Application.Extentions
{
    public static class CanvasExtensions
    {
        public static System.Windows.Controls.RichTextBox FindWrappedTextBox(this Canvas wrapper)
        {
            return wrapper.Children.OfType<System.Windows.Controls.RichTextBox>().FirstOrDefault();
        }
        public static List<T> FindElementsOfType<T>(this Canvas main) where T : UIElement
        {
            return main.Children.OfType<T>().ToList();
        }
        public static System.Windows.Controls.Image FindWrappedImage(this Canvas wrapper)
        {

            return wrapper.Children.OfType<System.Windows.Controls.Image>().FirstOrDefault();
        }

    }
}
