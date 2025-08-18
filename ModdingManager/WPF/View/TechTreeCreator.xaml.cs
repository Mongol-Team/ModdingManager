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
using System.Text.RegularExpressions;
using ModdingManager.managers.forms;
using ModdingManager.classes.args;
using ModdingManager.classes.extentions;
using ModdingManager.managers.@base;
using ModdingManager.classes.managers.gfx;
namespace ModdingManager
{
    public partial class TechTreeCreator : Window
    {
        private Dictionary<UIElement, System.Windows.Point> originalPositions = new();

        private const int CellSize = 62;
        private const int GridSize = 20;
        private UIElement draggedElement = null;
        private System.Windows.Point mouseOffset;
        private List<UIElement> marked = new List<UIElement>();
        public TechTreeConfig CurrentTechTree = new();

        public TechTreeCreator()
        {
            InitializeComponent();
            DrawGrid();
            Debugger.AttachIfDebug(this);
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

            var image = border.GetImage();
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
            var image = border.GetImage();
            if (image == null) return CellSize / 2;

            double halfWidth = image.Width / 2;
            double halfHeight = image.Height / 2;

            double dx = direction.X * halfWidth;
            double dy = direction.Y * halfHeight;

            return Math.Sqrt(dx * dx + dy * dy);
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

            TreeLedgerBox.Text = CurrentTechTree.Ledger;
            TechTreeNameBox.Text = CurrentTechTree.Name;
            foreach (ComboBoxItem item in OrientationBox.Items)
            {
                if (item.Content.ToString() == CurrentTechTree.Orientation)
                {
                    OrientationBox.SelectedItem = item;
                    break;
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
                Source = item.Image.GetCombinedTechImage(item.IsBig ? BigBackgroundImage.Source : SmallBackgroundImage.Source , item.IsBig ? 1 : 2),
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


        static string GetTabs(int count) => new string('\t', count);

        public void InsertGUITechTreeEntries(string modFolderPath, TechTreeConfig techTree, string gameFolderPath)
        {
            string filePath = GetOrCopyGuiFile(modFolderPath, gameFolderPath);
            if (filePath == null) return;

            string[] lines = File.ReadAllLines(filePath);
            List<string> newLines = new List<string>();
            bool isVertical = techTree.Orientation?.ToLower() == "vertical";

            int containerStart = -1, containerEnd = -1;
            int braceLevel = 0;
            bool inTargetContainer = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (!inTargetContainer && line.Contains("containerWindowType = {") &&
                    i + 1 < lines.Length && lines[i + 1].Trim().Contains("name = \"countrytechtreeview\""))
                {
                    containerStart = i;
                    inTargetContainer = true;
                    braceLevel = 1;
                    i++; // Пропускаем строку с name
                    continue;
                }

                if (inTargetContainer)
                {
                    braceLevel += line.Count(c => c == '{');
                    braceLevel -= line.Count(c => c == '}');

                    if (braceLevel == 0)
                    {
                        containerEnd = i;
                        break;
                    }
                }
            }

            if (containerStart == -1 || containerEnd == -1)
            {
                Console.WriteLine("Could not find countrytechtreeview container");
                return;
            }

            string content = GenerateTechTreeContent(techTree, isVertical,
                lines[containerStart].TakeWhile(c => c == '\t').Count() + 1);

            List<string> result = new List<string>();
            result.AddRange(lines.Take(containerEnd));
            result.Add(content);
            result.AddRange(lines.Skip(containerEnd));

            File.WriteAllLines(filePath, result);
            Console.WriteLine("Successfully inserted tech tree entries");
        }
        private string GenerateTechTreeContent(TechTreeConfig techTree, bool isVertical, int innerTabCount)
        {
            string techTreeName = techTree.Name;
            var rootItems = FindRootItems(techTree);

            StringBuilder entries = new StringBuilder();

            entries.AppendLine($"{GetTabs(innerTabCount)}containerWindowType = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}name = \"{techTreeName}\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}position = {{ x=0 y=47 }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}size = {{ width = 100%% height = 100%% }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}margin = {{ top = 13 left = 13 bottom = 24 right = 25}}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}drag_scroll = {{ left middle }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}verticalScrollbar = \"right_vertical_slider\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}horizontalScrollbar = \"bottom_horizontal_slider\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}scroll_wheel_factor = 40");
            entries.AppendLine();
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}background = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}name = \"Background\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}quadTextureSprite =\"GFX_tiled_window_2b_border\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}}}");
            entries.AppendLine();
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}containerWindowType = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}name = \"techtree_stripes\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}position = {{ x= 0 y= 0 }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}size = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}width = 1400 height = 1675");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}min = {{ width = 100%% height = 100%% }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}clipping = no");
            entries.AppendLine();
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}iconType = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}name =\"{techTreeName}_techtree_bg\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}spriteType = \"GFX_{techTreeName}_techtree_bg\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x=0 y=0 }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}alwaystransparent = yes");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
            entries.AppendLine();
            
            int startCoord = isVertical ? 80 : 170;
            int fixedCoord = 50;
            int year = techTree.Items.Any() ? techTree.Items.Min(i => i.StartYear) : 1955;

            for (int j = 2; j <= 14; j++)
            {
                entries.AppendLine($"{GetTabs(innerTabCount + 2)}instantTextBoxType = {{");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}name = \"{techTreeName}_year{j}\"");

                if (isVertical)
                    entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x = {fixedCoord} y = {startCoord} }}");
                else
                    entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x = {startCoord} y = {fixedCoord} }}");

                entries.AppendLine($"{GetTabs(innerTabCount + 3)}textureFile = \"\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}font = \"hoi_36header\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}borderSize = {{ x = 0 y = 0}}");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}text = \"{year}\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}maxWidth = 170");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}maxHeight = 32");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}format = left");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}Orientation = \"UPPER_LEFT\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
                entries.AppendLine();

                startCoord += 140;
                year += 2;
            }

            entries.AppendLine($"{GetTabs(innerTabCount + 2)}iconType = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}name = \"highlight_{techTreeName}_1\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}spriteType = \"GFX_tutorial_research_small_item_icon_glow\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x=135 y=170}}");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}hide = yes");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}alwaystransparent = yes");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}}}"); // Закрываем внутренний containerWindowType

            foreach (var item in rootItems)
            {
                entries.AppendLine($"{GetTabs(innerTabCount + 2)}gridboxType = {{");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}name = \"{item.Id}_tree\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x = {item.GridX*10 + (isVertical ? 15 : 0)} y = {item.GridY*10 + (isVertical ? 15 :55)} }}");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}slotsize = {{ width = 70 height = 70 }}");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}format = \"LEFT\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
                entries.AppendLine();
            }
            entries.AppendLine($"{GetTabs(innerTabCount)}}}"); // Закрываем основной containerWindowType

            return entries.ToString();
        }


        private List<TechTreeItemConfig> FindRootItems(TechTreeConfig techTree)
        {
            var rootItems = new List<TechTreeItemConfig>();
            var allChildren = techTree.ChildOf.SelectMany(pair => pair.Skip(1)).ToHashSet();

            foreach (var item in techTree.Items)
            {
                bool hasChildren = techTree.ChildOf.Any(pair => pair[0] == item.Id);

                bool isChild = allChildren.Contains(item.Id);

                if (hasChildren && !isChild)
                {
                    rootItems.Add(item);
                }
            }

            return rootItems;
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
                Source = overlayimg.Source.GetCombinedTechImage(backgroundimg.Source, isBig ? 1 : 2),
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
                System.Windows.MessageBox.Show("Введите коректное имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsCellOccupied(col, row))
            {
                var itemConfig = new TechTreeItemConfig();
                try
                {
                    itemConfig = new TechTreeItemConfig
                    {
                        GridX = col,
                        GridY = row,
                        Id = TechIdBox?.Text ?? "",
                        LocDescription = TechDescBox?.Text ?? "",
                        LocName = TechNameBox?.Text ?? "",
                        Cost = int.TryParse(TechCostBox?.Text, out var cost) ? cost : 0,
                        StartYear = int.TryParse(StartYeatBox?.Text, out var year) ? year : 0,
                        ModifCost = int.TryParse(TechCostModifBox?.Text, out var modifCost) ? modifCost : 0,

                        Categories = CathegoryModdifierBox != null ? GetRichTextBoxText(CathegoryModdifierBox) : "",
                        Dependencies = DependenciesBox != null ? GetRichTextBoxLines(DependenciesBox) : new List<string>(),
                        Allowed = AllowedBox != null ? GetRichTextBoxLines(AllowedBox) : new List<string>(),
                        Modifiers = ModdifierBox != null ? GetRichTextBoxLines(ModdifierBox) : new List<string>(),
                        Effects = EffectBox != null ? GetRichTextBoxLines(EffectBox) : new List<string>(),
                        Enables = EnablesBox != null ? GetRichTextBoxLines(EnablesBox) : new List<string>(),

                        AiWillDo = GetRichTextBoxText(AiWillDoBox) ?? "",
                        IsBig = isBig,
                        Image = overlayimg?.Source
                    };

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                double cellX = col * CellSize;
                double cellY = row * CellSize;

                double offsetX = isBig ? -(imageWidth - CellSize) / 2 : 0;
                double offsetY = isBig ? -(imageHeight - CellSize) / 2 : 0;

                Canvas.SetLeft(panel, cellX + offsetX);
                Canvas.SetTop(panel, cellY + offsetY);

                panel.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                panel.MouseRightButtonDown += Element_MouseRightButtonDown;

                GridCanvas.Children.Add(panel);
                

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
            var paragraph = new Paragraph();

            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                paragraph.Inlines.Add(new Run(lines[i]));
                if (i < lines.Length - 1)
                    paragraph.Inlines.Add(new LineBreak());
            }

            box.Document.Blocks.Add(paragraph);
        }

        private void SetRichTextBoxText(System.Windows.Controls.RichTextBox box, List<string> lines)
        {
            if (box == null || lines == null) return;

            box.Document.Blocks.Clear();
            var paragraph = new Paragraph();

            for (int i = 0; i < lines.Count; i++)
            {
                paragraph.Inlines.Add(new Run(lines[i]));
                if (i < lines.Count - 1)
                    paragraph.Inlines.Add(new LineBreak());
            }

            box.Document.Blocks.Add(paragraph);
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
            StartYeatBox.Text = item.StartYear.ToString();
            TechCostBox.Text = item.Cost.ToString();
            TechCostModifBox.Text = item.ModifCost.ToString();

            SetRichTextBoxText(EnablesBox, item.Enables);
            SetRichTextBoxText(AllowedBox, item.Allowed);
            SetRichTextBoxText(EffectBox, item.Effects);
            SetRichTextBoxText(AiWillDoBox, item.AiWillDo);
            SetRichTextBoxText(ModdifierBox, item.Modifiers);
            SetRichTextBoxText(CathegoryModdifierBox, item.Categories);
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
                var val = (OrientationBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                bool isHorizontal = val == "horizontal";

                if (isHorizontal)
                {
                    if (y1 != y2)
                    {
                        System.Windows.MessageBox.Show($"Элементы “{first.Name}” и “{second.Name}” находятся в разных строках\n(Y={y1} и Y={y2}).\nДля горизонтальной связи они должны быть на одной линии по Y.", "Ошибка связи");
                        continue;
                    }
                    if (x1 == x2)
                    {
                        System.Windows.MessageBox.Show($"Элементы “{first.Name}” и “{second.Name}” находятся в одном столбце\n(X={x1}).\nДля горизонтальной связи они должны быть в разных колонках.", "Ошибка связи");
                        continue;
                    }
                }
                else
                {
                    if (x1 != x2)
                    {
                        System.Windows.MessageBox.Show($"Элементы “{first.Name}” и “{second.Name}” находятся в разных колонках\n(X={x1} и X={x2}).\nДля вертикальной связи они должны быть в одном столбце по X.", "Ошибка связи");
                        continue;
                    }
                    if (y1 == y2)
                    {
                        System.Windows.MessageBox.Show($"Элементы “{first.Name}” и “{second.Name}” находятся в одной строке\n(Y={y1}).\nДля вертикальной связи они должны быть в разных строках.", "Ошибка связи");
                        continue;
                    }
                }

                Border parent = null;
                Border child = null;

                if (isHorizontal)
                {
                    int deltaX = x2 - x1;
                    parent = deltaX > 0 ? first : second;
                    child = deltaX > 0 ? second : first;
                }
                else
                {
                    int deltaY = y2 - y1;
                    parent = deltaY > 0 ? first : second;
                    child = deltaY > 0 ? second : first;
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
            if (!string.IsNullOrEmpty(TechTreeNameBox.Text) || !string.IsNullOrEmpty(TreeLedgerBox.Text) || !string.IsNullOrEmpty((OrientationBox.SelectedItem as ComboBoxItem)?.Content.ToString()))
            {
                DoneTreeButton_Click(sender, e);
            }
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

        


       
        public void InsertGUITechTreeContainers(string modFolderPath, TechTreeConfig techTree, string gamePath)
        {
            string filePath = GetOrCopyGuiFile(modFolderPath, gamePath);
            if (filePath == null) return;

            string[] lines = File.ReadAllLines(filePath);

            int lastBrace = -1;
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].Trim() == "}")
                {
                    lastBrace = i;
                    break;
                }
            }

            if (lastBrace == -1)
            {
                Console.WriteLine("Could not find guiTypes closing brace");
                return;
            }

            string content = GenerateTechTreeContainersContent(techTree);

            List<string> result = new List<string>();
            result.AddRange(lines.Take(lastBrace));
            result.Add(content);
            result.AddRange(lines.Skip(lastBrace));

            File.WriteAllLines(filePath, result);
            Console.WriteLine("Successfully added tech tree containers");
        }

        private string GenerateTechTreeContainersContent(TechTreeConfig techTree, int baseTabCount = 1)
        {
            string techTreeName = techTree.Name;
            string tab = new string('\t', baseTabCount);
            string innerTab = new string('\t', baseTabCount + 1);
            string doubleInnerTab = new string('\t', baseTabCount + 2);

            var sb = new StringBuilder();

            sb.AppendLine($"{tab}containerWindowType = {{");
            sb.AppendLine($"{innerTab}name = \"techtree_{techTreeName}_small_item\"");
            sb.AppendLine($"{innerTab}position = {{ x=0 y=0 }}");
            sb.AppendLine($"{innerTab}size = {{ width = 72 height = 72 }}");
            sb.AppendLine($"{innerTab}clipping = no");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}background = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"bg\"");
            sb.AppendLine($"{doubleInnerTab}quadTextureSprite =\"GFX_technology_unavailable_item_bg\"");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}iconType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"Icon\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x=3 y=3 }}");
            sb.AppendLine($"{doubleInnerTab}spriteType = \"GFX_technology_medium\"");
            sb.AppendLine($"{doubleInnerTab}alwaystransparent = yes");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}iconType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"bonus_icon\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x=-1 y=-22 }}");
            sb.AppendLine($"{doubleInnerTab}spriteType = \"GFX_tech_bonus\"");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}instantTextBoxType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"bonus\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x = -1 y = -21 }}");
            sb.AppendLine($"{doubleInnerTab}textureFile = \"\"");
            sb.AppendLine($"{doubleInnerTab}font = \"hoi_16mbs\"");
            sb.AppendLine($"{doubleInnerTab}borderSize = {{x = 4 y = 4}}");
            sb.AppendLine($"{doubleInnerTab}text = \"lol boat\"");
            sb.AppendLine($"{doubleInnerTab}maxWidth = 80");
            sb.AppendLine($"{doubleInnerTab}maxHeight = 20");
            sb.AppendLine($"{doubleInnerTab}format = center");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}iconType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"can_assign_design_team_icon\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x=0 y=42 }}");
            sb.AppendLine($"{doubleInnerTab}spriteType = \"GFX_design_team_icon\"");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine($"{tab}}}");
            sb.AppendLine();

            sb.AppendLine($"{tab}containerWindowType = {{");
            sb.AppendLine($"{innerTab}name = \"techtree_{techTreeName}_item\"");
            sb.AppendLine($"{innerTab}position = {{ x=-56 y=-7 }}");
            sb.AppendLine($"{innerTab}size = {{ width = 183 height = 84 }}");
            sb.AppendLine($"{innerTab}clipping = no");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}background = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"Background\"");
            sb.AppendLine($"{doubleInnerTab}quadTextureSprite =\"GFX_technology_unavailable_item_bg\"");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}iconType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"Icon\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x=91 y=50 }}");
            sb.AppendLine($"{doubleInnerTab}spriteType = \"GFX_technology_medium\"");
            sb.AppendLine($"{doubleInnerTab}centerposition = yes");
            sb.AppendLine($"{doubleInnerTab}alwaystransparent = yes");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}instantTextBoxType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"Name\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x = 3 y = -3 }}");
            sb.AppendLine($"{doubleInnerTab}textureFile = \"\"");
            sb.AppendLine($"{doubleInnerTab}font = \"hoi_20bs\"");
            sb.AppendLine($"{doubleInnerTab}borderSize = {{x = 4 y = 4}}");
            sb.AppendLine($"{doubleInnerTab}text = \"Happy-Go-Lucky-Tank\"");
            sb.AppendLine($"{doubleInnerTab}maxWidth = 160");
            sb.AppendLine($"{doubleInnerTab}maxHeight = 20");
            sb.AppendLine($"{doubleInnerTab}fixedsize = yes");
            sb.AppendLine($"{doubleInnerTab}format = left");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}iconType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"bonus_icon\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x=111 y=-22 }}");
            sb.AppendLine($"{doubleInnerTab}spriteType = \"GFX_tech_bonus\"");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}instantTextBoxType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"bonus\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x = 111 y = -22 }}");
            sb.AppendLine($"{doubleInnerTab}textureFile = \"\"");
            sb.AppendLine($"{doubleInnerTab}font = \"hoi_16mbs\"");
            sb.AppendLine($"{doubleInnerTab}borderSize = {{x = 4 y = 4}}");
            sb.AppendLine($"{doubleInnerTab}text = \"lol boat\"");
            sb.AppendLine($"{doubleInnerTab}maxWidth = 80");
            sb.AppendLine($"{doubleInnerTab}maxHeight = 20");
            sb.AppendLine($"{doubleInnerTab}format = center");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();

            sb.AppendLine($"{innerTab}containerWindowType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"sub_technology_slot_0\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x=141 y=1 }}");
            sb.AppendLine($"{doubleInnerTab}size = {{ width = 35 height = 26 }}");
            sb.AppendLine($"{doubleInnerTab}clipping = no");
            sb.AppendLine();
            sb.AppendLine($"{doubleInnerTab}background = {{");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}name = \"Background\"");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}spriteType =\"GFX_subtechnology_unavailable_item_bg\"");
            sb.AppendLine($"{doubleInnerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{doubleInnerTab}iconType = {{");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}name = \"picture\"");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}position = {{ x=2 y=2 }}");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}spriteType = \"GFX_subtech_rocket\"");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}alwaystransparent = yes");
            sb.AppendLine($"{doubleInnerTab}}}");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}containerWindowType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"sub_technology_slot_1\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x=141 y=1 }}");
            sb.AppendLine($"{doubleInnerTab}size = {{ width = 35 height = 26 }}");
            sb.AppendLine($"{doubleInnerTab}clipping = no");
            sb.AppendLine();
            sb.AppendLine($"{doubleInnerTab}background = {{");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}name = \"Background\"");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}spriteType =\"GFX_subtechnology_unavailable_item_bg\"");
            sb.AppendLine($"{doubleInnerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{doubleInnerTab}iconType = {{");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}name = \"picture\"");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}position = {{ x=2 y=2 }}");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}spriteType = \"GFX_subtech_td\"");
            sb.AppendLine($"{new string('\t', baseTabCount + 3)}alwaystransparent = yes");
            sb.AppendLine($"{doubleInnerTab}}}");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine();
            sb.AppendLine($"{innerTab}iconType = {{");
            sb.AppendLine($"{doubleInnerTab}name = \"can_assign_design_team_icon\"");
            sb.AppendLine($"{doubleInnerTab}position = {{ x=5 y=55 }}");
            sb.AppendLine($"{doubleInnerTab}spriteType = \"GFX_design_team_icon\"");
            sb.AppendLine($"{innerTab}}}");
            sb.AppendLine($"{tab}}}");

            return sb.ToString().TrimEnd();
        }

        private string GetOrCopyGuiFile(string modFolderPath, string gamePath)
        {
            string modFilePath = System.IO.Path.Combine(modFolderPath, "interface", "countrytechtreeview.gui");
            string gameFilePath = System.IO.Path.Combine(gamePath, "interface", "countrytechtreeview.gui");

            if (File.Exists(modFilePath)) return modFilePath;

            if (File.Exists(gameFilePath))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(modFilePath));
                File.Copy(gameFilePath, modFilePath);
                return modFilePath;
            }

            Console.WriteLine("GUI file not found in both mod and game directories");
            return null;
        }

        public void InsertGUIFolderTabButton(string modFolderPath, TechTreeConfig techTree, string gamePath)
        {
            string filePath = GetOrCopyGuiFile(modFolderPath, gamePath);
            if (filePath == null) return;

            string[] lines = File.ReadAllLines(filePath);

            // 1. Находим контейнер folder_tabs
            int tabsStart = -1, tabsEnd = -1;
            int braceLevel = 0;
            bool inTabsContainer = false;
            int lastButtonX = 22; // Значение по умолчанию

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (!inTabsContainer && line.Contains("containerWindowType = {") &&
                    i + 1 < lines.Length && lines[i + 1].Trim().Contains("name = \"folder_tabs\""))
                {
                    tabsStart = i;
                    inTabsContainer = true;
                    braceLevel = 1;
                    i++; 
                    continue;
                }

                if (inTabsContainer)
                {
                    if (line.Trim().StartsWith("buttonType = {"))
                    {
                        for (int j = i + 1; j < lines.Length; j++)
                        {
                            string nextLine = lines[j];
                            if (nextLine.Trim().StartsWith("}")) break; 

                            if (nextLine.Contains("position = { x ="))
                            {
                                var xMatch = Regex.Match(nextLine, @"position\s*=\s*\{\s*x\s*=\s*(\d+)");
                                if (xMatch.Success)
                                {
                                    lastButtonX = int.Parse(xMatch.Groups[1].Value);
                                }
                                break;
                            }
                        }
                    }

                    braceLevel += line.Count(c => c == '{');
                    braceLevel -= line.Count(c => c == '}');

                    if (braceLevel == 0)
                    {
                        tabsEnd = i;
                        break;
                    }
                }
            }

            if (tabsStart == -1 || tabsEnd == -1)
            {
                Console.WriteLine("Could not find folder_tabs container");
                return;
            }

            int tabCount = lines[tabsStart].TakeWhile(c => c == '\t').Count();
            string tabStr = new string('\t', tabCount);
            string innerTabStr = new string('\t', tabCount + 1);

            string buttonContent = $@"{tabStr}buttonType = {{
            {innerTabStr}name = ""{techTree.Name}_tab""
            {innerTabStr}position = {{ x = {lastButtonX + 89} y = 0 }}
            {innerTabStr}quadTextureSprite = ""GFX_{techTree.Name}_tab""
            {innerTabStr}frame = 1
            {innerTabStr}clicksound = ui_research_tab_infantry
            {tabStr}}}";
            List<string> result = new List<string>();
            result.AddRange(lines.Take(tabsEnd));

            if (tabsEnd > 0 && !string.IsNullOrWhiteSpace(lines[tabsEnd - 1]))
            {
                result.Add("");
            }

            result.Add(buttonContent);
            result.AddRange(lines.Skip(tabsEnd));

            File.WriteAllLines(filePath, result);
            Console.WriteLine($"Added {techTree.Name}_tab button at x={lastButtonX + 89}");
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

                        var resized = original.ResizeToBitmap(targetWidth, targetHeight);

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

        

        private void TabFolderImageCanvas_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string imagePath = files[0];

                    try
                    {
                        BitmapImage original = new BitmapImage(new Uri(imagePath));

                        var resizedFirst = original.ResizeToBitmap(25, 25);
                        TabFolderFirstImage.Source = resizedFirst;

                        var resizedSecond = original.ResizeToBitmap(25, 25);
                        TabFolderSecondImage.Source = resizedSecond;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                    }
                }
            }
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
                if (current.Name.Contains("Folder"))
                {
                    TabFolderFirstImage.Source = null;
                    TabFolderSecondImage.Source = null;
                }
            }
        }

        public static void SaveFolderTabIcon(TechTreeCreator window)
        {
            
          
            string techIconDir = System.IO.Path.Combine(ModManager.Directory, "gfx", "interface", "techtree");
            Directory.CreateDirectory(techIconDir);
            var firstCopy = window.TabFolderFirstImage.Source.Clone();
            var secondCopy = window.TabFolderFirstImage.Source.Clone();
            var bgCopy = window.TabFolderBackgroundImage.Source.Clone();
            var shadowingCopy = window.TabFolderShadowingImage.Source.Clone();

            List<ImageSourceArg> args = new List<ImageSourceArg>
            {
                new ImageSourceArg { Source = bgCopy, IsCompresed = true },
                new ImageSourceArg { Source = firstCopy, OffsetX = 45, OffsetY = 8 + 20 },
                new ImageSourceArg { Source = secondCopy, OffsetX = 124 + 12, OffsetY = 12 + 12  },
                new ImageSourceArg { Source = shadowingCopy, IsCompresed = true },
            };
            var img = ImageManager.GetCombinedImages(args, 182, 61);

           
            using (var bmp = img.ToBitmap())
            {
                bmp.SaveAsDDS(techIconDir, $"techtree_{window.CurrentTechTree.Name}_tab", 182, 61);
            }
        }
        

        public static void GenerateGfxFile(TechTreeConfig techTree)
        {
            string interfaceDir = System.IO.Path.Combine(ModManager.Directory, "interface");
            Directory.CreateDirectory(interfaceDir);

            string gfxFilePath = System.IO.Path.Combine(interfaceDir, $"{techTree.Name}.gfx");

            var sb = new StringBuilder();
            sb.AppendLine("spriteTypes = {");

            foreach (var item in techTree.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Id))
                    continue;

                sb.AppendLine("    spriteType = {");
                sb.AppendLine($"        name = \"GFX_{item.Id}_medium\"");
                sb.AppendLine($"        texturefile = \"gfx/interface/technologies/{item.Id}.dds\"");
                sb.AppendLine("    }");
            }
            sb.Append(
                $@"spriteType = {{
                    name = ""GFX_technology_{techTree.Name}_small_unavailable_item_bg""
                    textureFile = ""gfx//interface//techtree//tech_doctrine_unavailable_item_bg.dds""
                }}    

                spriteType = {{
                    name = ""GFX_technology_{techTree.Name}_small_available_item_bg""
                    textureFile = ""gfx//interface//techtree//tech_doctrine_available_item_bg.dds""
                }}

                spriteType = {{
                    name = ""GFX_technology_{techTree.Name}_small_researched_item_bg""
                    textureFile = ""gfx//interface//techtree//tech_landdoctrine_researched_item_bg.dds""
                }}    

                spriteType = {{
                    name = ""GFX_technology_{techTree.Name}_small_branch_item_bg""
                    textureFile = ""gfx/interface/techtree/tech_doctrine_branch_item_bg.dds""
                }}");
            sb.Append(
                $@"spriteType = {{
                    name = ""GFX_{techTree.Name}_tab""
                    textureFile = ""gfx/interface/techtree/techtree_{techTree.Name}_tab.dds""
                    noOfFrames = 2	
                }}");
            sb.Append(
                $@"
                spriteType = {{
		            name = ""GFX_{techTree.Name}_techtree_bg""
		            textureFile = ""gfx//interface//techtree//techtree_{techTree.Name}_bg.dds""
	            }}
                "
                );
            sb.AppendLine("}");
            
            var utf8WithoutBom = new UTF8Encoding(false);
            File.WriteAllText(gfxFilePath, sb.ToString(), utf8WithoutBom);
        }

        private void DoneTreeButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentTechTree.Name = TechTreeNameBox.Text;
            string value = (OrientationBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            CurrentTechTree.Ledger = TreeLedgerBox.Text;
        }

        public void CreateTAGTreeFolderName(string modFolderPath, TechTreeConfig techTree, string backupSourcePath = null)
        {
            string tagsFolderPath = System.IO.Path.Combine(modFolderPath, "common", "technology_tags");
            string tagsFilePath = null;

            if (Directory.Exists(tagsFolderPath))
            {
                var files = Directory.GetFiles(tagsFolderPath, "*.txt");
                tagsFilePath = files.FirstOrDefault();
            }

            if (tagsFilePath == null && !string.IsNullOrEmpty(backupSourcePath))
            {
                string backupTagsFolder = System.IO.Path.Combine(backupSourcePath, "common", "technology_tags");

                if (Directory.Exists(backupTagsFolder))
                {
                    var backupFiles = Directory.GetFiles(backupTagsFolder, "*.txt");
                    if (backupFiles.Length > 0)
                    {
                        Directory.CreateDirectory(tagsFolderPath);

                        string backupFile = backupFiles[0];
                        string newFileName = System.IO.Path.GetFileName(backupFile);
                        tagsFilePath = System.IO.Path.Combine(tagsFolderPath, newFileName);

                        File.Copy(backupFile, tagsFilePath);
                        Console.WriteLine($"Created new technology_tags file from backup: {tagsFilePath}");
                    }
                }
            }
            if (tagsFilePath == null)
            {
                Directory.CreateDirectory(tagsFolderPath);
                tagsFilePath = System.IO.Path.Combine(tagsFolderPath, "technology_tags.txt");

                File.WriteAllText(tagsFilePath,
                    "technology_folders = {\n" +
                    "\t# Add your technology folders here\n" +
                    "}");

                Console.WriteLine($"Created new technology_tags file: {tagsFilePath}");
            }

            string[] lines = File.ReadAllLines(tagsFilePath);
            List<string> newLines = new List<string>();
            bool foundFoldersBlock = false;
            bool insertedEntry = false;
            int braceLevel = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                newLines.Add(line);

                if (!foundFoldersBlock && line.Trim().StartsWith("technology_folders = {"))
                {
                    foundFoldersBlock = true;
                    braceLevel = 1;
                    continue;
                }

                if (foundFoldersBlock && !insertedEntry)
                {
                    braceLevel += line.Count(c => c == '{');
                    braceLevel -= line.Count(c => c == '}');

                    if ((braceLevel > 0 && string.IsNullOrWhiteSpace(line.Trim())) ||
                        (braceLevel <= 0 && !insertedEntry))
                    {
                        string entry = $"\t{techTree.Name} = {{\n" +
                                     $"\t\tledger = {techTree.Ledger}\n" +
                                     $"\t}}";

                        newLines.Insert(newLines.Count - 1, entry);
                        if (braceLevel <= 0) newLines.Insert(newLines.Count - 2, ""); 
                        insertedEntry = true;
                    }
                }
            }
            if (!foundFoldersBlock)
            {
                newLines.Add("");
                newLines.Add("technology_folders = {");
                newLines.Add($"\t{techTree.Name} = {{");
                newLines.Add($"\t\tledger = {techTree.Ledger}");
                newLines.Add("\t}");
                newLines.Add("}");
            }

            File.WriteAllLines(tagsFilePath, newLines);
            Console.WriteLine($"Successfully added {techTree.Name} to {System.IO.Path.GetFileName(tagsFilePath)}");
        }
        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            ExportTechTreeToFile();
            CurrentTechTree.SaveAllTechIconsAsDDS();
            GenerateGfxFile(CurrentTechTree);
            InsertGUITechTreeEntries(ModManager.Directory, CurrentTechTree, ModManager.GameDirectory);
            InsertGUIFolderTabButton(ModManager.Directory, CurrentTechTree, ModManager.GameDirectory);
            InsertGUITechTreeContainers(ModManager.Directory, CurrentTechTree, ModManager.GameDirectory);
            CreateTAGTreeFolderName(ModManager.Directory, CurrentTechTree, ModManager.GameDirectory);
            SaveFolderTabIcon(this);
            SaveFolderBG();
        }

        private void SaveFolderBG()
        {
            string techIconDir = System.IO.Path.Combine(ModManager.Directory, "gfx", "interface", "techtree");
            Directory.CreateDirectory(techIconDir);
            using (var bmp = TechBGImage.ImageSource.ToBitmap())
            {
                bmp.SaveAsDDS(techIconDir, $"techtree_{CurrentTechTree.Name}_bg", bmp.Width, bmp.Height);
            }
        }

        private void ExportTechTreeToFile()
        {
            string fileName = $"{CurrentTechTree.Name}.txt";
            string fullPath = System.IO.Path.Combine(ModManager.Directory, "common", "technologies", fileName);

            var sb = new StringBuilder();
            sb.AppendLine("technologies = {");

            foreach (var item in CurrentTechTree.Items)
            {
                sb.AppendLine($"\t{item.Id} = {{");

                foreach (var modif in item.Modifiers)
                {
                    var splited = modif.Split('=');
                    if (!string.IsNullOrWhiteSpace(modif) && splited.Length == 2)
                        sb.AppendLine($"\t\t{splited[0]} = {splited[1]}");
                }

                var children = CurrentTechTree.ChildOf
                    .Where(pair => pair.Count == 2 && pair[0] == item.Id)
                    .Select(pair => pair[1]);

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
                sb.AppendLine($"\t\t\tname = {CurrentTechTree.Name}");
                sb.AppendLine($"\t\t\tposition = {{ x = {item.GridY} y = {item.GridX} }}");
                sb.AppendLine($"\t\t}}");

                var usedEnableTypes = new HashSet<string>();

                foreach (var unlock in item.Enables.Where(e => !string.IsNullOrWhiteSpace(e) && e.Contains(":")))
                {
                    var typeSplit = unlock.Split(new[] { ':' }, 2);
                    if (typeSplit.Length != 2 || string.IsNullOrWhiteSpace(typeSplit[0]))
                        continue;

                    var enableType = typeSplit[0].Trim();
                    var parameters = typeSplit[1].Trim();

                    if (usedEnableTypes.Contains(enableType))
                        continue;

                    if (string.IsNullOrEmpty(parameters))
                        continue;

                    var values = parameters.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                    if (values.Count == 0)
                        continue;

                    sb.AppendLine($"\t\t{enableType} = {{");

                    foreach (var val in values)
                    {
                        if (val.Contains(":"))
                        {
                            var kv = val.Split(':');
                            if (kv.Length == 2 && !string.IsNullOrWhiteSpace(kv[1]))
                            {
                                sb.AppendLine($"\t\t\t{kv[0]} = {kv[1]}");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"\t\t\t{val}");
                        }
                    }

                    sb.AppendLine("\t\t}");

                    usedEnableTypes.Add(enableType);
                }




                if (!string.IsNullOrWhiteSpace(item.Categories))
                {
                    var lines = item.Categories.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0)
                    {
                        string categoryName = lines[0].Trim();
                        var sbCat = new StringBuilder();
                        sbCat.AppendLine($"\t\t{categoryName} = {{");

                        Dictionary<string, List<string>> terrarians = new();
                        string currentTerrarian = null;

                        for (int i = 1; i < lines.Length; i++)
                        {
                            var line = lines[i].Trim();
                            if (string.IsNullOrEmpty(line)) continue;

                            if (line.StartsWith("-"))
                            {
                                currentTerrarian = line.Substring(1).Trim();
                                if (!terrarians.ContainsKey(currentTerrarian))
                                    terrarians[currentTerrarian] = new List<string>();
                            }
                            else if (line.Contains(":"))
                            {
                                var parts = line.Split(':');
                                if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
                                {
                                    string formatted = $"{parts[0]} = {parts[1]}";
                                    if (currentTerrarian == null)
                                        sbCat.AppendLine($"\t\t\t{formatted}");
                                    else
                                        terrarians[currentTerrarian].Add($"\t\t\t\t{formatted}");
                                }
                            }
                        }

                        foreach (var kvp in terrarians)
                        {
                            sbCat.AppendLine($"\t\t\t{kvp.Key} = {{");
                            foreach (var mod in kvp.Value)
                                sbCat.AppendLine(mod);
                            sbCat.AppendLine("\t\t\t}");
                        }

                        sbCat.AppendLine("\t\t}");
                        sb.AppendLine(sbCat.ToString());
                    }
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
            var str = GetRichTextBoxText(rtb);
            var stroke = GetRichTextBoxText(rtb).Split(new[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return stroke;
        }


        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (marked.Count != 1)
            {
                System.Windows.MessageBox.Show("Выделите ровно один элемент для обновления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Border selected = marked.First() as Border;
            if (selected == null)
            {
                System.Windows.MessageBox.Show("Выделенный элемент некорректен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var item = CurrentTechTree.Items.FirstOrDefault(x => x.Id == selected.Name);
            if (item == null)
            {
                System.Windows.MessageBox.Show("Не удалось найти элемент в конфигурации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CurrentTechTree.Items.FirstOrDefault(x => x.Id == selected.Name).OldId = item.Id;

            item.LocName = TechNameBox.Text;
            item.LocDescription = TechDescBox.Text;
            item.ModifCost = int.TryParse(TechCostModifBox.Text, out int modifCost) ? modifCost : 0;
            item.Cost = Convert.ToInt32(TechCostBox.Text);
            item.StartYear = Convert.ToInt32(StartYeatBox.Text);
            item.Enables = GetRichTextBoxLines(EnablesBox);
            item.Allowed = GetRichTextBoxLines(AllowedBox);
            item.Effects = GetRichTextBoxLines(EffectBox);
            item.Modifiers = GetRichTextBoxLines(ModdifierBox);
            item.AiWillDo = GetRichTextBoxText(AiWillDoBox);
            item.Dependencies = GetRichTextBoxLines(DependenciesBox);
            item.Image = item.IsBig ? BigOverlayImage.Source : SmallOverlayImage.Source;
            var overlayimg = item.IsBig ? BigOverlayImage : SmallOverlayImage;
            var backgroundimg = item.IsBig ? BigBackgroundImage : BigOverlayImage;
            if (selected is Border border && border.Child is Canvas canavas)
            {
                foreach (var elem in canavas.Children)
                {
                    if (elem is System.Windows.Controls.Image img)
                    {
                        img.Source = overlayimg.Source.GetCombinedTechImage(backgroundimg.Source, item.IsBig ? 1 : 2);
                    }
                }
            }

            selected.Name = item.Id;

            var index = CurrentTechTree.Items.FindIndex(x => x.OldId == item.OldId);
            if (index >= 0)
            {
                CurrentTechTree.Items[index] = item;
            }

            System.Windows.MessageBox.Show("Элемент успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void LoadEvent(object sender, RoutedEventArgs e)
        {
            await WpfConfigManager.LoadConfigAsync(this);
        }

        private async void SaveEvent(object sender, RoutedEventArgs e)
        {
            await WpfConfigManager.SaveConfigAsync(this, TechTreeNameBox.Text);
        }

        private void CategoriesBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRange = new TextRange(
               EnablesBox.Document.ContentStart,
               EnablesBox.Document.ContentEnd
           );
            string fullText = textRange.Text;

            var formattedText = new FormattedText(
                fullText,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(EnablesBox.FontFamily, EnablesBox.FontStyle, EnablesBox.FontWeight, EnablesBox.FontStretch),
                EnablesBox.FontSize,
                System.Windows.Media.Brushes.Black,
                VisualTreeHelper.GetDpi(EnablesBox).PixelsPerDip
            );
            EnablesBox.Document.PageWidth = formattedText.Width + 20;
        }


        private void WrapperGridCanvas_Drop(object sender, System.Windows.DragEventArgs e)
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

    }
}
