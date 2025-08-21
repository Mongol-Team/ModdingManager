using global::ModdingManager.classes.controls;
using global::ModdingManager.managers.@base;
using global::ModdingManager.View;
using ModdingManager.classes.utils;
using ModdingManagerModels;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace ModdingManager.Presenter
{
    public sealed class MainWindowPresenter
    {
        private readonly MainWindow _view;
        private bool _isLoaded = false;

        public MainWindowPresenter(MainWindow view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            WireUp();
        }

        private void WireUp()
        {
            // аналог MainForm_Load
            _view.Loaded += OnLoaded;

            // клики 1:1 с WinForms
            _view.LocConvertButton.Click += LocConvertButton_Click;
            _view.StateManagerBtn.Click += Statebutton_Click;
            _view.LocTechButton.Click += LocTechButton_Click;
            _view.LocIdeaButton.Click += LocIdeaButton_Click;
            _view.LocStateButton.Click += LocStateButton_Click;
            _view.TechButton.Click += TechButton_Click;
            _view.FlagCrtButton.Click += FlagCrtButton_Click;
            _view.CountryCrtButton.Click += CountryCrtButton_Click;
            _view.CharCreator.Click += CharCreator_Click;
            _view.IdeaCreatorButton.Click += IdeaCreatorButton_Click;
            _view.ModButton.Click += ModButton_Click;
            _view.TemplateCreatorBtn.Click += TestButton_Click;
            _view.SuperEventCreatorButton.Click += SuperEventCreatorButton_Click;
            _view.DebugButton.Click += DebugButton_Click;
            _view.IdeologyCreatorBtn.Click += IdeologyCreatorBtn_Click;
            _view.button1.Click += button1_Click_1;

            // сохранение dir.json при изменении (как в DirBox_TextChanged)
            _view.DirBox.TextChanged += DirBoxes_TextChanged;
            _view.GameDirBox.TextChanged += DirBoxes_TextChanged;
        }

        // ====== перенос обработчиков из MainForm ======

        private void LocConvertButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var treeLoc = new TreeLoc();
                treeLoc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void Statebutton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var mapWindow = new MapWorkerWindow();
                mapWindow.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void LocTechButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
            }
            else
            {
                MessageBox.Show("введите дирку", "директорию забыл", MessageBoxButton.OK);
            }
        }

        private void LocIdeaButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                ModManager.GameDirectory = _view.GameDirBox.Text;
                ModManager.ModDirectory = _view.DirBox.Text;
                var ideaLoc = new IdeaLoc();
                ideaLoc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void LocStateButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                ModManager.GameDirectory = _view.GameDirBox.Text;
                ModManager.ModDirectory = _view.DirBox.Text;
                var stateLoc = new StateLoc();
                stateLoc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void TechButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var fc = new TechTreeCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void FlagCrtButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var fc = new FlagCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void CountryCrtButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var fc = new WPFCountryCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void CharCreator_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var fc = new CharacterCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void IdeaCreatorButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var fc = new IdeaCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void ModButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var fc = new ModifierCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void TestButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var fc = new TemplateCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void SuperEventCreatorButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var fc = new SupereventCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void DebugButton_Click(object? sender, RoutedEventArgs e)
        {
            ModManager.IsDebugRuning = true;
            var debugWindow = new DebugWindow();

            // подключение к вашему Debugger
            Debugger.Instance.DebugOutputControl = debugWindow.DebugBox;
            Debugger.Instance.AttachToWindow(_view);
            Debugger.Instance.LogMessage("Режим отладки активирован");

            try
            {
                throw new InvalidOperationException("Тестовое исключение для отладки");
            }
            catch (Exception ex)
            {
                Debugger.Instance.LogMessage($"Поймано исключение: {ex.Message}");
            }

            debugWindow.Show();
        }

        private void IdeologyCreatorBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                var fc = new IdeologyCreator();
                fc.Show();
            }
            else
            {
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void button1_Click_1(object? sender, RoutedEventArgs e)
        {
            try
            {
                // тест блока ошибок
                var errorPanel = new ErrorPanel();
                errorPanel.AddError(ErrorType.Warning, "This feature is not implemented yet.", "MapHealerWindow");
                errorPanel.AddError(ErrorType.Critical, "Пенис 24 вап ПРОСТО Я вапвап вап.",
                    @"C:\Users\Acer\Documents\Paradox Interactive\Hearts of Iron IV\mod\SME\history\states\1-France.txt");

                var dialog = new Window
                {
                    Title = "Ошибки",
                    Width = 475,
                    Height = 338,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = errorPanel,
                    Owner = _view
                };
                dialog.Show();
            }
            catch (Exception ex)
            {
                Debugger.Instance.LogMessage($"Поймано исключение в button1_Click: {ex}");
            }
        }

        // ====== загрузка и автосохранение путей ======

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            string relativePath = System.IO.Path.Combine("..", "..", "..", "data", "dir.json");
            string fullPath = System.IO.Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                string json = File.ReadAllText(fullPath);
                var path = JsonSerializer.Deserialize<PathConfig>(json);

                _view.GameDirBox.Text = path?.GamePath ?? string.Empty;
                _view.DirBox.Text = path?.ModPath ?? string.Empty;

                ModManager.ModDirectory = _view.DirBox.Text;
                ModManager.GameDirectory = _view.GameDirBox.Text;

                Registry.LoadInstance();

                Debugger.Instance.LogMessage(System.IO.Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace")
                                             + Directory.Exists(System.IO.Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace")));
                Debugger.Instance.LogMessage(System.IO.Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage)
                                             + Directory.Exists(System.IO.Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage)));
                Debugger.Instance.LogMessage(System.IO.Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage)
                                             + Directory.Exists(System.IO.Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage)));
                Debugger.Instance.LogMessage(System.IO.Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace")
                                             + Directory.Exists(System.IO.Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace")));

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debugger.Instance.LogMessage($"[MAIN WPF] On load exception: {ex.Message}{ex.StackTrace}");
            }
        }

        private void DirBoxes_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_isLoaded)
            {
                if (!(string.IsNullOrEmpty(_view.GameDirBox.Text) || string.IsNullOrEmpty(_view.DirBox.Text)))
                {
                    string relativePath = System.IO.Path.Combine("..", "..", "..", "data", "dir.json");
                    string fullPath = System.IO.Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);

                    var config = new PathConfig
                    {
                        GamePath = _view.GameDirBox.Text,
                        ModPath = _view.DirBox.Text
                    };

                    var json = JsonSerializer.Serialize(config);
                    File.WriteAllText(fullPath, json);
                }
                else
                {
                    MessageBox.Show("Укажите обе директории.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ====== утилиты ======

        // сохраняю исходную логику: проверка была через ИЛИ (||)
        private bool HasAnyDir()
            => !string.IsNullOrEmpty(_view.DirBox.Text) || !string.IsNullOrEmpty(_view.GameDirBox.Text);

        private void UpdateModManager()
        {
            ModManager.ModDirectory = _view.DirBox.Text;
            ModManager.GameDirectory = _view.GameDirBox.Text;
        }
    }
}

