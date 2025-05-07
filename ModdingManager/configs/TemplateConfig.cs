using ModdingManager.classes.args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.configs
{
    public class TemplateConfig
    {
        public List<RegimentConfig> SupportItems = new List<RegimentConfig>();
        public List<RegimentConfig> BrigadeItems = new List<RegimentConfig>();
        public string Name { get; set; }
        public bool IsLocked { get; set; }
        public string Namespace { get; set; }
        public bool AllowTrainig { get; set; }
        public string ModelName { get; set; }
        public int DivisionCap { get; set; }
        public int Priority { get; set; }
        public int CustomIconId { get; set; }
        public string OOBFileName { get; set; }
        public int OOBFFileYear { get; set; }
    }
}
