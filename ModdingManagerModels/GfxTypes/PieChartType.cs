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
    public class PieChartType : IGfx
    {
        public Identifier Id { get; set; }
        public int Size { get; set; }
        public List<Color> Colors { get; set; }
    }
}
