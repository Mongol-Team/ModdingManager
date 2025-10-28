using ModdingManager.managers.@base;
using ModdingManagerClassLib;
using ModdingManagerClassLib.Settings;
using ModdingManagerModels;
using ModdingManagerModels.Types.LocalizationData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace ModdingManager.Controls
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
            _localisation.Data.Add($"l_{ModManagerSettings.Instance.CurrentLanguage}:", "");
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
                case RegimentConfig regim:
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
