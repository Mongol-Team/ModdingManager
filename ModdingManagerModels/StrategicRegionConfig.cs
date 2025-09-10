using System.Drawing;
using System.Text.Json.Serialization;

namespace ModdingManagerModels
{
    public class StrategicRegionConfig : IConfig
    {
        public List<ProvinceConfig> Provinces { get; set; }
        public string LocKey { get; set; }
        [JsonIgnore]
        public int Id { get; set; }
        public Color Color { get; set; }
        public string FilePath { get; set; }
    }
}
