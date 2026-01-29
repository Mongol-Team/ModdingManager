using Application;
using Application.Extensions;
using Application.Extentions;
using Application.Settings;
using Data.Properties;
using Models.Configs;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ViewInterfaces;
using ViewPresenters;
using Label = System.Windows.Controls.Label;
namespace View
{
    /// <summary>
    /// Логика взаимодействия для WPFCoutryCreator.xaml
    /// </summary>
    public partial class WPFCountryCreator : BaseWindow, ICountryView
    {
        private readonly CountryPresenter _presenter;

        public WPFCountryCreator()
        {
            InitializeComponent();
            _presenter = new CountryPresenter(this);
        }
        public ConfigLocalisation Localisation
        {
            get;
            set;
        }
        public Identifier Tag
        {
            get => new(TagBox.Text);
            set => TagBox.Text = value.ToString();
        }

        public string? Name
        {
            get => CountryDescNameBox.Text;
            set => CountryDescNameBox.Text = value;
        }
        public string? CountryFileName
        {
            get => CountryFileNameBox.SelectedItem.ToString();
            set => CountryFileNameBox.SelectedItem = value;
        }
        public int? Capital
        {
            get => int.TryParse(CapitalIdBox.Text, out int result) ? result : 0;
            set => CapitalIdBox.Text = value.ToString();
        }
        public System.Windows.Media.Color? Color
        {
            get => CountryColorPicker.SelectedColor;
            set => CountryColorPicker.SelectedColor = value ?? System.Windows.Media.Color.FromRgb(0, 0, 2);
        }


        public string? GraphicalCulture
        {
            get => GraficalCultureBox.Text;
            set => GraficalCultureBox.Text = value;
        }
        public Dictionary<TechTreeItemConfig, int>? Technologies
        {
            get
            {
                var list = TechnologiesBox.GetLines();
                var result = new Dictionary<TechTreeItemConfig, int>();
                foreach (var i in list)
                {
                    var parts = i.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int value))
                    {
                        result[ModDataStorage.Mod.TechTreeLedgers.GetTreeItem(parts[0])] = value;
                    }

                }
                return result;
            }
            set
            {
                var result = new List<string>();
                foreach (var i in value)
                {
                    result.Add($"{i.Value}:{i.Key}");
                }
                TechnologiesBox.SetLines(result);
            }
        }
        public int? Convoys
        {
            get => int.TryParse(ConvoysBox.Text, out int result) ? result : 0;
            set => ConvoysBox.Text = value.ToString();
        }
        public string? OOB
        {
            get => StartOOBox.Text;
            set => StartOOBox.Text = value.ToString();
        }

        public double? Stab
        {
            get => double.TryParse(StabBox.Text, out double result) ? result / 100 : 0;
            set => StabBox.Text = (value * 100).ToString();
        }

        public double? WarSup
        {
            get => double.TryParse(WarSupBox.Text, out double result) ? result / 100 : 0;
            set => WarSupBox.Text = (value * 100).ToString();
        }

        public int? ResearchSlots
        {
            get => int.TryParse(ResearchSlotsBox.Text, out int result) ? result : 0;
            set => ResearchSlotsBox.Text = value.ToString();
        }

        public IdeologyConfig? RulingParty
        {
            get => (IdeologyConfig)RullingPartyBox.SelectedItem ?? null;
            set => RullingPartyBox.SelectedItem = value;
        }

        public DateOnly? LastElection
        {
            get => DateOnly.FromDateTime(LastElectionsDatePicker.SelectedDate ?? DateTime.MinValue);
            set => LastElectionsDatePicker.SelectedDate = value?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue;
        }

        public int? ElectionFrequency
        {
            get => int.TryParse(ElectionsDelayBox.Text, out int result) ? result : 0;
            set => ElectionsDelayBox.Text = value.ToString();
        }

        public bool? ElectionsAllowed
        {
            get => HaveElectionsBox.IsChecked ?? false;
            set => HaveElectionsBox.IsChecked = value;
        }

        public Dictionary<IdeologyConfig, int>? PartyPopularities
        {
            get
            {
                var result = new Dictionary<IdeologyConfig, int>();
                var lines = PartyPopularitiesBox.GetLines(); // предполагается, что StatesBox — это RichTextBox

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int popularity))
                    {
                        var ideo = ModDataStorage.Mod.GetIdeology(parts[0]);
                        result[ideo] = popularity != null ? popularity : 0;
                    }
                }
                return result;
            }
            set
            {
                var lines = value.Select(kvp => $"{kvp.Key}:{kvp.Value}");
                PartyPopularitiesBox.SetLines(lines.ToList());
            }
        }

        public List<IdeaConfig>? Ideas
        {
            get
            {
                List<IdeaConfig> result = new();
                foreach (string ideo in StartingIdeasBox.GetLines())
                {
                    IdeaConfig cfg = ModDataStorage.Mod.Ideas.FindById(ideo);
                    result.Add(cfg);
                }
                return result;
            }
            set
            {
                StartingIdeasBox.Document.Blocks.Clear();
                foreach (IdeaConfig cfg in value)
                {
                    StartingIdeasBox.AddLine(cfg.Id.ToString());
                }
            }
        }

        public List<CountryCharacterConfig>? Characters
        {
            get
            {
                List<CountryCharacterConfig> result = new();
                foreach (var line in RecruitingCharactersBox.GetLines())
                {
                    CountryCharacterConfig cfg = ModDataStorage.Mod.Characters.FindById(line);
                    result.Add(cfg);
                }
                return result;
            }
            set
            {
                RecruitingCharactersBox.Document.Blocks.Clear();
                foreach (CountryCharacterConfig cfg in value)
                {
                    RecruitingCharactersBox.AddLine(cfg.Id.ToString());
                }
            }
        }

        public Dictionary<StateConfig, bool>? States
        {
            get
            {
                var result = new Dictionary<StateConfig, bool>();
                var lines = StatesBox.GetLines(); // Получаем строки из RichTextBox

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int id))
                    {
                        string boolPart = parts[1];
                        bool isCore = boolPart is "true" or "1";
                        result[ModDataStorage.Mod.GetState(id)] = isCore;
                    }
                }

                return result;
            }
            set
            {
                var lines = value.Select(kvp =>
                    $"{kvp.Key}:{(kvp.Value ? "1" : "0")}");
                StatesBox.SetLines(lines.ToList());
            }
        }


        public Dictionary<IdeologyConfig, Bitmap>? CountryFlags
        {
            get
            {
                Dictionary<IdeologyConfig, Bitmap> result = new();
                foreach (var wrap in CountryFlagsCanvas.Children)
                {
                    Canvas canvasWrap = wrap as Canvas;
                    IdeologyConfig ideo = ModDataStorage.Mod.GetIdeology(canvasWrap.Name);
                    var image = (canvasWrap.Children.GetByName(canvasWrap.Name + "Img") as System.Windows.Controls.Image).Source;
                    result[ideo] = image.ToBitmap();
                }
                return result;
            }
            set
            {
                CountryFlagsCanvas.Children.Clear();

                foreach (var pair in value)
                {
                    string ideology = pair.Key.Id.ToString();
                    ImageSource imageSource = pair.Value.ToImageSource();

                    // Создаём контейнер Canvas
                    Canvas wrap = new()
                    {
                        Width = 245,
                        Height = 72,
                        AllowDrop = true,
                        Name = ideology
                    };

                    // Создаём изображение
                    var img = new System.Windows.Controls.Image
                    {
                        Width = 82,
                        Height = 52,
                        Name = ideology + "Img",
                        Source = imageSource
                    };
                    Canvas.SetLeft(img, 10);
                    Canvas.SetTop(img, 10);

                    // Создаём подпись
                    var name = new Label
                    {
                        Content = ideology
                    };
                    Canvas.SetLeft(name, 100);
                    Canvas.SetTop(name, 20);

                    // Подписка на Drop
                    wrap.Drop += new System.Windows.DragEventHandler(TechIconCanvas_Drop);

                    // Добавление в Canvas
                    wrap.Children.Add(img);
                    wrap.Children.Add(name);

                    // Добавление в StackPanel
                    CountryFlagsCanvas.Children.Add(wrap);
                }
            }

        }
        public event RoutedEventHandler ApplyClicked;
        public event RoutedEventHandler LoadConfigClicked;
        public event RoutedEventHandler SaveConfigClicked;

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyClicked?.Invoke(this, e);
        }

        private void LoadConfigButton_Click(object sender, RoutedEventArgs e)
        {
            LoadConfigClicked?.Invoke(this, e);
        }

        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfigClicked?.Invoke(this, e);
        }

        // Методы
        public void ShowMessage(string message)
        {
            ShowSuccess(message, NotificationCorner.TopRight);
        }

        public void ShowError(string message)
        {
            ShowError(message, NotificationCorner.TopRight);
        }

        public void ClearFlags()
        {
            CountryFlagsCanvas.Children.Clear();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> lines = new();
            foreach (var i in ModDataStorage.Mod.Ideologies)
            {
                // Создаём контейнер Canvas, аналогичный XAML
                Canvas wrap = new()
                {
                    Width = 245,
                    Height = 72,
                    AllowDrop = true
                };
                wrap.Name = i.Id.ToString();
                // Создаём изображение
                var img = new System.Windows.Controls.Image
                {
                    Width = 82,
                    Height = 52
                };
                img.Name = i.Id + "Img";
                Canvas.SetLeft(img, 10);
                Canvas.SetTop(img, 10);

                img.Source = Data.Properties.Resources.null_item_image.ToImageSource();
                // Создаём подпись
                var name = new Label
                {
                    Content = i.Id
                };
                Canvas.SetLeft(name, 100);
                Canvas.SetTop(name, 20);
                wrap.Drop += new System.Windows.DragEventHandler(TechIconCanvas_Drop);
                wrap.Children.Add(img);
                wrap.Children.Add(name);
                CountryFlagsCanvas.Children.Add(wrap);
                lines.Add(i.Id + ":");
            }
            PartyPopularitiesBox.SetLines(lines);

            CountryFileNameBox.ItemsSource = LoadCountryFileNames();
        }
        public List<string> LoadCountryFileNames()
        {
            string countriesDir = Path.Combine(ModManagerSettings.ModDirectory, "common", "country_tags");

            if (!System.IO.Directory.Exists(countriesDir))
            {
                ShowError("Папка 'countries' не найдена!", NotificationCorner.TopRight);
                return null;
            }

            var FileFullPaths = System.IO.Directory.GetFiles(countriesDir);
            var fileNamesLines = FileFullPaths.Select(path => Path.GetFileName(path)).ToList();
            return fileNamesLines;

        }

        private void TechIconCanvas_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string imagePath = files[0];
                    var current = sender as Canvas;

                    try
                    {
                        BitmapImage original = new(new Uri(imagePath));
                        string name = current.Name + "Img";
                        int targetWidth = 82;
                        int targetHeight = 52;
                        var resized = original.ResizeToBitmap(targetWidth, targetHeight);
                        System.Windows.Controls.Image img = (System.Windows.Controls.Image)current.Children.GetByName(name);
                        img.Source = resized;
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Ошибка при загрузке изображения: {ex.Message}", NotificationCorner.TopRight);
                    }
                }
            }
        }
        private void RullingPartyBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> strings = new();
            foreach (var ideo in ModDataStorage.Mod.Ideologies)
            {
                strings.Add(ideo.Id.ToString());
            }
            RullingPartyBox.ItemsSource = strings;
        }
    }
}