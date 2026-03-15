using Models.Enums.Gui;
using Models.GfxTypes;
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
    public class ButtonType : IGui
    {
        public SpriteType SpriteType { get; set; }
        public CorneredTileSpriteType QuadTextureSprite { get; set; }
        public int? Frame { get; set; }
        public string ButtonText { get; set; }
        public string ButtonFont { get; set; }
        public string Shortcut { get; set; }
        public string ClickSound { get; set; }
        public string OverSound { get; set; }
        public string HintTag { get; set; }
        public double? Scale { get; set; }
        public string WebLink { get; set; }
        public Identifier Id { get ; set ; }
        public Point Position { get ; set ; }
        public GuiOrientationType? Orientation { get ; set ; }
        public bool? AlwaysTransparent { get ; set ; }
        public string PdxTooltip { get ; set ; }
        public string PdxTooltipDelayed { get ; set ; }
    }

}
