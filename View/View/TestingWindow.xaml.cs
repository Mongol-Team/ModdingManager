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
            ideology.ElemSize = 60;
            ideology.Orientation = Orientation.Vertical;
            ideology.Source = ModDataStorage.Mod.Ideologies.Cast<IConfig>().ToList();
            chars.ElemSize = 60;
            chars.Orientation = Orientation.Horizontal;
            chars.Source = ModDataStorage.Mod.Characters.Cast<IConfig>().ToList();

            PenisildoBombardildo.BuildingContent = ModDataStorage.Mod.Ideas.Random();
            var dsfg = BitmapExtensions.LoadFromDDS("C:\\Users\\huiek\\Documents\\Paradox Interactive\\Hearts of Iron IV\\mod\\SME\\gfx\\interface\\ideas\\NIG\\nig_curse_1964_art.dds");
            var fimzdf = BitmapExtensions.LoadFromDDS("C:\\Users\\huiek\\Documents\\Paradox Interactive\\Hearts of Iron IV\\mod\\SME\\gfx\\interface\\ideas\\NIG\\nig_blood_diamods_art.dds");
            Logger.AddLog("idi na hui");
            Fimoz.Source = fimzdf.ToImageSource();
        }
    }
}
