using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public class IdeaConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public string Tag { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
        public string RemovalCost { get; set; }
        public string Available { get; set; }
        public string AvailableCivilWar { get; set; }
        public string OnAdd { get; set; }
    }
}
