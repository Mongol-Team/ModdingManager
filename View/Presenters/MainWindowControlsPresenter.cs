using Application;
using Application.Debugging;
using Application.utils;
using Application.Utils;
using Controls;
using Controls.Args;
using Controls.Docking;
using Models.Attributes;
using Models.Configs;
using Models.Interfaces;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using View.Models;

namespace ViewPresenters
{
    /// <summary>
    /// Presenter рабочей зоны MainWindow.
    ///
    /// ── Режимы ──────────────────────────────────────────────────────────────────
    ///   ModeEntity  вкладка EntityExplorer активна
    ///               → центр = рабочая зона (_centralWorkZone: FilesStripe + Viewer)
    ///   ModeMap     вкладка MapInspector активна
    ///               → центр = MapViewer
    ///
    /// ── FilesStripe пулы ────────────────────────────────────────────────────────
    ///   Каждый режим имеет независимый список снимков табов.
    ///   При смене режима: сохраняем текущий пул → очищаем FilesStripe →
    ///   переключаем центр → восстанавливаем пул нового режима → открываем
    ///   активный таб в viewer'е.
    ///
    /// ── Старт ───────────────────────────────────────────────────────────────────
    ///   Карта инициализируется данными, но НЕ попадает в центр.
    ///   Центр пуст до первого переключения пользователем на вкладку MapInspector.
    /// </summary>
    public class MainWindowControlsPresenter
    {
        // ─── Ключи режимов ───────────────────────────────────────────────────────
        private const string ModeEntity = "entity";
        private const string ModeMap = "map";

        // ─── Зависимости ─────────────────────────────────────────────────────────
        private readonly IMainWindow _view;
        private readonly DockManager _dockManager;
        private readonly FileExplorer _fileExplorer;
        private readonly FilesStripeControl _filesStripe;
        private readonly ContentControl _viewerContainer;
        private readonly UIElement _centralWorkZone;
        private readonly DockPanelInfo _fileExplorerPanel;

        // Задаётся в InitializeMapViewerPanel после добавления инспектора в DockManager
        private DockPanelInfo _mapInspectorPanel;

        // ─── Состояние режима ────────────────────────────────────────────────────
        private string _currentMode = ModeEntity;
        private MapViewer _mapViewer;
        private MapViewerPanelPresenter _mapViewerPanelPresenter;

        // ─── Пулы табов ──────────────────────────────────────────────────────────
        // Ключ: ModeEntity / ModeMap
        // Значение: список снимков табов FilesStripe для данного режима
        private readonly Dictionary<string, List<StripeTabSnapshot>> _pools = new()
        {
            [ModeEntity] = new(),
            [ModeMap] = new()
        };

        // ─── Конструктор ─────────────────────────────────────────────────────────

        public MainWindowControlsPresenter(
            IMainWindow view,
            DockManager dockManager,
            FileExplorer fileExplorer,
            DockPanelInfo fileExplorerPanel,
            FilesStripeControl filesStripe,
            ContentControl viewerContainer,
            UIElement centralWorkZone)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _dockManager = dockManager ?? throw new ArgumentNullException(nameof(dockManager));
            _fileExplorer = fileExplorer ?? throw new ArgumentNullException(nameof(fileExplorer));
            _fileExplorerPanel = fileExplorerPanel ?? throw new ArgumentNullException(nameof(fileExplorerPanel));
            _filesStripe = filesStripe ?? throw new ArgumentNullException(nameof(filesStripe));
            _viewerContainer = viewerContainer ?? throw new ArgumentNullException(nameof(viewerContainer));
            _centralWorkZone = centralWorkZone ?? throw new ArgumentNullException(nameof(centralWorkZone));

            SubscribeToFileExplorerEvents();
            SubscribeToFilesStripeEvents();

            // Реагируем на клик по вкладке правой зоны
            _dockManager.PanelSelectionChanged += OnRightPanelSelectionChanged;

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindowControls.Initialized"));
        }

        // ──────────────────────────────────────────────
        // Переключение режимов
        // ──────────────────────────────────────────────

        /// <summary>
        /// Вызывается из DockManager когда пользователь кликает по вкладке.
        /// Фильтруем только правую зону и только наши две панели.
        /// </summary>
        private void OnRightPanelSelectionChanged(DockSide side, DockPanelInfo panel)
        {
            if (side != DockSide.Right) return;

            if (ReferenceEquals(panel, _fileExplorerPanel))
                SwitchMode(ModeEntity);
            else if (_mapInspectorPanel != null && ReferenceEquals(panel, _mapInspectorPanel))
                SwitchMode(ModeMap);
        }

        private void SwitchMode(string newMode)
        {
            if (_currentMode == newMode) return;

            Logger.AddDbgLog(StaticLocalisation.GetString(
                "Log.MainWindow.ModeSwitching", _currentMode, newMode));

            // 1. Сохраняем снимок текущего состояния
            SnapshotCurrentStripe();

            // 2. Итерируем по КОПИИ пула чтобы избежать модификации коллекции
            //    во время итерации (RemoveTab может стрелять ActiveFileChanged → SnapshotCurrentStripe)
            var tabsToRemove = _pools[_currentMode].Select(s => s.FilePath).ToList();

            // Временно отписываемся чтобы RemoveTab не триггерил SnapshotCurrentStripe
            _filesStripe.ActiveFileChanged -= OnFilesStripeActiveFileChanged;

            foreach (var filePath in tabsToRemove)
                _filesStripe.RemoveTab(filePath);

            _filesStripe.ActiveFileChanged += OnFilesStripeActiveFileChanged;

            _viewerContainer.Content = null;
            _currentMode = newMode;

            // 3. Переключаем центр
            if (newMode == ModeMap && _mapViewer != null)
            {
                _dockManager.SetContent(_mapViewer);
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.CenterSwitchedToMap"));
            }
            else
            {
                _dockManager.SetContent(_centralWorkZone);
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.CenterSwitchedToWorkZone"));
            }

            // 4+5. Восстанавливаем пул нового режима
            RestorePool(newMode);
        }

        /// <summary>
        /// Снимает снимок текущего FilesStripe: обновляет пул _currentMode
        /// на основе актуального состояния GetAllTabs() / GetActiveTab().
        /// </summary>
        private void SnapshotCurrentStripe()
        {
            var pool = _pools[_currentMode];
            var activePath = _filesStripe.GetActiveTab()?.FilePath;

            pool.Clear();
            foreach (var tab in _filesStripe.GetAllTabs())
            {
                pool.Add(new StripeTabSnapshot
                {
                    FilePath = tab.FilePath,
                    DisplayName = tab.DisplayName,
                    FileObject = tab.FileObject,
                    IsPinned = tab.IsPinned,
                    IsActive = tab.FilePath == activePath
                });
            }

            Logger.AddDbgLog(StaticLocalisation.GetString(
                "Log.MainWindow.PoolSaved", _currentMode, pool.Count));
        }

        private void RestorePool(string mode)
        {
            var pool = _pools[mode];

            Logger.AddDbgLog(StaticLocalisation.GetString(
                "Log.MainWindow.PoolRestoring", mode, pool.Count));

            object activeObject = null;

            // Отписываемся чтобы AddTab не триггерил лишние снимки во время восстановления
            _filesStripe.ActiveFileChanged -= OnFilesStripeActiveFileChanged;
            _filesStripe.FileOpenRequested -= OnFilesStripeFileOpenRequested;

            try
            {
                foreach (var snap in pool)
                {
                    _filesStripe.AddTab(
                        filePath: snap.FilePath,
                        displayName: snap.DisplayName,
                        fileObject: snap.FileObject,
                        makeActive: snap.IsActive);

                    if (snap.IsPinned)
                        _filesStripe.SetPinned(snap.FilePath, true);

                    if (snap.IsActive)
                        activeObject = snap.FileObject;
                }
            }
            finally
            {
                _filesStripe.ActiveFileChanged += OnFilesStripeActiveFileChanged;
                _filesStripe.FileOpenRequested += OnFilesStripeFileOpenRequested;
            }

            if (activeObject != null)
                OpenItemInViewer(activeObject);

            Logger.AddDbgLog(StaticLocalisation.GetString(
                "Log.MainWindow.PoolRestored", mode, pool.Count));
        }

        // ──────────────────────────────────────────────
        // Инициализация MapViewer
        // ──────────────────────────────────────────────

        /// <summary>
        /// Создаёт MapViewer (инициализирует данными), добавляет панель MapInspector
        /// в правую зону, создаёт MapViewerPanelPresenter.
        /// MapViewer НЕ попадает в центр — только когда пользователь переключится
        /// на вкладку MapInspector.
        /// </summary>
        public void InitializeMapViewerPanel()
        {
            if (ModDataStorage.Mod?.Map == null)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MapViewerPanel.NoMapData"));
                return;
            }

            _mapViewer = CreateAndInitMapViewer();
            if (_mapViewer == null) return;

            var (inspectorPanel, inspector) = GetOrCreateInspectorPanel();
            if (inspectorPanel == null || inspector == null) return;

            _mapInspectorPanel = inspectorPanel;

            _mapViewerPanelPresenter = new MapViewerPanelPresenter(
                _mapViewer,
                inspector,
                inspectorPanel,
                _fileExplorerPanel,
                _dockManager);

            Logger.AddLog(StaticLocalisation.GetString("Log.MapViewerPanel.PanelReady"));
        }

        /// <summary>
        /// Создаёт и инициализирует MapViewer данными карты.
        /// SetContent НЕ вызывается — карта не попадает в центр при старте.
        /// </summary>
        private MapViewer CreateAndInitMapViewer()
        {
            try
            {
                var mapViewer = new MapViewer();
                Logger.AddLog(StaticLocalisation.GetString("Log.MapViewerPanel.InitializingData"));
                mapViewer.Initialize(ModDataStorage.Mod.Map);
                Logger.AddLog(StaticLocalisation.GetString("Log.MapViewerPanel.DataInitialized"));
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MapViewerPanel.MapViewerCreated"));
                return mapViewer;
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MapViewerPanel.InitError", ex.Message));
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("Error.MapViewerInitFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private (DockPanelInfo panel, GenericViewer inspector) GetOrCreateInspectorPanel()
        {
            var title = StaticLocalisation.GetString("Window.MapEntityInspector");

            var existingPanel = _view.FindPanelWithTitle(title);
            if (existingPanel != null)
            {
                // Переиспользуем существующую панель — но гарантируем что Content валиден
                if (existingPanel.Content is GenericViewer existing)
                {
                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.MapViewerPanel.InspectorReused"));
                    return (existingPanel, existing);
                }

                // Content == null или не GenericViewer — восстанавливаем
                // Content == null или не GenericViewer — восстанавливаем
                var placeholder = ModDataStorage.Mod.Map.Basic?.FirstOrDefault();
                if (placeholder == null)
                {
                    Logger.AddLog(StaticLocalisation.GetString("Log.MapViewerPanel.NoBasicEntities"));
                    return (null, null);
                }

                var restoredInspector = new GenericViewer(placeholder.GetType(), placeholder);
                existingPanel.Content = restoredInspector;

                // DockPanelInfo.Content — обычное свойство без уведомления DockZone.
                // DockZone уже построила TabItem-ы со старым Content (null после layout restore).
                // Принудительно обновляем TabItem в DockZone.
                _dockManager.RefreshPanelContent(existingPanel);

                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MapViewerPanel.InspectorContentRestored"));
                return (existingPanel, restoredInspector);
            }

            // Панели нет вообще — создаём с нуля
            var newPlaceholder = ModDataStorage.Mod.Map.Basic?.FirstOrDefault();
            if (newPlaceholder == null)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MapViewerPanel.NoBasicEntities"));
                return (null, null);
            }

            var inspector = new GenericViewer(newPlaceholder.GetType(), newPlaceholder);
            var panel = new DockPanelInfo
            {
                Title = title,
                Content = inspector,
                CanClose = false,
                CanPin = true,
                IsPinned = true
            };

            _dockManager.AddPanel(panel, DockSide.Right);

            var registeredPanel = _view.FindPanelWithTitle(title);
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MapViewerPanel.InspectorCreated"));
            return (registeredPanel ?? panel, inspector);
        }

        // ──────────────────────────────────────────────
        // Подписки
        // ──────────────────────────────────────────────

        private void SubscribeToFileExplorerEvents()
        {
            _fileExplorer.ItemSelected += OnFileExplorerItemSelected;
            _fileExplorer.OpenItemRequested += OnFileExplorerOpenItemRequested;
            _fileExplorer.AddFileRequested += OnAddFileRequested;
            _fileExplorer.AddEntityRequested += OnAddEntityRequested;
            _fileExplorer.DeleteItemRequested += OnDeleteItemRequested;
            _fileExplorer.MoveFileRequested += OnMoveFileRequested;
            _fileExplorer.MoveEntityRequested += OnMoveEntityRequested;
            _fileExplorer.RenameRequested += OnRenameRequested;
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindowControls.FileExplorerSubscribed"));
        }

        private void SubscribeToFilesStripeEvents()
        {
            _filesStripe.FileOpenRequested += OnFilesStripeFileOpenRequested;
            _filesStripe.ActiveFileChanged += OnFilesStripeActiveFileChanged;
            _filesStripe.FileCloseRequested += OnFilesStripeFileCloseRequested;
            _filesStripe.AllFilesCloseRequested += OnFilesStripeAllFilesCloseRequested;
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindowControls.FilesStripeSubscribed"));
        }

        // ──────────────────────────────────────────────
        // FileExplorer события
        // ──────────────────────────────────────────────

        private void OnFileExplorerItemSelected(object sender, RoutedEventArgs e)
        {
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileExplorerItemSelected"));
        }

        /// <summary>
        /// DoubleClick в FileExplorer:
        /// — Если текущий режим Map — переключаем на Entity через ActivatePanel
        ///   (это вызовет OnRightPanelSelectionChanged → SwitchMode, который
        ///   сохранит пул Map и восстановит Entity)
        /// — Добавляем таб в пул Entity и открываем viewer
        /// </summary>
        private void OnFileExplorerOpenItemRequested(object sender, OpenItemRequestedEventArgs e)
        {
            if (e.Item == null) return;

            var displayName = GetItemDisplayName(e.Item);
            var uniqueId = GetUniqueItemId(e.Item);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.OpeningItem", displayName, uniqueId));

            // Если мы в режиме карты — сначала переключаем через клик на вкладку FileExplorer.
            // SwitchMode выполнится синхронно внутри ActivatePanel → OnRightPanelSelectionChanged.
            if (_currentMode != ModeEntity)
                _dockManager.ActivatePanel(_fileExplorerPanel);

            // Теперь гарантированно режим Entity — добавляем таб
            _filesStripe.AddTab(
                filePath: uniqueId,
                displayName: displayName,
                fileObject: e.Item,
                makeActive: true);

            // Обновляем снимок пула Entity
            SnapshotCurrentStripe();

            OpenItemInViewer(e.Item);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.ItemOpenedInStripe", displayName));
        }

        // ──────────────────────────────────────────────
        // FilesStripe события
        // ──────────────────────────────────────────────

        private void OnFilesStripeFileOpenRequested(object sender, FileTabEventArgs e)
        {
            OpenItemInViewer(e.Tab.FileObject);
            SnapshotCurrentStripe();
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.StripeTabOpened", e.Tab.DisplayName));
        }

        private void OnFilesStripeActiveFileChanged(object sender, FileTabEventArgs e)
        {
            OpenItemInViewer(e.Tab.FileObject);
            SnapshotCurrentStripe();
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.StripeActiveFileChanged", e.Tab.DisplayName));
        }

        private void OnFilesStripeFileCloseRequested(object sender, FileTabEventArgs e)
        {
            // Удаляем из пула текущего режима
            _pools[_currentMode].RemoveAll(s => s.FilePath == e.Tab.FilePath);

            var activeTab = _filesStripe.GetActiveTab();
            if (activeTab == null || activeTab.FilePath == e.Tab.FilePath)
            {
                _viewerContainer.Content = null;
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.ViewerClearedAfterTabClose"));
            }

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.StripeTabClosed", e.Tab.DisplayName));
        }

        private void OnFilesStripeAllFilesCloseRequested(object sender, EventArgs e)
        {
            _pools[_currentMode].Clear();
            _viewerContainer.Content = null;
            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.AllStripeTabsClosed"));
        }

        // ──────────────────────────────────────────────
        // Viewer
        // ──────────────────────────────────────────────

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
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.OpeningCreator",
                itemType.Name, creatorAttribute.CreatorType));

            try
            {
                switch (creatorAttribute.CreatorType)
                {
                    case ConfigCreatorType.GenericCreator:
                        _viewerContainer.Content = new GenericViewer(itemType, item);
                        break;

                    case ConfigCreatorType.CountryCreator:
                    case ConfigCreatorType.MapCreator:
                    case ConfigCreatorType.GenericGuiCreator:
                        CustomMessageBox.Show(
                            StaticLocalisation.GetString("Info.CreatorNotImplemented", creatorAttribute.CreatorType),
                            StaticLocalisation.GetString("Dialog.Info"),
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        break;

                    default:
                        CustomMessageBox.Show(
                            StaticLocalisation.GetString("Error.UnknownCreatorType", creatorAttribute.CreatorType),
                            StaticLocalisation.GetString("Dialog.Error"),
                            MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ──────────────────────────────────────────────
        // Операции с данными мода
        // ──────────────────────────────────────────────

        private void OnAddFileRequested(object sender, AddFileRequestedEventArgs e)
        {
            var category = e.Category;
            if (category.ItemType == null || !category.ItemType.IsGenericType) return;

            try
            {
                var typeToCreate = e.SpecificType ?? category.ItemType;
                var newFile = Activator.CreateInstance(typeToCreate);
                if (newFile != null && category.Items != null)
                {
                    category.Items.Add(newFile);
                    _fileExplorer.LoadModData();
                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileAdded", category.DisplayName));
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.AddFileError", ex.Message));
                CustomMessageBox.Show(StaticLocalisation.GetString("Error.AddFileFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAddEntityRequested(object sender, AddEntityRequestedEventArgs e)
        {
            var fileNode = e.FileNode;
            if (fileNode.ConfigType == null || fileNode.Entities == null) return;

            try
            {
                var typeToCreate = e.SpecificType ?? fileNode.ConfigType;
                if (typeToCreate.IsInterface || typeToCreate.IsAbstract)
                    throw new InvalidOperationException(
                        StaticLocalisation.GetString("Error.CannotCreateAbstractType", typeToCreate.Name));

                var newEntity = Activator.CreateInstance(typeToCreate);
                if (newEntity != null)
                {
                    fileNode.Entities.Add(newEntity);
                    _fileExplorer.LoadModData();
                    RefreshViewerIfNeeded(fileNode.File);
                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.EntityAdded", fileNode.DisplayName));
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.AddEntityError", ex.Message));
                CustomMessageBox.Show(StaticLocalisation.GetString("Error.AddEntityFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteItemRequested(object sender, DeleteItemRequestedEventArgs e)
        {
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
                    var uniqueId = GetUniqueItemId(fileNode.File);
                    foreach (var prop in modConfig.GetType().GetProperties())
                    {
                        if (prop.GetValue(modConfig) is System.Collections.IList list && list.Contains(fileNode.File))
                        {
                            list.Remove(fileNode.File);
                            e.Confirmed = true;
                            RemoveFromPool(uniqueId);
                            _filesStripe.RemoveTab(uniqueId);
                            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileDeleted", fileNode.DisplayName));
                            break;
                        }
                    }
                }
                else if (e.Item is ModItemNode modItem && modItem.ParentFile != null)
                {
                    var uniqueId = GetUniqueItemId(modItem.Item);
                    var entitiesProp = modItem.ParentFile.GetType().GetProperty("Entities");
                    if (entitiesProp?.GetValue(modItem.ParentFile) is System.Collections.IList entities
                        && entities.Contains(modItem.Item))
                    {
                        entities.Remove(modItem.Item);
                        e.Confirmed = true;
                        RemoveFromPool(uniqueId);
                        _filesStripe.RemoveTab(uniqueId);
                        RefreshViewerIfNeeded(modItem.ParentFile);
                        Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.EntityDeleted", modItem.DisplayName));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.DeleteError", ex.Message));
                CustomMessageBox.Show(StaticLocalisation.GetString("Error.DeleteFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                    { list.Remove(e.SourceFile); break; }
                }
                e.TargetCategory.Items?.Add(e.SourceFile);
                _fileExplorer.LoadModData();

                var uniqueId = GetUniqueItemId(e.SourceFile);
                var snap = GetSnapFromPool(_currentMode, uniqueId);
                if (snap != null)
                {
                    var newName = GetItemDisplayName(e.SourceFile);
                    snap.DisplayName = newName;
                    _filesStripe.RemoveTab(uniqueId);
                    _filesStripe.AddTab(uniqueId, newName, e.SourceFile, makeActive: snap.IsActive);
                }

                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileMoved", e.TargetCategory.DisplayName));
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.MoveError", ex.Message));
                CustomMessageBox.Show(StaticLocalisation.GetString("Error.MoveFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnMoveEntityRequested(object sender, MoveEntityRequestedEventArgs e)
        {
            try
            {
                var srcEntities = e.SourceFile.GetType().GetProperty("Entities")
                    ?.GetValue(e.SourceFile) as System.Collections.IList;
                var tgtEntities = e.TargetFile.GetType().GetProperty("Entities")
                    ?.GetValue(e.TargetFile) as System.Collections.IList;

                if (srcEntities != null && tgtEntities != null && srcEntities.Contains(e.SourceItem))
                {
                    srcEntities.Remove(e.SourceItem);
                    tgtEntities.Add(e.SourceItem);
                    _fileExplorer.LoadModData();
                    RefreshViewerIfNeeded(e.SourceFile);
                    RefreshViewerIfNeeded(e.TargetFile);
                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.EntityMoved"));
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.MoveError", ex.Message));
                CustomMessageBox.Show(StaticLocalisation.GetString("Error.MoveFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnRenameRequested(object sender, RenameRequestedEventArgs e)
        {
            try
            {
                if (e.Item is ConfigFileNode fileNode)
                {
                    var uniqueId = GetUniqueItemId(fileNode.File);
                    var renameMethod = fileNode.File.GetType().GetMethod("Rename");
                    if (renameMethod?.Invoke(fileNode.File, new object[] { e.NewName }) is true)
                    {
                        fileNode.DisplayName = e.NewName;
                        e.Success = true;
                        RenameTabInStripe(uniqueId, e.NewName, fileNode.File);
                        Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.FileRenamed", e.NewName));
                    }
                    else
                    {
                        CustomMessageBox.Show(StaticLocalisation.GetString("Error.RenameFailed", e.NewName),
                            StaticLocalisation.GetString("Dialog.Warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else if (e.Item is ModItemNode modItem)
                {
                    var uniqueId = GetUniqueItemId(modItem.Item);
                    if (TrySetEntityId(modItem.Item, e.NewName))
                    {
                        modItem.DisplayName = e.NewName;
                        modItem.Id = e.NewName;
                        e.Success = true;
                        RenameTabInStripe(uniqueId, e.NewName, modItem.Item);
                        RefreshViewerIfNeeded(modItem.Item);
                        Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.EntityRenamed", e.NewName));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(StaticLocalisation.GetString("Log.MainWindow.RenameError", ex.Message));
                CustomMessageBox.Show(StaticLocalisation.GetString("Error.RenameFailed", ex.Message),
                    StaticLocalisation.GetString("Dialog.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ──────────────────────────────────────────────
        // Вспомогательные методы
        // ──────────────────────────────────────────────

        private void RefreshViewerIfNeeded(object fileOrItem)
        {
            var activeTab = _filesStripe.GetActiveTab();
            if (activeTab?.FileObject == fileOrItem)
            {
                OpenItemInViewer(fileOrItem);
                Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.ViewerRefreshed"));
            }
        }

        private void RenameTabInStripe(string uniqueId, string newDisplayName, object fileObject)
        {
            // Обновляем снимок в пуле
            var snap = GetSnapFromPool(_currentMode, uniqueId);
            if (snap != null) snap.DisplayName = newDisplayName;

            // Перестраиваем таб в FilesStripe
            var wasActive = _filesStripe.GetActiveTab()?.FilePath == uniqueId;
            _filesStripe.RemoveTab(uniqueId);
            _filesStripe.AddTab(uniqueId, newDisplayName, fileObject, makeActive: wasActive);

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MainWindow.StripeTabRenamed", newDisplayName));
        }

        /// <summary>Удаляет снимок из всех пулов (например при удалении сущности).</summary>
        private void RemoveFromPool(string filePath)
        {
            foreach (var pool in _pools.Values)
                pool.RemoveAll(s => s.FilePath == filePath);
        }

        private StripeTabSnapshot GetSnapFromPool(string mode, string filePath)
            => _pools[mode].FirstOrDefault(s => s.FilePath == filePath);

        // ──────────────────────────────────────────────
        // Статические хелперы
        // ──────────────────────────────────────────────

        public static string GetUniqueItemId(object item)
        {
            if (item == null) return Guid.NewGuid().ToString();
            var typeName = item.GetType().Name;

            if (item is IConfig config && config.Id != null)
                return $"{typeName}_{config.Id}_{(config.FileFullPath ?? "unknown").GetHashCode()}";
            if (item is IGfx gfx && gfx.Id != null)
                return $"{typeName}_{gfx.Id}_{GetItemFilePath(item).GetHashCode()}";

            var path = item.GetType().GetProperty("FileFullPath")?.GetValue(item) as string;
            return !string.IsNullOrEmpty(path)
                ? $"{typeName}_{path.GetHashCode()}"
                : $"{typeName}_{item.GetHashCode()}";
        }

        public static string GetItemDisplayName(object item)
        {
            if (item == null) return "Unknown";
            if (item is IConfig config && config.Id != null) return config.Id.ToString();
            if (item is IGfx gfx && gfx.Id != null) return gfx.Id.ToString();
            var fileName = item.GetType().GetProperty("FileName")?.GetValue(item) as string;
            return !string.IsNullOrEmpty(fileName) ? fileName : item.GetType().Name;
        }

        public static string GetItemFilePath(object item)
        {
            if (item == null) return string.Empty;
            if (item is IConfig config) return config.FileFullPath ?? string.Empty;
            return item.GetType().GetProperty("FileFullPath")?.GetValue(item) as string ?? string.Empty;
        }

        public static bool TrySetEntityId(object entity, string newName)
        {
            if (entity is IConfig config)
            {
                var idType = config.Id?.GetType() ?? typeof(Models.Types.Utils.Identifier);
                if (Activator.CreateInstance(idType, newName) is Models.Types.Utils.Identifier id)
                { config.Id = id; return true; }
            }
            else if (entity is IGfx gfx)
            {
                var idType = gfx.Id?.GetType() ?? typeof(Models.Types.Utils.Identifier);
                if (Activator.CreateInstance(idType, newName) is Models.Types.Utils.Identifier id)
                { gfx.Id = id; return true; }
            }
            return false;
        }
    }

    // ─── DTO снимка таба ─────────────────────────────────────────────────────────

    /// <summary>
    /// Снимок состояния одного таба FilesStripe для хранения в пуле режима.
    /// </summary>
    internal sealed class StripeTabSnapshot
    {
        public string FilePath { get; set; }
        public string DisplayName { get; set; }
        public object FileObject { get; set; }
        public bool IsPinned { get; set; }
        public bool IsActive { get; set; }
    }
}