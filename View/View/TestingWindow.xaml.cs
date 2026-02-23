using Application;
using Application.Debugging;
using Application.Extentions;
using Controls;
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
            Logger.AddLog("------------------------------------------------"); 
            var md = ModDataStorage.Mod.ModifierDefinitions
                .FirstOrDefault(md => md.ToString() == "build_cost_ic");
            MpViewr.Initialize(ModDataStorage.Mod.Map);
            Logger.AddLog(md?.ToString() ?? "govno");

        }
    }
}
