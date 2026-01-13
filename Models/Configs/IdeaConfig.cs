using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;

namespace Models.Configs
{
    public class IdeaConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public string PictureName { get; set;  }
        public ConfigLocalisation Localisation { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
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
