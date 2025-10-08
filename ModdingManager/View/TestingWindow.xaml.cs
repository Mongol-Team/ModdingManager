using ModdingManager.managers.@base;
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
            Fima.ElemSize = 23;
            Fima.Orientation = Orientation.Vertical;
            Fima.Source = ModManager.Mod.Ideas.Cast<IConfig>().ToList();
        }
    }
}
