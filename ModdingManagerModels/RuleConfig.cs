using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class RuleConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GroupId { get; set; }
        public string RequiredDlc { get; set; }
        public string ExcludedDlc { get; set; }
        public List<RuleOptionConfig> Options { get; set; }
    }
}
