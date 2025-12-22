using ModdingManagerModels.Enums;
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
    public class TriggerDefenitionConfig : IConfig
    {
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public ConfigLocalisation CustomTooltip { get; set; }
        public ScopeTypes Scope { get; set; }
        public List<TargetType> Target { get; set; }
        public Identifier Id { get; set; }
    }
}
