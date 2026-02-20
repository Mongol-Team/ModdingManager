using Application.Settings;
using Application.utils;
using Application.Utils;
using Controls;
using Controls.Docking;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using View.Models;
using ViewPresenters;
using Button = System.Windows.Controls.Button;
namespace View
{

    /// <summary>
    /// View — только UI, никакой бизнес-логики.
    /// Всё делегируется MainWindowPresenter.
    /// </summary>
    public partial class MainWindow : BaseWindow, IMainWindow
    {
        [DllImport("kernel32.dll")] private static extern bool AllocConsole();
        [DllImport("kernel32.dll")] private static extern bool FreeConsole();

        private readonly MainWindowPresenter _presenter;

        public MainWindow()
        {
            InitializeComponent();
            _presenter = new MainWindowPresenter(this);
            _presenter.Initialize();
        }

        // ──────────────────────────────────────────────
        // IMainWindow — реализация
        // ──────────────────────────────────────────────

        public void AddTopbarButton(Button button, PanelSide side)
            => Topbar.AddButton(button, side);

        public DockPanelInfo FindPanelWithTitle(string title)
        {
            foreach (var panel in DockManager.GetAllPanels())
            {
                if (panel.Title == title)
                    return panel;
            }
            return null;
        }

        public void AddDockPanel(DockPanelInfo panel, DockSide side)
            => DockManager.AddPanel(panel, side);

        public void SetPanelContent(DockPanelInfo panel, object content)
            => panel.Content = content as UIElement;

        public void OpenInDockZone(UIElement content)
        {
            // DockManager предоставляет метод для установки центрального контента
            DockManager.SetContent(content);
        }

        public IEnumerable<DockPanelInfo> GetAllDockPanels()
            => DockManager.GetAllPanels();

        public void LoadLayout(string layoutPath)
        {
            var layout = LayoutSerializer.LoadFromFile(layoutPath);
            if (layout != null)
                LayoutSerializer.Deserialize(DockManager, layout);
        }

        public void SaveLayout(string layoutPath)
        {
            var layout = LayoutSerializer.Serialize(DockManager);
            LayoutSerializer.SaveToFile(layout, layoutPath);
        }

        // ──────────────────────────────────────────────
        // UI Events — только проброс в Presenter
        // ──────────────────────────────────────────────

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            => _presenter.OnWindowClosing();
    }
}
