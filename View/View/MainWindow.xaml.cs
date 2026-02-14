using Application.Debugging;
using Application.Settings;
using Application.utils;
using Application.Utils;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using View.Controls;
using View.Utils;
using ViewControls;
using ViewControls.Docking;
using Button = System.Windows.Controls.Button;
namespace View
{
    public partial class MainWindow : BaseWindow
    {
        private FileExplorer _fileExplorerControl;
        [DllImport("kernel32.dll")] private static extern bool AllocConsole(); 
        [DllImport("kernel32.dll")] private static extern bool FreeConsole();

        public MainWindow()
        {
            InitializeComponent();
            InitializeDocking();
        }

        private void InitializeDocking()
        {
            LoadLayout();

            var solutionExplorerTitle = StaticLocalisation.GetString("Window.EntityExplorer");
            var existingPanel = FindPanelWithTitle(solutionExplorerTitle);
            Topbar.AddButton(new Button
            {
                Content = "Настройки",
                Name = "SettingsButton"

            }, PanelSide.Left);
            if (ModManagerSettings.IsDebugRunning)
            {
                var debugButton = new Button
                {
                    Content = "Отладка",
                    Name = "DebugButton"
                };
                debugButton.Click += (s, e) =>
                {
                    var window = new Window
                    {
                        Title = "Debug / Лог",
                        Width = 1000,
                        Height = 500,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        WindowStyle = WindowStyle.None, 
                        Background = Brushes.Black
                    };

                    var grid = new Grid();
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                    var titleBar = new WindowTitleBar();
                    Button saveToFileBtn = new Button
                    {
                        Content = "Сорханить в файл",
                        Name = "CloseButton"
                    };
                   

                    Grid.SetRow(titleBar, 0);
                    grid.Children.Add(titleBar);

                    var debugControl = new DebugControl();
                    saveToFileBtn.Click += (s, e) =>
                    {
                        debugControl.SendToFile();
                    };
                    titleBar.AddButton(saveToFileBtn, PanelSide.Left);
                    Grid.SetRow(debugControl, 1);
                    grid.Children.Add(debugControl);

                    window.Content = grid;

                    window.Show();


                };
                Topbar.AddButton(debugButton, PanelSide.Left);
                var testingButton = new Button
                {
                    Content = "Тест",
                    Name = "TestingButton"
                };
                testingButton.Click += (s, e) =>
                {
                    var testWindow = new TestingWindow();
                    testWindow.ShowDialog();
                };
                Topbar.AddButton(testingButton, PanelSide.Left);
            }
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

        private void SaveToFileBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
