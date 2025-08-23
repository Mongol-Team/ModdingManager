using System.Windows;
using System.Windows.Controls;

namespace ModdingManagerClassLib.Extentions
{
    public static class CollectionsExtension
    {
        public static UIElement? GetByName(this UIElementCollection collection, string name)
        {
            return collection
                .OfType<FrameworkElement>()
                .FirstOrDefault(el => el.Name == name);
        }
    }
}
