using SixLabors.ImageSharp.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ModdingManager.configs;
using System.IO;
using static System.Windows.Forms.LinkLabel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Security.Cryptography;
using System.Windows.Forms;
using ModdingManager.managers;
namespace ModdingManager
{
    /// <summary>
    /// Логика взаимодействия для TestWPF.xaml
    /// </summary>
    public partial class TechTreeCreator : Window
    {
        private Dictionary<UIElement, System.Windows.Point> originalPositions = new();

        private const int CellSize = 62;
        private const int GridSize = 20;
        private UIElement draggedElement = null;
        private System.Windows.Point mouseOffset;
        private List<UIElement> marked = new List<UIElement>();
        public TechTreeConfig CurrentTechTree = new();

        public ObservableCollection<string> Lines { get; set; } = new ObservableCollection<string>();
        public TechTreeCreator()
        {
            Lines.Add("строка 1");
            Lines.Add("строка 2");
            InitializeComponent();
            DrawGrid();
            InputManager.Current.PreProcessInput += (sender, e) =>
            {
                var args = e.StagingItem.Input as System.Windows.Input.KeyEventArgs;

                if (args != null && args.Key == Key.Enter)
                {
                    var focused = Keyboard.FocusedElement as System.Windows.Controls.RichTextBox;
                    if (focused != null)
                    {
                        var caret = focused.CaretPosition;
                        if (!caret.IsAtInsertionPosition)
                            caret = caret.GetInsertionPosition(LogicalDirection.Forward);

                        caret = caret.InsertLineBreak();
                        focused.CaretPosition = caret;

                        args.Handled = true;
                    }
                }
            };

        }

        private void DrawGrid()
        {
            for (int i = 0; i <= GridSize; i++)
            {
                double offset = i * CellSize;

                var vLine = new Line
                {
                    X1 = offset,
                    Y1 = 0,
                    X2 = offset,
                    Y2 = GridSize * CellSize,
                    Stroke = System.Windows.Media.Brushes.LightGray,
                    StrokeThickness = 1
                };
                GridCanvas.Children.Add(vLine);

                var hLine = new Line
                {
                    X1 = 0,
                    Y1 = offset,
                    X2 = GridSize * CellSize,
                    Y2 = offset,
                    Stroke = System.Windows.Media.Brushes.LightGray,
                    StrokeThickness = 1
                };
                GridCanvas.Children.Add(hLine);
            }
        }

        private System.Windows.Point GetVisualCenter(Border border)
        {
            double baseX = Canvas.GetLeft(border);
            double baseY = Canvas.GetTop(border);

            var image = GetImageFromBorder(border);
            if (image == null) return new System.Windows.Point(baseX + CellSize / 2, baseY + CellSize / 2);

            var transform = image.RenderTransform as TranslateTransform;
            double offsetX = transform?.X ?? 0;
            double offsetY = transform?.Y ?? 0;

            double centerX = baseX + CellSize / 2 + offsetX;
            double centerY = baseY + CellSize / 2 + offsetY;

            return new System.Windows.Point(centerX, centerY);
        }

        private double GetVisualRadius(Border border, Vector direction)
        {
            var image = GetImageFromBorder(border);
            if (image == null) return CellSize / 2;

            double halfWidth = image.Width / 2;
            double halfHeight = image.Height / 2;

            double dx = direction.X * halfWidth;
            double dy = direction.Y * halfHeight;

            return Math.Sqrt(dx * dx + dy * dy);
        }


        private ImageSource GetCombinedTechImage(System.Windows.Media.ImageSource overlayimg, System.Windows.Media.ImageSource backgroundimg, bool isBig)
        {
            double renderWidth = isBig ? 183 : 62;
            double renderHeight = isBig ? 84 : 62;

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                if (backgroundimg != null)
                {
                    dc.DrawImage(backgroundimg, new Rect(0, 0, renderWidth, renderHeight));
                }

                if (overlayimg != null)
                {
                    dc.DrawImage(overlayimg, new Rect(0, 0, renderWidth, renderHeight));
                }
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);

            return bmp;
        }

        private System.Windows.Controls.Image GetImageFromBorder(Border border)
        {
            if (border.Child is Canvas canvas)
            {
                return canvas.Children.OfType<System.Windows.Controls.Image>().FirstOrDefault();
            }
            return null;
        }


        public void RefreshTechTreeView()
        {
            var nonConnectionElements = GridCanvas.Children
                .OfType<UIElement>()
                .Where(el => !(el is Line) && !(el is Polygon))
                .ToList();

            foreach (var element in nonConnectionElements)
            {
                if (element is Border border)
                {
                    var relatedConnections = connections.Where(c => c.From == border || c.To == border).ToList();
                    foreach (var conn in relatedConnections)
                    {
                        if (conn.Line != null)
                            GridCanvas.Children.Remove(conn.Line);
                        if (conn.Arrow != null)
                            GridCanvas.Children.Remove(conn.Arrow);

                        connections.Remove(conn);
                    }
                }
            }

            foreach (var element in nonConnectionElements)
            {
                GridCanvas.Children.Remove(element);
            }

            if (CurrentTechTree == null)
                return;

            foreach (var item in CurrentTechTree.Items)
            {
                var element = CreateTechElement(item);

                System.Windows.Point position = GetCanvasPosition(item.GridX, item.GridY);
                Canvas.SetLeft(element, position.X);
                Canvas.SetTop(element, position.Y);

                GridCanvas.Children.Add(element);
            }
            RedrawAllConnections();
        }





        private UIElement CreateTechElement(TechTreeItemConfig item)
        {
            double imageWidth = item.IsBig ? 183 : 62;
            double imageHeight = item.IsBig ? 84 : 62;

            var image = new System.Windows.Controls.Image
            {
                Source = GetCombinedTechImage(item.Image, item.IsBig ? BigBackgroundImage.Source : SmallBackgroundImage.Source ,item.IsBig),
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
                Name = item.Id,
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1),
                Background = System.Windows.Media.Brushes.Transparent,
                Tag = false,
                ClipToBounds = false,
                Child = innerCanvas
            };

            border.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            border.MouseRightButtonDown += Element_MouseRightButtonDown;

            return border;
        }

        private void AttachEventHandlers(Canvas canvas)
        {
            foreach (var child in canvas.Children) 
            { 
                if (child.GetType() == typeof(System.Windows.Controls.Panel))
                {
                    var panel = canvas.Children[0];
                    panel.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                    panel.MouseRightButtonDown += Element_MouseRightButtonDown;
                }
            }
        }

        private System.Windows.Point GetCanvasPosition(int gridX, int gridY)
        {
            return new System.Windows.Point(gridX * CellSize, gridY * CellSize);
        }

        private Border FindElementBorderById(string id)
        {
            foreach (UIElement child in GridCanvas.Children)
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


        private void RedrawAllConnections()
        {
            if (CurrentTechTree == null) return;

            foreach (var pair in CurrentTechTree.ChildOf)
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

            foreach (var group in CurrentTechTree.Mutal)
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



        private void AddElement(int col, int row)
        {
            string newName = TechIdBox.Text;

            if (string.IsNullOrWhiteSpace(newName))
            {
                System.Windows.MessageBox.Show("Имя не может быть пустым!");
                return;
            }

            foreach (var child in GridCanvas.Children.OfType<Border>())
            {
                if (child.Name == newName)
                {
                    System.Windows.MessageBox.Show("Элемент с таким именем уже существует!");
                    return;
                }
            }

            System.Windows.Controls.Image overlayimg = null;
            System.Windows.Controls.Image backgroundimg = null;
            bool isBig = false;

            if (BigOverlayImage.Source != null && SmallOverlayImage.Source == null)
            {
                overlayimg = BigOverlayImage;
                backgroundimg = BigBackgroundImage;
                isBig = true;
            }
            else if (SmallOverlayImage.Source != null && BigOverlayImage.Source == null)
            {
                overlayimg = SmallOverlayImage;
                backgroundimg = SmallBackgroundImage;
                isBig = false;
            }
            else
            {
                System.Windows.MessageBox.Show("Картинку добавь", "калигула ебаная");
                return;
            }

            double imageWidth = isBig ? 183 : 62;
            double imageHeight = isBig ? 84 : 62;

            var image = new System.Windows.Controls.Image
            {
                Source = GetCombinedTechImage(overlayimg.Source, backgroundimg.Source, isBig),
                Width = imageWidth,
                Height = imageHeight,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(image, -(imageWidth - CellSize) / 2);
            Canvas.SetTop(image, -(imageHeight - CellSize) / 2);

            var canvas = new Canvas
            {
                Width = CellSize,
                Height = CellSize,
                ClipToBounds = false
            };
            canvas.Children.Add(image);
            Border panel = null;
            try {
                panel = new Border
                {
                    Width = CellSize,
                    Height = CellSize,
                    Name = newName,
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Tag = false,
                    ClipToBounds = false,
                    Child = canvas
                };
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("Введите коректное имя", "ашибькя", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsCellOccupied(col, row))
            {
                double cellX = col * CellSize;
                double cellY = row * CellSize;

                double offsetX = isBig ? -(imageWidth - CellSize) / 2 : 0;
                double offsetY = isBig ? -(imageHeight - CellSize) / 2 : 0;

                Canvas.SetLeft(panel, cellX + offsetX);
                Canvas.SetTop(panel, cellY + offsetY);

                panel.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                panel.MouseRightButtonDown += Element_MouseRightButtonDown;

                GridCanvas.Children.Add(panel);
                TextRange textRange = new TextRange(CategoriesBox.Document.ContentStart, CategoriesBox.Document.ContentEnd);
                var itemConfig = new TechTreeItemConfig
                {
                    GridX = col,
                    GridY = row,
                    Id = TechIdBox.Text,
                    LocDescription = TechDescBox.Text,
                    LocName = TechNameBox.Text,
                    Cost = TechCostBox.Text,
                    Folder = FolderBox.Text,
                    StartYear = StartYeatBox.Text,
                    Dependencies = new TextRange(DependenciesBox.Document.ContentStart, DependenciesBox.Document.ContentEnd).Text.Trim(),
                    Allowed = new TextRange(AllowedBox.Document.ContentStart, AllowedBox.Document.ContentEnd).Text.Trim(),
                    Modifiers = new TextRange(ModdifierBox.Document.ContentStart, ModdifierBox.Document.ContentEnd).Text.Trim(),
                    Effects = new TextRange(EffectBox.Document.ContentStart, EffectBox.Document.ContentEnd).Text.Trim(),
                    AiWillDo = new TextRange(AiWillDoBox.Document.ContentStart, AiWillDoBox.Document.ContentEnd).Text.Trim(),
                    IsBig = isBig,
                    Image = overlayimg.Source,
                    Categories = textRange.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList()

                };

                CurrentTechTree.Items.Add(itemConfig);
            }
            else
            {
                System.Windows.MessageBox.Show("Эта ячейка уже занята!");
            }
        }


        private void UpdateItemGridPosition(UIElement element)
        {
            if (element is Border border)
            {
                string id = border.Name;
                var item = CurrentTechTree.Items.FirstOrDefault(i => i.Id == id);
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


        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggedElement = sender as UIElement;
            mouseOffset = e.GetPosition(GridCanvas);
            mouseOffset.X -= Canvas.GetLeft(draggedElement);
            mouseOffset.Y -= Canvas.GetTop(draggedElement);

            originalPositions[draggedElement] = new System.Windows.Point(Canvas.GetLeft(draggedElement), Canvas.GetTop(draggedElement));

            GridCanvas.CaptureMouse();
            
        }



        private void Element_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border element)
                return;

            var contextMenu = new ContextMenu();

            var deleteItem = new MenuItem { Header = "Удалить" };
            deleteItem.Click += (s, args) => DeleteElement(element);

            var markItem = new MenuItem { Header = "Выделить" };
            markItem.Click += (s, args) => SetMark(element);

            var editItem = new MenuItem { Header = "Изменить" };
            editItem.Click += (s, args) => EditElement(element); 

            contextMenu.Items.Add(deleteItem);
            contextMenu.Items.Add(markItem);
            contextMenu.Items.Add(editItem);

            element.ContextMenu = contextMenu;
            contextMenu.IsOpen = true;

            e.Handled = true;
        }

        private void SetRichTextBoxText(System.Windows.Controls.RichTextBox box, string text)
        {
            if (box == null || string.IsNullOrWhiteSpace(text)) return;
            box.Document.Blocks.Clear();
            box.Document.Blocks.Add(new Paragraph(new Run(text)));
        }
        private void SetRichTextBoxText(System.Windows.Controls.RichTextBox box, List<string> lines)
        {
            if(box == null || lines == null) return;
            box.Document.Blocks.Clear();
            foreach (var line in lines)
            {
                box.Document.Blocks.Add(new Paragraph(new Run(line)));
            }
        }

        private void EditElement(Border element)
        {
            var id = (string)element.Name;
            var item = CurrentTechTree.Items.FirstOrDefault(i => i.Id == id);
            
            if (item == null) return;
            CurrentTechTree.Items.FirstOrDefault(i => i.Id == id).OldId = item.Id;
            TechIdBox.Text = item.Id;
            TechNameBox.Text = item.LocName;
            TechDescBox.Text = item.LocDescription;
            FolderBox.Text = item.Folder;
            StartYeatBox.Text = item.StartYear.ToString();
            TechCostBox.Text = item.Cost.ToString();
            TechCostModifBox.Text = item.ModifCost.ToString();

            SetRichTextBoxText(CategoriesBox, item.Categories);
            SetRichTextBoxText(AllowedBox, item.Allowed);
            SetRichTextBoxText(EffectBox, item.Effects);
            SetRichTextBoxText(AiWillDoBox, item.AiWillDo);
            SetRichTextBoxText(ModdifierBox, item.Modifiers);
            SetRichTextBoxText(DependenciesBox, item.Dependencies);
        }
        private void DeleteElement(Border element)
        {
            string id = element.Name;

            var relatedConnections = connections
                .Where(c => c.From == element || c.To == element)
                .ToList();

            foreach (var conn in relatedConnections)
            {
                if (conn.Line != null)
                    GridCanvas.Children.Remove(conn.Line);
                if (conn.Arrow != null)
                    GridCanvas.Children.Remove(conn.Arrow);
                connections.Remove(conn);
            }

            CurrentTechTree.Mutal.RemoveAll(pair => pair.Contains(id));
            CurrentTechTree.ChildOf.RemoveAll(pair => pair.Contains(id));
            var itemToRemove = CurrentTechTree.Items.FirstOrDefault(i => i.Id == id);
            if (itemToRemove != null)
                CurrentTechTree.Items.Remove(itemToRemove);
            GridCanvas.Children.Remove(element);
            marked.Remove(element);
        }
        private void SetMark(Border element)
        {
            bool isMarked = (bool)element.Tag;

            // Получаем Canvas внутри Border
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

                marked.Add(element);
                element.Tag = true;
            }
            else
            {
                var existingRect = canvas.Children
                    .OfType<System.Windows.Shapes.Rectangle>()
                    .FirstOrDefault(r => r.Name == selectionRectName);

                if (existingRect != null)
                    canvas.Children.Remove(existingRect);

                marked.Remove(element);
                element.Tag = false;
            }
        }
        
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedElement != null)
            {
                double left = Canvas.GetLeft(draggedElement);
                double top = Canvas.GetTop(draggedElement);

                int col = (int)Math.Round(left / CellSize);
                int row = (int)Math.Round(top / CellSize);

                double snapX = col * CellSize;
                double snapY = row * CellSize;

                Canvas.SetLeft(draggedElement, snapX);
                Canvas.SetTop(draggedElement, snapY);

                UpdateItemGridPosition(draggedElement);
                UpdateConnectionsForElement((Border)draggedElement);
                draggedElement = null;
                GridCanvas.ReleaseMouseCapture();
            }
        }



        private void RestoreOriginalPosition()
        {
            if (originalPositions.TryGetValue(draggedElement, out var originalPos))
            {
                Canvas.SetLeft(draggedElement, originalPos.X);
                Canvas.SetTop(draggedElement, originalPos.Y);
            }
        }


        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (draggedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point pos = e.GetPosition(GridCanvas);

                double newX = pos.X - mouseOffset.X;
                double newY = pos.Y - mouseOffset.Y;

                Canvas.SetLeft(draggedElement, newX);
                Canvas.SetTop(draggedElement, newY);

                UpdateConnectionsForElement((Border)draggedElement);
            }
        }

        private void SetMutal()
        {
            if (marked.Count < 2) return;

            var elements = marked.Cast<Border>().ToList();

            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = i + 1; j < elements.Count; j++)
                {
                    var nameA = elements[i].Name;
                    var nameB = elements[j].Name;
                    if (CurrentTechTree.Mutal.Any(pair => pair.Contains(nameA) && pair.Contains(nameB)) ||
    CurrentTechTree.ChildOf.Any(pair => pair.Contains(nameA) && pair.Contains(nameB)))
                    {
                        continue;
                    }

                    CurrentTechTree.Mutal.Add(new List<string> { nameA, nameB });
                    DrawConnection(elements[i], elements[j], System.Windows.Media.Brushes.Red, true);
                }
            }
        }
        private void SetChild()
        {
            if (marked.Count < 2) return;

            var elements = marked.Cast<Border>().OrderBy(el =>
            {
                return OrientationBox.SelectedItem.ToString() == "vertical"
                    ? GetGridY(el)
                    : GetGridX(el);
            }).ToList();

            for (int i = 0; i < elements.Count - 1; i++)
            {
                var first = elements[i];
                var second = elements[i + 1];

                int x1 = GetGridX(first);
                int y1 = GetGridY(first);
                int x2 = GetGridX(second);
                int y2 = GetGridY(second);

                bool isHorizontal = OrientationBox.SelectedItem.ToString() == "horizontal";

                if (isHorizontal)
                {
                    if (y1 != y2)
                    {
                        System.Windows.MessageBox.Show("Для горизонтальной связи элементы должны быть на одной линии по Y.", "Ошибка связи");
                        continue;
                    }
                    if (x1 == x2)
                    {
                        System.Windows.MessageBox.Show("Для горизонтальной связи X координаты не могут совпадать.", "Ошибка связи");
                        continue;
                    }
                }
                else 
                {
                    if (x1 != x2)
                    {
                        System.Windows.MessageBox.Show("Для вертикальной связи элементы должны быть в одном столбце по X.", "Ошибка связи");
                        continue;
                    }
                    if (y1 == y2)
                    {
                        System.Windows.MessageBox.Show("Для вертикальной связи Y координаты не могут совпадать.", "Ошибка связи");
                        continue;
                    }
                }

                Border parent = null;
                Border child = null;

                if (isHorizontal)
                {
                    parent = x1 < x2 ? first : second;
                    child = x1 < x2 ? second : first;
                }
                else // vertical
                {
                    parent = y1 < y2 ? first : second;
                    child = y1 < y2 ? second : first;
                }

                string parentName = parent.Name;
                string childName = child.Name;

                if (CurrentTechTree.ChildOf.Any(pair => (pair[0] == parentName && pair[1] == childName) || (pair[0] == childName && pair[1] == parentName)) ||
                    CurrentTechTree.Mutal.Any(pair => pair.Contains(parentName) && pair.Contains(childName)))
                {
                    continue;
                }

                CurrentTechTree.ChildOf.Add(new List<string> { parentName, childName });
                DrawConnection(parent, child, System.Windows.Media.Brushes.Blue, false);
            }
        }

        private void DrawConnection(Border from, Border to, System.Windows.Media.Brush color, bool isMutual)
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

            GridCanvas.Children.Insert(0, line);

            Polygon arrow = null;
            if (!isMutual)
            {
                var vec = end - start;
                vec.Normalize();
                var perp = new Vector(-vec.Y, vec.X);

                System.Windows.Point arrowTip = end;
                System.Windows.Point base1 = arrowTip - vec * 10 + perp * 5;
                System.Windows.Point base2 = arrowTip - vec * 10 - perp * 5;

                arrow = new Polygon
                {
                    Points = new PointCollection { arrowTip, base1, base2 },
                    Fill = color,
                    IsHitTestVisible = false
                };

                GridCanvas.Children.Insert(0, arrow);
            }

            connections.Add(new ElementConnection
            {
                From = from,
                To = to,
                Line = line,
                Arrow = arrow,
                IsMutual = isMutual
            });
        }



        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddElement(1, 1);
        }


        private void MutalEvent(object sender, RoutedEventArgs e)
        {
            SetMutal();
        }

        private void ChildEvent(object sender, RoutedEventArgs e)
        {
            SetChild();
        }

        private class ElementConnection
        {
            public Polygon Arrow;
            public Border From;
            public Border To;
            public Line Line;
            public bool IsMutual;
        }

        private void UpdateConnectionsForElement(Border element)
        {
            foreach (var conn in connections)
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



        private bool IsCellOccupied(int col, int row, Border ignore = null)
        {
            foreach (var child in GridCanvas.Children.OfType<Border>())
            {
                if (child == ignore) continue;

                int x = (int)(Canvas.GetLeft(child) / CellSize);
                int y = (int)(Canvas.GetTop(child) / CellSize);

                if (x == col && y == row)
                    return true;
            }
            return false;
        }


        private List<ElementConnection> connections = new List<ElementConnection>();

        [DllImport("user32.dll")]
        static extern int ToUnicode(uint virtualKeyCode, uint scanCode,
    byte[] keyboardState, [Out, MarshalAs(UnmanagedType.LPWStr)]
    StringBuilder receivingBuffer, int bufferSize, uint flags);

        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        private string GetCharFromKey(Key key)
        {
            string result = "";

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            if (!GetKeyboardState(keyboardState))
                return result;

            uint scanCode = MapVirtualKey((uint)virtualKey, 0);
            StringBuilder stringBuilder = new StringBuilder(2);

            int count = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            if (count > 0)
                result = stringBuilder.ToString();

            return result;
        }


        private void MyTextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            TechNameBox.Text += e.Text;
            TechNameBox.CaretIndex = TechNameBox.Text.Length;
            e.Handled = true;
        }

        private void MyTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string typedChar = GetCharFromKey(e.Key);
            var thіs = sender as System.Windows.Controls.TextBox;
            if (thіs == null) return;

            if (e.Key == Key.Left)
            {
                if (thіs.CaretIndex > 0)
                    thіs.CaretIndex--;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Right)
            {
                if (thіs.CaretIndex < thіs.Text.Length)
                    thіs.CaretIndex++;
                e.Handled = true;
                return;
            }

            if (!string.IsNullOrEmpty(typedChar))
            {
                int caretPos = thіs.CaretIndex;
                thіs.Text = thіs.Text.Insert(caretPos, typedChar);
                thіs.CaretIndex = caretPos + typedChar.Length;
                e.Handled = true;
            }
        }


        private void MyRichBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true; 
        }

        private void MyRichBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var richText = sender as System.Windows.Controls.RichTextBox;
            if (richText == null) return;

            var caret = richText.CaretPosition;
            if (!caret.IsAtInsertionPosition)
                caret = caret.GetInsertionPosition(LogicalDirection.Backward);

            if (e.Key == Key.Back)
            {
                var backPos = caret.GetPositionAtOffset(-1, LogicalDirection.Backward);
                if (backPos != null)
                {
                    var range = new TextRange(backPos, caret);
                    range.Text = ""; 
                    richText.CaretPosition = backPos;
                }
                e.Handled = true;
                return;
            }

            string typedChar = GetCharFromKey(e.Key);
            if (!string.IsNullOrEmpty(typedChar))
            {
                caret.InsertTextInRun(typedChar);
                richText.CaretPosition = caret.GetPositionAtOffset(typedChar.Length, LogicalDirection.Forward);
                e.Handled = true;
            }
            else
            {
                var insertionPos = caret.GetPositionAtOffset(typedChar.Length, LogicalDirection.Forward);
                insertionPos.InsertTextInRun(typedChar);
            }
        }

        private void InsertTextAtCaret(System.Windows.Controls.RichTextBox richText, string typedChar)
        {
            var caret = richText.CaretPosition;
            var insertionPos = caret.GetPositionAtOffset(typedChar.Length, LogicalDirection.Forward);

            if (insertionPos == null)
            {
                // Устанавливаем каретку в начало документа
                richText.CaretPosition = richText.Document.ContentStart;
                insertionPos = richText.CaretPosition;
            }

            // Вставляем текст
            richText.CaretPosition = insertionPos;
            richText.CaretPosition.InsertTextInRun(typedChar);
        }





        private void TechIconCanvas_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string imagePath = files[0];
                    var current = sender as System.Windows.Controls.Canvas;

                    try
                    {
                        BitmapImage original = new BitmapImage(new Uri(imagePath));

                        int targetWidth = current.Name.Contains("Small") ? 62 : 183;
                        int targetHeight = current.Name.Contains("Small") ? 62 : 84;

                        var resized = ResizeToBitmap(original, targetWidth, targetHeight);

                        if (current.Name.Contains("Small"))
                            SmallOverlayImage.Source = resized;
                        else if (current.Name.Contains("Big"))
                            BigOverlayImage.Source = resized;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                    }
                }
            }
        }

        private BitmapSource ResizeToBitmap(BitmapSource source, int targetWidth, int targetHeight)
        {
            var scale = new ScaleTransform(
                (double)targetWidth / source.PixelWidth,
                (double)targetHeight / source.PixelHeight);

            var transformed = new TransformedBitmap(source, scale);

            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                context.DrawImage(transformed, new Rect(0, 0, targetWidth, targetHeight));
            }

            var target = new RenderTargetBitmap(
                targetWidth, targetHeight, 96, 96, PixelFormats.Pbgra32);
            target.Render(drawingVisual);

            return target;
        }


        private void RemoveImageEvent(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var current = sender as System.Windows.Controls.Canvas;
                if (current.Name.Contains("Small"))
                {
                    SmallOverlayImage.Source = null;
                }
                else if (current.Name.Contains("Big"))
                {
                    BigOverlayImage.Source = null;
                }
            }
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            ExportTechTreeToFile();
        }

        private void ExportTechTreeToFile()
        {
            string fileName = $"{TechTreeNameBox.Text}.txt";
            string fullPath = System.IO.Path.Combine(ModManager.Directory, "common", "technologies", fileName);

            var sb = new StringBuilder();
            sb.AppendLine("technologies = {");

            foreach (var item in CurrentTechTree.Items)
            {
                sb.AppendLine($"\t{item.Id} = {{");

                if (!string.IsNullOrWhiteSpace(item.EnableType) && !string.IsNullOrWhiteSpace(item.Enable))
                {
                    sb.AppendLine($"\t\tenable_{item.EnableType} = {{");
                    sb.AppendLine($"\t\t\t{item.Enable}");
                    sb.AppendLine($"\t\t}}");
                }

                var children = CurrentTechTree.ChildOf.Where(pair => pair.Count == 2 && pair[0] == item.Id).Select(pair => pair[1]);
                var penis = children;
                foreach (var child in children)
                {
                    sb.AppendLine($"\t\tpath = {{");
                    sb.AppendLine($"\t\t\tleads_to_tech = {child}");
                    sb.AppendLine($"\t\t\tresearch_cost_coeff = {item.Cost}");
                    sb.AppendLine($"\t\t}}");
                }

                sb.AppendLine($"\t\tresearch_cost = {item.ModifCost}");
                sb.AppendLine($"\t\tstart_year = {item.StartYear}");

                sb.AppendLine($"\t\tfolder = {{");
                sb.AppendLine($"\t\t\tname = {item.Folder}");
                sb.AppendLine($"\t\t\tposition = {{ x = {item.GridX} y = @{item.StartYear} }}");
                sb.AppendLine($"\t\t}}");

                if (item.Categories != null && item.Categories.Any())
                {
                    sb.AppendLine("\t\tcategories = {");
                    foreach (var cat in item.Categories)
                        sb.AppendLine($"\t\t\t{cat}");
                    sb.AppendLine("\t\t}");
                }

                if (!string.IsNullOrWhiteSpace(item.AiWillDo))
                {
                    sb.AppendLine("\t\tai_will_do = {");
                    sb.AppendLine($"\t\t\t{item.AiWillDo}");
                    sb.AppendLine("\t\t}");
                }

                var mutalItems = CurrentTechTree.Mutal
                    .Where(group => group.Contains(item.Id))
                    .SelectMany(group => group)
                    .Where(name => name != item.Id)
                    .Distinct()
                    .ToList();

                if (mutalItems.Any())
                {
                    sb.AppendLine("\t\tXOR = {");
                    foreach (var xor in mutalItems)
                        sb.AppendLine($"\t\t\t{xor}");
                    sb.AppendLine("\t\t}");
                }

                sb.AppendLine("\t}");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, sb.ToString());

            System.Windows.MessageBox.Show($"Файл сохранён в: {fullPath}");
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            var i = 1;
            foreach (var sublist in CurrentTechTree.ChildOf)
            {
                sb.AppendLine("Svaz " + i);
                i++;
                foreach (var item in sublist)
                {
                    sb.AppendLine(item);
                }
            }
            System.Windows.MessageBox.Show(sb.ToString());

            foreach (UIElement elem in marked)
            {
                var elemNest = elem as Border;
                var id = elemNest.Name;
                var item = CurrentTechTree.Items.FirstOrDefault(i => i.Id == id);

                System.Windows.MessageBox.Show($"{elemNest.Name}: {item.GridX} {item.GridY}");
            }
        }

        private int GetGridX(Border border)
        {
            double left = Canvas.GetLeft(border);
            double centerX = left + CellSize / 2;
            return (int)(centerX / CellSize);
        }

        private int GetGridY(Border border)
        {
            double top = Canvas.GetTop(border);
            double centerY = top + CellSize / 2;
            return (int)(centerY / CellSize);
        }

        private string GetRichTextBoxText(System.Windows.Controls.RichTextBox rtb)
        {
            return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.Trim();
        }

        private List<string> GetRichTextBoxLines(System.Windows.Controls.RichTextBox rtb)
        {
            return GetRichTextBoxText(rtb).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }


        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (marked.Count != 1)
            {
                System.Windows.MessageBox.Show("Выделите ровно один элемент для обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Берем выделенный Border
            Border selected = marked.First() as Border;
            if (selected == null)
            {
                System.Windows.MessageBox.Show("Выделенный элемент некорректен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Ищем элемент конфигурации по Border.Name
            var item = CurrentTechTree.Items.FirstOrDefault(x => x.Id == selected.Name);
            if (item == null)
            {
                System.Windows.MessageBox.Show("Не удалось найти элемент в конфигурации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Запоминаем старый Id
            CurrentTechTree.Items.FirstOrDefault(x => x.Id == selected.Name).OldId = item.Id;

            // Обновляем поля из UI
            item.LocName = TechNameBox.Text;
            item.LocDescription = TechDescBox.Text;
            item.ModifCost = int.TryParse(TechCostModifBox.Text, out int modifCost) ? modifCost : 0;
            item.Cost = TechCostBox.Text;
            item.Folder = FolderBox.Text;
            item.StartYear = StartYeatBox.Text;
            item.Categories = GetRichTextBoxLines(CategoriesBox);
            item.Allowed = GetRichTextBoxText(AllowedBox);
            item.Effects = GetRichTextBoxText(EffectBox);
            item.Modifiers = GetRichTextBoxText(ModdifierBox);
            item.AiWillDo = GetRichTextBoxText(AiWillDoBox);
            item.Dependencies = GetRichTextBoxText(DependenciesBox);
            item.Image = item.IsBig ? BigOverlayImage.Source : SmallOverlayImage.Source;
            var overlayimg = item.IsBig ? BigOverlayImage : SmallOverlayImage;
            var backgroundimg = item.IsBig ? BigBackgroundImage : BigOverlayImage;
            // Обновляем изображение, если оно выбрано
            if (selected is Border border && border.Child is Canvas canavas)
            {
                foreach (var elem in canavas.Children)
                {
                    if (elem is System.Windows.Controls.Image img)
                    {
                        img.Source = GetCombinedTechImage(overlayimg.Source, backgroundimg.Source, item.IsBig);
                    }
                }
            }

            // Обновляем имя Border
            selected.Name = item.Id;

            // Обновляем элемент в списке Items
            var index = CurrentTechTree.Items.FindIndex(x => x.OldId == item.OldId);
            if (index >= 0)
            {
                CurrentTechTree.Items[index] = item;
            }

            System.Windows.MessageBox.Show("Элемент успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadEvent(object sender, RoutedEventArgs e)
        {
            WpfConfigManager.LoadConfigAsync(this);
        }

        private async void SaveEvent(object sender, RoutedEventArgs e)
        {
            await WpfConfigManager.SaveConfigAsync(this, TechTreeNameBox.Text);
        }
    }
}
