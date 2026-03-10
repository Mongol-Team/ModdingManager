using Models.Enums;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace View.Interfaces
{
    public interface IMod
    {
        public Bitmap? Image { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Authors { get; set; }
        public string ModVersion { get; set; }
        public string GameVersion { get; set; }
        public List<string> ReplacePathes { get; set; }
        public ModTypes Type { get; set; }
    }
}