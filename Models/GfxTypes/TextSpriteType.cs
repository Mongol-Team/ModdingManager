using Models.Interfaces;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GfxTypes
{
    public class TextSpriteType : IGfx
    {
        public Identifier Id { get; set; }
        public string TexturePath { get; set; }
        public Bitmap? Content { get; set; }
        public int NoOfFrames { get; set; }
        public string EffectFile { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
