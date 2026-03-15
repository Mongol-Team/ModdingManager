using Models.Enums.Gui;
using Models.GuiTypes.Defenitions;
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
    public class ScrollbarType : IGui
    {

        public ButtonType Slider { get; set; }
        public ButtonType Track { get; set; }
        public ButtonType LeftButton { get; set; }
        public ButtonType RightButton { get; set; }
        public Point Position { get; set; }
        public SizeDefinition BorderSize { get; set; }
        public SizeDefinition Size { get; set; }
        public int? Priority { get; set; }
        public int? MaxValue { get; set; }
        public int? MinValue { get; set; }
        public int? StepSize { get; set; }
        public int? StartValue { get; set; }
        public bool Horizontal { get; set; }
        public Identifier Id { get; set; }
        public GuiOrientationType? Orientation { get; set; }
        public bool? AlwaysTransparent { get; set; }
        public string PdxTooltip { get; set; }
        public string PdxTooltipDelayed { get; set; }
    }
}
