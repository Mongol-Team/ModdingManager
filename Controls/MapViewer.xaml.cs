using Application;
using Application.Debugging;
using Application.Extensions;
using Application.Extentions;
using Application.utils;
using Application.utils.Math;
using Controls;
using Models.Args;
using Models.Configs;
using Models.Interfaces;
using Models.Types.Utils;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Polygon = System.Windows.Shapes.Polygon;

namespace Controls
{
    /// <summary>
    /// Оптимизированный контрол для отображения политической карты
    /// Поддерживает вложенность IMapEntity с переиспользованием Polygon объектов
    /// </summary>
    public partial class MapViewer : UserControl
    {
        #region Приватные поля

        private IPoliticalMap _politicalMap;
        private readonly Dictionary<string, LayerInfo> _layers = new();
        private string _currentLayer = "Basic";
        private IBasicMapEntity _draggedBasicEntity;
        private Point _dragStartPoint;
        private readonly DispatcherTimer _clickTimer;
        private MouseButton _lastClickButton;
        private Point _lastClickPosition;
        private bool _isDoubleClick = false;
        public bool _showIds = false;
        // Кэш полигонов: BasicEntity.Id -> Polygon
        private readonly Dictionary<string, BasicShapeCache> _pointsCache = new();

        #endregion

        #region События

        public event Action<EntityDoubleClickEventArg> OnEntityDoubleClick;
        public event Action<EntityMoveEventArg> OnEntityMove;
        public event Action<EntityClickEventArg> OnEntityLeftClick;
        public event Action<EntityClickEventArg> OnEntityRightClick;
        public event Action<string> OnLayerChanged;

        #endregion

        #region Конструктор

        public MapViewer()
        {
            InitializeComponent();
            _clickTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _clickTimer.Tick += ClickTimer_Tick;
        }

        #endregion

        #region Публичные методы

        public void Initialize(IPoliticalMap politicalMap)
        {
            if (politicalMap == null)
            {
                Logger.AddLog(StaticLocalisation.GetString("MapViewer.NullPoliticalMap"));
                throw new ArgumentNullException(nameof(politicalMap));
            }

            _politicalMap = politicalMap;

            if (_politicalMap.MapImage != null)
            {
                DisplayView.Width = _politicalMap.MapImage.Width;
                DisplayView.Height = _politicalMap.MapImage.Height;
            }

            ComputeProvinceShapes();
            CreatePolygonCache();
            CreateLayers();
            DrawLayers();
            CreateLayerButtons();
            SwitchToLayer("Basic");

            Logger.AddLog(StaticLocalisation.GetString("MapViewer.Initialized"));
        }

        public void SwitchToLayer(string layerName)
        {
            if (!_layers.ContainsKey(layerName)) return;

            _currentLayer = layerName;

            foreach (var layer in _layers.Values)
            {
                layer.ParentCanvas.Visibility = Visibility.Collapsed;
            }

            _layers[_currentLayer].ParentCanvas.Visibility = Visibility.Visible;

            // Восстанавливаем состояние ShowIds для нового слоя
            ShowIds(_showIds);

            OnLayerChanged?.Invoke(_currentLayer);
        }

        public void ShowIds(bool show)
        {
            // Скрываем IdCanvas у всех слоёв
            foreach (var layer in _layers.Values)
            {
                layer.IdCanvas.Visibility = Visibility.Collapsed;
            }

            // Показываем только для текущего активного слоя, если show=true
            if (show && _layers.TryGetValue(_currentLayer, out var currentLayerInfo))
            {
                currentLayerInfo.IdCanvas.Visibility = Visibility.Visible;
            }
        }

        public void SearchAndCenter(int entityId)
        {
            if (!_layers.ContainsKey(_currentLayer)) return;

            var layerInfo = _layers[_currentLayer];
            IBasicMapEntity foundBasic = null;

            if (_currentLayer == "Basic")
            {
                foundBasic = _politicalMap.Basic?.FirstOrDefault(b => b.Id.ToInt() == entityId);
            }
            else
            {
                var entity = layerInfo.Entities?.FirstOrDefault(e => e.Id.ToInt() == entityId);
                if (entity != null)
                {
                    foundBasic = entity.GetAllBasicEntities()?.FirstOrDefault();
                }
            }

            if (foundBasic?.Shape?.Pos != null)
            {
                var pos = new Point(foundBasic.Shape.Pos.X, foundBasic.Shape.Pos.Y);
                DisplayScrollViewer.SetViewCenter(pos);
                Logger.AddLog(StaticLocalisation.GetString("MapViewer.EntityFound", entityId));
            }
            else
            {
                Logger.AddLog(StaticLocalisation.GetString("MapViewer.EntityNotFound", entityId));
            }
        }

        #endregion

       

        #region Создание слоёв

        private void CreateLayers()
        {
            _layers.Clear();
            DisplayView.Children.Clear();

            CreateLayer("Basic", _politicalMap.Basic?.ToList(), null);

            foreach (var (layerName, entities) in _politicalMap.GetLayers())
            {
                CreateLayer(layerName, null, entities?.ToList());
            }
        }

        private void CreateLayer(string layerName, List<IBasicMapEntity> basicEntities, List<IMapEntity> mapEntities)
        {
            var parentCanvas = new Canvas
            {
                Background = System.Windows.Media.Brushes.Transparent,
                Visibility = Visibility.Collapsed
            };

            var renderCanvas = new Canvas { Background = System.Windows.Media.Brushes.Transparent };
            var idCanvas = new Canvas
            {
                Background = System.Windows.Media.Brushes.Transparent,
                Visibility = Visibility.Collapsed
            };

            renderCanvas.MouseMove += RenderCanvas_MouseMove;

            parentCanvas.Children.Add(renderCanvas);
            parentCanvas.Children.Add(idCanvas);
            DisplayView.Children.Add(parentCanvas);

            _layers[layerName] = new LayerInfo
            {
                Name = layerName,
                ParentCanvas = parentCanvas,
                RenderCanvas = renderCanvas,
                IdCanvas = idCanvas,
                BasicEntities = basicEntities,
                Entities = mapEntities
            };
        }

        #endregion

        #region Отрисовка слоёв

        private void DrawLayers()
        {
            foreach (var layerInfo in _layers.Values)
            {
                if (layerInfo.Name == "Basic")
                {
                    DrawBasicLayer(layerInfo);
                }
                else
                {
                    DrawMapEntityLayer(layerInfo);
                }
            }

            Logger.AddLog(StaticLocalisation.GetString("MapViewer.LayersDrawn", _layers.Count));
        }

        private void DrawBasicLayer(LayerInfo layerInfo)
        {
            if (layerInfo.BasicEntities == null || layerInfo.BasicEntities.Count == 0) return;

            layerInfo.RenderCanvas.Children.Clear();
            layerInfo.IdCanvas.Children.Clear();

            // Фильтруем сразу по кэшу
            var validEntities = layerInfo.BasicEntities
                .Where(b => _pointsCache.ContainsKey(b.Id.ToString()))
                .ToList();

            foreach (var basic in validEntities)
            {
                var poly = CreatePolygon(basic);
                if (poly == null) continue;

                poly.Fill = new SolidColorBrush(basic.Color?.ToMediaColor() ?? Colors.Gray);
                poly.ToolTip = $"ID: {basic.Id}";
                layerInfo.RenderCanvas.Children.Add(poly);
            }

            var textBlocks = CreateTextBlocksUniversal(validEntities.Cast<IBasicMapEntity>().ToList(), null, layerInfo.Name);
            foreach (var tb in textBlocks)
                layerInfo.IdCanvas.Children.Add(tb);

            Logger.AddDbgLog($"Basic layer drawn: {validEntities.Count}/{layerInfo.BasicEntities.Count} entities (filtered by cache)", caller: "MapViewer");
        }

        private void DrawMapEntityLayer(LayerInfo layerInfo)
        {
            if (layerInfo.Entities == null || layerInfo.Entities.Count == 0) return;

            layerInfo.RenderCanvas.Children.Clear();
            layerInfo.IdCanvas.Children.Clear();

            foreach (var entity in layerInfo.Entities)
            {
                // Рекурсивно собираем Basic, фильтруем по наличию в кэше
                var allBasicEntities = entity.GetAllBasicEntities()?
                    .Where(b => _pointsCache.ContainsKey(b.Id.ToString()))
                    .ToList();

                if (allBasicEntities.Count == 0) continue;

                var color = entity.Color ?? GenerateRandomColor();

                foreach (var basic in allBasicEntities)
                {
                    var poly = CreatePolygon(basic, entity);
                    if (poly == null) continue;

                    poly.Fill = new SolidColorBrush(color.ToMediaColor());
                    poly.ToolTip = $"{layerInfo.Name}: {entity.Id} | Province: {basic.Id}";
                    layerInfo.RenderCanvas.Children.Add(poly);
                }

                var textBlocks = CreateTextBlocksUniversal(allBasicEntities, entity.Id.ToString(), layerInfo.Name);
                foreach (var tb in textBlocks)
                    layerInfo.IdCanvas.Children.Add(tb);
            }

            Logger.AddLog($"Layer '{layerInfo.Name}' drawn: {layerInfo.Entities.Count} entities, {layerInfo.RenderCanvas.Children.Count} polygons");
        }

        private System.Drawing.Color GenerateRandomColor()
        {
            var random = new Random();
            return System.Drawing.Color.FromArgb(255, random.Next(50, 255), random.Next(50, 255), random.Next(50, 255));
        }

        #endregion

        #region CreateTextBlocksUniversal - из легаси

        /// <summary>
        /// Универсальный метод создания TextBlock с правильным размером и позицией
        /// Адаптирован для работы с IBasicMapEntity вместо ProvinceConfig
        /// </summary>
        private List<TextBlock> CreateTextBlocksUniversal(IEnumerable<IBasicMapEntity> basicEntities, string text = null, string layer = "")
        {
            var result = new List<TextBlock>();

            // Фильтруем только те у кого есть кэш — Shape не трогаем
            var entityList = basicEntities?
                .Where(e => e != null && _pointsCache.ContainsKey(e.Id.ToString()))
                .ToList() ?? new();

            if (entityList.Count == 0) return result;

            if (text != null)
            {
                // Групповой лейбл
                var allPoints = entityList
                    .SelectMany(e => _pointsCache[e.Id.ToString()].ContourPoints)
                    .ToList();

                if (allPoints.Count == 0) return result;

                var bounds = GetBoundingBox(allPoints);
                double size = Math.Max(bounds.Width, bounds.Height);
                double fontSize = CalculateFontSize(size);
                if (fontSize <= 0) return result;

                // Центр — среднее Pos из кэша
                double centerX = entityList.Average(e => _pointsCache[e.Id.ToString()].Pos.X);
                double centerY = entityList.Average(e => _pointsCache[e.Id.ToString()].Pos.Y);
                double textWidth = text.Length * fontSize * 0.6;

                var tb = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    FontSize = fontSize,
                    Tag = "Group",
                    RenderTransform = new TranslateTransform(
                        centerX - textWidth / 2,
                        centerY - fontSize / 2
                    )
                };
                Logger.AddDbgLog($"Текст {text ?? "Basic"} → позиция {Math.Floor(tb.RenderTransform.Value.OffsetX)},{Math.Floor(tb.RenderTransform.Value.OffsetY)}, слой {layer}", caller: "MapViewer");
                result.Add(tb);
            }
            else
            {
                // Индивидуальный лейбл для Basic слоя
                foreach (var entity in entityList)
                {
                    var cache = _pointsCache[entity.Id.ToString()];
                    var bounds = GetBoundingBox(cache.ContourPoints);
                    double size = Math.Min(bounds.Width, bounds.Height);
                    double fontSize = CalculateFontSize(size);
                    if (fontSize <= 0) continue;

                    string displayText = entity.Id.ToString();
                    double textWidth = displayText.Length * fontSize * 0.6;

                    // Центр из кэша
                    var candidatePoint = FindBestCenterPoint(cache.ContourPoints);
                    if (!IsInsidePolygon(candidatePoint, cache.ContourPoints))
                        candidatePoint = bounds.Center;

                    result.Add(new TextBlock
                    {
                        Text = displayText,
                        Foreground = Brushes.Black,
                        FontWeight = FontWeights.Bold,
                        FontSize = fontSize,
                        Tag = entity.Id.ToString(),
                        RenderTransform = new TranslateTransform(
                            candidatePoint.X - textWidth / 2,
                            candidatePoint.Y - fontSize / 2
                        )
                    });
                }

            }

            return result;
        }

        #endregion

        #region Helper Methods для TextBlock
        private void CreatePolygonCache()
        {
            _pointsCache.Clear();

            foreach (var basic in _politicalMap.Basic)
            {
                if (basic.Shape?.ContourPoints == null || basic.Shape.ContourPoints.Length < 3)
                    continue;

                _pointsCache[basic.Id.ToString()] = new BasicShapeCache
                {
                    Points = new PointCollection(basic.Shape.ContourPoints.ToWindowsPoints()),
                    Pos = basic.Shape.Pos,
                    ContourPoints = basic.Shape.ContourPoints
                };
            }

            Logger.AddDbgLog($"Points cache created: {_pointsCache.Count} entries", caller: "MapViewer");
        }

        // Фабричный метод — каждый раз новый Polygon, но точки берём из кэша
        private Polygon CreatePolygon(IBasicMapEntity basic, IMapEntity parentEntity = null)
        {
            if (!_pointsCache.TryGetValue(basic.Id.ToString(), out var cache))
                return null;

            return new Polygon
            {
                Points = cache.Points,
                StrokeThickness = 0,
                Fill = Brushes.Gray,
                Tag = new PolygonTag { BasicEntity = basic, ParentEntity = parentEntity }
            };
        }
        private double CalculateFontSize(double size)
        {
            const double MinVisibleSize = 1;
            const double MaxFontSize = 25.0;
            const double MinFontSize = 1.0;
            const double ScalingFactor = 0.2;

            if (size < MinVisibleSize) return 0;

            double fontSize = size * ScalingFactor;
            return Math.Min(MaxFontSize, Math.Max(MinFontSize, fontSize));
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
            var xAvg = contour.Average(p => p.X);
            var yAvg = contour.Average(p => p.Y);
            return new System.Drawing.Point((int)xAvg, (int)yAvg);
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

        /// <summary>
        /// Обновление позиции и размера TextBlock при изменении состава entity
        /// </summary>
        public void UpdateEntityLabel(string layerName, int entityId)
        {
            if (!_layers.TryGetValue(layerName, out var layerInfo)) return;

            // Находим ВСЕ TextBlock с этим текстом и удаляем их
            var oldLabels = layerInfo.IdCanvas.Children
                .OfType<TextBlock>()
                .Where(tb => tb.Text == entityId.ToString())
                .ToList();

            foreach (var tb in oldLabels)
                layerInfo.IdCanvas.Children.Remove(tb);

            var entity = layerInfo.Entities?.FirstOrDefault(e => e.Id.ToInt() == entityId);
            if (entity == null) return;

            var allBasic = entity.GetAllBasicEntities()?.ToList();
            if (allBasic == null || allBasic.Count == 0) return;

            var newTextBlocks = CreateTextBlocksUniversal(allBasic, entityId.ToString(), layerName);
            if (newTextBlocks.Count == 0) return;

            // Добавляем новый
            layerInfo.IdCanvas.Children.Add(newTextBlocks[0]);
        }

        #endregion

        #region ComputeProvinceShapes

        public void ComputeProvinceShapes()
        {
            using var mat = Application.Extentions.BitmapExtensions.ToMat(ModDataStorage.Mod.Map.MapImage);
            if (mat.Empty())
                throw new InvalidOperationException("Не удалось загрузить provinces.bmp");

            Logger.AddDbgLog($"🔍 Начало обработки {_politicalMap.Basic.Count()} провинций...", caller:"MapViewer");

            int successCount = 0;
            var timer = System.Diagnostics.Stopwatch.StartNew();

            int maxThreads = ParallelTaskCounter.CalculateMaxDegreeOfParallelism();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxThreads };

            Parallel.ForEach(_politicalMap.Basic, parallelOptions, province =>
            {
                try
                {
                    using var mask = new Mat();
                    Cv2.InRange(mat,
                        new Scalar(province.Color.Value.B, province.Color.Value.G, province.Color.Value.R),
                        new Scalar(province.Color.Value.B, province.Color.Value.G, province.Color.Value.R),
                        mask);

                    int pixelCount = Cv2.CountNonZero(mask);
                    if (pixelCount == 0) return;

                    Cv2.FindContours(mask, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                    if (contours.Length == 0) return;

                    var mainContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();

                    double area = Cv2.ContourArea(mainContour);
                    double perimeter = Cv2.ArcLength(mainContour, true);

                    bool isSimple = mainContour.Length < 50 || (4 * Math.PI * area / (perimeter * perimeter) > 0.5);

                    if (!isSimple)
                    {
                        Cv2.FindContours(mask, out contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);
                        mainContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
                    }

                    var moments = Cv2.Moments(mainContour);
                    if (moments.M00 <= 0.5) return;

                    province.Shape = new ProvinceShapeArg
                    {
                        ContourPoints = mainContour.Select(p => new System.Drawing.Point(p.X, p.Y)).ToArray(),
                        Pos = new System.Drawing.Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00)),
                        FillColor = System.Drawing.Color.FromArgb(255, province.Color.Value.R, province.Color.Value.G, province.Color.Value.B)
                    };

                    Interlocked.Increment(ref successCount);
                }
                catch (Exception ex)
                {
                    Logger.AddDbgLog($"🔥 Ошибка при обработке провинции {province.Id}: {ex.Message}", caller: "MapViewer");
                }
            });

            timer.Stop();
            Logger.AddDbgLog($"ОБРАБОТКА ЗАВЕРШЕНА за {timer.Elapsed.TotalSeconds:F2} сек. Успешно: {successCount}", caller: "MapViewer");
        }

        #endregion

        #region Создание кнопок слоёв

        private void CreateLayerButtons()
        {
            ButtonStrip.Children.Clear();

            foreach (var layerName in _layers.Keys)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(0, 0, 0, 5),
                    Content = new TextBlock
                    {
                        Text = layerName.Substring(0, Math.Min(2, layerName.Length)).ToUpper(),
                        FontSize = 12,
                        FontWeight = FontWeights.Bold
                    },
                    ToolTip = layerName,
                    Tag = layerName
                };

                button.Click += LayerButton_Click;
                ButtonStrip.Children.Add(button);
            }

            var showIdsCheckbox = new CheckBox
            {
                Content = "ID",
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                ToolTip = StaticLocalisation.GetString("MapViewer.ShowIds")
            };

            showIdsCheckbox.Checked += (s, e) => { _showIds = true; ShowIds(true); };
            showIdsCheckbox.Unchecked += (s, e) => { _showIds = false; ShowIds(false); };

            ButtonStrip.Children.Add(showIdsCheckbox);
        }

        private void LayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string layerName)
            {
                SwitchToLayer(layerName);
            }
        }

        #endregion

        #region Обработка мыши - Drag & Drop

        private void Display_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed) return;
            if (_currentLayer == "Basic") return;

            var hit = VisualTreeHelper.HitTest(DisplayView, e.GetPosition(DisplayView));
            if (hit?.VisualHit is Polygon polygon && polygon.Tag is PolygonTag tag)
            {
                _draggedBasicEntity = tag.BasicEntity;
                _dragStartPoint = e.GetPosition(DisplayView);
            }
        }

        private void Display_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedBasicEntity == null || e.RightButton != MouseButtonState.Pressed) return;

            var currentPos = e.GetPosition(DisplayView);
            if ((currentPos - _dragStartPoint).Length < 10) return;

            Cursor = Cursors.Hand;
        }

        private void RenderCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Display_MouseMove(sender, e);
        }

        private void Display_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_draggedBasicEntity == null) return;

            var hit = VisualTreeHelper.HitTest(DisplayView, e.GetPosition(DisplayView));
            if (hit?.VisualHit is Polygon targetPolygon && targetPolygon.Tag is PolygonTag targetTag)
            {
                HandleEntityMove(_draggedBasicEntity, targetTag.BasicEntity);
            }

            _draggedBasicEntity = null;
            Cursor = Cursors.Arrow;
        }

        private void HandleEntityMove(IBasicMapEntity draggedBasic, IBasicMapEntity targetBasic)
        {
            if (_currentLayer == "Basic") return;

            var layerInfo = _layers[_currentLayer];
            if (layerInfo.Entities == null) return;

            IMapEntity sourceParent = FindParentEntity(draggedBasic, layerInfo.Entities);
            IMapEntity targetParent = FindParentEntity(targetBasic, layerInfo.Entities);

            if (sourceParent == null || targetParent == null || sourceParent.Id == targetParent.Id) return;

            object childToMove = FindDirectChild(sourceParent, draggedBasic);
            if (childToMove == null) return;

            var moveArg = new EntityMoveEventArg
            {
                BasicEntityId = draggedBasic.Id.ToInt(),
                SourceParent = sourceParent,
                TargetParent = targetParent,
                LayerName = _currentLayer,
                MovedChild = childToMove
            };

            OnEntityMove?.Invoke(moveArg);

            sourceParent.RemoveChild(childToMove);
            targetParent.AddChild(childToMove);

            // Обновляем лейблы после перемещения
            UpdateEntityLabel(_currentLayer, sourceParent.Id.ToInt());
            UpdateEntityLabel(_currentLayer, targetParent.Id.ToInt());

            // Перерисовываем слой
            DrawMapEntityLayer(layerInfo);

            Logger.AddLog(StaticLocalisation.GetString("MapViewer.EntityMoved", childToMove, sourceParent.Id, targetParent.Id));
        }

        private IMapEntity FindParentEntity(IBasicMapEntity basic, List<IMapEntity> entities)
        {
            foreach (var entity in entities)
            {
                var allBasic = entity.GetAllBasicEntities();
                if (allBasic != null && allBasic.Any(b => b.Id == basic.Id))
                {
                    return entity;
                }
            }
            return null;
        }

        private object FindDirectChild(IMapEntity parent, IBasicMapEntity basic)
        {
            var children = parent.GetChildren();

            foreach (var child in children)
            {
                if (child is IBasicMapEntity basicChild && basicChild.Id == basic.Id)
                {
                    return child;
                }
                else if (child is IMapEntity mapChild)
                {
                    var allBasic = mapChild.GetAllBasicEntities();
                    if (allBasic != null && allBasic.Any(b => b.Id == basic.Id))
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Обработка кликов

        private void Display_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                _isDoubleClick = true;
                _clickTimer.Stop();
                HandleDoubleClick(e.GetPosition(DisplayView));
            }
            else
            {
                _isDoubleClick = false;
                _lastClickButton = MouseButton.Left;
                _lastClickPosition = e.GetPosition(DisplayView);
                _clickTimer.Start();
            }
        }

        private void Display_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                _isDoubleClick = true;
                _clickTimer.Stop();
                HandleDoubleClick(e.GetPosition(DisplayView));
            }
            else if (e.ClickCount == 1 && _draggedBasicEntity == null)
            {
                _isDoubleClick = false;
                _lastClickButton = MouseButton.Right;
                _lastClickPosition = e.GetPosition(DisplayView);
                _clickTimer.Start();
            }
        }

        private void ClickTimer_Tick(object sender, EventArgs e)
        {
            _clickTimer.Stop();

            if (!_isDoubleClick)
            {
                HandleSingleClick(_lastClickPosition, _lastClickButton);
            }
        }

        private void HandleDoubleClick(Point position)
        {
            var hit = VisualTreeHelper.HitTest(DisplayView, position);
            if (hit?.VisualHit is not Polygon polygon || polygon.Tag is not PolygonTag tag) return;

            var arg = new EntityDoubleClickEventArg
            {
                LayerName = _currentLayer,
                BasicEntity = tag.BasicEntity
            };

            if (tag.ParentEntity != null)
            {
                arg.MapEntity = tag.ParentEntity;
                arg.Entity = tag.ParentEntity;
            }
            else
            {
                arg.Entity = tag.BasicEntity;
            }

            OnEntityDoubleClick?.Invoke(arg);
        }

        private void HandleSingleClick(Point position, MouseButton button)
        {
            var hit = VisualTreeHelper.HitTest(DisplayView, position);
            if (hit?.VisualHit is not Polygon polygon || polygon.Tag is not PolygonTag tag) return;

            var arg = new EntityClickEventArg
            {
                LayerName = _currentLayer,
                X = position.X,
                Y = position.Y,
                BasicEntity = tag.BasicEntity
            };

            if (tag.ParentEntity != null)
            {
                arg.MapEntity = tag.ParentEntity;
                arg.Entity = tag.ParentEntity;
            }
            else
            {
                arg.Entity = tag.BasicEntity;
            }

            if (button == MouseButton.Left)
            {
                OnEntityLeftClick?.Invoke(arg);
            }
            else if (button == MouseButton.Right)
            {
                OnEntityRightClick?.Invoke(arg);
            }
        }

        #endregion

        #region Вспомогательные классы
        private class BasicShapeCache
        {
            public PointCollection Points { get; set; }
            public System.Drawing.Point Pos { get; set; }
            public System.Drawing.Point[] ContourPoints { get; set; }
        }
        private class LayerInfo
        {
            public string Name { get; set; }
            public Canvas ParentCanvas { get; set; }
            public Canvas RenderCanvas { get; set; }
            public Canvas IdCanvas { get; set; }
            public List<IBasicMapEntity> BasicEntities { get; set; }
            public List<IMapEntity> Entities { get; set; }
        }

        private class PolygonTag
        {
            public IBasicMapEntity BasicEntity { get; set; }
            public IMapEntity ParentEntity { get; set; }
        }

        #endregion
    }
}