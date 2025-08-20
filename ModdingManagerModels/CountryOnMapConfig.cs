using System.Text.Json.Serialization;

namespace ModdingManagerModels
{
    public class CountryOnMapConfig : IModel
    {
        public List<StateConfig> States { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public string Tag { get; set; }
        public System.Windows.Media.Color Color { get; set; }
    }
}
