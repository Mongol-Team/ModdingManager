using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;
namespace Models
{
    public class TechTreeItemConfig : IConfig
    {
        public Identifier Id { get; set; } //+
        public ConfigLocalisation Localisation { get; set; }
        public Identifier OldId { get; set; }
        public int GridX { get; set; } //+
        public int GridY { get; set; }//+
        public bool? IsBig { get; set; }//+
        public bool? ShowEqIcon { get; set; }//+
        public string SpecialDescKey { get; set; }//+
        public int ModifCost { get; set; }
        public List<TechCategoryConfig> Categories { get; set; }//+
        public Dictionary<BuildingConfig, object> EnableBuildings { get; set; }//+
        public List<EquipmentConfig> EnableEquipments { get; set; }//+
        public List<SubUnitConfig> EnableUnits { get; set; }//+
        public int Cost { get; set; }//+
        public int StartYear { get; set; }//+
        public List<TechTreeItemConfig> ChildOf { get; set; }//+
        public List<TechTreeItemConfig> Mutal { get; set; }//+
        public string Allowed { get; set; } //raw trigger data //+
        public string AllowBranch { get; set; } //raw trigger data //+
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; } //+
        public string Effects { get; set; } //raw effect data //+
        public string AiWillDo { get; set; } //raw ai data //+
        public Dictionary<TechTreeItemConfig, int> Dependencies { get; set; } //+
        [JsonIgnore]
        public IGfx Gfx { get; set; }
    }
}
