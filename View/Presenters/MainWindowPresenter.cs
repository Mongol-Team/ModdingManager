using Application;
using Application.Debugging;
using Application.Settings;
using Application.utils;
using Application.Utils;
using Controls;
using Controls.Args;
using Controls.Docking;
using global::View;
using Models.Attributes;
using Models.Configs;
using Models.Interfaces;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using View.Models;
using MessageBox = System.Windows.MessageBox;

namespace ViewPresenters
{
    /// <summary>
    /// Presenter для MainWindow. Содержит всю бизнес-логику инициализации,
    /// управления layout'ом и обработки взаимодействия с FileExplorer.
    /// </summary>
    public class MainWindowPresenter
    {
        private readonly IMainWindow _view;
        private FileExplorer _fileExplorerControl;

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
            InitializeFileExplorer();

            Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.Initialized"));
        }

        /// <summary>
        /// Сохранить layout при закрытии окна.
        /// </summary>
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
                Content = StaticLocalisation.GetString("Button.Settings"),
                Name = "SettingsButton"
            };
            // TODO: settingsButton.Click += OnSettingsClicked;
            _view.AddTopbarButton(settingsButton, PanelSide.Left);

            if (ModManagerSettings.IsDebugRunning)
            {
                AddDebugButtons();
            }
        }

        private void AddDebugButtons()
        {
            // Кнопка отладки
            var debugButton = new Button
            {
                Content = StaticLocalisation.GetString("Button.Debug"),
                Name = "DebugButton"
            };
            debugButton.Click += (s, e) => OpenDebugWindow();
            _view.AddTopbarButton(debugButton, PanelSide.Left);

            // Кнопка тестирования
            var testingButton = new Button
            {
                Content = StaticLocalisation.GetString("Button.Test"),
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

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition
            { Height = new GridLength(30) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition
            { Height = new GridLength(1, GridUnitType.Star) });

            var titleBar = new WindowTitleBar();
            var saveToFileBtn = new Button
            {
                Content = StaticLocalisation.GetString("Button.SaveToFile"),
                Name = "SaveToFileButton"
            };

            System.Windows.Controls.Grid.SetRow(titleBar, 0);
            grid.Children.Add(titleBar);

            var debugControl = new DebugControl();
            saveToFileBtn.Click += (s, e) => debugControl.SendToFile();
            titleBar.AddButton(saveToFileBtn, PanelSide.Left);

            System.Windows.Controls.Grid.SetRow(debugControl, 1);
            grid.Children.Add(debugControl);

            window.Content = grid;
            window.Show();

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.DebugWindowOpened"));
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
                // Панели нет — создаём с нуля
                _fileExplorerControl = CreateFileExplorer(title);

                var panel = new DockPanelInfo
                {
                    Title = title,
                    Content = _fileExplorerControl,
                    CanClose = false,
                    CanPin = true,
                    IsPinned = true
                };

                _view.AddDockPanel(panel, DockSide.Right);
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerPanelCreated"));
            }
            else if (existingPanel.Content is null or not FileExplorer)
            {
                // Панель есть, но контент не тот — заменяем
                _fileExplorerControl = CreateFileExplorer(title);
                _view.SetPanelContent(existingPanel, _fileExplorerControl);
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerContentReplaced"));
            }
            else
            {
                // Панель есть с правильным контентом — переиспользуем
                _fileExplorerControl = (FileExplorer)existingPanel.Content;
                _fileExplorerControl.OpenItemRequested += OnFileExplorerOpenItemRequested;
                _fileExplorerControl.ItemSelected += OnFileExplorerItemSelected;
                _fileExplorerControl.OpenItemRequested += OnFileExplorerOpenItemRequested;
                _fileExplorerControl.AddFileRequested += OnAddFileRequested;
                _fileExplorerControl.AddEntityRequested += OnAddEntityRequested;
                _fileExplorerControl.DeleteItemRequested += OnDeleteItemRequested;
                _fileExplorerControl.MoveFileRequested += OnMoveFileRequested;
                _fileExplorerControl.MoveEntityRequested += OnMoveEntityRequested;
                _fileExplorerControl.RenameRequested += OnRenameRequested;
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerReused"));
            }
        }

        /// <summary>
        /// Создаёт и настраивает экземпляр FileExplorer с подпиской на события.
        /// </summary>
        // В CreateFileExplorer добавить подписки:
        private FileExplorer CreateFileExplorer(string title)
        {
            var explorer = new FileExplorer { Title = title };
            explorer.LoadModData();
            explorer.ItemSelected += OnFileExplorerItemSelected;
            explorer.OpenItemRequested += OnFileExplorerOpenItemRequested;
            explorer.AddFileRequested += OnAddFileRequested;
            explorer.AddEntityRequested += OnAddEntityRequested;
            explorer.DeleteItemRequested += OnDeleteItemRequested;
            explorer.MoveFileRequested += OnMoveFileRequested;
            explorer.MoveEntityRequested += OnMoveEntityRequested;
            explorer.RenameRequested += OnRenameRequested;
            return explorer;
        }

        /// <summary>
        /// Обработка выбора элемента в FileExplorer (одиночный клик).
        /// </summary>
        private void OnFileExplorerItemSelected(object sender, RoutedEventArgs e)
        {
            // Дополнительная логика при выборе элемента (например, статусбар)
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerItemSelected"));
        }

        /// <summary>
        /// Обработка запроса на открытие элемента (двойной клик) из FileExplorer.
        /// Именно здесь принимается решение — что открывать и куда.
        /// </summary>
        private void OnFileExplorerOpenItemRequested(object sender, OpenItemRequestedEventArgs e)
        {
            OpenItemInDockZone(e.Item);
        }

        /// <summary>
        /// Бизнес-логика открытия элемента мода в рабочей зоне DockManager.
        /// Перенесена из FileExplorer.OpenCreatorForItem.
        /// </summary>
        private void OpenItemInDockZone(object item)
        {
            if (item == null) return;

            var itemType = item.GetType();
            var creatorAttribute = itemType.GetCustomAttribute<ConfigCreatorAttribute>();

            if (creatorAttribute == null)
            {
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("Error.NoCreatorAttribute", itemType.Name),
                    StaticLocalisation.GetString("Dialog.Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.OpeningCreator",
                itemType.Name, creatorAttribute.CreatorType));

            try
            {
                switch (creatorAttribute.CreatorType)
                {
                    case ConfigCreatorType.GenericCreator:
                        var viewer = new GenericViewer(itemType, item);
                        _view.OpenInDockZone(viewer);
                        break;

                    case ConfigCreatorType.CountryCreator:
                    case ConfigCreatorType.MapCreator:
                    case ConfigCreatorType.GenericGuiCreator:
                        CustomMessageBox.Show(
                            StaticLocalisation.GetString("Info.CreatorNotImplemented",
                                creatorAttribute.CreatorType),
                            StaticLocalisation.GetString("Dialog.Info"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        break;

                    default:
                        CustomMessageBox.Show(
                            StaticLocalisation.GetString("Error.UnknownCreatorType",
                                creatorAttribute.CreatorType),
                            StaticLocalisation.GetString("Dialog.Error"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.OpenCreatorError",
                    itemType.Name, ex.Message));

                CustomMessageBox.Show(
                    StaticLocalisation.GetString("Error.OpenCreatorFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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
        // ──────────────────────────────────────────────
        // FileExplorer — обработка операций с данными
        // ──────────────────────────────────────────────

        private void OnAddFileRequested(object sender, AddFileRequestedEventArgs e)
        {
            var category = e.Category;
            if (category.ItemType == null || !category.ItemType.IsGenericType) return;

            try
            {
                var newFile = Activator.CreateInstance(category.ItemType);
                if (newFile != null && category.Items != null)
                {
                    category.Items.Add(newFile);
                    _fileExplorerControl?.LoadModData();
                    Logger.AddDbgLog(StaticLocalisation.GetString(
                        "Log.MainWindow.FileAdded", category.DisplayName));
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.AddFileError", ex.Message));
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("Error.AddFileFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAddEntityRequested(object sender, AddEntityRequestedEventArgs e)
        {
            var fileNode = e.FileNode;
            if (fileNode.ConfigType == null || fileNode.Entities == null) return;

            try
            {
                var newEntity = Activator.CreateInstance(fileNode.ConfigType);
                if (newEntity != null)
                {
                    fileNode.Entities.Add(newEntity);
                    _fileExplorerControl?.LoadModData();
                    Logger.AddDbgLog(StaticLocalisation.GetString(
                        "Log.MainWindow.EntityAdded", fileNode.DisplayName));
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.AddEntityError", ex.Message));
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("Error.AddEntityFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteItemRequested(object sender, DeleteItemRequestedEventArgs e)
        {
            // Presenter показывает диалог подтверждения
            var result = CustomMessageBox.Show(
                StaticLocalisation.GetString("Dialog.ConfirmDelete", e.DisplayName),
                StaticLocalisation.GetString("Dialog.Confirm"),
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var modConfig = ModDataStorage.Mod;

                if (e.Item is ConfigFileNode fileNode)
                {
                    // Удаляем файл из соответствующей коллекции ModConfig
                    foreach (var prop in modConfig.GetType().GetProperties())
                    {
                        if (prop.GetValue(modConfig) is System.Collections.IList list && list.Contains(fileNode.File))
                        {
                            list.Remove(fileNode.File);
                            e.Confirmed = true;
                            Logger.AddDbgLog(StaticLocalisation.GetString(
                                "Log.MainWindow.FileDeleted", fileNode.DisplayName));
                            break;
                        }
                    }
                }
                else if (e.Item is ModItemNode modItem && modItem.ParentFile != null)
                {
                    // Удаляем entity из родительского файла
                    var entitiesProp = modItem.ParentFile.GetType().GetProperty("Entities");
                    if (entitiesProp?.GetValue(modItem.ParentFile) is System.Collections.IList entities
                        && entities.Contains(modItem.Item))
                    {
                        entities.Remove(modItem.Item);
                        e.Confirmed = true;
                        Logger.AddDbgLog(StaticLocalisation.GetString(
                            "Log.MainWindow.EntityDeleted", modItem.DisplayName));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.DeleteError", ex.Message));
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("Error.DeleteFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnMoveFileRequested(object sender, MoveFileRequestedEventArgs e)
        {
            try
            {
                var modConfig = ModDataStorage.Mod;
                foreach (var prop in modConfig.GetType().GetProperties())
                {
                    if (prop.GetValue(modConfig) is System.Collections.IList list && list.Contains(e.SourceFile))
                    {
                        list.Remove(e.SourceFile);
                        break;
                    }
                }
                e.TargetCategory.Items?.Add(e.SourceFile);
                _fileExplorerControl?.LoadModData();
                Logger.AddDbgLog(StaticLocalisation.GetString(
                    "Log.MainWindow.FileMoved", e.TargetCategory.DisplayName));
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.MoveError", ex.Message));
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("Error.MoveFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnMoveEntityRequested(object sender, MoveEntityRequestedEventArgs e)
        {
            try
            {
                var sourceEntities = e.SourceFile.GetType().GetProperty("Entities")
                    ?.GetValue(e.SourceFile) as System.Collections.IList;
                var targetEntities = e.TargetFile.GetType().GetProperty("Entities")
                    ?.GetValue(e.TargetFile) as System.Collections.IList;

                if (sourceEntities != null && targetEntities != null
                    && sourceEntities.Contains(e.SourceItem))
                {
                    sourceEntities.Remove(e.SourceItem);
                    targetEntities.Add(e.SourceItem);
                    _fileExplorerControl?.LoadModData();
                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.EntityMoved"));
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.MoveError", ex.Message));
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("Error.MoveFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnRenameRequested(object sender, RenameRequestedEventArgs e)
        {
            try
            {
                if (e.Item is ConfigFileNode fileNode)
                {
                    var renameMethod = fileNode.File.GetType().GetMethod("Rename");
                    if (renameMethod?.Invoke(fileNode.File, new object[] { e.NewName }) is true)
                    {
                        fileNode.DisplayName = e.NewName;
                        e.Success = true;
                        Logger.AddDbgLog(StaticLocalisation.GetString(
                            "Log.MainWindow.FileRenamed", e.NewName));
                    }
                    else
                    {
                        CustomMessageBox.Show(
                            StaticLocalisation.GetString("Error.RenameFailed", e.NewName),
                            StaticLocalisation.GetString("Dialog.Warning"),
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else if (e.Item is ModItemNode modItem)
                {
                    if (TrySetEntityId(modItem.Item, e.NewName))
                    {
                        modItem.DisplayName = e.NewName;
                        modItem.Id = e.NewName;
                        e.Success = true;
                        Logger.AddDbgLog(StaticLocalisation.GetString(
                            "Log.MainWindow.EntityRenamed", e.NewName));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.RenameError", ex.Message));
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("Error.RenameFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Устанавливает Id через IConfig или IGfx, создавая новый Identifier.
        /// </summary>
        private static bool TrySetEntityId(object entity, string newName)
        {
            if (entity is IConfig config)
            {
                var idType = config.Id?.GetType() ?? typeof(Models.Types.Utils.Identifier);
                if (Activator.CreateInstance(idType, newName) is Models.Types.Utils.Identifier id)
                {
                    config.Id = id;
                    return true;
                }
            }
            else if (entity is IGfx gfx)
            {
                var idType = gfx.Id?.GetType() ?? typeof(Models.Types.Utils.Identifier);
                if (Activator.CreateInstance(idType, newName) is Models.Types.Utils.Identifier id)
                {
                    gfx.Id = id;
                    return true;
                }
            }
            return false;
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

