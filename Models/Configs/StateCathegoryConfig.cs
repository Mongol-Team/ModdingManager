using Models.Attributes;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Configs
{
    [ConfigCreator(ConfigCreatorType.GenericCreator)]
    public class StateCathegoryConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Color Color { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; } = new();
        public ConfigLocalisation Localisation { get; set; }
        public Identifier Id { get; set; }
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
