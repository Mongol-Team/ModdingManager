using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Models
{
    public class StateConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        [JsonIgnore]
        public List<ProvinceConfig> Provinces { get; set; }
        [JsonIgnore]
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public Color Color { get; set; }
        [JsonIgnore]
        public string FilePath { get; set; }
        public string LocalizationKey { get; set; } = string.Empty;
        public StateCathegoryConfig Cathegory { get; set; }
        public int? Manpower { get; set; }
        public double? LocalSupply { get; set; }
        public Dictionary<BuildingConfig, int> Buildings { get; set; }

    }
}
