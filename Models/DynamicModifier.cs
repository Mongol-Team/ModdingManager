using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;


namespace ModdingManagerModels
{
    public class DynamicModifierConfig : IConfig, IModifier
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public string EnableTrigger { get; set; }
        public string RemovalTrigger { get; set; }
        public IGfx Gfx { get; set; }
        public bool HasAttackerEffect { get; set; }
        public ConfigLocalisation ConfigLocalisation { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
    }
}
