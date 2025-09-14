using System.Drawing;

namespace ModdingManagerModels.GfxTypes
{
    public struct ProgressbarType
    {
        public string name;              // "GFX_<name>"
        public string texturefile1;      // "<path>"
        public string texturefile2;      // "<path>"

        public Color color;              // { r g b [a] }
        public Color colortwo;           // { r g b [a] }

        public Point size;               // { x y }

        public string effectfile;        // "<path>"
        public bool horizontal;          // <bool>
        public int steps;                // <int>
    }
}
