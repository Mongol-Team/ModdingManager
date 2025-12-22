using ModdingManager.Presenter;
using System.Windows;

namespace ModdingManager.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var presenter = new MainWindowPresenter(this);
        }

        private void TestingBtn_Click(object sender, RoutedEventArgs e)
        {
            TestingWindow tw = new TestingWindow();
            tw.Show();
        }
    }
}
