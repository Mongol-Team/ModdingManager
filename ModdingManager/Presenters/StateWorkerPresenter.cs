using ModdingManager.Controls;
using ModdingManager.classes.utils;
using ModdingManager.classes.views;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.Extentions;
using ModdingManagerModels;
using ModdingManagerModels.Args;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using Polygon = System.Windows.Shapes.Polygon;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;
using Window = System.Windows.Window;

public class StateWorkerPresenter
{
    private readonly IStateWorkerView _view;
    private readonly StateWorkerHandler _handler;
    private Dictionary<int, System.Windows.Media.Color> _stateColors = new Dictionary<int, System.Windows.Media.Color>();
    private LoadingWindow _loadingWindow = new();

    public StateWorkerPresenter(IStateWorkerView view)
    {
        _view = view;
        _handler = new StateWorkerHandler();
        _view.Loaded += OnLoaded;
        _view.ShowIdsChanged += OnShowIdsChanged;
        _view.MapLayerChanged += OnMapLayerChanged;
        _view.MarkEvent += OnMarkEvent;
        _view.ProvinceTransferRequested += OnProvinceTransferRequested;
        _view.StateTransferRequested += OnStateTransferRequested;
        _view.SearchElement += SearchAction;
        
    }
   
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var window = _view as Window;
        if (window == null || !window.IsLoaded) return;

        var loadingWindow = new LoadingWindow
        {
            Owner = window,
            Message = "Извлекаем формы провинций с картинки..."
        };
        loadingWindow.SetProgressBounds(0, 4); 
        loadingWindow.Show();

        string modDirectory = ModManager.ModDirectory;
        _view.Display.Width = ModManager.Mod.Map.Bitmap.Width;
        _view.Display.Height = ModManager.Mod.Map.Bitmap.Height;

        loadingWindow.Message = "Рисуем провинции...";
        DrawProvinceLayer();
        loadingWindow.Progress = 1;
        await Dispatcher.Yield();

        loadingWindow.Message = "Рисуем штаты...";
        DrawStateLayer();
        loadingWindow.Progress = 2;
        await Dispatcher.Yield();

        loadingWindow.Message = "Рисуем страны...";
        DrawCountryLayer();
        loadingWindow.Progress = 3;
        await Dispatcher.Yield();

        loadingWindow.Message = "Рисуем стратегические регионы...";
        DrawStrategicLayer(); 
        loadingWindow.Progress = 4;

        loadingWindow.EndLoading(); 

        UpdateLayer("PROVINCE");
    }
    private void OnShowIdsChanged(bool showIds, string layer)
    {
        UpdateIdVisibility(layer);
    }
    #region Events
    private void OnMarkEvent(MarkEventArg arg)
    {
        object targetObject = arg.MarkedState ?? (object)arg.MarkedRegion ?? arg.MarkedCountry ?? (object)arg.MarkedProvince;

        if (targetObject != null)
        {
            var viewer = new ClassViewer
            {
                Width = 195,
                BuildingContent = targetObject,
                ElementOrientation = ClassViewer.ContentOrientation.Left,
            };
            viewer.OnPropertyChange += OnPropertyChanging;

            _view.Menu.Children.Clear();
            _view.Menu.Children.Add(viewer);

            viewer.Loaded += (s, e) =>
            {
                if (!double.IsNaN(viewer.ActualHeight))
                {
                    _view.Menu.Height = viewer.ActualHeight;
                }
                else
                {
                    // Альтернативный вариант, если ActualHeight тоже NaN
                    viewer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    viewer.Arrange(new Rect(viewer.DesiredSize));
                    _view.Menu.Height = viewer.DesiredSize.Height;
                }
            };
        }
    }
    private void OnPropertyChanging(object sender, PropertyChangedEventArg e)
    {
        if (sender is ClassViewer)
        {
            var viewer = sender as ClassViewer;
            if (viewer.BuildingContent is ProvinceConfig)
            {
                var prov = viewer.BuildingContent as ProvinceConfig;
                _handler.ChangeProvince(prov);
            }
            if (viewer.BuildingContent is StateConfig)
            {
                var state = viewer.BuildingContent as StateConfig;
                _handler.ChangeState(state, e.OldValue.ToString(), e.NewValue.ToString());
            }
        }
    }
    private void OnProvinceTransferRequested(ProvinceTransferArg arg)
    {
        UpdateDataForProvinceTransfer(arg);
    }

    private void OnStateTransferRequested(StateTransferArg arg)
    {
        UpdateVisualsForStateTransfer(arg);
    }
    private void UpdateIdVisibility(string currentLayer)
    {
        _view.ProvinceIDLayer.Visibility = Visibility.Collapsed;
        _view.StateIDLayer.Visibility = Visibility.Collapsed;
        _view.CountryIDLayer.Visibility = Visibility.Collapsed;
        _view.StrategicIDLayer.Visibility = Visibility.Collapsed;

        if (_view.IsShowIdsChecked)
        {
            switch (currentLayer)
            {
                case "PROVINCE":
                    _view.ProvinceIDLayer.Visibility = Visibility.Visible;
                    break;
                case "STATE":
                    _view.StateIDLayer.Visibility = Visibility.Visible;
                    break;
                case "COUNTRY":
                    _view.CountryIDLayer.Visibility = Visibility.Visible;
                    break;
                case "STRATEGIC":
                    _view.StrategicIDLayer.Visibility = Visibility.Visible;
                    break;
            }
        }
        UpdateElementsVisibility(_view.ProvinceIDLayer, currentLayer == "PROVINCE");
        UpdateElementsVisibility(_view.StateIDLayer, currentLayer == "STATE");
        UpdateElementsVisibility(_view.CountryIDLayer, currentLayer == "COUNTRY");
        UpdateElementsVisibility(_view.StrategicIDLayer, currentLayer == "STRATEGIC");
    }

    private void UpdateElementsVisibility(Canvas canvas, bool isVisible)
    {
        foreach (var child in canvas.Children)
        {
            if (child is UIElement element)
            {
                element.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
    private void OnMapLayerChanged(string layer)
    {
        UpdateLayer(layer);
    }
    private void SearchAction(string currentLayer, int id)
    {
        if (string.IsNullOrWhiteSpace(currentLayer))
        {
            return;
        }
        switch (currentLayer)
        {
            case "PROVINCE":
                SearchProvince(id);
                break;
            case "STATE":
                SearchState(id);
                break;
            case "COUNTRY":
                SearchCountry(id);
                break;
            case "STRATEGIC":
                SearchStrategicRegion(id);
                break;
        }
    }
    public void SearchProvince(int id)
    {
        SearchAndCenterView(_view.ProvinceIDLayer, id.ToString());
        ProvinceConfig prov = ModManager.Mod.Map.Provinces.FirstOrDefault(p => p.Id.ToInt() == id);
        MarkEventArg markEventArg = new MarkEventArg
        {
            MarkedProvince = prov
        };
        OnMarkEvent(markEventArg);
    }

    public void SearchState(int id)
    {
        SearchAndCenterView(_view.StateIDLayer, id.ToString());
        StateConfig state = ModManager.Mod.Map.States.FirstOrDefault(s => s.Id.ToInt() == id);
        MarkEventArg markEventArg = new MarkEventArg
        {
            MarkedState = state
        };
    }

    public void SearchCountry(int id)
    {
        SearchAndCenterView(_view.CountryIDLayer, id.ToString());
        CountryConfig country = ModManager.Mod.Map.Countries.FirstOrDefault(c => c.Id.ToString() == id.ToString());
        MarkEventArg markEventArg = new MarkEventArg
        {
            MarkedCountry = country
        };
    }

    public void SearchStrategicRegion(int id)
    {
        SearchAndCenterView(_view.StrategicIDLayer, id.ToString());
        StrategicRegionConfig region = ModManager.Mod.Map.StrategicRegions.FirstOrDefault(r => r.Id.ToInt() == id);
        MarkEventArg markEventArg = new MarkEventArg
        {
            MarkedRegion = region
        };
    }

    private void SearchAndCenterView(Canvas canvas, string searchText)
    {
        if (canvas == null) return;

        foreach (var child in canvas.Children)
        {
            if (child is TextBlock textBlock && textBlock.Text == searchText)
            {
                double left = Canvas.GetLeft(textBlock);
                double top = Canvas.GetTop(textBlock);

                if (double.IsNaN(left) || double.IsNaN(top))
                {
                    left = textBlock.RenderTransform.Value.OffsetX;
                    top = textBlock.RenderTransform.Value.OffsetY;

                    if (double.IsNaN(left) || double.IsNaN(top))
                    {
                        MessageBox.Show("Cannot determine text block position");
                        return;
                    }
                }

                Point center = new Point(
                    left + textBlock.ActualWidth / 2,
                    top + textBlock.ActualHeight / 2);

                _view.Scene.SetViewCenter(center);
                return;
            }
        }

        MessageBox.Show($"Element with ID {searchText} not found in {canvas.Name}");
    }
    public void UpdateLayer(string layer)
    {
        _view.ProvinceLayer.Visibility = Visibility.Collapsed;
        _view.StateLayer.Visibility = Visibility.Collapsed;
        _view.CountryLayer.Visibility = Visibility.Collapsed;
        _view.StrategicLayer.Visibility = Visibility.Collapsed;

        
        switch (layer)
        {
            case "PROVINCE":
                _view.ProvinceLayer.Visibility = Visibility.Visible;
                break;
            case "STATE":
                _view.StateLayer.Visibility = Visibility.Visible;
                break;
            case "COUNTRY":
                _view.CountryLayer.Visibility = Visibility.Visible;
                break;
            case "STRATEGIC":
                _view.StrategicLayer.Visibility = Visibility.Visible;
                break;
        }

        UpdateIdVisibility(layer);

        _view.CurrentMapState = layer;
    }
    #endregion

    #region Drawing Methods
    private void DrawProvince(Canvas canvas, ProvinceConfig province, System.Drawing.Color color, string tooltipText = null)
    {
        if (province.Shape?.ContourPoints == null || province.Shape.ContourPoints.Length < 3)
            return;

        var poly = new Polygon
        {
            Fill = new SolidColorBrush(color.ToMediaColor()),
            Points = new PointCollection(province.Shape.ContourPoints.ToWindowsPoints()),
            StrokeThickness = 0,
            ToolTip = tooltipText ?? $"Province: {province.Id}",
            Tag = province.Id
        };

        canvas.Children.Add(poly);
    }
    private void UpdateDataForProvinceTransfer(ProvinceTransferArg arg)
    {
        // Определяем активный слой
        switch (_view.CurrentMapState)
        {
            case "STATE":
                UpdateStateLayer(arg);
                UpdateCountryLayerAfterProvinceTransfer(arg);
                break;
            case "STRATEGIC":
                UpdateRegionLayer(arg);
                break;
            case "COUNTRY":
                UpdateCountryLayerAfterProvinceTransfer(arg);
                break;
        }
    }

    private void UpdateVisualsForStateTransfer(StateTransferArg arg)
    {
        if (_view.CurrentMapState == "COUNTRY")
        {
            UpdateCountryLayerAfterStateTransfer(arg);
        }
    }
    private void UpdateRegionLayer(ProvinceTransferArg arg)
    {
        var sourceRegion = ModManager.Mod.Map.StrategicRegions.FirstOrDefault(s => s.Id == arg.SourceRegion?.Id);
        var targetRegion = ModManager.Mod.Map.StrategicRegions.FirstOrDefault(s => s.Id == arg.TargetRegion?.Id);
        var province = ModManager.Mod.Map.Provinces.FirstOrDefault(p => p.Id.ToInt() == arg.ProvinceId);

        if (targetRegion == null || province == null)
            return;

        sourceRegion?.Provinces.Remove(province);
        targetRegion.Provinces.Add(province);

        UpdateProvinceVisual(_view.StrategicRenderLayer, arg.ProvinceId, arg.TargetRegion.Color.ToMediaColor());
        UpdateProvinceTooltip(_view.StrategicRenderLayer, arg.ProvinceId,
            $"Province: {arg.ProvinceId}\nRegion: {arg.TargetRegion.Id}");

        //_handler.MoveProvinceToStrategicRegion(arg.ProvinceId, sourceRegion, targetRegion);
        UpdateTextBlockPosition(arg.TargetRegion.Id.ToInt(), _view.CurrentMapState);
        if (arg.SourceRegion != null)
        {
            UpdateTextBlockPosition(arg.SourceRegion.Id.ToInt(), _view.CurrentMapState);
        }
    }
    private void UpdateStateLayer(ProvinceTransferArg arg)
    {
        var sourceState = ModManager.Mod.Map.States.FirstOrDefault(s => s.Id == arg.SourceState?.Id);
        var targetState = ModManager.Mod.Map.States.FirstOrDefault(s => s.Id == arg.TargetState?.Id);
        var province = ModManager.Mod.Map.Provinces.FirstOrDefault(p => p.Id.ToInt() == arg.ProvinceId);

        if (targetState == null || province == null)
            return;

        sourceState?.Provinces.Remove(province);
        targetState.Provinces.Add(province);

        UpdateProvinceVisual(_view.StateRenderLayer, arg.ProvinceId, arg.TargetState.Color.ToMediaColor());
        UpdateProvinceTooltip(_view.StateRenderLayer, arg.ProvinceId,
            $"Province: {arg.ProvinceId}\nState: {arg.TargetState.Id}");

        //_handler.MoveProvinceToState(arg.ProvinceId, sourceState, targetState);
        UpdateTextBlockPosition(arg.TargetState.Id.HasValue() ? arg.TargetState.Id.ToInt()  : -1, _view.CurrentMapState);
        if (arg.SourceState != null)
        {
            UpdateTextBlockPosition(arg.SourceState.Id.HasValue() ? arg.SourceState.Id.ToInt() : -1, _view.CurrentMapState);
        }
    }


    private void UpdateCountryLayerAfterProvinceTransfer(ProvinceTransferArg arg)
    {
        var state = ModManager.Mod.Map.States.FirstOrDefault(s => s.Id == arg.TargetState?.Id);
        if (state == null)
            return;

        var country = GetCountryForState(state.Id.ToInt());
        if (country == null)
            return;

        UpdateProvinceVisual(_view.CountryRenderLayer, arg.ProvinceId, country.Color.ToMediaColor() ?? System.Windows.Media.Color.FromArgb(255, 128, 128, 128));
        UpdateProvinceTooltip(_view.CountryRenderLayer, arg.ProvinceId,
            $"Province: {arg.ProvinceId}\nCountry: {country.Id}");

        //_handler.MoveStateToCountry(state, country.Tag);
    }


    private void UpdateCountryLayerAfterStateTransfer(StateTransferArg arg)
    {
        var state = ModManager.Mod.Map.States.FirstOrDefault(s => s.Id.ToInt() == arg.StateId);
        if (state == null)
            return;

        var sourceCountry = ModManager.Mod.Map.Countries.FirstOrDefault(c =>
            c.States.Any(s => s.Id.ToInt() == arg.StateId));
        var targetCountry = ModManager.Mod.Map.Countries.FirstOrDefault(c =>
            c.Id.ToString() == arg.TargetCountryTag);

        if (targetCountry == null)
            return;

        sourceCountry?.States.Remove(state);
        targetCountry.States.Add(state);

        foreach (var province in state.Provinces)
        {
            UpdateProvinceVisual(_view.CountryRenderLayer, province.Id.ToInt(), targetCountry.Color.ToMediaColor() ?? System.Windows.Media.Color.FromArgb(255, 128, 128, 128));
            UpdateProvinceTooltip(_view.CountryRenderLayer, province.Id.ToInt(),
                $"Province: {province.Id}\nCountry: {targetCountry.Id.ToString()}");
        }

    }

    private List<TextBlock> CreateTextBlocksUniversal(IEnumerable<ProvinceConfig> provinces, string text = null)
    {
        var result = new List<TextBlock>();
        var provinceList = provinces?.Where(p => p?.Shape?.ContourPoints?.Length >= 3).ToList() ?? new();

        if (provinceList.Count == 0) return result;

        if (text != null)
        {
            // Групповая отрисовка – как раньше
            var allPoints = provinceList.SelectMany(p => p.Shape.ContourPoints).ToList();
            if (allPoints.Count == 0) return result;

            var bounds = GetBoundingBox(allPoints);
            double size = Math.Max(bounds.Width, bounds.Height);
            double fontSize = CalculateFontSize(size);
            var displayText = new TextBlock
            {
                Text = text,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = fontSize,
                Tag = "Group"
            };

            displayText.RenderTransform = new TranslateTransform(
                bounds.Center.X - (text.Length * fontSize * 0.6) / 2,
                bounds.Center.Y - fontSize / 2
            );

            result.Add(displayText);
        }
        else
        {
            foreach (var prov in provinceList)
            {
                var points = prov.Shape.ContourPoints;
                if (points.Length < 3) continue;

                var bounds = GetBoundingBox(points);
                double size = Math.Min(bounds.Width, bounds.Height);
                double fontSize = CalculateFontSize(size);
                string displayText = prov.Id.ToString();
                double textWidth = displayText.Length * fontSize * 0.6;

                var candidatePoint = FindBestCenterPoint(points);
                if (!IsInsidePolygon(candidatePoint, points))
                    candidatePoint = bounds.Center;

                result.Add(new TextBlock
                {
                    Text = displayText,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    FontSize = fontSize,
                    Tag = prov.Id,
                    RenderTransform = new TranslateTransform(
                        candidatePoint.X - textWidth / 2,
                        candidatePoint.Y - fontSize / 2
                    )
                });
            }
        }

        return result;
    }

    private void DrawStrategicLayer()
    {
        // Исправление 1: Очищаем правильные слои
        _view.StrategicRenderLayer.Children.Clear();
        _view.StrategicIDLayer.Children.Clear();

        if (ModManager.Mod.Map?.StrategicRegions == null || ModManager.Mod.Map.Provinces == null)
            return;

        var assignedProvinceIds = new HashSet<int>();

        foreach (var reg in ModManager.Mod.Map.StrategicRegions)
        {
            // Исправление 2: Получаем реальные провинции
            var provincesInRegion = reg.Provinces
                .Join(ModManager.Mod.Map.Provinces,
                    pr => pr.Id,
                    p => p.Id,
                    (pr, p) => p)
                .ToList();

            // Исправление 3: Добавляем метки в правильный слой
            var textBlocks = CreateTextBlocksUniversal(provincesInRegion, reg.Id.ToString());
            foreach (var textBlock in textBlocks)
            {
                _view.StrategicIDLayer.Children.Add(textBlock);
            }

            foreach (var province in provincesInRegion)
            {
                // Исправление 4: Отрисовываем в StrategicRenderLayer
                DrawProvince(_view.StrategicRenderLayer, province, reg.Color,
                    $"Province: {province.Id}\nStrategic: {reg.Id}");
                assignedProvinceIds.Add(province.Id.ToInt());
            }
        }

        var unassigned = ModManager.Mod.Map.Provinces.Where(p => !assignedProvinceIds.Contains(p.Id.ToInt()));
        foreach (var province in unassigned)
        {
            // Исправление 5: Отрисовываем в правильный слой
            DrawProvince(_view.StrategicRenderLayer, province, Colors.Gray.ToDrawingColor(),
                $"Province: {province.Id}\nStrategic: NONE");
        }
    }
    private void DrawProvinceLayer()
    {
        var provinces = _handler.ComputeProvinceShapes();

        _view.ProvinceRenderLayer.Children.Clear();
        _view.ProvinceIDLayer.Children.Clear();

        foreach (var province in provinces)
        {
            if (province.Shape?.ContourPoints == null || province.Shape.ContourPoints.Length < 3)
                continue;

            DrawProvince(_view.ProvinceRenderLayer, province, province.Shape.FillColor, $"Province: {province.Id}");

            var textBlock = CreateTextBlocksUniversal(new List<ProvinceConfig> { province }).First();
            _view.ProvinceIDLayer.Children.Add(textBlock);
        }
    }

    private void DrawStateLayer()
    {
        _view.StateRenderLayer.Children.Clear();
        _view.StateIDLayer.Children.Clear();

        if (ModManager.Mod.Map?.States == null || ModManager.Mod.Map.Provinces == null)
            return;

        foreach (var province in ModManager.Mod.Map.Provinces)
        {
            var state = ModManager.Mod.Map.States.FirstOrDefault(s => s.Provinces.Any(p => p.Id == province.Id));

            if (state != null)
            {
                DrawProvince(_view.StateRenderLayer, province, state.Color, $"Province: {province.Id}\nState: {state.Id}");
            }
            else
            {
                DrawProvince(_view.StateRenderLayer, province, Colors.Gray.ToDrawingColor(), $"Province: {province.Id}\nState: NONE");
            }
        }

        foreach (var state in ModManager.Mod.Map.States)
        {
            if (state.Provinces.Count == 0)
                continue;

            var provinces = ModManager.Mod.Map.Provinces
                .Where(p => state.Provinces.Any(sp => sp.Id == p.Id))
                .ToList();

            if (provinces.Count == 0)
                continue;

            var textBlocks = CreateTextBlocksUniversal(provinces, state.Id.ToString());
            foreach (var textBlock in textBlocks)
            {
                _view.StateIDLayer.Children.Add(textBlock);
            }
        }
    }
    private void DrawCountryLayer()
    {
        _view.CountryRenderLayer.Children.Clear();
        _view.CountryIDLayer.Children.Clear();

        if (ModManager.Mod.Map?.Countries == null || ModManager.Mod.Map.Provinces == null)
            return;
        
        var provinceToCountry = new Dictionary<int, CountryConfig>();

        foreach (var country in ModManager.Mod.Map.Countries)
        {
            if (country.States == null) continue;

            foreach (var state in country.States)
            {
                if (state.Provinces == null) continue;

                foreach (var province in state.Provinces)
                {
                    // Сохраняем связь провинция -> страна
                    provinceToCountry[province.Id.ToInt()] = country;
                }
            }
        }

        // Отрисовываем провинции цветом их страны
        foreach (var province in ModManager.Mod.Map.Provinces)
        {
            if (provinceToCountry.TryGetValue(province.Id.ToInt(), out var country))
            {
                DrawProvince(
                    _view.CountryRenderLayer,
                    province,
                    country.Color ?? System.Drawing.Color.FromArgb(255, 128, 128, 128),
                    $"Province: {province.Id}\nCountry: {country.Id}"
                );
            }
            else
            {
                DrawProvince(
                    _view.CountryRenderLayer,
                    province,
                    Colors.Gray.ToDrawingColor(),
                    $"Province: {province.Id}\nCountry: NONE"
                );
            }
        }

        // Отрисовываем метки стран
        foreach (var country in ModManager.Mod.Map.Countries)
        {
            // Собираем все провинции страны
            var allProvinces = new List<ProvinceConfig>();

            if (country.States != null)
            {
                foreach (var state in country.States)
                {
                    if (state.Provinces != null)
                    {
                        foreach (var province in state.Provinces)
                        {
                            var fullProvince = ModManager.Mod.Map.Provinces.FirstOrDefault(p => p.Id == province.Id);
                            if (fullProvince != null)
                            {
                                allProvinces.Add(fullProvince);
                            }
                        }
                    }
                }
            }

            if (allProvinces.Count > 0)
            {
                var textBlocks = CreateTextBlocksUniversal(allProvinces, country.Id.ToString());
                foreach (var textBlock in textBlocks)
                {
                    _view.CountryIDLayer.Children.Add(textBlock);
                }
            }
        }
    }
    #endregion
    #region Helper Methods
    private double CalculateFontSize(double size)
    {
        const double MinVisibleSize = 1;
        const double MaxFontSize = 25.0;
        const double MinFontSize = 1.0;
        const double ScalingFactor = 0.2;

        if (size < MinVisibleSize)
            return 0;
        double fontSize = size * ScalingFactor;

        return Math.Min(MaxFontSize, Math.Max(MinFontSize, fontSize));
    }
    private void UpdateProvinceVisual(Canvas layer, int? provinceId, System.Windows.Media.Color color)
    {
        foreach (var child in layer.Children)
        {
            if (child is Polygon poly && poly.Tag is int id && id == provinceId)
            {
                poly.Fill = new SolidColorBrush(color);
                break;
            }
        }
    }
    private (double Width, double Height, System.Drawing.Point Center) GetBoundingBox(IEnumerable<System.Drawing.Point> points)
    {
        double minX = points.Min(p => p.X);
        double maxX = points.Max(p => p.X);
        double minY = points.Min(p => p.Y);
        double maxY = points.Max(p => p.Y);
        return (maxX - minX, maxY - minY, new System.Drawing.Point((int)((minX + maxX) / 2), (int)((minY + maxY) / 2)));
    }

    private System.Drawing.Point FindBestCenterPoint(System.Drawing.Point[] contour)
    {
        // Пример: средняя точка между крайними вершинами
        var xAvg = contour.Average(p => p.X);
        var yAvg = contour.Average(p => p.Y);
        return new System.Drawing.Point((int)(xAvg), (int)(yAvg));
    }

    private bool IsInsidePolygon(System.Drawing.Point point, System.Drawing.Point[] polygon)
    {
        bool result = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            if ((polygon[i].Y < point.Y && polygon[j].Y >= point.Y) || (polygon[j].Y < point.Y && polygon[i].Y >= point.Y))
            {
                if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                    result = !result;
            }
            j = i;
        }
        return result;
    }

    private void UpdateProvinceTooltip(Canvas layer, int? provinceId, string tooltip)
    {
        foreach (var child in layer.Children)
        {
            if (child is Polygon poly && poly.Tag is int id && id == provinceId)
            {
                poly.ToolTip = tooltip;
                break;
            }
        }
    }
    private void UpdateTextBlockPosition(int entityId, string layerType)
    {
        Canvas targetLayer = layerType switch
        {
            "COUNTRY" => _view.CountryIDLayer,
            "STATE" => _view.StateIDLayer,
            "STRATEGIC" => _view.StrategicIDLayer,
            _ => null
        };

        if (targetLayer == null) return;

        // Находим нужный TextBlock на слое
        TextBlock targetTextBlock = targetLayer.Children
            .OfType<TextBlock>()
            .FirstOrDefault(tb => tb.Text == entityId.ToString());

        if (targetTextBlock == null) return;

        // Получаем список провинций для этого entity
        List<ProvinceConfig> provinces = layerType switch
        {
            "COUNTRY" => ModManager.Mod.Map.Countries
                .FirstOrDefault(c => c.Id.ToString() == entityId.ToString())?
                .States.SelectMany(s => s.Provinces)
                .ToList(),
            "STATE" => ModManager.Mod.Map.States
                .FirstOrDefault(s => s.Id.ToInt() == entityId)?
                .Provinces.ToList(),
            "STRATEGIC" => ModManager.Mod.Map.StrategicRegions
                .FirstOrDefault(sr => sr.Id.ToInt() == entityId)?
                .Provinces.ToList(),
            _ => null
        };

        if (provinces == null || provinces.Count == 0) return;

        // Вычисляем новый bounding box
        var allPoints = provinces
            .Where(p => p?.Shape != null)
            .SelectMany(p => p.Shape.ContourPoints ?? Array.Empty<System.Drawing.Point>())
            .ToList();

        if (allPoints.Count == 0) return;

        double minX = allPoints.Min(p => p.X);
        double maxX = allPoints.Max(p => p.X);
        double minY = allPoints.Min(p => p.Y);
        double maxY = allPoints.Max(p => p.Y);

        // Вычисляем размер и новый шрифт
        double size = Math.Min(maxX - minX, maxY - minY);
        double newFontSize = CalculateFontSize(size);

        if (newFontSize <= 0) return;

        // Вычисляем новый центр (используем среднее центров провинций как в CreateTextBlocksUniversal)
        double centerX = provinces.Average(p => p.Shape.Pos.X);
        double centerY = provinces.Average(p => p.Shape.Pos.Y);

        // Обновляем свойства TextBlock
        targetTextBlock.FontSize = newFontSize;
        double textWidth = targetTextBlock.Text.Length * (newFontSize * 0.6);

        targetTextBlock.RenderTransform = new TranslateTransform(
            centerX - textWidth / 2,
            centerY - newFontSize / 2
        );
    }
    private CountryConfig GetCountryForState(int? stateId)
    {
        return ModManager.Mod.Map.Countries?.FirstOrDefault(c =>
            c.States?.Any(s => s.Id.ToInt() == stateId) == true);
    }
    #endregion

}

