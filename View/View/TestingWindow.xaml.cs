using Application;
using Application.Debugging;
using Application.Extentions;
using Models.Configs;
using System.Windows;
using Orientation = System.Windows.Controls.Orientation;

namespace View
{
    /// <summary>
    /// Логика взаимодействия для TestingWindow.xaml
    /// </summary>
    public partial class TestingWindow : Window
    {
        public TestingWindow()
        {
            InitializeComponent();
        }

        private void ConfigListViewer_Loaded(object sender, RoutedEventArgs e)
        {
            Fima.ElemSize = 60;
            Fima.Orientation = Orientation.Vertical;
            Fima.Source = ModDataStorage.Mod.Ideas.Cast<IConfig>().ToList();
            var dsfg = BitmapExtensions.LoadFromDDS("C:\\Users\\huiek\\Documents\\Paradox Interactive\\Hearts of Iron IV\\mod\\SME\\gfx\\interface\\ideas\\NIG\\nig_curse_1964_art.dds");
            var fimzdf = BitmapExtensions.LoadFromDDS("C:\\Users\\huiek\\Documents\\Paradox Interactive\\Hearts of Iron IV\\mod\\SME\\gfx\\interface\\ideas\\NIG\\nig_blood_diamods_art.dds");
            Logger.AddLog("idi na hui");
            Fimoz.Source = fimzdf.ToImageSource();
        }
    }
}
