using ModdingManager.managers.@base;
using ModdingManagerClassLib;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Orientation = System.Windows.Controls.Orientation;

namespace ModdingManager.View
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
