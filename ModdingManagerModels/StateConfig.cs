using System.Text.Json.Serialization;

namespace ModdingManagerModels
{
    public class StateConfig : IModel
    {
        [JsonIgnore]
        public List<ProvinceConfig> Provinces { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int? Id { get; set; }
        public System.Windows.Media.Color Color { get; set; }
        [JsonIgnore]
        public string FilePath { get; set; }
        public string LocalizationKey { get; set; } = string.Empty;
        public string Cathegory { get; set; }
        public int? Manpower { get; set; }
        public double? LocalSupply { get; set; }
        public List<Var> Buildings { get; set; } = new List<Var>();

    }
}
