using Models.Attributes;
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
    [ConfigCreator(ConfigCreatorType.GenericCreator)]
    public class ArrowType : IGfx
    {
        public Identifier Id { get; set; }
        public string EffectPath { get; set; }
        public string TexturePath { get; set; }
        public Bitmap? Content { get; set; }
        public string NormalPath { get; set; }
        public Bitmap? NormalContent { get; set; }
        public string FileFullPath { get; set; }
        public string SpecularPath { get; set; }
        public Bitmap? SpecularContent { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
