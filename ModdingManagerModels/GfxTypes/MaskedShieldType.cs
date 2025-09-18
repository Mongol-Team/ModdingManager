using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;

namespace ModdingManagerModels.GfxTypes
{
    public class MaskedShieldType : IConfig, IGfx
    {
        //tyt bil duvblicat CorneredTileSpriteType, nyzhno realizovat етот класс
        public Identifier Id { get; set; } //string
        public ConfigLocalisation Localisation { get; set; }
        public string TexturePath { get; set; }
        public string MathTexturePath { get; set; }
        public string EffectFile { get; set; }
        public Bitmap? Content { get; set; }
    }
}


