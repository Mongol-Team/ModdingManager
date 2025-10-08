using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;

namespace ModdingManagerModels.GfxTypes
{
    public class FrameAnimatedSpriteType : IGfx
    {
        public Identifier Id { get; set; }
        public string TexturePath { get; set; }          // "<path>"
        public Bitmap Content { get; set; }
        public int NoOfFrames { get; set; }              // <int>
        public string EffectFile { get; set; }
        public int AnimationRateFps { get; set; }       // <int>
        public bool Looping { get; set; }             // <bool>
        public bool PlayOnShow { get; set; }             // <bool>
        public double PauseOnLoop { get; set; }
        public bool AllwaysTransparent { get; set; }    // <bool>
    }
}
