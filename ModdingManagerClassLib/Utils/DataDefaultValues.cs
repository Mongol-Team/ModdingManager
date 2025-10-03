using ModdingManagerClassLib.Properties;
using ModdingManagerModels.GfxTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels.Types.Utils
{
    public class DataDefaultValues
    {
        public const string Null = "Null";
        public const string NaN = "NaN";
        public const string None = "None";
        public static SpriteType NullImage = new SpriteType(Resources.null_item_image, "Null");
        public const int NullInt = -1;

    }
}
