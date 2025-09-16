using ModdingManagerModels.Types.Utils;


namespace ModdingManagerModels
{
    public class DynamicModifierConfig : IConfig
    {
        public Identifier Id { get; set; }
        public string EnableTrigger { get; set; }
        public string RemovalTrigger { get; set; }
        public string Trigger { get; set; }
        public bool HasAttackerEffect { get; set; }
        public string PowerBalance { get; set; }
        public string RelationTrigger { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
    }
}
