using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;

namespace Models.GfxTypes
{
    public class MaskedShieldType : IGfx
    {
        //tyt bil duvblicat CorneredTileSpriteType, nyzhno realizovat етот класс
        public Identifier Id { get; set; } //string
        public string TexturePath { get; set; }
        public string MaskTexturePath { get; set; }
        public string EffectFile { get; set; }
        public Bitmap? Content { get; set; }
        public Bitmap MaskContent { get; set; }
    }
}


