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
    public class StaticModifierConfig : IModifier, IConfig
    {
        public Identifier Id { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public bool HasAttackerEffect { get; set; }

    }
}
