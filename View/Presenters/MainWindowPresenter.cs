using Application;
using Application.Debugging;
using Application.Settings;
using View.Utils;
using global::View;
using System.Windows;
using System.Windows.Controls;
using ViewControls;
using MessageBox = System.Windows.MessageBox;

namespace ViewPresenters
{
    public sealed class MainWindowPresenter
    {
        private readonly MainWindow _view;
        private bool _isLoaded = false;
        private bool _isApplicationInitialized = false;

        public MainWindowPresenter(MainWindow view)
        {
            ModdingManagerSettings.Load();
            ModDataStorage.ComposeMod();
            _view = view ?? throw new ArgumentNullException(nameof(view));
            WireUp();
            _view.Loaded += OnWindowLoaded;
        }

        private void WireUp()
        {
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
        }

        private async void OnWindowLoaded(object? sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            await InitializeApplicationAsync();
        }

        private async System.Threading.Tasks.Task InitializeApplicationAsync()
        {
            var loadingWindow = new LoadingWindow
            {
                Owner = _view,
                Message = UILocalization.GetString("Info.LoadingSettings")
            };
            loadingWindow.SetProgressBounds(0, 2);
            loadingWindow.Show();

            await System.Threading.Tasks.Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    loadingWindow.Message = UILocalization.GetString("Info.LoadingSettings");
                });
                ModManagerSettingsLoader.Load();
            });

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                loadingWindow.Progress = 1;
                loadingWindow.Message = UILocalization.GetString("Info.InitializingData");
                LoadConfig();
            });

            await System.Threading.Tasks.Task.Run(() =>
            {
                ModDataStorage.ComposeMod();
            });

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                loadingWindow.Progress = 2;
                loadingWindow.EndLoading();
                _isApplicationInitialized = true;
            });
        }

        private void LoadConfig()
        {
            _view.DirBox.Text = ModdingManagerSettings.Instance.ModDirectory;
            _view.GameDirBox.Text = ModdingManagerSettings.Instance.GameDirectory;
        }
        private void LocConvertButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                PlaceholderWindow.ShowPlaceholder("Функция локализации по дереву фокусов временно недоступна", _view);
            }
            else
            {
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
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
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
            }
        }

        private void LocTechButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                PlaceholderWindow.ShowPlaceholder("Функция локализации технологий пока не готова", _view);
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
                PlaceholderWindow.ShowPlaceholder("Функция локализации для идей временно недоступна", _view);
            }
            else
            {
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
            }
        }

        private void LocStateButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                PlaceholderWindow.ShowPlaceholder("Функция локализации по стейтам временно недоступна", _view);
            }
            else
            {
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
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
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
            }
        }

        private void FlagCrtButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                PlaceholderWindow.ShowPlaceholder("Создатель флагов временно недоступен", _view);
            }
            else
            {
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
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
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
            }
        }

        private void CharCreator_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                PlaceholderWindow.ShowPlaceholder("Создатель персонажей временно недоступен", _view);
            }
            else
            {
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
            }
        }

        private void IdeaCreatorButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                PlaceholderWindow.ShowPlaceholder("Создатель идей временно недоступен", _view);
            }
            else
            {
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
            }
        }

        private void ModButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                PlaceholderWindow.ShowPlaceholder("Создатель модификаторов временно недоступен", _view);
            }
            else
            {
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
            }
        }

        private void TestButton_Click(object? sender, RoutedEventArgs e)
        {
            if (HasAnyDir())
            {
                UpdateModManager();
                PlaceholderWindow.ShowPlaceholder("Создатель шаблонов временно недоступен", _view);
            }
            else
            {
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
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
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
            }
        }

        private void DebugButton_Click(object? sender, RoutedEventArgs e)
        {
            ConsoleHelper.ShowConsole();
            Logger.AddDbgLog("Режим отладки активирован");
            Logger.FlushBuffer();
            try
            {
                throw new InvalidOperationException("Тестовое исключение для отладки");
            }
            catch (Exception ex)
            {
                Logger.AddDbgLog($"Поймано исключение: {ex.Message}");
            }
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
                MessageBox.Show(UILocalization.GetString("Error.EnterBothDirectories"), UILocalization.GetString("Error.Error"), MessageBoxButton.OK);
            }
        }

        private bool HasAnyDir()
        {
            return !string.IsNullOrEmpty(ModManagerSettings.ModDirectory) || !string.IsNullOrEmpty(ModManagerSettings.GameDirectory);
        }

        private void UpdateModManager()
        {
            if (!_isApplicationInitialized)
            {
                MessageBox.Show(UILocalization.GetString("Info.AppNotInitialized"), UILocalization.GetString("Info.Info"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }
    }
}

