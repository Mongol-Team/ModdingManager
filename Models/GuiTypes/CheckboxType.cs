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
    public class CheckboxType : IGui
    {
        public string SpriteType { get; set; }
        public string QuadTextureSprite { get; set; }
        public int? Frame { get; set; }
        public string ButtonText { get; set; }
        public string ButtonFont { get; set; }
        public string Shortcut { get; set; }
        public string ClickSound { get; set; }
        public string HintTag { get; set; }
        public float? Scale { get; set; }
        public string Tooltip { get; set; } // Never used
        public string TooltipText { get; set; } // Never used
        public string DelayedTooltipText { get; set; } // Never used
        public string Name { get ; set ; }
        public Point Position { get ; set ; }
        public GuiOrientationType? Orientation { get ; set ; }
        public bool? AlwaysTransparent { get ; set ; }
        public string PdxTooltip { get ; set ; }
        public string PdxTooltipDelayed { get ; set ; }
    }
}
