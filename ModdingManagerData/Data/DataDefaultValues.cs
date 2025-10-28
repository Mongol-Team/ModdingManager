
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerData
{
    public static class DataDefaultValues
    {
        public const string Null = "Null";
        public const string NaN = "NaN";
        public static readonly KeyValuePair<string, string> NullLocalistion = new(Null, Null);
        public const string None = "None";
        public static readonly Bitmap NullImageSource = ModdingManagerData.Properties.Resources.null_item_image;
        public static readonly Bitmap ItemWithNoGfxImage = ModdingManagerData.Properties.Resources.item_with_no_gfx_image;
        public const int NullInt = -1;

    }
}
