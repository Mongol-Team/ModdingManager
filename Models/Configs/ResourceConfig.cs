using Models.Attributes;
using Models.Configs;
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
    public class ResourceConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public bool IsCore { get; set; }
        public double Cost { get; set; }
        public double Convoys { get; set; }
        public int IconIndex { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
    }
}
