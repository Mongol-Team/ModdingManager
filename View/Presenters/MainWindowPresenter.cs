using Application.Settings;
using global::View;
using System.Windows;
using View.Utils;
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
            ModManagerSettingsLoader.Load();
            _view = view ?? throw new ArgumentNullException(nameof(view));
            WireUp();
            _view.Loaded += OnWindowLoaded;
        }

        private void WireUp()
        {

        }

        private async void OnWindowLoaded(object? sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;
            _isLoaded = true;

            await InitializeApplicationAsync();
        }

        private async System.Threading.Tasks.Task InitializeApplicationAsync()
        {
            _isApplicationInitialized = true;
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

