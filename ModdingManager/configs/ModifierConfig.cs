using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.configs
{
    public class ModifierConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public string Modifiers { get; set; }
        public string Type { get; set; }
        public string Variation { get; set; }
        public string EnableTrigger { get; set; }
        public string RemovalTrigger { get; set; }
        public string Trigger { get; set; }
        public string AttackerEffect { get; set; }
        public string PowerBalance { get; set; }
        public string RelationTrigger { get; set; }
        public bool IsTrading { get; set; }
        public int Days { get; set; }
        public int Decay { get; set; }
        public int MinTrust { get; set; }
        public int MaxTrust { get; set; }
        public int Value { get; set; }
        public string IconPath { get; set; }
    }
}
