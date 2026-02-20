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
            JUDES();
        }

        private void JUDES()
        {
            errTest.AddErrors(ModDataStorage.CsvErrors);
            errTest.AddErrors(ModDataStorage.TxtErrors);
            Logger.AddLog("------------------------------------------------"); 
            var md = ModDataStorage.Mod.ModifierDefinitions
                .FirstOrDefault(md => md.ToString() == "build_cost_ic");
            filesStripe.AddTab("govno", "hueta", ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(), true);
            Logger.AddLog(md?.ToString() ?? "govno");

        }
    }
}
