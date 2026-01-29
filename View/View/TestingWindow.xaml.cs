using Application;
using Application.Debugging;
using Application.Extentions;
using Models.Configs;
using RawDataWorker.Healers;
using System.Windows;
using Orientation = System.Windows.Controls.Orientation;

namespace View
{
    /// <summary>
    /// Логика взаимодействия для TestingWindow.xaml
    /// </summary>
    public partial class TestingWindow : BaseWindow
    {
        public TestingWindow()
        {
            InitializeComponent();
        }

        private void ConfigListViewer_Loaded(object sender, RoutedEventArgs e)
        {
            errTest.AddErrors(ModDataStorage.CsvErross);
        }
    }
}
