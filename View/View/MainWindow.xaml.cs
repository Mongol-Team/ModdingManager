using ViewPresenters;
using System.Windows;
using Application.Settings;
using System.IO;
using View.Utils;

namespace View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var presenter = new MainWindowPresenter(this);
            InitializeFileExplorer();
        }

        private void InitializeFileExplorer()
        {
            var modDirectory = ModManagerSettings.ModDirectory;
            if (!string.IsNullOrEmpty(modDirectory) && Directory.Exists(modDirectory))
            {
                FileExplorerControl.RootPath = modDirectory;
            }
            
            FileExplorerControl.Title = View.Utils.UILocalization.GetString("Window.SolutionExplorer");
        }

        private void GameDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new GameDirectorySettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void TestingBtn_Click(object sender, RoutedEventArgs e)
        {
            PlaceholderWindow.ShowPlaceholder("Тестовая страница пока не готова", this);
        }

        private void FileExplorerControl_PathSelected(object sender, RoutedEventArgs e)
        {
        }
    }
}
