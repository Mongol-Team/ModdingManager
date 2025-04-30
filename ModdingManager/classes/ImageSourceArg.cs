using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ModdingManager.classes
{
    public class ImageSourceArg
    {
        public ImageSource Source { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public double ScaleY { get; set; } = 1.0;
        public double ScaleX { get; set; } = 1.0;
        public bool IsCompresed { get; set; } = false;
        public ImageSourceArg() { 
        }
    }
}
