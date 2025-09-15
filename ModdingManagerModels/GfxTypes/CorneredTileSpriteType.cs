using System.Drawing;

namespace ModdingManagerModels.GfxTypes
{
    public class CorneredTileSpriteType
    {
        public string Name { get; set; }              // "GFX_<name>"
        public string TexturePath { get; set; }      // "<path>"
        public int NoOfFrames { get; set; }
        public Point Size { get; set; }                // { x y }
        public Point BorderSize { get; set; }
        public string EffectFile { get; set; }
        public bool AllwaysTrancparent { get; set; }
        public bool TilingCenter { get; set; }
        public bool Looping { get; set; }
        public int AnimationRateSpf { get; set; }
    }
}

