using Application.Utils;
using System.IO;
using System.Windows;
using View.Utils;
using ViewControls;
using ViewControls.Docking;

namespace View
{
    public partial class MainWindow : BaseWindow
    {
        private FileExplorer _fileExplorerControl;


        public MainWindow()
        {
            InitializeComponent();
            InitializeDocking();
        }

        private void InitializeDocking()
        {
            LoadLayout();

            var solutionExplorerTitle = UILocalization.GetString("Window.SolutionExplorer");
            var existingPanel = FindPanelWithTitle(solutionExplorerTitle);

            if (existingPanel == null)
            {
                _fileExplorerControl = new FileExplorer
                {
                    Title = solutionExplorerTitle
                };
                _fileExplorerControl.LoadModData();
                _fileExplorerControl.ItemSelected += FileExplorerControl_ItemSelected;

                var fileExplorerPanel = new DockPanelInfo
                {
                    Title = solutionExplorerTitle,
                    Content = _fileExplorerControl,
                    CanClose = false,
                    CanPin = true,
                    IsPinned = true
                };

                DockManager.AddPanel(fileExplorerPanel, DockSide.Right);
            }
            else if (existingPanel.Content is null or not FileExplorer)
            {
                _fileExplorerControl = new FileExplorer
                {
                    Title = solutionExplorerTitle
                };
                _fileExplorerControl.LoadModData();
                _fileExplorerControl.ItemSelected += FileExplorerControl_ItemSelected;
                existingPanel.Content = _fileExplorerControl;
            }
            else
            {
                _fileExplorerControl = existingPanel.Content as FileExplorer;
                if (_fileExplorerControl != null)
                {
                    _fileExplorerControl.ItemSelected += FileExplorerControl_ItemSelected;
                }
            }
        }

        private DockPanelInfo FindPanelWithTitle(string title)
        {
            foreach (var panel in DockManager.GetAllPanels())
            {
                if (panel.Title == title)
                {
                    return panel;
                }
            }
            return null;
        }


        private void LoadLayout()
        {
            try
            {
                var layoutPath = Path.Combine(AppPaths.DataDirectory, "layout.json");
                var layout = LayoutSerializer.LoadFromFile(layoutPath);
                if (layout != null)
                {
                    LayoutSerializer.Deserialize(DockManager, layout);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading layout: {ex.Message}");
            }
        }

        private void SaveLayout()
        {
            try
            {
                var layout = LayoutSerializer.Serialize(DockManager);
                var layoutPath = Path.Combine(AppPaths.DataDirectory, "layout.json");
                LayoutSerializer.SaveToFile(layout, layoutPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving layout: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveLayout();
        }

        private void FileExplorerControl_ItemSelected(object sender, RoutedEventArgs e)
        {
            // Обработка выбора элемента мода
        }
    }
}
