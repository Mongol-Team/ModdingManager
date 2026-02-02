using Models.Configs;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;
using System.Windows;
using System.Windows.Media;

namespace ViewInterfaces
{
    public interface ICountryView
    {
        // Свойства для доступа к элементам управления
        public Identifier? Tag { get; set; }
        public string? Name { get; set; }
        public int? Capital { get; set; }
        public string? GraphicalCulture { get; set; }
        public System.Windows.Media.Color? Color { get; set; }
        public Dictionary<TechTreeItemConfig, int>? Technologies { get; set; }
        public int? Convoys { get; set; }
        public string? OOB { get; set; }
        public double? Stab { get; set; }
        public double? WarSup { get; set; }
        public string? CountryFileName { get; set; }
        public int? ResearchSlots { get; set; }
        public IdeologyConfig? RulingParty { get; set; }
        public DateOnly? LastElection { get; set; }
        public int? ElectionFrequency { get; set; }
        public bool? ElectionsAllowed { get; set; }
        public Dictionary<IdeologyConfig, Bitmap>? CountryFlags { get; set; }
        public Dictionary<IdeologyConfig, int>? PartyPopularities { get; set; }
        public List<IdeaConfig>? Ideas { get; set; }
        public List<CountryCharacterConfig>? Characters { get; set; }
        public Dictionary<StateConfig, bool>? States { get; set; }
        public ConfigLocalisation? Localisation { get; set; }
        // События
        event RoutedEventHandler ApplyClicked;
        event RoutedEventHandler LoadConfigClicked;
        event RoutedEventHandler SaveConfigClicked;

        // Методы
        void ShowMessage(string message);
        void ShowError(string message);
        void ClearFlags();

    }
}
