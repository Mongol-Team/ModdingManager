using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Configs
{
    public class ConfigFile<T> where T : IConfig
    {
        public List<T> Entities { get; set; } = new List<T>(); 
        public string FileFullPath { get; set; }
        public bool IsOverride { get; set; }
        public string FileName => Path.GetFileName(FileFullPath);
        public ConfigFile() { }
    }
}