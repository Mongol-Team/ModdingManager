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
    public class LineChartType : IGfx
    {
        public Bitmap Content { get; set; }
        public Identifier Id { get; set; }
        public Point Size { get; set; }
        public double LineWidth { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
