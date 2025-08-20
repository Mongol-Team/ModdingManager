using ModdingManager.classes.args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModdingManager.classes.metadata
{
    public static class ProvinceMetadata
    {
        public static readonly DependencyProperty ProvinceShapeProperty =
            DependencyProperty.RegisterAttached(
                "ProvinceShape",
                typeof(ProvinceShapeArg),
                typeof(ProvinceMetadata),
                new PropertyMetadata(null));

        public static void SetProvinceShape(UIElement element, ProvinceShapeArg value)
        {
            element.SetValue(ProvinceShapeProperty, value);
        }

        public static ProvinceShapeArg GetProvinceShape(UIElement element)
        {
            return (ProvinceShapeArg)element.GetValue(ProvinceShapeProperty);
        }
    }

}
