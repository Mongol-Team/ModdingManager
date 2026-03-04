using Application;
using Application.Debugging;
using Application.utils;
using Controls;
using Controls.Docking;
using Models.Args;
using Models.Interfaces;
using System.Windows;

namespace ViewPresenters
{
    /// <summary>
    /// Presenter панели карты.
    ///
    /// Архитектура:
    /// — Центр DockManager: MapViewer (карта мода)
    /// — DockSide.Right:    одна зона с двумя вкладками:
    ///       • "FileExplorer" — дерево файлов мода (всегда доступна)
    ///       • "EntityInspector" — GenericViewer (открывается при DoubleClick по entity)
    ///
    /// Взаимодействие:
    ///   DoubleClick на entity → переключить правую зону на вкладку инспектора
    ///                           → показать данные entity в GenericViewer
    ///   EntityChanged в GenericViewer → обновить tooltip/лейблы полигона на карте
    /// </summary>
    public class MapViewerPanelPresenter
    {
        private readonly MapViewer _mapViewer;
        private readonly GenericViewer _entityInspector;
        private readonly DockPanelInfo _inspectorPanel;
        private readonly DockPanelInfo _fileExplorerPanel;
        private readonly DockManager _dockManager;

        // ─── Конструктор ─────────────────────────────────────────────────────────

        /// <param name="mapViewer">Контрол карты (в центре).</param>
        /// <param name="entityInspector">GenericViewer для инспектора entity (правая зона).</param>
        /// <param name="inspectorPanel">DockPanelInfo инспектора — нужен для активации вкладки.</param>
        /// <param name="fileExplorerPanel">DockPanelInfo FileExplorer'а — нужен для обратного переключения.</param>
        /// <param name="dockManager">DockManager для вызова ActivatePanel.</param>
        public MapViewerPanelPresenter(
            MapViewer mapViewer,
            GenericViewer entityInspector,
            DockPanelInfo inspectorPanel,
            DockPanelInfo fileExplorerPanel,
            DockManager dockManager)
        {
            _mapViewer = mapViewer ?? throw new ArgumentNullException(nameof(mapViewer));
            _entityInspector = entityInspector ?? throw new ArgumentNullException(nameof(entityInspector));
            _inspectorPanel = inspectorPanel ?? throw new ArgumentNullException(nameof(inspectorPanel));
            _fileExplorerPanel = fileExplorerPanel ?? throw new ArgumentNullException(nameof(fileExplorerPanel));
            _dockManager = dockManager ?? throw new ArgumentNullException(nameof(dockManager));

            SubscribeToMapViewerEvents();
            SubscribeToEntityInspectorEvents();

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MapViewerPanel.PresenterInitialized"));
        }

        // ─── Подписки ────────────────────────────────────────────────────────────

        private void SubscribeToMapViewerEvents()
        {
            // DoubleClick — открыть entity в инспекторе (правая панель)
            _mapViewer.OnEntityDoubleClick += OnMapEntityDoubleClick;

            // OnEntityMove — разрешаем, обновляем инспектор если нужно
            _mapViewer.OnEntityMove += OnMapEntityMove;

            // OnLayerChanged — возвращаем вкладку FileExplorer при смене слоя
            _mapViewer.OnLayerChanged += OnMapLayerChanged;

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MapViewerPanel.MapViewerSubscribed"));
        }

        private void SubscribeToEntityInspectorEvents()
        {
            // EntityChanged — обновляем полигон на карте при изменении данных в GenericViewer
            _entityInspector.EntityChanged += OnInspectorEntityChanged;

            Logger.AddDbgLog(StaticLocalisation.GetString("Log.MapViewerPanel.InspectorSubscribed"));
        }

        // ─── Обработчики MapViewer ───────────────────────────────────────────────

        /// <summary>
        /// Двойной клик по entity на карте:
        /// 1. Загружаем entity в GenericViewer
        /// 2. Переключаем правую вкладку на инспектор
        /// </summary>
        private void OnMapEntityDoubleClick(EntityDoubleClickEventArg e)
        {
            if (e?.Entity == null) return;

            Logger.AddLog(StaticLocalisation.GetString(
                "Log.MapViewerPanel.EntityDoubleClicked",
                e.Entity.GetType().Name,
                e.LayerName));

            _dockManager.ActivatePanel(_inspectorPanel);
            // Показываем entity в GenericViewer
            _entityInspector.ShowEntity(e.Entity);

            // Переключаем правую зону на вкладку инспектора
            var tes = ReferenceEquals(_inspectorPanel.Content, _entityInspector);
            Logger.AddDbgLog(StaticLocalisation.GetString(
                "Log.MapViewerPanel.InspectorActivated",
                e.Entity.GetType().Name));
        }

        /// <summary>
        /// Перемещение entity (drag & drop между регионами):
        /// — Разрешаем перемещение
        /// — Если перемещённый объект открыт в инспекторе — обновляем его
        /// </summary>
        private void OnMapEntityMove(EntityMoveEventArg e)
        {
            e.AllowMove = true;

            Logger.AddLog(StaticLocalisation.GetString(
                "Log.MapViewerPanel.EntityMoved",
                e.BasicEntityId,
                e.SourceParent?.Id.ToString() ?? StaticLocalisation.GetString("MapViewerPanel.Unassigned"),
                e.TargetParent?.Id.ToString() ?? "null",
                e.LayerName));

            // Если перемещённый объект (или его родитель) открыт в инспекторе — обновляем
            if (_entityInspector.CurrentEntity != null)
            {
                var current = _entityInspector.CurrentEntity;
                if (ReferenceEquals(current, e.MovedChild) || ReferenceEquals(current, e.SourceParent))
                {
                    _entityInspector.ShowEntity(current);
                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.MapViewerPanel.InspectorRefreshedAfterMove"));
                }
            }
        }

        /// <summary>
        /// Смена слоя на карте — возвращаем правую зону на FileExplorer,
        /// инспектор очищать не нужно (данные пусть остаются до нового двойного клика).
        /// </summary>
        private void OnMapLayerChanged(string layerName)
        {

            Logger.AddDbgLog(StaticLocalisation.GetString(
                "Log.MapViewerPanel.LayerChanged", layerName));
        }

        // ─── Обработчики GenericViewer (инспектор) ───────────────────────────────

        /// <summary>
        /// Данные entity изменились в GenericViewer:
        /// обновляем tooltip и лейблы полигона на карте.
        /// </summary>
        private void OnInspectorEntityChanged(object entity)
        {
            if (entity == null) return;

            _mapViewer.RefreshEntityDisplay(entity);

            Logger.AddDbgLog(StaticLocalisation.GetString(
                "Log.MapViewerPanel.EntityDisplayRefreshedFromInspector",
                entity.GetType().Name));
        }
    }
}