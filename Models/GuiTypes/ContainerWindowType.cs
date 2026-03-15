using Models.Enums.Gui;
using Models.GuiTypes.Defenitions;
using Models.Interfaces;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuiTypes
{
    public class ContainerWindowType : IGui
    {
        public SizeDefinition Size { get; set; }
        public GuiBackgroundDefenition Background { get; set; }
        public bool? Clipping { get; set; }
        public bool? Moveable { get; set; }
        public bool? FullScreen { get; set; }

        // Animation properties
        public Point? ShowPosition { get; set; }
        public Point? HidePosition { get; set; }
        public GuiAnimationType? ShowAnimationType { get; set; }
        public GuiAnimationType? HideAnimationType { get; set; }
        public GuiAnimationType? AnimationType { get; set; }
        public int? AnimationTime { get; set; }
        public string ShowSound { get; set; }
        public string HideSound { get; set; }
        public int? FadeTime { get; set; }
        public GuiFadeType? FadeType { get; set; }

        // Scrolling properties
        public ScrollbarType VerticalScrollbar { get; set; }
        public ScrollbarType HorizontalScrollbar { get; set; }
        public GuiDragScrollButtonType? DragScroll { get; set; }
        public GuiMarginDefenition Margin { get; set; }
        public bool? AutohideScrollbars { get; set; }
        public int? ScrollWheelFactor { get; set; }
        public bool? SmoothScrolling { get; set; }

        // Nested containers and elements
        public List<ContainerWindowType> NestedContainers { get; set; } = new List<ContainerWindowType>();
        public List<IconType> Icons { get; set; } = new List<IconType>();
        public List<InstantTextBoxType> TextBoxes { get; set; } = new List<InstantTextBoxType>();
        public List<ButtonType> Buttons { get; set; } = new List<ButtonType>();
        public List<SmoothListboxType> SmoothListboxes { get; set; } = new List<SmoothListboxType>();
        public List<ListboxType> Listboxes { get; set; } = new List<ListboxType>();
        public List<CheckboxType> Checkboxes { get; set; } = new List<CheckboxType>();
        public List<OverlappingElementsBoxType> OverlappingBoxes { get; set; } = new List<OverlappingElementsBoxType>();
        public List<EditBoxType> EditBoxes { get; set; } = new List<EditBoxType>();

        public bool IsIndependent => NestedContainers.Count == 0; // Simplified logic

        public Identifier Id { get; set; }
        public Point Position { get; set; }
        public GuiOrientationType? Orientation { get; set; }
        public bool? AlwaysTransparent { get; set; }
        public string PdxTooltip { get; set; }
        public string PdxTooltipDelayed { get; set; }
    }
}
