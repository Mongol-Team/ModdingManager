using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class StateCathegoryConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Color Color { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; } = new();
        public ConfigLocalisation Localisation { get; set; }
        public Identifier Id { get; set; }
    }
}
