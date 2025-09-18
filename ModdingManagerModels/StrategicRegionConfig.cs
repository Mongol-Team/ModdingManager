using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;

namespace ModdingManagerModels
{
    public class StrategicRegionConfig : IConfig
    {
        public List<ProvinceConfig> Provinces { get; set; }
        public string LocKey { get; set; }
        [JsonIgnore]
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public Color Color { get; set; }
        public string FilePath { get; set; }
    }
}
