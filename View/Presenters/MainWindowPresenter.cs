using Application;
using Application.Debugging;
using Application.Settings;
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
                Message = "Загрузка настроек..."
            };
            loadingWindow.SetProgressBounds(0, 2);
            loadingWindow.Show();

            await System.Threading.Tasks.Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    loadingWindow.Message = "Загрузка настроек...";
                });
                ModManagerSettings.Load();
            });

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                loadingWindow.Progress = 1;
                loadingWindow.Message = "Инициализация данных приложения...";
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
                MessageBox.Show("Функция локализации по дереву фокусов временно недоступна (класс TreeLoc в Obsolete).", "Информация", MessageBoxButton.OK);
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
                MessageBox.Show("Функция локализации для идей временно недоступна (класс IdeaLoc в Obsolete).", "Информация", MessageBoxButton.OK);
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
                MessageBox.Show("Функция локализации по стейтам временно недоступна (класс StateLoc в Obsolete).", "Информация", MessageBoxButton.OK);
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
                MessageBox.Show("Создатель флагов временно недоступен (класс FlagCreator в Obsolete).", "Информация", MessageBoxButton.OK);
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
                MessageBox.Show("Создатель персонажей временно недоступен (класс CharacterCreator в Obsolete).", "Информация", MessageBoxButton.OK);
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
                MessageBox.Show("Создатель идей временно недоступен (класс IdeaCreator в Obsolete).", "Информация", MessageBoxButton.OK);
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
                MessageBox.Show("Создатель модификаторов временно недоступен (класс ModifierCreator в Obsolete).", "Информация", MessageBoxButton.OK);
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
                MessageBox.Show("Создатель шаблонов временно недоступен (класс TemplateCreator в Obsolete).", "Информация", MessageBoxButton.OK);
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
                MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK);
            }
        }

        private bool HasAnyDir()
        {
            var settings = ModManagerSettings.Instance;
            return !string.IsNullOrEmpty(settings?.ModDirectory) || !string.IsNullOrEmpty(settings?.GameDirectory);
        }

        private void UpdateModManager()
        {
            if (!_isApplicationInitialized)
            {
                MessageBox.Show("Приложение еще не инициализировано. Пожалуйста, подождите.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }
    }
}

