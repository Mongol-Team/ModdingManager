using ModdingManagerClassLib.Extentions;
using ModdingManager.classes.views;
using ModdingManager.managers.@base;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Label = System.Windows.Controls.Label;
using Registry = ModdingManager.classes.utils.Registry;
namespace ModdingManager
{
    /// <summary>
    /// Логика взаимодействия для WPFCoutryCreator.xaml
    /// </summary>
    public partial class WPFCountryCreator : Window, ICountryView
    {
        private readonly CountryPresenter _presenter;

        public WPFCountryCreator()
        {
            InitializeComponent();
            _presenter = new CountryPresenter(this);
        }

        public string Tag
        {
            get => TagBox.Text;
            set => TagBox.Text = value;
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
        public Dictionary<string, int>? Technologies
        {
            get
            {
                var list = TechnologiesBox.GetLines();
                var result = new Dictionary<string, int>();
                foreach (var i in list)
                {
                    var parts = i.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int value))
                    {
                        result[parts[0]] = value;
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

        public string? RulingParty
        {
            get => RullingPartyBox.SelectedItem?.ToString() ?? string.Empty;
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

        public Dictionary<string, int>? PartyPopularities
        {
            get
            {
                var result = new Dictionary<string, int>();
                var lines = PartyPopularitiesBox.GetLines(); // предполагается, что StatesBox — это RichTextBox

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int popularity))
                    {
                        var ideo = Registry.Instance.GetIdeology(parts[0]);
                        result[ideo?.Id] = popularity != null ? popularity : 0;
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

        public List<string>? Ideas
        {
            get => StartingIdeasBox.GetLines();
            set
            {
                StartingIdeasBox.Document.Blocks.Clear();
                StartingIdeasBox.SetLines(value);
            }
        }

        public List<string>? Characters
        {
            get => RecruitingCharactersBox.GetLines();
            set
            {
                RecruitingCharactersBox.Document.Blocks.Clear();
                RecruitingCharactersBox.SetLines(value);
            }
        }

        public Dictionary<int, bool>? States
        {
            get
            {
                var result = new Dictionary<int, bool>();
                var lines = StatesBox.GetLines(); // Получаем строки из RichTextBox

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int id))
                    {
                        string boolPart = parts[1];
                        bool isCore = boolPart == "true" || boolPart == "1";
                        result[id] = isCore;
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


        public Dictionary<string, Bitmap>? CountryFlags
        {
            get
            {
                Dictionary<string, Bitmap> result = new Dictionary<string, Bitmap>();
                foreach (var wrap in CountryFlagsCanvas.Children)
                {
                    Canvas canvasWrap = wrap as Canvas;
                    var ideo = Registry.Instance.GetIdeology(canvasWrap.Name);
                    var image = (canvasWrap.Children.GetByName(canvasWrap.Name + "Img") as System.Windows.Controls.Image).Source;
                    result[ideo?.Id] = image.ToBitmap();
                }
                return result;
            }
            set
            {
                CountryFlagsCanvas.Children.Clear();

                foreach (var pair in value)
                {
                    string ideology = pair.Key;
                    ImageSource imageSource = pair.Value.ToImageSource();

                    // Создаём контейнер Canvas
                    Canvas wrap = new Canvas
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
            System.Windows.MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message)
        {
            System.Windows.MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ClearFlags()
        {
            CountryFlagsCanvas.Children.Clear();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> lines = new List<string>();
            foreach (var i in Registry.Instance.Ideologies)
            {
                // Создаём контейнер Canvas, аналогичный XAML
                Canvas wrap = new Canvas
                {
                    Width = 245,
                    Height = 72,
                    AllowDrop = true
                };
                wrap.Name = i.Id;
                // Создаём изображение
                var img = new System.Windows.Controls.Image
                {
                    Width = 82,
                    Height = 52
                };
                img.Name = i.Id + "Img";
                Canvas.SetLeft(img, 10);
                Canvas.SetTop(img, 10);

                img.Source = Properties.Resources.null_item_image.ToImageSource();
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

            CountryFileNameBox.ItemsSource = ModManager.LoadCountryFileNames();
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
                        BitmapImage original = new BitmapImage(new Uri(imagePath));
                        string name = current.Name + "Img";
                        int targetWidth = 82;
                        int targetHeight = 52;
                        var resized = original.ResizeToBitmap(targetWidth, targetHeight);
                        System.Windows.Controls.Image img = (System.Windows.Controls.Image)current.Children.GetByName(name);
                        img.Source = resized;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                    }
                }
            }
        }
        private void RullingPartyBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> strings = new List<string>();
            foreach (var ideo in Registry.Instance.Ideologies)
            {
                strings.Add(ideo.Id);
            }
            RullingPartyBox.ItemsSource = strings;
        }
    }
}