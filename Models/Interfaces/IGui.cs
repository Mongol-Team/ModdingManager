using Models.Enums.Gui;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для всех GUI элементов
    /// </summary>
    public interface IGui
    {
        public string Name { get; set; }
        public Point Position { get; set; }
        public GuiOrientationType? Orientation { get; set; }
        public bool? AlwaysTransparent { get; set; }
        public string PdxTooltip { get; set; }
        public string PdxTooltipDelayed { get; set; }
    }
}
