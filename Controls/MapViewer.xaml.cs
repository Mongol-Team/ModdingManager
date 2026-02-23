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
    /// Универсальный контрол для отображения политической карты
    /// Поддерживает вложенность IMapEntity (матрешки)
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

        #endregion

        #region События

        /// <summary>
        /// Событие двойного клика по сущности
        /// </summary>
        public event Action<EntityDoubleClickEventArg> OnEntityDoubleClick;

        /// <summary>
        /// Событие перемещения сущности между родителями
        /// </summary>
        public event Action<EntityMoveEventArg> OnEntityMove;

        /// <summary>
        /// Событие левого клика по сущности
        /// </summary>
        public event Action<EntityClickEventArg> OnEntityLeftClick;

        /// <summary>
        /// Событие правого клика по сущности
        /// </summary>
        public event Action<EntityClickEventArg> OnEntityRightClick;

        /// <summary>
        /// Событие изменения слоя
        /// </summary>
        public event Action<string> OnLayerChanged;

        #endregion

        #region Конструктор

        public MapViewer()
        {
            InitializeComponent();

            // Таймер для различения одиночного и двойного клика (300мс)
            _clickTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _clickTimer.Tick += ClickTimer_Tick;
        }


        #endregion

        #region Публичные методы

        /// <summary>
        /// Инициализация контрола с политической картой
        /// </summary>
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
                Logger.AddDbgLog($"Map size: {_politicalMap.MapImage.Width}x{_politicalMap.MapImage.Height}");
            }
            ComputeProvinceShapes();
            CreateLayers();
            DrawLayers();
            CreateLayerButtons();

            // Переключаемся на базовый слой
            SwitchToLayer("Basic");

            Logger.AddLog(StaticLocalisation.GetString("MapViewer.Initialized"));
        }

        /// <summary>
        /// Переключение на указанный слой
        /// </summary>
        public void SwitchToLayer(string layerName)
        {
            if (!_layers.ContainsKey(layerName))
            {
                Logger.AddLog(StaticLocalisation.GetString("MapViewer.LayerNotFound", layerName));
                return;
            }

            _currentLayer = layerName;

            // Скрываем все слои
            foreach (var layer in _layers.Values)
            {
                layer.ParentCanvas.Visibility = Visibility.Collapsed;
            }

            // Показываем текущий
            _layers[_currentLayer].ParentCanvas.Visibility = Visibility.Visible;

            OnLayerChanged?.Invoke(_currentLayer);
            Logger.AddDbgLog($"Switched to layer: {layerName}");
        }

        /// <summary>
        /// Показать/скрыть ID на текущем слое
        /// </summary>
        public void ShowIds(bool show)
        {
            if (_layers.ContainsKey(_currentLayer))
            {
                _layers[_currentLayer].IdCanvas.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Поиск и центрирование на сущности по ID
        /// </summary>
        public void SearchAndCenter(int entityId)
        {
            if (!_layers.ContainsKey(_currentLayer)) return;

            var layerInfo = _layers[_currentLayer];

            // Ищем сущность
            IBasicMapEntity foundBasic = null;

            if (_currentLayer == "Basic")
            {
                foundBasic = _politicalMap.Basic?.FirstOrDefault(b => b.Id.ToInt() == entityId);
            }
            else
            {
                // Ищем в текущем слое
                var entity = layerInfo.Entities?.FirstOrDefault(e => e.Id.ToInt() == entityId);
                if (entity != null)
                {
                    // Берём первую базовую сущность этого entity
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

            // Создаём базовый слой
            CreateLayer("Basic", _politicalMap.Basic?.ToList(), null);

            // Создаём остальные слои из IPoliticalMap
            foreach (var (layerName, entities) in _politicalMap.GetLayers())
            {
                CreateLayer(layerName, null, entities?.ToList());
            }

            Logger.AddDbgLog($"Created {_layers.Count} layers");
        }

        private void CreateLayer(string layerName, List<IBasicMapEntity> basicEntities, List<IMapEntity> mapEntities)
        {
            // Создаём родительский Canvas для слоя
            var parentCanvas = new Canvas
            {
                Background = System.Windows.Media.Brushes.Transparent,
                Visibility = Visibility.Collapsed
            };

            // Canvas для отрисовки полигонов
            var renderCanvas = new Canvas
            {
                Background = System.Windows.Media.Brushes.Transparent
            };

            // Canvas для ID меток
            var idCanvas = new Canvas
            {
                Background = System.Windows.Media.Brushes.Transparent,
                Visibility = Visibility.Collapsed
            };

            // Добавляем обработчик для drag
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

            Logger.AddDbgLog($"Layer '{layerName}' created");
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

            foreach (var basic in layerInfo.BasicEntities)
            {
                DrawBasicEntity(layerInfo.RenderCanvas, basic, basic.Color);
                AddIdLabel(layerInfo.IdCanvas, basic);
            }

            Logger.AddDbgLog($"Basic layer drawn: {layerInfo.BasicEntities.Count} entities");
        }
        public void ComputeProvinceShapes()
        {
            using var mat = Application.Extentions.BitmapExtensions.ToMat(ModDataStorage.Mod.Map.MapImage);
            if (mat.Empty())
                throw new InvalidOperationException("Не удалось загрузить provinces.bmp");

            Logger.AddDbgLog($"🔍 Начало обработки {_politicalMap.Basic.Count()} провинций...");

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
                    if (pixelCount == 0)
                    {
                        Logger.AddDbgLog($"⚠️ Провинция {province.Id} не найдена (цвет R:{province.Color.Value.R}, G:{province.Color.Value.G}, B:{province.Color.Value.B})");
                        return;
                    }

                    Cv2.FindContours(mask, out var contours, out _,
             RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                    if (contours.Length == 0)
                        return;

                    var mainContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();

                    double area = Cv2.ContourArea(mainContour);
                    double perimeter = Cv2.ArcLength(mainContour, true);

                    bool isSimple = mainContour.Length < 50 ||
                                    (4 * Math.PI * area / (perimeter * perimeter) > 0.5);

                    if (!isSimple)
                    {

                        Cv2.FindContours(mask, out contours, out _,
                            RetrievalModes.External, ContourApproximationModes.ApproxNone);

                        mainContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
                    }

                    var moments = Cv2.Moments(mainContour);
                    if (moments.M00 <= 0.5)
                    {
                        Logger.AddDbgLog($"⚠️ Провинция {province.Id}: контур слишком мал (площадь {moments.M00})");
                        return;
                    }

                    // 5. Заполняем Shape (берём ВСЕ точки контура)
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
                    Logger.AddDbgLog($"🔥 Ошибка при обработке провинции {province.Id}: {ex.Message}");
                }
            });

            timer.Stop();
            Logger.AddDbgLog("\n====================================");
            Logger.AddDbgLog($"ОБРАБОТКА ЗАВЕРШЕНА за {timer.Elapsed.TotalSeconds:F2} сек");
            Logger.AddDbgLog($"Успешно: {successCount} | Не удалось: {_politicalMap.Basic.Count() - successCount}");
            Logger.AddDbgLog("====================================\n");
            
        }
        private void DrawMapEntityLayer(LayerInfo layerInfo)
        {
            if (layerInfo.Entities == null || layerInfo.Entities.Count == 0) return;

            // ВАЖНО: Очищаем Canvas перед отрисовкой!
            layerInfo.RenderCanvas.Children.Clear();
            layerInfo.IdCanvas.Children.Clear();

            Logger.AddDbgLog($"Drawing layer '{layerInfo.Name}' with {layerInfo.Entities.Count} entities");

            foreach (var entity in layerInfo.Entities)
            {
                var allBasicEntities = entity.GetAllBasicEntities()?.ToList();
                if (allBasicEntities == null || allBasicEntities.Count == 0)
                {
                    Logger.AddDbgLog($"  Entity {entity.Id} has no basic entities!");
                    continue;
                }

                var color = entity.Color ?? GenerateRandomColor();

                Logger.AddDbgLog($"  Drawing entity {entity.Id} with {allBasicEntities.Count} provinces, color: {color}");

                int drawnCount = 0;
                foreach (var basic in allBasicEntities)
                {
                    DrawBasicEntity(layerInfo.RenderCanvas, basic, color, entity);
                    drawnCount++;
                }

                Logger.AddDbgLog($"    → Drew {drawnCount} polygons on canvas (total canvas children: {layerInfo.RenderCanvas.Children.Count})");

                AddEntityLabel(layerInfo.IdCanvas, entity);
            }

            Logger.AddLog($"Layer '{layerInfo.Name}' drawn: {layerInfo.Entities.Count} entities, {layerInfo.RenderCanvas.Children.Count} polygons");
        }

        private System.Drawing.Color GenerateRandomColor()
        {
            var random = new Random();
            return System.Drawing.Color.FromArgb(
                255,
                random.Next(50, 255),
                random.Next(50, 255),
                random.Next(50, 255)
            );
        }

        private void DrawBasicEntity(Canvas canvas, IBasicMapEntity entity, System.Drawing.Color? color, IMapEntity parentEntity = null)
        {
            if (entity.Shape?.ContourPoints == null || entity.Shape.ContourPoints.Length < 3)
                return;

            var poly = new Polygon
            {
                Fill = new SolidColorBrush(color?.ToMediaColor() ?? Colors.Gray),
                Points = new PointCollection(entity.Shape.ContourPoints.ToWindowsPoints()),
                StrokeThickness = 0,
                ToolTip = $"ID: {entity.Id}",
                Tag = new PolygonTag
                {
                    BasicEntity = entity,
                    ParentEntity = parentEntity
                }
            };

            canvas.Children.Add(poly);
        }

        private void AddIdLabel(Canvas canvas, IBasicMapEntity entity)
        {
            if (entity.Shape?.Pos == null) return;

            var textBlock = new TextBlock
            {
                Text = entity.Id.ToString(),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Tag = entity
            };

            textBlock.RenderTransform = new TranslateTransform(
                entity.Shape.Pos.X - 10,
                entity.Shape.Pos.Y - 6
            );

            canvas.Children.Add(textBlock);
        }
        
        private void AddEntityLabel(Canvas canvas, IMapEntity entity)
        {
            var allBasic = entity.GetAllBasicEntities()?.ToList();
            if (allBasic == null || allBasic.Count == 0) return;

            // Вычисляем центр всех базовых сущностей
            var allPoints = allBasic
                .Where(b => b.Shape?.ContourPoints != null)
                .SelectMany(b => b.Shape.ContourPoints)
                .ToList();

            if (allPoints.Count == 0) return;

            double centerX = allPoints.Average(p => p.X);
            double centerY = allPoints.Average(p => p.Y);

            var textBlock = new TextBlock
            {
                Text = entity.Id.ToString(),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Tag = entity
            };

            textBlock.RenderTransform = new TranslateTransform(
                centerX - 15,
                centerY - 8
            );

            canvas.Children.Add(textBlock);
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

            // Чекбокс для показа ID
            var showIdsCheckbox = new CheckBox
            {
                Content = "ID",
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                ToolTip = StaticLocalisation.GetString("MapViewer.ShowIds")
            };

            showIdsCheckbox.Checked += (s, e) => ShowIds(true);
            showIdsCheckbox.Unchecked += (s, e) => ShowIds(false);

            ButtonStrip.Children.Add(showIdsCheckbox);

            Logger.AddDbgLog($"Created {_layers.Count} layer buttons");
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
            if (_currentLayer == "Basic") return; // В базовом слое нельзя перемещать

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
            // Вызываем общий обработчик для Drag
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

            // Находим родительские IMapEntity для обоих базовых сущностей
            // ВАЖНО: используем рекурсивный поиск через GetAllBasicEntities()
            IMapEntity sourceParent = FindParentEntity(draggedBasic, layerInfo.Entities);
            IMapEntity targetParent = FindParentEntity(targetBasic, layerInfo.Entities);

            if (sourceParent == null || targetParent == null || sourceParent.Id == targetParent.Id)
            {
                Logger.AddDbgLog($"Cannot move: source={sourceParent?.Id}, target={targetParent?.Id}");
                return;
            }

            // Находим непосредственную дочернюю сущность sourceParent, которая содержит draggedBasic
            object childToMove = FindDirectChild(sourceParent, draggedBasic);

            if (childToMove == null)
            {
                Logger.AddDbgLog($"Cannot find direct child containing basic entity {draggedBasic.Id}");
                return;
            }

            // Создаём событие
            var moveArg = new EntityMoveEventArg
            {
                BasicEntityId = draggedBasic.Id.ToInt(),
                SourceParent = sourceParent,
                TargetParent = targetParent,
                LayerName = _currentLayer,
                MovedChild = childToMove
            };

            OnEntityMove?.Invoke(moveArg);

            // Выполняем перемещение
            sourceParent.RemoveChild(childToMove);
            targetParent.AddChild(childToMove);

            // Перерисовываем слой
            DrawMapEntityLayer(layerInfo);

            Logger.AddLog(StaticLocalisation.GetString(
                "MapViewer.EntityMoved",
                childToMove,
                sourceParent.Id,
                targetParent.Id));
        }

        /// <summary>
        /// Находит IMapEntity родителя для данной IBasicMapEntity
        /// Использует рекурсивный поиск через GetAllBasicEntities()
        /// </summary>
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

        /// <summary>
        /// Находит непосредственную дочернюю сущность parent, которая содержит basic
        /// Это может быть сам basic (если он прямой ребёнок) или вложенный IMapEntity
        /// </summary>
        private object FindDirectChild(IMapEntity parent, IBasicMapEntity basic)
        {
            var children = parent.GetChildren();

            foreach (var child in children)
            {
                if (child is IBasicMapEntity basicChild && basicChild.Id == basic.Id)
                {
                    // Прямой ребёнок
                    return child;
                }
                else if (child is IMapEntity mapChild)
                {
                    // Проверяем содержит ли этот IMapEntity наш basic
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
                // Только если не начали drag
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
