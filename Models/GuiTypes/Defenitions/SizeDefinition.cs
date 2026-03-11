using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuiTypes.Defenitions
{
    /// <summary>
    /// Представляет размер, который может быть задан в пикселях или процентах
    /// </summary>
    public class SizeDefinition
    {
        public int? Height { get; set; }
        public int? Width { get; set; }
        public string HeightPercent { get; set; }
        public string WidthPercent { get; set; }
        public bool IsHeightPercent => !string.IsNullOrEmpty(HeightPercent);
        public bool IsWidthPercent => !string.IsNullOrEmpty(WidthPercent);
    }
}
