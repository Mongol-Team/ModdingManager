using ModdingManagerModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class StaticModifierConfig : IModifier
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<ModifierDefenitionConfig, object> Modifiers { get; set; }
    }
}
