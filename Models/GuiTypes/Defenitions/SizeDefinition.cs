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
        public string MinHeightPercent { get; set; }
        public string MinWidthPercent { get; set; }
        public string MaxHeightPercent { get; set; }
        public string MaxWidthPercent { get; set; }
    }
}
