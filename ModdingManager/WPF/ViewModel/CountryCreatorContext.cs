using ModdingManager.classes.utils;
using ModdingManager.managers.@base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace ModdingManager.WPF.ViewModel
{



    public class CountryCreatorContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public CountryCreatorContext()
        {
            // Загрузка идеологий при инициализации
            foreach (var ideology in Registry.Instance.Ideologies)
            {
                RullingPartyItems.Add(ideology.Id);
            }
        }

        #region Properties

        private string _countryTag = "";
        public string CountryTag
        {
            get => _countryTag;
            set { _countryTag = value; NotifyPropertyChanged(nameof(CountryTag)); }
        }

        private string _countryName = "";
        public string CountryName
        {
            get => _countryName;
            set { _countryName = value; NotifyPropertyChanged(nameof(CountryName)); }
        }

        private string _capital = "";
        public string Capital
        {
            get => _capital;
            set { _capital = value; NotifyPropertyChanged(nameof(Capital)); }
        }

        private string _graphicalCulture = "";
        public string GraphicalCulture
        {
            get => _graphicalCulture;
            set { _graphicalCulture = value; NotifyPropertyChanged(nameof(GraphicalCulture)); }
        }

        private System.Windows.Media.Color _countryColor = Colors.Magenta;
        public System.Windows.Media.Color CountryColor
        {
            get => _countryColor;
            set { _countryColor = value; NotifyPropertyChanged(nameof(CountryColor)); }
        }

        private string _selectedRullingParty = "";
        public string SelectedRullingParty
        {
            get => _selectedRullingParty;
            set { _selectedRullingParty = value; NotifyPropertyChanged(nameof(SelectedRullingParty)); }
        }

        public ObservableCollection<string> RullingPartyItems { get; } = new ObservableCollection<string>();

        // Другие свойства (TechBox, StartOOBBox, и т.д.) по аналогии...

        #endregion

        #region Commands

        public ICommand ApplyCommand => new BaseCommand(_ =>
        {
            CreateCountryHistoryFile();
            UpdateStateOwnership();
            CreateCommonCountriesFile();
            AddCountryTag();
            CreateLocalizationFiles(ModManager.Directory, CountryTag);
        });

        public ICommand PickColorCommand => new BaseCommand(_ =>
        {
            // Здесь можно реализовать диалог выбора цвета
            // Например, используя System.Windows.Forms.ColorDialog
        });

        public ICommand LoadConfigCommand => new BaseCommand(_ =>
        {
            WPFConfigManager.LoadConfigWrapper(this);
        });

        public ICommand SaveConfigCommand => new BaseCommand(_ =>
        {
            WPFConfigManager.SaveConfigWrapper(this);
        });

        #endregion

        #region Logic

        private void CreateLocalizationFiles(string modPath, string countryTag)
        {
            try
            {
                string ruLocPath = Path.Combine(modPath, "localisation", "russian");
                string enLocPath = Path.Combine(modPath, "localisation", "english");
                Directory.CreateDirectory(ruLocPath);
                Directory.CreateDirectory(enLocPath);

                string ruContent = GenerateLocalizationContent(countryTag, "l_russian");
                string enContent = GenerateLocalizationContent(countryTag, "l_english");

                string ruFilePath = Path.Combine(ruLocPath, $"{countryTag}_history_l_russian.yml");
                string enFilePath = Path.Combine(enLocPath, $"{countryTag}_history_l_english.yml");

                File.WriteAllText(ruFilePath, ruContent, new UTF8Encoding(true));
                File.WriteAllText(enFilePath, enContent, new UTF8Encoding(true));
            }
            catch (Exception ex)
            {
                Debugger.Instance.LogMessage($"Ошибка создания файлов локализации: {ex.Message}");
            }
        }

        private static string GenerateLocalizationContent(string tag, string languageKey)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{languageKey}:");
            sb.AppendLine($" {tag}_fascism: \"\"");
            // ... остальная генерация локализации
            return sb.ToString();
        }

        private void AddCountryTag()
        {
            if (CountryTag.Length != 3)
            {
                Debugger.Instance.LogMessage("Тег страны должен состоять из 3 символов!");
                return;
            }

            string tagsDir = Path.Combine(ModManager.Directory, "common", "country_tags");
            string countryFileName = $"{CountryTag} - {CountryName}.txt";

            try
            {
                if (!Directory.Exists(tagsDir))
                {
                    Directory.CreateDirectory(tagsDir);
                }

                var tagFiles = Directory.GetFiles(tagsDir, "*.txt");
                string newEntry = $"{CountryTag} = \"countries/{countryFileName}\"";

                if (tagFiles.Length == 0)
                {
                    string newTagFile = Path.Combine(tagsDir, "00_countries.txt");
                    File.WriteAllText(newTagFile, newEntry, new UTF8Encoding(false));
                }
                else
                {
                    string tagFile = tagFiles[0];
                    string content = File.ReadAllText(tagFile);
                    if (content.Contains($"{CountryTag} ="))
                    {
                        Debugger.Instance.LogMessage($"Тег {CountryTag} уже существует!");
                        return;
                    }

                    File.AppendAllText(tagFile, $"\n{newEntry}");
                }
            }
            catch (Exception ex)
            {
                Debugger.Instance.LogMessage($"Ошибка при добавлении тега страны: {ex.Message}");
            }
        }

        // Остальные методы (UpdateStateOwnership, CreateCountryHistoryFile и т.д.) по аналогии...

        #endregion

        #region Drag & Drop Handlers (для флагов)

        private ImageSource _neutralFlagImage;
        public ImageSource NeutralFlagImage
        {
            get => _neutralFlagImage;
            set { _neutralFlagImage = value; NotifyPropertyChanged(nameof(NeutralFlagImage)); }
        }

        // Аналогично для других флагов (FascismFlagImage, CommunismFlagImage, DemocraticFlagImage)

        public void HandleFlagDrop(string filePath, string flagType)
        {
            if (Path.GetExtension(filePath).ToLower() == ".jpg" || Path.GetExtension(filePath).ToLower() == ".png")
            {
                try
                {
                    // Здесь преобразование System.Drawing.Image в ImageSource
                    // и установка соответствующего свойства в зависимости от flagType
                }
                catch (Exception ex)
                {
                    Debugger.Instance.LogMessage($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
            else
            {
                Debugger.Instance.LogMessage("Пожалуйста, используйте изображения в формате JPG или PNG.");
            }
        }

        #endregion
    }
}

