
using global::ModdingManager.configs;
using global::ModdingManager.managers.@base;
using ModdingManager.classes.utils;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace ModdingManager.WPF.ViewModel
{
    internal class MainContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainContext()
        {
            LoadConfig();
        }

        #region Props

        private string modDirectory = "";
        public string ModDirectory
        {
            get => modDirectory;
            set { modDirectory = value; NotifyPropertyChanged(nameof(ModDirectory)); }
        }

        private string gameDirectory = "";
        public string GameDirectory
        {
            get => gameDirectory;
            set { gameDirectory = value; NotifyPropertyChanged(nameof(GameDirectory)); }
        }

        #endregion

        #region Commands

        public ICommand LocConvertCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new TreeLoc();
                wnd.Show();
            }
        });

        public ICommand StateManagerCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new StateWorkerWindow();
                ElementHost.EnableModelessKeyboardInterop(wnd);
                wnd.Show();
            }
        });

        public ICommand LocIdeaCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new IdeaLoc();
                wnd.Show();
            }
        });

        public ICommand LocStateCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new StateLoc();
                wnd.Show();
            }
        });

        public ICommand TechCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new TechTreeCreator();
                ElementHost.EnableModelessKeyboardInterop(wnd);
                wnd.Show();
            }
        });

        public ICommand FlagCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new FlagCreator();
                wnd.Show();
            }
        });

        public ICommand CountryCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new WPFCountryCreator();
                ElementHost.EnableModelessKeyboardInterop(wnd);
                wnd.Show();
            }
        });

        public ICommand CharCreatorCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new CharacterCreator();
                wnd.Show();
            }
        });

        public ICommand IdeaCreatorCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new IdeaCreator();
                wnd.Show();
            }
        });

        public ICommand ModCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new ModifierCreator();
                wnd.Show();
            }
        });

        public ICommand TemplateCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new TemplateCreator();
                wnd.Show();
            }
        });

        public ICommand SuperEventCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new SupereventCreator();
                ElementHost.EnableModelessKeyboardInterop(wnd);
                wnd.Show();
            }
        });

        public ICommand DebugCommand => new BaseCommand(_ =>
        {
            ModManager.IsDebugRuning = true;
            var debugWindow = new DebugWindow();

            Debugger.Instance.DebugOutputControl = debugWindow.DebugBox;
            Debugger.Instance.AttachToWindow(debugWindow);
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
        });

        public ICommand IdeologyCommand => new BaseCommand(_ =>
        {
            if (CheckDirs())
            {
                var wnd = new IdeologyCreator();
                ElementHost.EnableModelessKeyboardInterop(wnd);
                wnd.Show();
            }
        });

        #endregion

        #region Logic

        private void LoadConfig()
        {
            string relativePath = Path.Combine("..", "..", "..", "data", "dir.json");
            string fullPath = Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                string json = File.ReadAllText(fullPath);
                var path = JsonSerializer.Deserialize<PathConfig>(json);
                if (path != null)
                {
                    GameDirectory = path.GamePath;
                    ModDirectory = path.ModPath;
                    ModManager.Directory = ModDirectory;
                    ModManager.GameDirectory = GameDirectory;
                }
                Registry.LoadInstance();
            }
            catch (Exception ex)
            {
                Debugger.Instance.LogMessage($"[MAIN Window] On load exception : {ex.Message + ex.StackTrace}");
            }
        }

        private bool CheckDirs()
        {
            if (string.IsNullOrEmpty(ModDirectory) || string.IsNullOrEmpty(GameDirectory))
            {
                System.Windows.MessageBox.Show("Введите обе директории.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            ModManager.Directory = ModDirectory;
            ModManager.GameDirectory = GameDirectory;
            return true;
        }

        #endregion
    }
}


