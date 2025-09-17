using ModdingManagerClassLib.Extentions;
using ModdingManagerModels;
using ModdingManagerModels.Types.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace ModdingManager.Controls
{
    public partial class TechnologyGrid : UserControl
    {
        private class ElementConnection
        {
            public Polygon Arrow;
            public Border From;
            public Border To;
            public Line Line;
            public bool IsMutual;
        }
        private const int CellSize = 62;
        private const int GridSize = 20;
        private UIElement _draggedElement;
        private Point _mouseOffset;
        private readonly Dictionary<UIElement, Point> _originalPositions = new();
        private readonly List<ElementConnection> _connections = new();
        private readonly List<UIElement> _markedElements = new();
        private TechTreeConfig _techTree = new();

        public event EventHandler<TechTreeConfig> TreeSet;
        public event EventHandler<TechTreeConfig> TreeGet;
        public event EventHandler<TechTreeItemConfig> ItemAdded;
        public event EventHandler<TechTreeItemConfig> ItemRemoved;
        public event EventHandler<TechTreeItemConfig> ItemEdited;

        public TechnologyGrid()
        {
            InitializeComponent();
            DrawGrid();
        }

        #region Public Methods
        public void SetTree(TechTreeConfig tree)
        {
            _techTree = tree;
            RefreshView();
            TreeSet?.Invoke(this, _techTree);
        }

        public TechTreeConfig GetTree()
        {
            TreeGet?.Invoke(this, _techTree);
            return _techTree;
        }

        public void AddItem(TechTreeItemConfig item, int gridX, int gridY)
        {
            item.GridX = gridX;
            item.GridY = gridY;
            
            while (IsCellOccupied(gridX, gridY))
            {
                gridX++;
                if (gridX >= GridSize) { gridX = 0; gridY++; }
            }

            _techTree.Items.Add(item);
            var element = CreateTechElement(item);
            Point position = GetCanvasPosition(gridX, gridY);
            Canvas.SetLeft(element, position.X);
            Canvas.SetTop(element, position.Y);
            TechGrid.Children.Add(element);
            
            ItemAdded?.Invoke(this, item);
        }

        public void RemoveItem(string itemId)
        {
            var border = FindElementBorderById(itemId);
            if (border != null) DeleteElement(border);
        }

        public void EditItem(TechTreeItemConfig updatedItem)
        {
            var border = FindElementBorderById(updatedItem.OldId.AsString() ?? updatedItem.Id.AsString());
            if (border == null) return;

            if (updatedItem.OldId != null && updatedItem.OldId != updatedItem.Id)
            {
                UpdateTechId(updatedItem.OldId.AsString(), updatedItem.Id.AsString());
            }

            if (border.Child is Canvas innerCanvas && innerCanvas.Children[0] is Image image)
            {
                var background = updatedItem.IsBig ? 
                    new BitmapImage(new Uri("pack://application:,,,/data/gfx/interface/technology_available_item_bg.png")) :
                    new BitmapImage(new Uri("pack://application:,,,/data/gfx/interface/tech_industry_available_item_bg.png"));
                
                image.Source = ImageExtensions.ToImageSource(updatedItem.Image)
                    .GetCombinedTechImage(background, updatedItem.IsBig ? 1 : 2);
            }

            ItemEdited?.Invoke(this, updatedItem);
        }
        #endregion

        #region Private Methods
        private void DrawGrid()
        {
            for (int i = 0; i <= GridSize; i++)
            {
                double offset = i * CellSize;

                TechGrid.Children.Add(new Line
                {
                    X1 = offset, Y1 = 0, X2 = offset, Y2 = GridSize * CellSize,
                    Stroke = Brushes.LightGray, StrokeThickness = 1
                });

                TechGrid.Children.Add(new Line
                {
                    X1 = 0, Y1 = offset, X2 = GridSize * CellSize, Y2 = offset,
                    Stroke = Brushes.LightGray, StrokeThickness = 1
                });
            }
        }

        public void RefreshView()
        {
            ClearElements();
            DrawElements();
            RedrawAllConnections();
        }
        private void RedrawAllConnections()
        {
            if (_techTree == null) return;

            foreach (var pair in _techTree.ChildOf)
            {
                if (pair.Count == 2)
                {
                    var from = FindElementBorderById(pair[0]);
                    var to = FindElementBorderById(pair[1]);
                    if (from != null && to != null)
                    {
                        DrawConnection(from, to, System.Windows.Media.Brushes.Blue, isMutual: false);
                    }
                }
            }

            foreach (var group in _techTree.Mutal)
            {
                for (int i = 0; i < group.Count; i++)
                {
                    for (int j = i + 1; j < group.Count; j++)
                    {
                        var a = FindElementBorderById(group[i]);
                        var b = FindElementBorderById(group[j]);
                        if (a != null && b != null)
                        {
                            DrawConnection(a, b, System.Windows.Media.Brushes.Red, isMutual: true);
                        }
                    }
                }
            }
        }
        public Point GetVisualCenter(Border border)
        {

            double baseX = Canvas.GetLeft(border);
            double baseY = Canvas.GetTop(border);

            var image = border.GetImage();
            if (image == null) return new Point(baseX + CellSize / 2, baseY + CellSize / 2);

            var transform = image.RenderTransform as TranslateTransform;
            double offsetX = transform?.X ?? 0;
            double offsetY = transform?.Y ?? 0;

            double centerX = baseX + CellSize / 2 + offsetX;
            double centerY = baseY + CellSize / 2 + offsetY;

            return new Point(centerX, centerY);
        }
        private double GetVisualRadius(Border border, Vector direction)
        {
            var image = border.GetImage();
            if (image == null) return CellSize / 2;

            double halfWidth = image.Width / 2;
            double halfHeight = image.Height / 2;

            double dx = direction.X * halfWidth;
            double dy = direction.Y * halfHeight;

            return Math.Sqrt(dx * dx + dy * dy);
        }
        private void DrawConnection(Border from, Border to, Brush color, bool isMutual)
        {
            var fromCenter = GetVisualCenter(from);
            var toCenter = GetVisualCenter(to);
            Vector direction = toCenter - fromCenter;
            direction.Normalize();
            double fromRadius = GetVisualRadius(from, direction);
            double toRadius = GetVisualRadius(to, -direction);

            var start = fromCenter + direction * fromRadius;
            var end = toCenter - direction * toRadius;

            var line = new Line
            {
                Stroke = color,
                StrokeThickness = 2,
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                IsHitTestVisible = false
            };

            TechGrid.Children.Insert(0, line);

            Polygon arrow = null;
            if (!isMutual)
            {
                var vec = end - start;
                vec.Normalize();
                var perp = new Vector(-vec.Y, vec.X);

                Point arrowTip = end;
                Point base1 = arrowTip - vec * 10 + perp * 5;
                Point base2 = arrowTip - vec * 10 - perp * 5;

                arrow = new Polygon
                {
                    Points = new PointCollection { arrowTip, base1, base2 },
                    Fill = color,
                    IsHitTestVisible = false
                };

                TechGrid.Children.Insert(0, arrow);
            }

            _connections.Add(new ElementConnection
            {
                From = from,
                To = to,
                Line = line,
                Arrow = arrow,
                IsMutual = isMutual
            });
        }

        private void ClearElements()
        {
            var nonConnectionElements = TechGrid.Children
                .OfType<UIElement>()
                .Where(el => !(el is Line) && !(el is Polygon))
                .ToList();

            foreach (var element in nonConnectionElements)
            {
                TechGrid.Children.Remove(element);
            }
            _connections.Clear();
        }

        private void DrawElements()
        {
            foreach (var item in _techTree.Items)
            {
                var element = CreateTechElement(item);
                Point position = GetCanvasPosition(item.GridX, item.GridY);
                Canvas.SetLeft(element, position.X);
                Canvas.SetTop(element, position.Y);
                TechGrid.Children.Add(element);
            }
        }

        private UIElement CreateTechElement(TechTreeItemConfig item)
        {
            double imageWidth = item.IsBig ? 183 : 62;
            double imageHeight = item.IsBig ? 84 : 62;

            var image = new System.Windows.Controls.Image
            {
                Source = item.Image.ToImageSource().GetCombinedTechImage(item.IsBig ? Properties.Resources.technology_available_item_bg.ToImageSource() : Properties.Resources.tech_industry_available_item_bg.ToImageSource(), item.IsBig ? 1 : 2),
                Width = imageWidth,
                Height = imageHeight,
                IsHitTestVisible = false
            };


            Canvas innerCanvas = new Canvas
            {
                Width = CellSize,
                Height = CellSize,
                ClipToBounds = false
            };

            Canvas.SetLeft(image, -(imageWidth - CellSize) / 2);
            Canvas.SetTop(image, -(imageHeight - CellSize) / 2);
            innerCanvas.Children.Add(image);

            Border border = new Border
            {
                Width = CellSize,
                Height = CellSize,
                Name = item.Id.AsString(),
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1),
                Background = System.Windows.Media.Brushes.Transparent,
                Tag = false,
                ClipToBounds = false,
                Child = innerCanvas
            };

        
            return border;
        }

        private void SetMark(Border element)
        {
            bool isMarked = (bool)element.Tag;

            var canvas = element.Child as Canvas;
            if (canvas == null)
                return;

            const string selectionRectName = "SelectionHighlight";
            var lastImage = canvas.Children
                    .OfType<System.Windows.Controls.Image>()
                    .LastOrDefault();
            if (!isMarked)
            {
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Name = selectionRectName,
                    Stroke = System.Windows.Media.Brushes.Blue,
                    StrokeThickness = 2,
                    Fill = System.Windows.Media.Brushes.Transparent,
                    IsHitTestVisible = false,
                    Width = lastImage.Width,
                    Height = lastImage.Height
                };


                if (lastImage != null)
                {
                    int imageIndex = canvas.Children.IndexOf(lastImage);
                    canvas.Children.Insert(imageIndex + 1, rect);
                    Canvas.SetLeft(rect, -(lastImage.Width - CellSize) / 2);
                    Canvas.SetTop(rect, -(lastImage.Height - CellSize) / 2);
                }
                else
                {
                    canvas.Children.Add(rect);
                    Canvas.SetLeft(rect, -(lastImage.Width - CellSize) / 2);
                    Canvas.SetTop(rect, -(lastImage.Height - CellSize) / 2);
                }

                _markedElements.Add(element);
                element.Tag = true;
            }
            else
            {
                var existingRect = canvas.Children
                    .OfType<System.Windows.Shapes.Rectangle>()
                    .FirstOrDefault(r => r.Name == selectionRectName);

                if (existingRect != null)
                    canvas.Children.Remove(existingRect);

                _markedElements.Remove(element);
                element.Tag = false;
            }
        }
        private void DeleteElement(Border border)
        {
            TechGrid.Children.Remove(border);
            var item = _techTree.Items.FirstOrDefault(i => i.Id.AsString() == border.Name);
            if (item != null)
            {
                _techTree.Items.Remove(item);
                ItemRemoved?.Invoke(this, item);
            }

            RemoveConnectionsForElement(border);
            RemoveFromRelations(border.Name);
        }
        private void UpdateConnectionsForElement(Border element)
        {
            foreach (var conn in _connections)
            {
                if (conn.From == element || conn.To == element)
                {
                    var fromCenter = GetVisualCenter(conn.From);
                    var toCenter = GetVisualCenter(conn.To);

                    Vector direction = toCenter - fromCenter;
                    direction.Normalize();

                    double fromRadius = GetVisualRadius(conn.From, direction);
                    double toRadius = GetVisualRadius(conn.To, -direction);

                    var start = fromCenter + direction * fromRadius;
                    var end = toCenter - direction * toRadius;

                    conn.Line.X1 = start.X;
                    conn.Line.Y1 = start.Y;
                    conn.Line.X2 = end.X;
                    conn.Line.Y2 = end.Y;

                    if (!conn.IsMutual && conn.Arrow != null)
                    {
                        var vec = end - start;
                        vec.Normalize();
                        var perp = new Vector(-vec.Y, vec.X);

                        System.Windows.Point arrowTip = end;
                        System.Windows.Point base1 = arrowTip - vec * 10 + perp * 5;
                        System.Windows.Point base2 = arrowTip - vec * 10 - perp * 5;

                        conn.Arrow.Points = new PointCollection { arrowTip, base1, base2 };
                    }
                }
            }
        }
        private void RemoveConnectionsForElement(Border element)
        {
            var relatedConnections = _connections.Where(c => c.From == element || c.To == element).ToList();
            foreach (var conn in relatedConnections)
            {
                TechGrid.Children.Remove(conn.Line);
                if (conn.Arrow != null) TechGrid.Children.Remove(conn.Arrow);
                _connections.Remove(conn);
            }
        }

        private void RemoveFromRelations(string elementId)
        {
            _techTree.ChildOf = _techTree.ChildOf.Where(p => !p.Contains(elementId)).ToList();
            _techTree.Mutal = _techTree.Mutal.Select(g => g.Where(name => name != elementId).ToList())
                .Where(g => g.Count > 1).ToList();
        }
        #endregion
        #region Helper Methods
        private bool IsCellOccupied(int col, int row, Border ignore = null)
        {
            foreach (var child in TechGrid.Children.OfType<Border>())
            {
                if (child == ignore) continue;

                int x = (int)(Canvas.GetLeft(child) / CellSize);
                int y = (int)(Canvas.GetTop(child) / CellSize);

                if (x == col && y == row)
                    return true;
            }
            return false;
        }
        private Border FindElementBorderById(string id)
        {
            foreach (UIElement child in TechGrid.Children)
            {
                if (child is Border border)
                {
                    if (border.Name == id)
                    {
                        return border;
                    }
                }
            }
            return null;
        }
        private void UpdateTechId(string oldId, string newId)
        {
            foreach (var item in _techTree.Items)
            {
                if (item.Dependencies != null)
                {
                    item.Dependencies = item.Dependencies.Select(d => d == oldId ? newId : d).ToList();
                }
            }

            foreach (var pair in _techTree.ChildOf)
            {
                for (int i = 0; i < pair.Count; i++)
                {
                    if (pair[i] == oldId) pair[i] = newId;
                }
            }

            foreach (var group in _techTree.Mutal)
            {
                for (int i = 0; i < group.Count; i++)
                {
                    if (group[i] == oldId) group[i] = newId;
                }
            }

            var border = FindElementBorderById(oldId);
            if (border != null) border.Name = newId;
        }
        public List<string> GetMarkedIds()
        {
            return _markedElements
                .OfType<Border>()
                .Select(b => b.Name)
                .ToList();
        }

        public void AddChildConection()
        {
            var markedIds = GetMarkedIds();
            if (markedIds.Count != 2)
            {
                MessageBox.Show("Выберите ровно два элемента для создания связи", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _techTree.ChildOf.Add(new List<string> { markedIds[0], markedIds[1] });
            RedrawAllConnections();
        }

        public void AddMutualConection()
        {
            var markedIds = GetMarkedIds();
            if (markedIds.Count < 2)
            {
                MessageBox.Show("Выберите хотя бы два элемента для создания взаимной связи", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _techTree.Mutal.Add(markedIds);
            RedrawAllConnections();
        }

        private UIElement GetElementUnderPoint(Point cursor)
        {
            // Перебираем элементы в обратном порядке (сверху вниз)
            for (int i = TechGrid.Children.Count - 1; i >= 0; i--)
            {
                var element = TechGrid.Children[i] as UIElement;

                if (element == null)
                    continue;

                // Проверяем, попал ли курсор в границы элемента
                Rect bounds = new Rect(
                    Canvas.GetLeft(element),
                    Canvas.GetTop(element),
                    element.RenderSize.Width,
                    element.RenderSize.Height);

                if (bounds.Contains(cursor))
                {
                    // Если у элемента нет детей (листовой), возвращаем его
                    if (!(element is System.Windows.Controls.Panel panel && panel.Children.Count > 0))
                        return element;
                }
            }

            return null; // ничего не нашли
        }

        private System.Windows.Point GetCanvasPosition(int gridX, int gridY)
        {
            return new System.Windows.Point(gridX * CellSize, gridY * CellSize);
        }
        #endregion
        #region Events
        private void TechTreeBgEvent(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];

                    string extension = System.IO.Path.GetExtension(filePath).ToLower();
                    if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                    {
                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(filePath);
                            bitmap.EndInit();

                            TechBGImage.ImageSource = bitmap;
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                        }
                    }
                }
            }
        }
        private void ElementDraggingEndEvent(object sender, MouseButtonEventArgs e)
        {
            if (_draggedElement != null)
            {
                double left = Canvas.GetLeft(_draggedElement);
                double top = Canvas.GetTop(_draggedElement);

                int col = (int)Math.Round(left / CellSize);
                int row = (int)Math.Round(top / CellSize);

                double snapX = col * CellSize;
                double snapY = row * CellSize;

                Canvas.SetLeft(_draggedElement, snapX);
                Canvas.SetTop(_draggedElement, snapY);

                UpdateItemGridPosition(_draggedElement);
                UpdateConnectionsForElement((Border)_draggedElement);
                _draggedElement = null;
            }
        }
        private void UpdateItemGridPosition(UIElement element)
        {
            if (element is Border border)
            {
                string id = border.Name;
                var item = _techTree.Items.FirstOrDefault(i => i.Id.AsString() == id);
                if (item != null)
                {
                    double left = Canvas.GetLeft(border);
                    double top = Canvas.GetTop(border);

                    int newCol = (int)Math.Round(left / CellSize);
                    int newRow = (int)Math.Round(top / CellSize);

                    item.GridX = newCol;
                    item.GridY = newRow;
                }
            }
        }
        private void ElementOnMouseEvents(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.ClickCount == 2)
            {
                ElementMenuInteractionsEvent(sender, e);
            }
            else if(e.RightButton == MouseButtonState.Pressed || e.ClickCount < 2)
            {
                ElementCaptureEvent(sender, e);
            }
        }
        private void ElementCaptureEvent(object sender, MouseButtonEventArgs e)
        {
            Point cursor = e.GetPosition(TechGrid);

            _draggedElement = GetElementUnderPoint(cursor); // TechGrid — это Visual
            if (_draggedElement == null)
                return;

            // Топ-левел точки элемента в координатах TechGrid
            Point elementTopLeft = ((Visual)_draggedElement)
                .TransformToAncestor(TechGrid)
                .Transform(new Point(0, 0));

            _mouseOffset = new Point(cursor.X - elementTopLeft.X, cursor.Y - elementTopLeft.Y);

            _originalPositions[_draggedElement] = new Point(
                Canvas.GetLeft(_draggedElement),
                Canvas.GetTop(_draggedElement));

            TechGrid.CaptureMouse();
        }


        private void ElementMenuInteractionsEvent(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border element)
            {
                element = GetElementUnderPoint(e.GetPosition(TechGrid)) as Border;
            }
            if (element == null)
                return;
            TechTreeItemConfig cfg = _techTree.Items.FirstOrDefault(i => i.Id.AsString() == element.Name);
            var contextMenu = new ContextMenu();

            var deleteItem = new MenuItem { Header = "Удалить" };
            deleteItem.Click += (s, args) => DeleteElement(element);

            var markItem = new MenuItem { Header = "Выделить" };
            markItem.Click += (s, args) => SetMark(element);

            var editItem = new MenuItem { Header = "Изменить" };
            editItem.Click += (s, args) => ItemEdited?.Invoke(sender, cfg);

            contextMenu.Items.Add(deleteItem);
            contextMenu.Items.Add(markItem);
            contextMenu.Items.Add(editItem);

            element.ContextMenu = contextMenu;
            contextMenu.IsOpen = true;

            e.Handled = true;
        }

        private void ElementMoveEvent(object sender, MouseEventArgs e)
        {
            if (_draggedElement != null && e.RightButton == MouseButtonState.Pressed)
            {
                System.Windows.Point pos = e.GetPosition(TechGrid);

                double newX = pos.X - _mouseOffset.X;
                double newY = pos.Y - _mouseOffset.Y;

                Canvas.SetLeft(_draggedElement, newX);
                Canvas.SetTop(_draggedElement, newY);

                UpdateConnectionsForElement((Border)_draggedElement);
            }
        }
        #endregion
    }
}


