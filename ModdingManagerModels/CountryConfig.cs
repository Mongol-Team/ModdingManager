using System.Drawing;
using System.Text.Json.Serialization;

namespace ModdingManagerModels
{
    public class CountryConfig : IConfig
    {
        public string Tag { get; set; }
        public CountryLocalisationConfig? Localisation { get; set; }
        public int? Capital { get; set; }
        public string? CountryFileName { get; set; }
        public string? GraphicalCulture { get; set; }
        public Color? Color { get; set; }
        public Dictionary<string, int>? Technologies { get; set; }
        public int? Convoys { get; set; }
        public string? OOB { get; set; }
        public double? Stab { get; set; }
        public double? WarSup { get; set; }
        public int? ResearchSlots { get; set; }
        public string? RulingParty { get; set; }
        public DateOnly? LastElection { get; set; }
        public int? ElectionFrequency { get; set; }
        public bool? ElectionsAllowed { get; set; }
        [JsonIgnore]

        public List<StateConfig> States { get; set; }
        public Dictionary<string, Bitmap>? CountryFlags { get; set; }
        public Dictionary<string, int>? PartyPopularities { get; set; }
        public List<string>? Ideas { get; set; } = new List<string>();
        public List<string>? Characters { get; set; } = new List<string>();
        public Dictionary<int, bool>? StateCores { get; set; } = new Dictionary<int, bool>();
    }

}
