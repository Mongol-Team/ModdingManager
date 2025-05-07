using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.configs
{
    public class RegimentConfig
    {
        public RegimentConfig() { }
        public string Name { get; set; }
        public Image Icon { get; set; }
        public List<string> Categories {  get; set; }
        public int X {  get; set; }
        public int Y { get; set; }
    }
}
