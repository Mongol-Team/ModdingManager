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
    public class PieChartType : IGfx
    {
        public Bitmap Content { get; set; }
        public Identifier Id { get; set; }
        public int Size { get; set; }
        public List<Color> Colors { get; set; }
        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
