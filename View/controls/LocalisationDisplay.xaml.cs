using Application;
using Application.Settings;
using Models;
using Models.Types.LocalizationData;
using System.Text;
using UserControl = System.Windows.Controls.UserControl;

namespace ViewControls
{
    /// <summary>
    /// Логика взаимодействия для LocalisationDisplay.xaml
    /// </summary>
    public partial class LocalisationDisplay : UserControl
    {
        public LocalisationDisplay()
        {
            InitializeComponent();
        }
        private char Delimiter = ':';
        private ConfigLocalisation _localisation;
        public ConfigLocalisation Localisation
        {
            get => _localisation;
            set
            {
                if (value == null)
                    return;
                _localisation = value;
                if (value.Data == null || value.Data.Count == 0)
                    BuildDefaultLocData();

                var sb = new StringBuilder();
                foreach (var line in _localisation.Data)
                    sb.AppendLine($"{line.Key}{Delimiter} {line.Value}");

                LocalisationTextBlock.Text = sb.ToString();
            }
        }

        private void BuildDefaultLocData()
        {
            _localisation.Data = new Dictionary<string, string>();
            _localisation.Data.Add($"l_{ModdingManagerSettings.Instance.CurrentLanguage}:", "");
            switch (_localisation.Source)
            {
                case CountryConfig country:
                    _localisation.Data.Add($"{country.Id}_ADJ:", "");
                    _localisation.Data.Add($"{country.Id}:", "");
                    foreach (IdeologyConfig cfg in ModDataStorage.Mod.Ideologies)
                    {
                        _localisation.Data.Add($"{country.Id}_{cfg.Id}_DEF:", "");
                        _localisation.Data.Add($"{country.Id}_{cfg.Id}:", "");
                    }
                    break;

                case StateConfig state:
                    _localisation.Data.Add($"{state.LocalizationKey}:", "");
                    break;
                case IdeologyConfig ideology:
                    _localisation.Data.Add($"{ideology.Id}:", "");
                    _localisation.Data.Add($"{ideology.Id}_desc:", "");
                    _localisation.Data.Add($"{ideology.Id}_noun:", "");
                    break;
                case ProvinceConfig province:
                    _localisation.Data.Add($"VICTORY_POINTS_{province.Id}:", "");
                    break;
                case SubUnitConfig regim:
                    _localisation.Data.Add($"{regim.Id}:", "");
                    break;
                case IConfig genericConfig:
                    _localisation.Data.Add($"{genericConfig.Id}:", "");
                    break;
                default:
                    break;
            }

        }
    }
}
