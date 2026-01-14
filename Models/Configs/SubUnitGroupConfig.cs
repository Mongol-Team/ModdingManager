using Models.Attributes;
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
    [ConfigCreator(ConfigCreatorType.GenericCreator)]
    public class SubUnitGroupConfig : IConfig
    {
        public Identifier Id { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
    }
}
