using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.configs
{
    public class IdeologyConfig
    {
        public string Id {  get; set; }
        public string Description { get; set; }
        public string Noun { get; set; }
        public string Name { get; set; }
        public List<IdeologyType> SubTypes { get; set; }
        public Color Color { get; set; }
        public Dictionary<string, bool> Rules { get; set; }
        public Dictionary<string, double> Modifiers { get; set; }
        public bool CanFormExileGoverment { get; set; }
        public double WarImpactOnTension { get; set; }
        public double FactionImpactOnTension { get; set; }
        public bool CanBeBoosted { get; set; }
        public bool CanColaborate { get; set; }
        public Dictionary<string, double> FactionModifiers { get; set; }
        public string AiIdeologyName { get; set; }
        public List<string> DynamicFactionNames { get; set; }
    }
    public class IdeologyType
    {
        public bool CanBeRandomlySelected { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
    }
}
