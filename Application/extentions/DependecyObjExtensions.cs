using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace Application.Extentions
{
    public static class DependencyObjExtensions
    {
        public static T FindChild<T>(this DependencyObject parent, string childName)
            where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T && ((FrameworkElement)child).Name == childName)
                {
                    return (T)child;
                }

                var result = FindChild<T>(child, childName);
                if (result != null) return result;
            }

            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        public static T FindAncestor<T>(this DependencyObject current)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                    return ancestor;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        /// <summary>
        /// Получает визуального родителя элемента
        /// </summary>
        public static DependencyObject GetVisualParent(this DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            return System.Windows.Media.VisualTreeHelper.GetParent(element);
        }
    }
}
