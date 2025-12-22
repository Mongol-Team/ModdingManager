using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;

namespace ModdingManagerModels.GfxTypes
{
    public class ProgressbarType : IGfx
    {
        public Identifier Id { get; set; }
        public string TexturePath { get; set; }      // "<path>"
        public string SecondTexturePath { get; set; }
        public Bitmap BgContent { get; set; }
        public Bitmap Content { get; set; }
        public Color Color { get; set; }              // { r g b [a] }
        public Color SecondColor { get; set; }
        public Point Size { get; set; }
        public string EffectFile { get; set; }        // "<path>"
        public bool IsHorisontal { get; set; }          // <bool>
        public int Steps { get; set; }                // <int>
    }
}
