using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.args
{
    public class ProvinceShapeArg
    {
        public System.Windows.Point[] ContourPoints { get; set; }  // точки в оригинальном масштабе
        public System.Windows.Point Pos { get; set; }  
        public System.Windows.Media.Color FillColor { get; set; }
    }
}
