using ModdingManagerModels.Enums;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;

namespace ModdingManagerModels
{
    public class IdeaConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public string Tag { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
        public int RemovalCost { get; set; }
        public int Cost { get; set; }
        public string Allowed { get; set; }
        public string AllowedToRemove { get; set; }
        public string Available { get; set; }
        public string AvailableCivilWar { get; set; }
        public string OnAdd { get; set; }
    }
}
