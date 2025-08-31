using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class DynamicModifierConfig : IModel
    {
        public string EnableTrigger { get; set; }
        public string RemovalTrigger { get; set; }
        public string Trigger { get; set; }
        public bool HasAttackerEffect { get; set; }
        public string PowerBalance { get; set; }
        public string RelationTrigger { get; set; }
        public Dictionary<ModifierDefenitionConfig, object> Modifiers { get; set; }
    }
}
