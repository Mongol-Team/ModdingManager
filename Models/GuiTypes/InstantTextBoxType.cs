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
    public class InstantTextBoxType : IGui
    {
        public string Text { get; set; }
        public string Font { get; set; }
        public int? MaxWidth { get; set; }
        public int? MaxHeight { get; set; }
        public GuiTextFormatType? Format { get; set; }
        public bool? Fixedsize { get; set; }
        public int? BorderSize { get; set; }
        public ScrollbarType Scrollbar { get; set; }
        public string TextureFile { get; set; } // Rarely used
        public string Name { get; set; }
        public Point Position { get; set; }
        public GuiOrientationType? Orientation { get; set; }
        public bool? AlwaysTransparent { get; set; }
        public string PdxTooltip { get; set; }
        public string PdxTooltipDelayed { get; set; }
    }
}
