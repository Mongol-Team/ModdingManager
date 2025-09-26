using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class RuleConfig : IConfig
    {
        public string GroupId { get; set; }
        public string RequiredDlc { get; set; }
        public string ExcludedDlc { get; set; }
        public List<BaseConfig> Options { get; set; }
        public BaseConfig Default { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public Identifier Id { get; set; }
        public IGfx Icon { get; set; }

    }
}
