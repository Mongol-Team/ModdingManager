using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EntityFiles
{
    public class GfxFile<T> where T : IGfx
    {
        public List<T> Entities { get; set; } = new List<T>();
        public string FileFullPath { get; set; }
        public bool IsOverride { get; set; }
        public bool IsCore { get; set; }
        public string FileName => Path.GetFileName(FileFullPath);
        public GfxFile() { }
    }
}
