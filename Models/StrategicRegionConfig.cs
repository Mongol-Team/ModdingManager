using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Models
{
    public class StrategicRegionConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public List<ProvinceConfig> Provinces { get; set; }
        public string LocKey { get; set; }
        [JsonIgnore]
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public Color Color { get; set; }
        public string FilePath { get; set; }
    }
}
