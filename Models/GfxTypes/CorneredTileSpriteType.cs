using Models.Interfaces;
using Models.Types.Utils;
using System.Drawing;

namespace Models.GfxTypes
{
    public class CorneredTileSpriteType : IGfx
    {
        public string TexturePath { get; set; }      // "<path>"
        public int NoOfFrames { get; set; }
        public Point Size { get; set; }                // { x y }
        public Point BorderSize { get; set; }
        public string EffectFile { get; set; }
        public bool AllwaysTrancparent { get; set; }
        public bool TilingCenter { get; set; }
        public bool Looping { get; set; }
        public int AnimationRateSpf { get; set; }
        public Identifier Id { get; set; }
        public Bitmap? Content { get; set; }
    }
}

