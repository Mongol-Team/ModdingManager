using Application.Settings;
using Application.utils;
using global::View;
using System.Windows;
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
            _view.Loaded += OnWindowLoaded;
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
    }
}

