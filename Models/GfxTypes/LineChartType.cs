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
    public class LineChartType : IGfx
    {
        public Bitmap Content { get; set; }
        public Identifier Id { get; set; }
        public Point Size { get; set; }
        public string FileFullPath { get; set; }
        public double LineWidth { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
