using Application;
using ViewControls;
using ViewInterfaces;
using ViewPresenters;
using Models.Args;
using Models.Types.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Models.Configs;


namespace View
{
    public partial class MapWorkerWindow : Window, IStateWorkerView
    {
        private System.Windows.Point _mousePosition;
        public event Action<string> MapLayerChanged;
        public event Action<ProvinceTransferArg> ProvinceTransferRequested;
        public event Action<StateTransferArg> StateTransferRequested;
        public event Action<string, int> SearchElement;
        public event Action<MarkEventArg> MarkEvent;
        private StateWorkerPresenter _presenter;
        public event Action<string> MapChanged;
        public MarkEventArg _markedElement;
        public string CurrentMapLayer { get; set; } = "PROVINCE"; // Начальный слой по умолчанию
        public MapConfig Map
        {
            get;
            set;
        }
        public event Action<bool, string> ShowIdsChanged;
        public bool IsShowIdsChecked => DisplayIdsBox.IsChecked ?? false;
        public MapWorkerWindow()
        {
            InitializeComponent();
            _presenter = new StateWorkerPresenter(this);
            DisplayIdsBox.Checked += (s, e) => ShowIdsChanged?.Invoke(true, CurrentMapState);
            DisplayIdsBox.Unchecked += (s, e) => ShowIdsChanged?.Invoke(false, CurrentMapState);
        }
        public event RoutedEventHandler Loaded
        {
            add => base.Loaded += value;
            remove => base.Loaded -= value;
        }
        public Canvas Display
        {
            get => DisplayView;
            set => DisplayView = value;
        }

        public Canvas ProvinceIDLayer
        {
            get => IdProvCanv;
            set => IdProvCanv = value;
        }
        public Canvas ProvinceRenderLayer
        {
            get => ProvRenderCanv;
            set => ProvRenderCanv = value;
        }
        public Canvas StateIDLayer
        {
            get => IdStateCanv;
            set => IdStateCanv = value;
        }
        public Canvas StateRenderLayer
        {
            get => StateRenderCanv;
            set => StateRenderCanv = value;
        }
        public StackPanel Menu
        {
            get => MenuPanel;
            set => MenuPanel = value;
        }
        public Canvas StrategicIDLayer
        {
            get => IdStrategicCanv;
            set => IdStrategicCanv = value;
        }
        public Canvas StrategicRenderLayer
        {
            get => StrategicRenderCanv;
            set => StrategicRenderCanv = value;
        }
        public Canvas CountryIDLayer
        {
            get => IdCountryCanv;
            set => IdCountryCanv = value;
        }
        public Canvas CountryRenderLayer
        {
            get => CountryRenderCanv;
            set => CountryRenderCanv = value;
        }
        public Canvas StateLayer
        {
            get => StateLayerCanvas;
            set => StateLayerCanvas = value;
        }
        public Canvas CountryLayer
        {
            get => CountryLayerCanvas;
            set => CountryLayerCanvas = value;
        }
        public Canvas StrategicLayer
        {
            get => StrategicLayerCanvas;
            set => StrategicLayerCanvas = value;
        }
        public Canvas ProvinceLayer
        {
            get => ProvinceLayerCanvas;
            set => ProvinceLayerCanvas = value;
        }
        public SceneViewer Scene
        {
            get => DisplayScrollViewer;
            set => DisplayScrollViewer = value;
        }
        public string CurrentMapState { get; set; }

        #region Drag & Drop Implementation

        private ProvinceConfig _draggedProvince;
        private System.Windows.Point _dragStartPoint;



        private void Display_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed || e.ClickCount > 2) return;

            var hit = VisualTreeHelper.HitTest(Display, e.GetPosition(Display));
            if (hit?.VisualHit is Polygon polygon && polygon.Tag is int provinceId)
            {
                _draggedProvince = ModDataStorage.Mod.Map.Provinces.FirstOrDefault(p => p.Id.ToInt() == provinceId);
                _dragStartPoint = e.GetPosition(Display);
            }
        }

        private void Display_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedProvince == null || e.RightButton != MouseButtonState.Pressed) return;

            var currentPos = e.GetPosition(Display);
            if ((currentPos - _dragStartPoint).Length < 10) return;

            Cursor = Cursors.Hand;
        }

        private void Display_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_draggedProvince == null) return;

            var hit = VisualTreeHelper.HitTest(Display, e.GetPosition(Display));
            if (hit?.VisualHit is Polygon targetPolygon && targetPolygon.Tag is int targetProvinceId)
            {
                var targetProvince = ModDataStorage.Mod.Map.Provinces.FirstOrDefault(p => p.Id.ToInt() == targetProvinceId);

                switch (CurrentMapLayer)
                {
                    case "STATE":
                        HandleStateTransfer(_draggedProvince, targetProvince);
                        break;
                    case "STRATEGIC":
                        HandleRegionTransfer(_draggedProvince, targetProvince);
                        break;
                    case "COUNTRY":
                        HandleCountryTransfer(_draggedProvince, targetProvince);
                        break;
                }
            }

            _draggedProvince = null;
            Cursor = Cursors.Arrow;
        }

        #endregion

        #region Transfer Handlers

        private void HandleStateTransfer(ProvinceConfig sourceProvince, ProvinceConfig targetProvince)
        {
            var sourceState = GetStateForProvince(sourceProvince.Id);
            var targetState = GetStateForProvince(targetProvince.Id);

            if (targetState != null)
            {

                if (sourceState != null && sourceState.Id == targetState.Id)
                {
                    return;
                }
                ProvinceTransferRequested?.Invoke(new ProvinceTransferArg
                {
                    ProvinceId = sourceProvince.Id.HasValue() ? sourceProvince.Id.ToInt() : -1,
                    SourceState = sourceState,
                    TargetState = targetState
                });
            }
        }
        private void HandleRegionTransfer(ProvinceConfig sourceProvince, ProvinceConfig targetProvince)
        {
            var sourceState = GetRegionForProvince(sourceProvince.Id);
            var targetState = GetRegionForProvince(targetProvince.Id);

            if (targetState != null)
            {
                if (sourceState != null && sourceState.Id == targetState.Id)
                {
                    return;
                }
                ProvinceTransferRequested?.Invoke(new ProvinceTransferArg
                {
                    ProvinceId = sourceProvince.Id.HasValue() ? sourceProvince.Id.ToInt() : -1,
                    SourceRegion = sourceState,
                    TargetRegion = targetState
                });
            }
        }
        private void HandleCountryTransfer(ProvinceConfig sourceProvince, ProvinceConfig targetProvince)
        {
            var sourceState = GetStateForProvince(sourceProvince.Id);
            var targetState = GetStateForProvince(targetProvince.Id);

            if (targetState == null) return;

            var sourceCountry = GetCountryForState(sourceState.Id);
            var targetCountry = GetCountryForState(targetState.Id);

            if (targetCountry != null && sourceCountry.Id != targetCountry.Id)
            {
                if (sourceState != null && sourceCountry.Id != targetCountry.Id)
                {
                    return;
                }
                StateTransferRequested?.Invoke(new StateTransferArg
                {
                    StateId = sourceState.Id.HasValue() ? (int?)sourceState.Id.ToInt() : null,
                    SourceCountryTag = sourceCountry.Id.HasValue() ? sourceCountry.Id.ToString() : "None",
                    TargetCountryTag = targetCountry.Id.HasValue() ? targetCountry.Id.ToString() : "None"
                });
            }
        }

        #endregion
        #region Marking Implementation
        private void OnDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
                return;

            var hit = VisualTreeHelper.HitTest(Display, e.GetPosition(Display));
            if (hit?.VisualHit is not Polygon targetPolygon || targetPolygon.Tag is not int targetProvinceId)
                return;

            _markedElement = null;

            switch (CurrentMapLayer)
            {
                case "STATE":
                    var state = ModDataStorage.Mod.Map.States
                        .FirstOrDefault(s => s.Provinces.Any(p => p.Id.ToInt() == targetProvinceId));
                    if (state != null)
                    {
                        _markedElement = new MarkEventArg { MarkedState = state };
                    }
                    break;

                case "PROVINCE":
                    var province = ModDataStorage.Mod.Map.Provinces
                        .FirstOrDefault(p => p.Id.ToInt() == targetProvinceId);
                    if (province != null)
                    {
                        _markedElement = new MarkEventArg { MarkedProvince = province };
                    }
                    break;

                case "STRATEGIC":
                    var region = ModDataStorage.Mod.Map.StrategicRegions
                        .FirstOrDefault(r => r.Provinces.Any(p => p.Id.ToInt() == targetProvinceId));
                    if (region != null)
                    {
                        _markedElement = new MarkEventArg { MarkedRegion = region };
                    }
                    break;

                case "COUNTRY":
                    var country = ModDataStorage.Mod.Map.Countries
                        .FirstOrDefault(c => c.States.Any(s => s.Provinces.Any(p => p.Id.ToInt() == targetProvinceId)));
                    if (country != null)
                    {
                        _markedElement = new MarkEventArg { MarkedCountry = country };
                    }
                    break;
            }

            if (_markedElement != null)
            {
                HandleMarking(_markedElement);
            }

            _draggedProvince = null;
            Cursor = Cursors.Arrow;
        }

        #region Marking Handler
        private void HandleMarking(MarkEventArg arg)
        {
            if (arg != null)
            {
                MarkEvent?.Invoke(arg);
            }
        }
        #endregion
        #endregion
        #region Helper Methods

        private StateConfig GetStateForProvince(Identifier? provinceId)
        {
            return ModDataStorage.Mod.Map.States?.FirstOrDefault(s =>
                s.Provinces?.Any(p => p.Id == provinceId) == true);
        }
        private StrategicRegionConfig GetRegionForProvince(Identifier? provinceId)
        {
            return ModDataStorage.Mod.Map.StrategicRegions?.FirstOrDefault(s =>
                s.Provinces?.Any(p => p.Id == provinceId) == true);
        }
        private CountryConfig GetCountryForState(Identifier? stateId)
        {
            return ModDataStorage.Mod.Countries?.FirstOrDefault(c =>
                c.States?.Any(s => s.Id == stateId) == true);
        }

        #endregion
        #region Events
        private void HealerButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MapHealerWindow healerWindow = new();
            healerWindow.Show();
        }
        private void DraggingProvinceEvent(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MapChanged?.Invoke(CurrentMapLayer);
        }

        private void ProvinceLayerButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMapLayer = "PROVINCE";
            MapLayerChanged?.Invoke(CurrentMapLayer);
        }
        private void CountryLayerButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMapLayer = "COUNTRY";
            MapLayerChanged?.Invoke(CurrentMapLayer);
        }
        private void StateLayerButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMapLayer = "STATE";
            MapLayerChanged?.Invoke(CurrentMapLayer);
        }
        private void StrategicLayerButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMapLayer = "STRATEGIC";
            MapLayerChanged?.Invoke(CurrentMapLayer);
        }

        private void SearchByIdBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(SearchByIdBox.Text, out int res))
            {
                SearchElement?.Invoke(CurrentMapState, res);
            }
        }

        #endregion


    }
}
