using Models.Attributes;
using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.ObectCacheData;
using Models.Types.Utils;
using System.Drawing;

namespace Models.Configs
{
    [ConfigCreator(ConfigCreatorType.GenericCreator)]
    public class IdeologyConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public string Description { get; set; }
        public string Noun { get; set; }
        public string Name { get; set; }
        public List<IdeologyType> SubTypes { get; set; }
        public Color Color { get; set; }
        public Dictionary<RuleConfig, bool> Rules { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
        public bool CanFormExileGoverment { get; set; }
        public double WarImpactOnTension { get; set; }
        public double FactionImpactOnTension { get; set; }
        public bool CanBeBoosted { get; set; }
        public bool CanColaborate { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> FactionModifiers { get; set; }
        public IdeologyAIType AiIdeologyName { get; set; }
        public List<string> DynamicFactionNames { get; set; }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
