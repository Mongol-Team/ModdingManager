using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class StaticModifierConfig : IModifier
    {
        public Identifier Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
    }
}
