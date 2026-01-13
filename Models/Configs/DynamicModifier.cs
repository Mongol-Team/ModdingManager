using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;


namespace Models.Configs
{
    public class DynamicModifierConfig : IConfig, IModifier
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public string EnableTrigger { get; set; }
        public string RemovalTrigger { get; set; }
        public IGfx Gfx { get; set; }
        public bool HasAttackerEffect { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public ConfigLocalisation ConfigLocalisation { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
    }
}
