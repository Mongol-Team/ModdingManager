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
    /// Presenter для MainWindow. Містить всю бізнес-логіку ініціалізації,
    /// управління layout'ом, роботи з FileExplorer та FilesStripe.
    /// </summary>
    public class MainWindowPresenter
    {
        private readonly IMainWindow _view;
        private FileExplorer _fileExplorerControl;
        private FilesStripeControl _filesStripeControl;
        private Grid _centralWorkZone; // Центральна робоча зона з FilesStripe + контент
        private ContentControl _viewerContainer; // Контейнер для GenericViewer та інших редакторів

        public MainWindowPresenter(IMainWindow view)
        {
            _view = view;
        }

        /// <summary>
        /// Точка входу — викликається з MainWindow після InitializeComponent.
        /// </summary>
        public void Initialize()
        {
            Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.Initializing"));

            LoadLayout();
            InitializeTopbar();
            InitializeCentralWorkZone();
            InitializeFileExplorer();

            Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.Initialized"));
        }

        /// <summary>
        /// Зберегти layout при закритті вікна.
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
                Content = StaticLocalisation.GetString("TobBar.Button.Settings"),
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
            // Кнопка відладки
            var debugButton = new Button
            {
                Content = StaticLocalisation.GetString("TobBar.Button.Debug"),
                Name = "DebugButton"
            };
            debugButton.Click += (s, e) => OpenDebugWindow();
            _view.AddTopbarButton(debugButton, PanelSide.Left);

            // Кнопка тестування
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
        // Центральна робоча зона з FilesStripe
        // ──────────────────────────────────────────────

        /// <summary>
        /// Створює центральну робочу зону з FilesStripe у першому рядку
        /// та контейнером для viewer'ів у другому.
        /// </summary>
        private void InitializeCentralWorkZone()
        {
            // Створюємо Grid з двома рядками
            _centralWorkZone = new Grid();
            _centralWorkZone.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // FilesStripe
            _centralWorkZone.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Viewer

            // FilesStripe у першому рядку
            _filesStripeControl = new FilesStripeControl
            {
                Orientation = StripeOrientation.Horizontal
            };

            // Підписка на події FilesStripe
            _filesStripeControl.FileOpenRequested += OnFilesStripeFileOpenRequested;
            _filesStripeControl.ActiveFileChanged += OnFilesStripeActiveFileChanged;
            _filesStripeControl.FileCloseRequested += OnFilesStripeFileCloseRequested;
            _filesStripeControl.AllFilesCloseRequested += OnFilesStripeAllFilesCloseRequested;

            Grid.SetRow(_filesStripeControl, 0);
            _centralWorkZone.Children.Add(_filesStripeControl);

            // ContentControl для viewer'ів у другому рядку
            _viewerContainer = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetRow(_viewerContainer, 1);
            _centralWorkZone.Children.Add(_viewerContainer);

            // Додаємо робочу зону в DockManager
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
                // Панелі немає — створюємо з нуля
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
                // Панель є, але контент не той — замінюємо
                _fileExplorerControl = CreateFileExplorer(title);
                _view.SetPanelContent(existingPanel, _fileExplorerControl);
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerContentReplaced"));
            }
            else
            {
                // Панель є з правильним контентом — перевикористовуємо
                _fileExplorerControl = (FileExplorer)existingPanel.Content;
                SubscribeToFileExplorerEvents();
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerReused"));
            }
        }

        /// <summary>
        /// Створює та налаштовує екземпляр FileExplorer з підпискою на події.
        /// </summary>
        private FileExplorer CreateFileExplorer(string title)
        {
            var explorer = new FileExplorer { Title = title };
            explorer.LoadModData();
            _fileExplorerControl = explorer;
            SubscribeToFileExplorerEvents();
            return explorer;
        }

        /// <summary>
        /// Підписка на всі події FileExplorer.
        /// </summary>
        private void SubscribeToFileExplorerEvents()
        {
            _fileExplorerControl.ItemSelected += OnFileExplorerItemSelected;
            _fileExplorerControl.OpenItemRequested += OnFileExplorerOpenItemRequested;
            _fileExplorerControl.AddFileRequested += OnAddFileRequested;
            _fileExplorerControl.AddEntityRequested += OnAddEntityRequested;
            _fileExplorerControl.DeleteItemRequested += OnDeleteItemRequested;
            _fileExplorerControl.MoveFileRequested += OnMoveFileRequested;
            _fileExplorerControl.MoveEntityRequested += OnMoveEntityRequested;
            _fileExplorerControl.RenameRequested += OnRenameRequested;
        }

        /// <summary>
        /// Обробка вибору елемента в FileExplorer (одиночний клік).
        /// </summary>
        private void OnFileExplorerItemSelected(object sender, RoutedEventArgs e)
        {
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerItemSelected"));
        }

        /// <summary>
        /// Обробка запиту на відкриття елемента (подвійний клік) з FileExplorer.
        /// Додає таб у FilesStripe та відкриває viewer.
        /// </summary>
        private void OnFileExplorerOpenItemRequested(object sender, OpenItemRequestedEventArgs e)
        {
            if (e.Item == null) return;

            var displayName = GetItemDisplayName(e.Item);
            var uniqueId = GetUniqueItemId(e.Item);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.OpeningItem", displayName, uniqueId));

            // Додаємо таб у FilesStripe з унікальним ID
            _filesStripeControl.AddTab(
                filePath: uniqueId,
                displayName: displayName,
                fileObject: e.Item,
                makeActive: true
            );

            // Відкриваємо viewer
            OpenItemInViewer(e.Item);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.ItemOpenedInStripe", displayName));
        }

        /// <summary>
        /// Бізнес-логіка відкриття елемента мода в робочій зоні.
        /// </summary>
        private void OpenItemInViewer(object item)
        {
            if (item == null)
            {
                _viewerContainer.Content = null;
                return;
            }

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
                        _viewerContainer.Content = viewer;
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
        // FilesStripe Events
        // ──────────────────────────────────────────────

        /// <summary>
        /// Обробка кліку по табу в FilesStripe — відкриває відповідний viewer.
        /// </summary>
        private void OnFilesStripeFileOpenRequested(object sender, FileTabEventArgs e)
        {
            OpenItemInViewer(e.Tab.FileObject);
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.StripeTabOpened", e.Tab.DisplayName));
        }

        /// <summary>
        /// Обробка зміни активного файлу в FilesStripe.
        /// </summary>
        private void OnFilesStripeActiveFileChanged(object sender, FileTabEventArgs e)
        {
            OpenItemInViewer(e.Tab.FileObject);
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.StripeActiveFileChanged", e.Tab.DisplayName));
        }

        /// <summary>
        /// Обробка закриття одного табу в FilesStripe.
        /// </summary>
        private void OnFilesStripeFileCloseRequested(object sender, FileTabEventArgs e)
        {
            // Якщо закритий таб був активним, очищуємо viewer
            var activeTab = _filesStripeControl.GetActiveTab();
            if (activeTab == null || activeTab.FilePath == e.Tab.FilePath)
            {
                _viewerContainer.Content = null;
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.ViewerClearedAfterTabClose"));
            }

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.StripeTabClosed", e.Tab.DisplayName));
        }

        /// <summary>
        /// Обробка закриття всіх табів у FilesStripe.
        /// </summary>
        private void OnFilesStripeAllFilesCloseRequested(object sender, EventArgs e)
        {
            _viewerContainer.Content = null;
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.AllStripeTabsClosed"));
        }

        // ──────────────────────────────────────────────
        // FileExplorer — обробка операцій з даними
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

                    // Оновлюємо відкритий viewer, якщо він відображає цей файл
                    RefreshViewerIfNeeded(fileNode.File);

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
            // Presenter показує діалог підтвердження
            var result = CustomMessageBox.Show(
                StaticLocalisation.GetString("Dialog.ConfirmDelete", e.DisplayName),
                StaticLocalisation.GetString("Dialog.Confirm"),
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var modConfig = ModDataStorage.Mod;
                var uniqueId = string.Empty;

                if (e.Item is ConfigFileNode fileNode)
                {
                    uniqueId = GetUniqueItemId(fileNode.File);

                    // Видаляємо файл з відповідної колекції ModConfig
                    foreach (var prop in modConfig.GetType().GetProperties())
                    {
                        if (prop.GetValue(modConfig) is System.Collections.IList list && list.Contains(fileNode.File))
                        {
                            list.Remove(fileNode.File);
                            e.Confirmed = true;

                            // Видаляємо таб з FilesStripe
                            _filesStripeControl.RemoveTab(uniqueId);

                            Logger.AddDbgLog(StaticLocalisation.GetString(
                                "Log.MainWindow.FileDeleted", fileNode.DisplayName));
                            break;
                        }
                    }
                }
                else if (e.Item is ModItemNode modItem && modItem.ParentFile != null)
                {
                    uniqueId = GetUniqueItemId(modItem.Item);

                    // Видаляємо entity з батьківського файлу
                    var entitiesProp = modItem.ParentFile.GetType().GetProperty("Entities");
                    if (entitiesProp?.GetValue(modItem.ParentFile) is System.Collections.IList entities
                        && entities.Contains(modItem.Item))
                    {
                        entities.Remove(modItem.Item);
                        e.Confirmed = true;

                        // Видаляємо таб з FilesStripe
                        _filesStripeControl.RemoveTab(uniqueId);

                        // Оновлюємо viewer якщо він відкритий
                        RefreshViewerIfNeeded(modItem.ParentFile);

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

                // Оновлюємо таб у FilesStripe якщо файл відкритий
                var uniqueId = GetUniqueItemId(e.SourceFile);
                var tab = _filesStripeControl.GetTab(uniqueId);
                if (tab != null)
                {
                    // Оновлюємо відображуване ім'я
                    var newDisplayName = GetItemDisplayName(e.SourceFile);
                    _filesStripeControl.RemoveTab(uniqueId);
                    _filesStripeControl.AddTab(uniqueId, newDisplayName, e.SourceFile, makeActive: true);
                }

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

                    // Оновлюємо viewer якщо один з файлів відкритий
                    RefreshViewerIfNeeded(e.SourceFile);
                    RefreshViewerIfNeeded(e.TargetFile);

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
                var uniqueId = string.Empty;

                if (e.Item is ConfigFileNode fileNode)
                {
                    uniqueId = GetUniqueItemId(fileNode.File);
                    var renameMethod = fileNode.File.GetType().GetMethod("Rename");
                    if (renameMethod?.Invoke(fileNode.File, new object[] { e.NewName }) is true)
                    {
                        fileNode.DisplayName = e.NewName;
                        e.Success = true;

                        // Оновлюємо таб у FilesStripe
                        UpdateTabDisplayName(uniqueId, e.NewName);

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
                    uniqueId = GetUniqueItemId(modItem.Item);

                    if (TrySetEntityId(modItem.Item, e.NewName))
                    {
                        modItem.DisplayName = e.NewName;
                        modItem.Id = e.NewName;
                        e.Success = true;

                        // Оновлюємо таб у FilesStripe
                        UpdateTabDisplayName(uniqueId, e.NewName);

                        // Оновлюємо viewer
                        RefreshViewerIfNeeded(modItem.Item);

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

        // ──────────────────────────────────────────────
        // Допоміжні методи
        // ──────────────────────────────────────────────

        /// <summary>
        /// Генерує унікальний ідентифікатор для елемента (файлу або сутності).
        /// Формат: тип_хешкод або тип_id для IConfig/IGfx.
        /// </summary>
        private static string GetUniqueItemId(object item)
        {
            if (item == null) return Guid.NewGuid().ToString();

            var itemType = item.GetType();
            var typeName = itemType.Name;

            // Для IConfig та IGfx використовуємо Id як частину унікального ключа
            if (item is IConfig config && config.Id != null)
            {
                var filePath = config.FileFullPath ?? "unknown";
                return $"{typeName}_{config.Id}_{filePath.GetHashCode()}";
            }

            if (item is IGfx gfx && gfx.Id != null)
            {
                var filePath = GetItemFilePath(item);
                return $"{typeName}_{gfx.Id}_{filePath.GetHashCode()}";
            }

            // Для файлів використовуємо FileFullPath
            var filePathProp = itemType.GetProperty("FileFullPath");
            if (filePathProp != null)
            {
                var path = filePathProp.GetValue(item) as string;
                if (!string.IsNullOrEmpty(path))
                    return $"{typeName}_{path.GetHashCode()}";
            }

            // Fallback — хеш об'єкта
            return $"{typeName}_{item.GetHashCode()}";
        }

        /// <summary>
        /// Оновлює відображуване ім'я табу в FilesStripe.
        /// </summary>
        private void UpdateTabDisplayName(string uniqueId, string newDisplayName)
        {
            var tab = _filesStripeControl.GetTab(uniqueId);
            if (tab != null)
            {
                var fileObject = tab.FileObject;
                var wasActive = _filesStripeControl.GetActiveTab()?.FilePath == uniqueId;

                _filesStripeControl.RemoveTab(uniqueId);
                _filesStripeControl.AddTab(uniqueId, newDisplayName, fileObject, makeActive: wasActive);

                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.StripeTabRenamed", newDisplayName));
            }
        }

        /// <summary>
        /// Оновлює viewer якщо відображається вказаний файл або об'єкт.
        /// </summary>
        private void RefreshViewerIfNeeded(object fileOrItem)
        {
            var activeTab = _filesStripeControl.GetActiveTab();
            if (activeTab?.FileObject == fileOrItem)
            {
                OpenItemInViewer(fileOrItem);
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.ViewerRefreshed"));
            }
        }

        /// <summary>
        /// Отримує відображуване ім'я елемента.
        /// </summary>
        private static string GetItemDisplayName(object item)
        {
            if (item == null) return "Unknown";

            // Для IConfig та IGfx використовуємо Id
            if (item is IConfig config && config.Id != null)
                return config.Id.ToString();

            if (item is IGfx gfx && gfx.Id != null)
                return gfx.Id.ToString();

            // Для файлів використовуємо FileName
            var fileNameProp = item.GetType().GetProperty("FileName");
            if (fileNameProp != null)
            {
                var fileName = fileNameProp.GetValue(item) as string;
                if (!string.IsNullOrEmpty(fileName))
                    return fileName;
            }

            // Fallback на ім'я типу
            return item.GetType().Name;
        }

        /// <summary>
        /// Отримує шлях до файлу елемента.
        /// </summary>
        private static string GetItemFilePath(object item)
        {
            if (item == null) return string.Empty;

            // Для IConfig використовуємо FileFullPath
            if (item is IConfig config)
                return config.FileFullPath ?? string.Empty;

            // Для файлів використовуємо FileFullPath через рефлексію
            var filePathProp = item.GetType().GetProperty("FileFullPath");
            if (filePathProp != null)
            {
                var filePath = filePathProp.GetValue(item) as string;
                if (!string.IsNullOrEmpty(filePath))
                    return filePath;
            }

            return string.Empty;
        }

        /// <summary>
        /// Встановлює Id через IConfig або IGfx, створюючи новий Identifier.
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