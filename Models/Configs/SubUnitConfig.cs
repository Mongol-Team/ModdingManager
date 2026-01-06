using Models.Attributes;
using Models.Enums;
using Models.Interfaces;
using Models.Types;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Models.Configs
{
    [ConfigCreator(ConfigCreatorType.GenericCreator)]
    public class SubUnitConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public IGfx White { get; set; }
        public IGfx Small { get; set; }
        public SubUnitConfig() { }
        public Identifier Id { get; set; }
        [JsonIgnore]
        public ConfigLocalisation Localisation { get; set; } = new ConfigLocalisation();
        public string EntitySprite { get; set; } //todo: entity sprite type
        public bool Active { get; set; }
        public int Priority { get; set; }
        public int AiPriority { get; set; }
        public UnitMapIconType MapIconCategory { get; set; } = new UnitMapIconType();
        public bool CanExfiltrateFromCoast { get; set; }
        public bool AffectsSpeed { get; set; } 
        public EquipmentConfig UseTransportSpeed { get; set; } = new EquipmentConfig();
        public SubUnitGroupConfig Group { get; set; } = new SubUnitGroupConfig();
        public List<IternalUnitType> Types { get; set; } = [];
        public List<SubUnitCategoryConfig> Chategories { get; set; } = [];
        public List<EquipmentConfig> Essential { get; set; } = [];
        public Dictionary<EquipmentConfig, int> Need { get; set; } = [];
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; } = [];
        public Dictionary<ProvinceTerrain, Dictionary<ModifierDefinitionConfig, object>> TerrainModifiers { get; set; } = [];
    }
}
