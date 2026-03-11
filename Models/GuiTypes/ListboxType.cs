using Models.Enums.Gui;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuiTypes
{
    public class ListboxType : IGui
    {
        public Size Size { get; set; }
        public int? Spacing { get; set; }
        public bool? Horizontal { get; set; }
        public ScrollbarType ScrollbarType { get; set; }
        public int? BorderSize { get; set; }
        public string Background { get; set; } // Never used
        public string Name { get ; set ; }
        public Point Position { get ; set ; }
        public GuiOrientationType? Orientation { get ; set ; }
        public bool? AlwaysTransparent { get ; set ; }
        public string PdxTooltip { get ; set ; }
        public string PdxTooltipDelayed { get ; set ; }
    }
}
