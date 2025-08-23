using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ModdingManagerClassLib.Extentions
{

    public static class VisualTreeExtensions
    {
        public static T ClosestOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            while (start != null)
            {
                if (start is T t) return t;
                start = VisualTreeHelper.GetParent(start);
            }
            return null;
        }
        public static T FindVisualChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result && ((FrameworkElement)child).Name == childName)
                {
                    return result;
                }

                T childResult = FindVisualChild<T>(child, childName);
                if (childResult != null)
                {
                    return childResult;
                }
            }
            return null;
        }
        public static T FindChildByName<T>(this DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild && typedChild.Name == name)
                {
                    return typedChild;
                }

                var result = FindChildByName<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }

}
