using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Models.Configs
{
    public class CountryConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public int? Capital { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public string? CountryFileName { get; set; }
        public string? GraphicalCulture { get; set; }
        public Color? Color { get; set; }
        public Dictionary<TechTreeItemConfig, int>? Technologies { get; set; }
        public int? Convoys { get; set; }
        public string? OOB { get; set; }
        public double? Stab { get; set; }
        public double? WarSup { get; set; }
        public int? ResearchSlots { get; set; }
        public IdeologyConfig? RulingParty { get; set; }
        public DateOnly? LastElection { get; set; }
        public int? ElectionFrequency { get; set; }
        public bool? ElectionsAllowed { get; set; }
        [JsonIgnore]
        public List<StateConfig> States { get; set; }
        public Dictionary<IdeologyConfig, Bitmap>? CountryFlags { get; set; }
        public Dictionary<IdeologyConfig, int>? PartyPopularities { get; set; }
        public List<IdeaConfig>? Ideas { get; set; } = new List<IdeaConfig>();
        public List<CountryCharacterConfig>? Characters { get; set; } = new List<CountryCharacterConfig>();
        public Dictionary<StateConfig, bool>? StateCores { get; set; } = new Dictionary<StateConfig, bool>();
    }

}
