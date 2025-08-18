using ModdingManager.WPF.ViewModel;
using System.Windows;


namespace ModdingManager.WPF.View
{
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
            DataContext = new MainContext(); // назначаем контекст
        }
    }
}
