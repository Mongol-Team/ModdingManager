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
    public class LineChartType : IGfx
    {
        public Bitmap Content { get; set; }
        public Identifier Id { get; set; }
        public Point Size { get; set; }
        public double LineWidth { get; set; }
    }
}
