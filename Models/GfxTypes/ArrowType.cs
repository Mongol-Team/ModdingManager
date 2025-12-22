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
    public class ArrowType : IGfx
    {
        public Identifier Id { get; set; }
        public string EffectPath { get; set; }
        public string TexturePath { get; set; }
        public Bitmap? Content { get; set; }
        public string NormalPath { get; set; }
        public Bitmap? NormalContent { get; set; }
        public string SpecularPath { get; set; }
        public Bitmap? SpecularContent { get; set; }
    }
}
