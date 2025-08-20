using System.Text.Json.Serialization;

namespace ModdingManagerModels
{
    public class StrategicRegionConfig : IModel
    {
        public List<ProvinceConfig> Provinces { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int Id { get; set; }
        public System.Windows.Media.Color Color { get; set; }
        public string FilePath { get; set; }
    }
}
