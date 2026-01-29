using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Configs
{
    public class TriggerDefenitionConfig : IConfig
    {
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public ConfigLocalisation CustomTooltip { get; set; }
        public ScopeTypes Scope { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public List<TargetType> Target { get; set; }
        public Identifier Id { get; set; }
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
