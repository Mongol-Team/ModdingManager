using Models.Enums.Gui;
using Models.Interfaces;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuiTypes
{
    public class EditBoxType : IGui
    {
        public string Text { get; set; }
        public string Font { get; set; }
        public int? MaxWidth { get; set; }
        public int? MaxHeight { get; set; }
        public GuiTextFormatType? Format { get; set; }
        public bool? Fixedsize { get; set; }
        public int? BorderSize { get; set; }
        public bool? IgnoreTabNavigation { get; set; }
        public Identifier Id { get ; set ; }
        public Point Position { get ; set ; }
        public GuiOrientationType? Orientation { get ; set ; }
        public bool? AlwaysTransparent { get ; set ; }
        public string PdxTooltip { get ; set ; }
        public string PdxTooltipDelayed { get ; set ; }
    }
}
