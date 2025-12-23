using ViewPresenters;
using System.Windows;

namespace View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var presenter = new MainWindowPresenter(this);
        }

        private void GameDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new GameDirectorySettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void TestingBtn_Click(object sender, RoutedEventArgs e)
        {
            TestingWindow tw = new TestingWindow();
            tw.Show();
        }
    }
}
