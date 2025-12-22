using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels.GfxTypes
{
    public class CircularProgressBarType : IGfx
    {
        public Identifier Id { get; set; }
        public string TexturePath { get; set; }
        public Bitmap? Content { get; set; }
        public string EffectPath { get; set; }
        public Bitmap? EffectContent { get; set; }
        public int Size { get; set; }
        public int Rotation { get; set; }
        public int Amount { get; set; }
    }
}
