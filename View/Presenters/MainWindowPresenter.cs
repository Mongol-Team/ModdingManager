using Application;
using Application.Debugging;
using Application.Settings;
using Application.utils;
using Application.Utils;
using Controls;
using Controls.Docking;
using global::View;
using Models.Configs;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using View.Models;

namespace ViewPresenters
{
    /// <summary>
    /// Presenter для MainWindow.
    /// Отвечает за инициализацию окна, layout, topbar и создание рабочей зоны.
    /// Бизнес-логика контролов вынесена в <see cref="MainWindowControlsPresenter"/>.
    /// </summary>
    public class MainWindowPresenter
    {
        private readonly IMainWindow _view;

        private FileExplorer _fileExplorerControl;
        private DockPanelInfo _fileExplorerPanel;   // нужен MapViewerPanelPresenter для обратного переключения
        private FilesStripeControl _filesStripeControl;
        private Grid _centralWorkZone;
        private ContentControl _viewerContainer;

        private MainWindowControlsPresenter _controlsPresenter;

        public MainWindowPresenter(IMainWindow view)
        {
            _view = view;
        }

        /// <summary>
        /// Точка входа — вызывается из MainWindow после InitializeComponent.
        /// </summary>
        public void Initialize()
        {
            Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.Initializing"));

            LoadLayout();
            InitializeTopbar();
            InitializeCentralWorkZone();
            InitializeFileExplorer();
            InitializeMapViewer();

            Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.Initialized"));
        }

        public void OnWindowClosing()
        {
            SaveLayout();
        }

        // ──────────────────────────────────────────────
        // Топбар
        // ──────────────────────────────────────────────

        private void InitializeTopbar()
        {
            var settingsButton = new Button
            {
                Content = StaticLocalisation.GetString("TobBar.Button.Settings"),
                Name = "SettingsButton"
            };
            _view.AddTopbarButton(settingsButton, PanelSide.Left);

            if (ModManagerSettings.IsDebugRunning)
                AddDebugButtons();
        }

        private void AddDebugButtons()
        {
            var debugButton = new Button
            {
                Content = StaticLocalisation.GetString("TobBar.Button.Debug"),
                Name = "DebugButton"
            };
            debugButton.Click += (s, e) => OpenDebugWindow();
            _view.AddTopbarButton(debugButton, PanelSide.Left);

            var testingButton = new Button
            {
                Content = StaticLocalisation.GetString("TobBar.Button.Test"),
                Name = "TestingButton"
            };
            testingButton.Click += (s, e) =>
            {
                var testWindow = new TestingWindow();
                testWindow.ShowDialog();
            };
            _view.AddTopbarButton(testingButton, PanelSide.Left);
        }

        private void OpenDebugWindow()
        {
            var window = new Window
            {
                Title = StaticLocalisation.GetString("Window.Debug"),
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
            var saveToFileBtn = new Button
            {
                Content = StaticLocalisation.GetString("DebugWindow.Button.SaveToFile"),
                Name = "SaveToFileButton"
            };

            Grid.SetRow(titleBar, 0);
            grid.Children.Add(titleBar);

            var debugControl = new DebugControl();
            saveToFileBtn.Click += (s, e) => debugControl.SendToFile();
            titleBar.AddButton(saveToFileBtn, PanelSide.Left);

            Grid.SetRow(debugControl, 1);
            grid.Children.Add(debugControl);

            window.Content = grid;
            window.Show();

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.DebugWindowOpened"));
        }

        // ──────────────────────────────────────────────
        // Центральная рабочая зона
        // ──────────────────────────────────────────────

        private void InitializeCentralWorkZone()
        {
            _centralWorkZone = new Grid();
            _centralWorkZone.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _centralWorkZone.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            _filesStripeControl = new FilesStripeControl
            {
                Orientation = StripeOrientation.Horizontal
            };
            Grid.SetRow(_filesStripeControl, 0);
            _centralWorkZone.Children.Add(_filesStripeControl);

            _viewerContainer = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetRow(_viewerContainer, 1);
            _centralWorkZone.Children.Add(_viewerContainer);

            _view.OpenInDockZone(_centralWorkZone);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.CentralWorkZoneInitialized"));
        }

        // ──────────────────────────────────────────────
        // FileExplorer
        // ──────────────────────────────────────────────

        private void InitializeFileExplorer()
        {
            var title = StaticLocalisation.GetString("Window.EntityExplorer");
            var existingPanel = _view.FindPanelWithTitle(title);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerPanelSearch", title,
                existingPanel != null ? "найдена" : "не найдена"));

            if (existingPanel == null)
            {
                _fileExplorerControl = new FileExplorer { Title = title };
                _fileExplorerControl.LoadModData();

                _fileExplorerPanel = new DockPanelInfo
                {
                    Title = title,
                    Content = _fileExplorerControl,
                    CanClose = false,
                    CanPin = true,
                    IsPinned = true
                };

                _view.AddDockPanel(_fileExplorerPanel, DockSide.Right);
                CreateControlsPresenter();

                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerPanelCreated"));
            }
            else if (existingPanel.Content is null or not FileExplorer)
            {
                _fileExplorerControl = new FileExplorer { Title = title };
                _fileExplorerControl.LoadModData();
                _fileExplorerPanel = existingPanel;
                _view.SetPanelContent(existingPanel, _fileExplorerControl);
                CreateControlsPresenter();

                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerContentReplaced"));
            }
            else
            {
                _fileExplorerControl = (FileExplorer)existingPanel.Content;
                _fileExplorerPanel = existingPanel;
                CreateControlsPresenter();

                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerReused"));
            }
        }

        /// </summary>
        private void CreateControlsPresenter()
        {
            _controlsPresenter = new MainWindowControlsPresenter(
                _view,
                _view.GetDockManager(),
                _fileExplorerControl,
                _fileExplorerPanel,
                _filesStripeControl,
                _viewerContainer,
                _centralWorkZone);      // ← новый 7-й аргумент
        }
        // ──────────────────────────────────────────────
        // MapViewer
        // ──────────────────────────────────────────────

        /// <summary>
        /// Делегирует инициализацию карты в ControlsPresenter.
        /// Вызывается после создания ControlsPresenter — гарантируем что он уже есть.
        /// </summary>
        private void InitializeMapViewer()
        {
            _controlsPresenter?.InitializeMapViewerPanel();
        }

        // ──────────────────────────────────────────────
        // Layout
        // ──────────────────────────────────────────────

        private void LoadLayout()
        {
            try
            {
                var layoutPath = Path.Combine(AppPaths.DataDirectory, "layout.json");
                _view.LoadLayout(layoutPath);
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.LayoutLoaded"));
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.LayoutLoadError", ex.Message));
            }
        }

        private void SaveLayout()
        {
            try
            {
                var layoutPath = Path.Combine(AppPaths.DataDirectory, "layout.json");
                _view.SaveLayout(layoutPath);
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.LayoutSaved"));
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.LayoutSaveError", ex.Message));
            }
        }
    }
}