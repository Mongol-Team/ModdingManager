using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuiTypes
{
    public class ScrollbarType
    {
        public string Name { get; set; }
        public ButtonType Slider { get; set; }
        public ButtonType Track { get; set; }
        public ButtonType LeftButton { get; set; }
        public ButtonType RightButton { get; set; }
        public Point Position { get; set; }
        public Size Size { get; set; }
        public int? Priority { get; set; }
        public int? BorderSize { get; set; }
        public int? MaxValue { get; set; }
        public int? MinValue { get; set; }
        public int? StepSize { get; set; }
        public int? StartValue { get; set; }
        public bool Horizontal { get; set; }
    }
}
