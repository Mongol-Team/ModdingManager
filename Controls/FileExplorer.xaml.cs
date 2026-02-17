using Application;
using Application.Extentions;
using Application.utils;
using Controls.Docking;
using Models.Attributes;
using Models.Configs;
using Models.EntityFiles;
using Models.Interfaces;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Clipboard = System.Windows.Clipboard;
using FontFamily = System.Windows.Media.FontFamily;
using Orientation = System.Windows.Controls.Orientation;
using TreeViewItem = System.Windows.Controls.TreeViewItem;

namespace Controls
{
    public class ModCategoryNode
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public IList Items { get; set; }
        public Type ItemType { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }

    public class ConfigFileNode
    {
        public object File { get; set; }
        public string DisplayName { get; set; }
        public Type ConfigType { get; set; }
        public int EntityCount { get; set; }
        public IList Entities { get; set; }
        public PropertyInfo ParentProperty { get; set; }
    }

    public class ModItemNode
    {
        public object Item { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public object ParentFile { get; set; }
    }

    public partial class FileExplorer : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(FileExplorer),
                new PropertyMetadata("Mod Explorer", OnTitleChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(FileExplorer),
                new PropertyMetadata(null));

        public static readonly RoutedEvent ItemSelectedEvent =
            EventManager.RegisterRoutedEvent(nameof(ItemSelected), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(FileExplorer));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            private set => SetValue(SelectedItemProperty, value);
        }

        public event RoutedEventHandler ItemSelected
        {
            add => AddHandler(ItemSelectedEvent, value);
            remove => RemoveHandler(ItemSelectedEvent, value);
        }

        private TreeViewItem _selectedItem;
        private ContextMenu _fileContextMenu;
        private ContextMenu _categoryContextMenu;
        private Point _dragStartPoint;
        private bool _isDragging = false;
        private Dictionary<string, bool> _expansionState = new Dictionary<string, bool>();
        private string _currentSearchText = "";
        // Добавить эти поля в класс FileExplorer:
        private TreeViewItem _renamingItem;
        private TextBox _renameTextBox;
        public FileExplorer()
        {
            InitializeComponent();
            InitializeContextMenus();
            LoadContextMenuLocalization();
            SetupSearchPlaceholder();
            UpdateTitle();
            LoadModData();
        }

        private void InitializeContextMenus()
        {
            // Контекстное меню для файлов и элементов
            _fileContextMenu = new ContextMenu();

            var openMenuItem = new MenuItem { Name = "OpenMenuItem", Header = "Open" };
            openMenuItem.Click += OpenMenuItem_Click;
            _fileContextMenu.Items.Add(openMenuItem);

            var openInExplorerMenuItem = new MenuItem { Name = "OpenInExplorerMenuItem", Header = "Open in Explorer" };
            openInExplorerMenuItem.Click += OpenInExplorerMenuItem_Click;
            _fileContextMenu.Items.Add(openInExplorerMenuItem);

            _fileContextMenu.Items.Add(new Separator());

            var copyPathMenuItem = new MenuItem { Name = "CopyPathMenuItem", Header = "Copy Path" };
            copyPathMenuItem.Click += CopyPathMenuItem_Click;
            _fileContextMenu.Items.Add(copyPathMenuItem);

            var renameMenuItem = new MenuItem { Name = "RenameMenuItem", Header = "Rename" };
            renameMenuItem.Click += RenameMenuItem_Click;
            _fileContextMenu.Items.Add(renameMenuItem);

            var deleteMenuItem = new MenuItem { Name = "DeleteMenuItem", Header = "Delete" };
            deleteMenuItem.Click += DeleteMenuItem_Click;
            _fileContextMenu.Items.Add(deleteMenuItem);

            // Контекстное меню для категорий
            _categoryContextMenu = new ContextMenu();

            var addFileMenuItem = new MenuItem { Name = "AddFileMenuItem", Header = "Add File" };
            addFileMenuItem.Click += AddFileMenuItem_Click;
            _categoryContextMenu.Items.Add(addFileMenuItem);
        }

        private void UpdateTitle()
        {
            // Title update logic if needed
        }

        private void SetupSearchPlaceholder()
        {
            var placeholderText = "Поиск объектов мода...";
            var placeholderColor = System.Windows.Media.Color.FromRgb(133, 133, 133);
            var normalColor = System.Windows.Media.Color.FromRgb(204, 204, 204);

            SearchTextBox.Text = placeholderText;
            SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush(placeholderColor);

            SearchTextBox.GotFocus += (s, e) =>
            {
                if (SearchTextBox.Text == placeholderText)
                {
                    SearchTextBox.Text = "";
                    SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush(normalColor);
                }
            };

            SearchTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    SearchTextBox.Text = placeholderText;
                    SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush(placeholderColor);
                }
            };
        }

        private void LoadContextMenuLocalization()
        {
            if (_fileContextMenu?.Items.Count >= 5)
            {
                if (_fileContextMenu.Items[0] is MenuItem openMenuItem)
                    openMenuItem.Header = StaticLocalisation.GetString("Menu.Open");
                if (_fileContextMenu.Items[1] is MenuItem openInExplorerMenuItem)
                    openInExplorerMenuItem.Header = StaticLocalisation.GetString("Menu.OpenInExplorer");
                if (_fileContextMenu.Items[3] is MenuItem copyPathMenuItem)
                    copyPathMenuItem.Header = StaticLocalisation.GetString("Menu.CopyFullPath");
                if (_fileContextMenu.Items[4] is MenuItem renameMenuItem)
                    renameMenuItem.Header = StaticLocalisation.GetString("Menu.Rename");
                if (_fileContextMenu.Items[5] is MenuItem deleteMenuItem)
                    deleteMenuItem.Header = StaticLocalisation.GetString("Menu.Delete");
            }

            if (_categoryContextMenu?.Items.Count >= 1)
            {
                if (_categoryContextMenu.Items[0] is MenuItem addFileMenuItem)
                    addFileMenuItem.Header = StaticLocalisation.GetString("Menu.AddFile") ?? "Add File";
            }
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileExplorer explorer)
            {
                explorer.UpdateTitle();
            }
        }

        #region Expansion State Management

        private void SaveExpansionState()
        {
            _expansionState.Clear();
            SaveExpansionStateRecursive(FileTreeView.Items);
        }

        private void SaveExpansionStateRecursive(ItemCollection items)
        {
            foreach (TreeViewItem item in items)
            {
                var key = GetNodeKey(item);
                if (!string.IsNullOrEmpty(key))
                {
                    _expansionState[key] = item.IsExpanded;
                }

                if (item.Items.Count > 0)
                {
                    SaveExpansionStateRecursive(item.Items);
                }
            }
        }

        private void RestoreExpansionState()
        {
            RestoreExpansionStateRecursive(FileTreeView.Items);
        }

        private void RestoreExpansionStateRecursive(ItemCollection items)
        {
            foreach (TreeViewItem item in items)
            {
                var key = GetNodeKey(item);
                if (!string.IsNullOrEmpty(key) && _expansionState.ContainsKey(key))
                {
                    item.IsExpanded = _expansionState[key];
                }

                if (item.Items.Count > 0)
                {
                    RestoreExpansionStateRecursive(item.Items);
                }
            }
        }

        private string GetNodeKey(TreeViewItem item)
        {
            if (item.Tag is ModCategoryNode category)
            {
                return $"Category_{category.Name}";
            }
            else if (item.Tag is ConfigFileNode fileNode)
            {
                return $"File_{fileNode.DisplayName}_{fileNode.ConfigType?.Name}";
            }
            else if (item.Tag is ModItemNode modItem)
            {
                return $"Item_{modItem.Id}";
            }
            return null;
        }

        #endregion

        public void LoadModData()
        {
            SaveExpansionState();
            FileTreeView.Items.Clear();

            if (ModDataStorage.Mod == null)
            {
                return;
            }

            var modConfig = ModDataStorage.Mod;
            var categories = GetModCategories(modConfig);

            foreach (var category in categories)
            {
                if (category.Items != null && category.Items.Count > 0)
                {
                    var categoryItem = CreateCategoryTreeViewItem(category);
                    FileTreeView.Items.Add(categoryItem);
                }
            }

            RestoreExpansionState();
        }

        private List<ModCategoryNode> GetModCategories(ModConfig modConfig)
        {
            var categories = new List<ModCategoryNode>();
            var modType = typeof(ModConfig);
            var properties = modType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (prop.PropertyType.IsGenericType)
                {
                    var genericDef = prop.PropertyType.GetGenericTypeDefinition();
                    if (genericDef == typeof(ObservableCollection<>))
                    {
                        var itemType = prop.PropertyType.GetGenericArguments()[0];

                        if (itemType.IsGenericType &&
                            (itemType.GetGenericTypeDefinition() == typeof(ConfigFile<>) ||
                             itemType.GetGenericTypeDefinition() == typeof(GfxFile<>)))
                        {
                            if (prop.GetValue(modConfig) is IList value && value.Count > 0)
                            {
                                categories.Add(new ModCategoryNode
                                {
                                    Name = prop.Name,
                                    DisplayName = FormatCategoryName(prop.Name),
                                    Items = value,
                                    ItemType = itemType,
                                    PropertyInfo = prop
                                });
                            }
                        }
                    }
                }
                else if (prop.PropertyType == typeof(MapConfig))
                {
                    if (prop.GetValue(modConfig) is MapConfig map)
                    {
                        var mapCategory = new ModCategoryNode
                        {
                            Name = "Map",
                            DisplayName = "Map",
                            Items = new List<object> { map },
                            ItemType = typeof(MapConfig),
                            PropertyInfo = prop
                        };
                        categories.Add(mapCategory);
                    }
                }
            }

            return categories.OrderBy(c => c.DisplayName).ToList();
        }

        private string FormatCategoryName(string name)
        {
            var result = System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim();
            return result;
        }

        private TreeViewItem CreateCategoryTreeViewItem(ModCategoryNode category)
        {
            var item = new TreeViewItem
            {
                Header = CreateCategoryHeader(category),
                Tag = category,
                IsExpanded = false
            };

            item.Items.Add(new TreeViewItem { Header = "Loading...", IsEnabled = false });
            item.Expanded += CategoryItem_Expanded;
            item.MouseRightButtonDown += CategoryItem_MouseRightButtonDown;

            return item;
        }

        private TreeViewItem CreateFileTreeViewItem(ConfigFileNode fileNode)
        {
            var item = new TreeViewItem
            {
                Header = CreateFileHeader(fileNode),
                Tag = fileNode,
                IsExpanded = false,
                AllowDrop = true
            };

            item.Items.Add(new TreeViewItem { Header = "Loading...", IsEnabled = false });
            item.Expanded += FileItem_Expanded;
            item.MouseMove += TreeViewItem_MouseMove;
            item.PreviewMouseLeftButtonDown += TreeViewItem_PreviewMouseLeftButtonDown;
            item.MouseRightButtonDown += FileItem_MouseRightButtonDown;

            return item;
        }

        private TreeViewItem CreateModItemTreeViewItem(ModItemNode modItem)
        {
            var item = new TreeViewItem
            {
                Header = CreateItemHeader(modItem.DisplayName),
                Tag = modItem,
                IsExpanded = false,
                AllowDrop = true
            };

            item.MouseMove += TreeViewItem_MouseMove;
            item.PreviewMouseLeftButtonDown += TreeViewItem_PreviewMouseLeftButtonDown;
            item.MouseRightButtonDown += ModItem_MouseRightButtonDown;

            return item;
        }

        private StackPanel CreateCategoryHeader(ModCategoryNode category)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var icon = new TextBlock
            {
                Text = "\uE8B7",
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0),
                Foreground = (Brush)System.Windows.Application.Current.Resources["FolderLayer1"]
            };

            var textBlock = CreateHighlightedTextBlock(category.DisplayName, _currentSearchText);
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = (Brush)System.Windows.Application.Current.Resources["TextPrimary"];

            var addButton = new Button
            {
                Content = new TextBlock { Text = "+", FontSize = 12, FontWeight = FontWeights.Bold },
                Style = (Style)FindResource("FileControlButton"),
                Margin = new Thickness(8, 0, 0, 0),
                ToolTip = "Add new file"
            };
            addButton.Click += (s, e) =>
            {
                e.Handled = true;
                AddNewFileToCategory(category);
            };

            panel.Children.Add(icon);
            panel.Children.Add(textBlock);
            panel.Children.Add(addButton);

            return panel;
        }

        private StackPanel CreateFileHeader(ConfigFileNode fileNode)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var icon = new TextBlock
            {
                Text = "\uE8A5",
                Style = (Style)FindResource("FileIconStyle"),
                Foreground = (Brush)System.Windows.Application.Current.Resources["Link"]
            };

            var textBlock = CreateHighlightedTextBlock(fileNode.DisplayName, _currentSearchText);
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = (Brush)System.Windows.Application.Current.Resources["TextPrimary"];

            var typeText = new TextBlock
            {
                Text = $"<{GetShortTypeName(fileNode.ConfigType)}>",
                Style = (Style)FindResource("FileTypeStyle")
            };

            var countText = new TextBlock
            {
                Text = $"({fileNode.EntityCount})",
                Style = (Style)FindResource("ItemCountStyle")
            };

            // Кнопка добавления элемента в файл
            var addButton = new Button
            {
                Content = new TextBlock { Text = "+", FontSize = 12, FontWeight = FontWeights.Bold },
                Style = (Style)FindResource("FileControlButton"),
                Margin = new Thickness(8, 0, 0, 0),
                ToolTip = "Add new entity"
            };
            addButton.Click += (s, e) =>
            {
                e.Handled = true;
                AddNewEntityToFile(fileNode);
            };

            var deleteButton = new Button
            {
                Content = new TextBlock { Text = "−", FontSize = 12, FontWeight = FontWeights.Bold },
                Style = (Style)FindResource("FileControlButton"),
                Margin = new Thickness(4, 0, 0, 0),
                ToolTip = "Remove file"
            };
            deleteButton.Click += (s, e) =>
            {
                e.Handled = true;
                RemoveFile(fileNode);
            };

            panel.Children.Add(icon);
            panel.Children.Add(textBlock);
            panel.Children.Add(typeText);
            panel.Children.Add(countText);
            panel.Children.Add(addButton);
            panel.Children.Add(deleteButton);

            return panel;
        }

        private UIElement CreateItemHeader(string name)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var icon = new TextBlock
            {
                Text = "\uE7C3",
                Style = (Style)FindResource("FileIconStyle"),
                Foreground = (Brush)System.Windows.Application.Current.Resources["TextTertiary"]
            };

            var textBlock = CreateHighlightedTextBlock(name, _currentSearchText);
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Foreground = (Brush)System.Windows.Application.Current.Resources["TextPrimary"];

            panel.Children.Add(icon);
            panel.Children.Add(textBlock);

            return panel;
        }

        private TextBlock CreateHighlightedTextBlock(string text, string searchText)
        {
            var textBlock = new TextBlock();

            if (string.IsNullOrEmpty(searchText) || searchText == "Поиск объектов мода...")
            {
                textBlock.Text = text;
                return textBlock;
            }

            var index = text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);

            if (index == -1)
            {
                textBlock.Text = text;
                return textBlock;
            }

            // До совпадения
            if (index > 0)
            {
                textBlock.Inlines.Add(new Run(text.Substring(0, index)));
            }

            // Совпадение (подсвечиваем)
            var highlightedRun = new Run(text.Substring(index, searchText.Length))
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 144, 226)),
                Foreground = Brushes.White
            };
            textBlock.Inlines.Add(highlightedRun);

            // После совпадения
            if (index + searchText.Length < text.Length)
            {
                textBlock.Inlines.Add(new Run(text.Substring(index + searchText.Length)));
            }

            return textBlock;
        }

        private string GetShortTypeName(Type type)
        {
            if (type == null) return "Unknown";
            var name = type.Name;
            if (name.EndsWith("Config"))
                name = name.Substring(0, name.Length - 6);
            return name;
        }

        private void CategoryItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item && item.Tag is ModCategoryNode category)
            {
                if (item.Items.Count == 1 && item.Items[0] is TreeViewItem loadingItem && !loadingItem.IsEnabled)
                {
                    item.Items.Clear();
                    LoadCategoryContents(item, category);
                }
            }
        }

        private void FileItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item && item.Tag is ConfigFileNode fileNode)
            {
                if (item.Items.Count == 1 && item.Items[0] is TreeViewItem loadingItem && !loadingItem.IsEnabled)
                {
                    item.Items.Clear();
                    LoadFileContents(item, fileNode);
                }
            }
        }

        private void LoadCategoryContents(TreeViewItem parentItem, ModCategoryNode category)
        {
            try
            {
                if (category.Items == null)
                    return;

                foreach (var obj in category.Items)
                {
                    if (obj == null)
                        continue;

                    var fileType = obj.GetType();

                    Type configType = null;
                    IList entities = null;
                    string fileName = "Unknown";

                    if (fileType.IsGenericType)
                    {
                        var genericDef = fileType.GetGenericTypeDefinition();
                        if (genericDef == typeof(ConfigFile<>) || genericDef == typeof(GfxFile<>))
                        {
                            configType = fileType.GetGenericArguments()[0];

                            var entitiesProp = fileType.GetProperty("Entities");
                            if (entitiesProp != null)
                            {
                                entities = entitiesProp.GetValue(obj) as IList;
                            }

                            var fileNameProp = fileType.GetProperty("FileName");
                            if (fileNameProp != null)
                            {
                                fileName = fileNameProp.GetValue(obj) as string ?? "Unknown";
                            }
                        }
                    }

                    if (configType != null && entities != null)
                    {
                        // Проверяем, соответствует ли файл поиску
                        if (!ShouldShowItem(fileName))
                            continue;

                        var fileNode = new ConfigFileNode
                        {
                            File = obj,
                            DisplayName = fileName,
                            ConfigType = configType,
                            EntityCount = entities.Count,
                            Entities = entities,
                            ParentProperty = category.PropertyInfo
                        };

                        var itemNode = CreateFileTreeViewItem(fileNode);
                        parentItem.Items.Add(itemNode);
                    }
                }
            }
            catch (Exception)
            {
                // Error handling
            }
        }

        private void LoadFileContents(TreeViewItem parentItem, ConfigFileNode fileNode)
        {
            try
            {
                if (fileNode.Entities == null)
                    return;

                var items = new List<ModItemNode>();

                foreach (var obj in fileNode.Entities)
                {
                    if (obj == null)
                        continue;

                    string displayName = GetItemDisplayName(obj);
                    string id = GetItemId(obj);

                    if (ShouldShowItem(displayName) || ShouldShowItem(id))
                    {
                        items.Add(new ModItemNode
                        {
                            Item = obj,
                            DisplayName = displayName,
                            Id = id,
                            ParentFile = fileNode.File
                        });
                    }
                }

                foreach (var modItem in items.OrderBy(i => i.DisplayName))
                {
                    var itemNode = CreateModItemTreeViewItem(modItem);
                    parentItem.Items.Add(itemNode);
                }
            }
            catch (Exception)
            {
                // Error handling
            }
        }

        private string GetItemDisplayName(object item)
        {
            if (item is IConfig config && config.Id != null)
            {
                return config.Id.ToString();
            }
            else if (item is IGfx gfx && gfx.Id != null)
            {
                return gfx.Id.ToString();
            }

            return item.GetType().Name;
        }

        private string GetItemId(object item)
        {
            if (item is IConfig config && config.Id != null)
            {
                return config.Id.ToString();
            }
            else if (item is IGfx gfx && gfx.Id != null)
            {
                return gfx.Id.ToString();
            }

            return item.GetType().Name;
        }

        private bool ShouldShowItem(string name)
        {
            var searchText = SearchTextBox.Text;
            var placeholderText = "Поиск объектов мода...";

            if (string.IsNullOrEmpty(searchText) || searchText == placeholderText)
                return true;

            if (string.IsNullOrEmpty(name))
                return false;

            return name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

     

        private void ExpandAllMatchingItems(ItemCollection items, string searchText)
        {
            foreach (TreeViewItem item in items)
            {
                bool shouldExpand = false;

                if (item.Tag is ModCategoryNode category)
                {
                    if (category.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        shouldExpand = true;
                    }
                    else if (HasMatchingChildren(item, searchText))
                    {
                        shouldExpand = true;
                    }
                }
                else if (item.Tag is ConfigFileNode fileNode)
                {
                    if (fileNode.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        shouldExpand = true;
                        ExpandParents(item);
                    }
                    else if (HasMatchingChildren(item, searchText))
                    {
                        shouldExpand = true;
                        ExpandParents(item);
                    }
                }
                else if (item.Tag is ModItemNode modItem)
                {
                    if (modItem.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ExpandParents(item);
                    }
                }

                if (shouldExpand)
                {
                    item.IsExpanded = true;
                }

                if (item.Items.Count > 0)
                {
                    ExpandAllMatchingItems(item.Items, searchText);
                }
            }
        }

        private bool HasMatchingChildren(TreeViewItem item, string searchText)
        {
            // Загружаем содержимое если еще не загружено
            if (item.Items.Count == 1 && item.Items[0] is TreeViewItem loadingItem && !loadingItem.IsEnabled)
            {
                if (item.Tag is ModCategoryNode category)
                {
                    LoadCategoryContents(item, category);
                }
                else if (item.Tag is ConfigFileNode fileNode)
                {
                    LoadFileContents(item, fileNode);
                }
            }

            foreach (TreeViewItem child in item.Items)
            {
                if (child.Tag is ConfigFileNode fileNode)
                {
                    if (fileNode.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
                else if (child.Tag is ModItemNode modItem)
                {
                    if (modItem.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }

                if (HasMatchingChildren(child, searchText))
                    return true;
            }

            return false;
        }

        private void ExpandParents(TreeViewItem item)
        {
            var parent = item.Parent as TreeViewItem;
            while (parent != null)
            {
                parent.IsExpanded = true;
                parent = parent.Parent as TreeViewItem;
            }
        }

        #region Drag & Drop

        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point currentPosition = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPosition;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    TreeViewItem treeViewItem = sender as TreeViewItem;
                    if (treeViewItem != null)
                    {
                        _isDragging = true;
                        StartDragOperation(treeViewItem);
                    }
                }
            }
        }

        private void StartDragOperation(TreeViewItem item)
        {
            var tag = item.Tag;

            if (tag is ConfigFileNode fileNode)
            {
                var data = new DataObject();
                data.SetData("ConfigFile", fileNode.File);
                data.SetData("ConfigFileType", fileNode.ConfigType);
                DragDrop.DoDragDrop(item, data, DragDropEffects.Move | DragDropEffects.Copy);
            }
            else if (tag is ModItemNode modItem)
            {
                var data = new DataObject();
                data.SetData("ConfigItem", modItem.Item);
                data.SetData("ParentFile", modItem.ParentFile);
                DragDrop.DoDragDrop(item, data, DragDropEffects.Move | DragDropEffects.Copy);
            }

            _isDragging = false;
        }

        private void FileTreeView_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            var targetItem = GetTreeViewItemFromPoint(e.GetPosition(FileTreeView));
            if (targetItem == null)
                return;

            var targetTag = targetItem.Tag;

            if (e.Data.GetDataPresent("ConfigFile") && targetTag is ModCategoryNode category)
            {
                var sourceConfigType = e.Data.GetData("ConfigFileType") as Type;
                var targetConfigType = GetConfigTypeFromCategory(category);

                if (sourceConfigType != null && targetConfigType != null && sourceConfigType == targetConfigType)
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
            else if (e.Data.GetDataPresent("ConfigItem") && targetTag is ConfigFileNode fileNode)
            {
                var sourceItem = e.Data.GetData("ConfigItem");
                if (sourceItem != null && fileNode.ConfigType == sourceItem.GetType())
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
        }

        private void FileTreeView_Drop(object sender, DragEventArgs e)
        {
            var targetItem = GetTreeViewItemFromPoint(e.GetPosition(FileTreeView));
            if (targetItem == null)
                return;

            var targetTag = targetItem.Tag;

            if (e.Data.GetDataPresent("ConfigFile") && targetTag is ModCategoryNode category)
            {
                var sourceFile = e.Data.GetData("ConfigFile");
                MoveFileBetweenCategories(sourceFile, category);
            }
            else if (e.Data.GetDataPresent("ConfigItem") && targetTag is ConfigFileNode fileNode)
            {
                var sourceItem = e.Data.GetData("ConfigItem");
                var sourceParentFile = e.Data.GetData("ParentFile");
                MoveItemBetweenFiles(sourceItem, sourceParentFile, fileNode.File);
            }
        }

        private TreeViewItem GetTreeViewItemFromPoint(Point point)
        {
            var hitTestResult = VisualTreeHelper.HitTest(FileTreeView, point);
            if (hitTestResult == null)
                return null;

            var element = hitTestResult.VisualHit;
            while (element != null && !(element is TreeViewItem))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            return element as TreeViewItem;
        }

        private Type GetConfigTypeFromCategory(ModCategoryNode category)
        {
            if (category.ItemType == null || !category.ItemType.IsGenericType)
                return null;

            var genericDef = category.ItemType.GetGenericTypeDefinition();
            if (genericDef == typeof(ConfigFile<>) || genericDef == typeof(GfxFile<>))
            {
                return category.ItemType.GetGenericArguments()[0];
            }

            return null;
        }

        private void MoveFileBetweenCategories(object sourceFile, ModCategoryNode targetCategory)
        {
            try
            {
                var modConfig = ModDataStorage.Mod;
                var properties = modConfig.GetType().GetProperties();

                foreach (var prop in properties)
                {
                    if (prop.GetValue(modConfig) is IList list && list.Contains(sourceFile))
                    {
                        list.Remove(sourceFile);
                        break;
                    }
                }

                if (targetCategory.Items != null)
                {
                    targetCategory.Items.Add(sourceFile);
                    LoadModData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error moving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MoveItemBetweenFiles(object sourceItem, object sourceFile, object targetFile)
        {
            try
            {
                var sourceFileType = sourceFile.GetType();
                var targetFileType = targetFile.GetType();

                var sourceEntitiesProp = sourceFileType.GetProperty("Entities");
                var targetEntitiesProp = targetFileType.GetProperty("Entities");

                if (sourceEntitiesProp != null && targetEntitiesProp != null)
                {
                    var sourceEntities = sourceEntitiesProp.GetValue(sourceFile) as IList;
                    var targetEntities = targetEntitiesProp.GetValue(targetFile) as IList;

                    if (sourceEntities != null && targetEntities != null && sourceEntities.Contains(sourceItem))
                    {
                        sourceEntities.Remove(sourceItem);
                        targetEntities.Add(sourceItem);
                        LoadModData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error moving item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region File Management

        private void AddNewFileToCategory(ModCategoryNode category)
        {
            try
            {
                if (category.ItemType == null || !category.ItemType.IsGenericType)
                    return;

                var fileType = category.ItemType;
                var newFile = Activator.CreateInstance(fileType);

                if (newFile != null && category.Items != null)
                {
                    category.Items.Add(newFile);
                    LoadModData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding new file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewEntityToFile(ConfigFileNode fileNode)
        {
            try
            {
                if (fileNode.ConfigType == null || fileNode.Entities == null)
                    return;

                var newEntity = Activator.CreateInstance(fileNode.ConfigType);

                if (newEntity != null)
                {
                    fileNode.Entities.Add(newEntity);
                    LoadModData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding new entity: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveFile(ConfigFileNode fileNode)
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove '{fileNode.DisplayName}'?",
                    "Confirm Removal",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var modConfig = ModDataStorage.Mod;
                    var properties = modConfig.GetType().GetProperties();

                    foreach (var prop in properties)
                    {
                        if (prop.GetValue(modConfig) is IList list && list.Contains(fileNode.File))
                        {
                            list.Remove(fileNode.File);
                            LoadModData();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelectedItem()
        {
            if (_selectedItem == null)
                return;

            try
            {
                if (_selectedItem.Tag is ModItemNode modItem)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete '{modItem.DisplayName}'?",
                        "Confirm Deletion",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes && modItem.ParentFile != null)
                    {
                        var fileType = modItem.ParentFile.GetType();
                        var entitiesProp = fileType.GetProperty("Entities");

                        if (entitiesProp != null)
                        {
                            var entities = entitiesProp.GetValue(modItem.ParentFile) as IList;
                            if (entities != null && entities.Contains(modItem.Item))
                            {
                                entities.Remove(modItem.Item);
                                LoadModData();
                            }
                        }
                    }
                }
                else if (_selectedItem.Tag is ConfigFileNode fileNode)
                {
                    RemoveFile(fileNode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Event Handlers

        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item)
            {
                _selectedItem = item;

                if (item.Tag is ModItemNode modItem)
                {
                    SelectedItem = modItem.Item;
                    RaiseEvent(new RoutedEventArgs(ItemSelectedEvent));
                }
                else if (item.Tag is ConfigFileNode fileNode)
                {
                    SelectedItem = fileNode.File;
                    RaiseEvent(new RoutedEventArgs(ItemSelectedEvent));
                }
                else if (item.Tag is ModCategoryNode category)
                {
                    SelectedItem = category;
                    RaiseEvent(new RoutedEventArgs(ItemSelectedEvent));
                }
            }
        }

        private void CategoryItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                _selectedItem = item;
                item.ContextMenu = _categoryContextMenu;
                e.Handled = true;
            }
        }

        private void FileItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                _selectedItem = item;
                item.ContextMenu = _fileContextMenu;
                e.Handled = true;
            }
        }

        private void ModItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                _selectedItem = item;
                item.ContextMenu = _fileContextMenu;
                e.Handled = true;
            }
        }

        private void FileTreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Fallback для TreeView
        }

        private void FileTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var item = source.FindAncestor<TreeViewItem>();

                if (item != null && item.Tag is ModItemNode modItem)
                {
                    OpenCreatorForItem(modItem.Item);
                }
            }
        }

        private void FileTreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Контекстное меню уже установлено в обработчиках MouseRightButtonDown
        }

        
        

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem?.Tag is ModItemNode modItem)
            {
                OpenCreatorForItem(modItem.Item);
            }
        }
        /// <summary>
        /// Копирование полного пути к файлу
        /// </summary>
        private void CopyPathMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullPath = null;

                if (_selectedItem?.Tag is ConfigFileNode fileNode)
                {
                    // Получаем FileFullPath из файла
                    var fileType = fileNode.File.GetType();
                    var fullPathProp = fileType.GetProperty("FileFullPath");
                    if (fullPathProp != null)
                    {
                        fullPath = fullPathProp.GetValue(fileNode.File) as string;
                    }
                }
                else if (_selectedItem?.Tag is ModItemNode modItem)
                {
                    // Получаем FileFullPath из IConfig или IGfx
                    if (modItem.Item is IConfig config)
                    {
                        fullPath = config.FileFullPath;
                    }
                    else if (modItem.Item is IGfx gfx)
                    {
                        // IGfx не имеет FileFullPath, получаем из родительского файла
                        if (modItem.ParentFile != null)
                        {
                            var parentType = modItem.ParentFile.GetType();
                            var fullPathProp = parentType.GetProperty("FileFullPath");
                            if (fullPathProp != null)
                            {
                                fullPath = fullPathProp.GetValue(modItem.ParentFile) as string;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(fullPath))
                {
                    Clipboard.SetText(fullPath);
                    // Опционально: показать уведомление
                    // MessageBox.Show($"Path copied: {fullPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("File path not available", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying path: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Открытие папки в проводнике
        /// </summary>
        private void OpenInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullPath = null;
                bool isCore = false;

                if (_selectedItem?.Tag is ConfigFileNode fileNode)
                {
                    // Получаем FileFullPath и IsCore из файла
                    var fileType = fileNode.File.GetType();

                    var fullPathProp = fileType.GetProperty("FileFullPath");
                    if (fullPathProp != null)
                    {
                        fullPath = fullPathProp.GetValue(fileNode.File) as string;
                    }

                    var isCoreProp = fileType.GetProperty("IsCore");
                    if (isCoreProp != null)
                    {
                        var isCoreValue = isCoreProp.GetValue(fileNode.File);
                        if (isCoreValue is bool core)
                        {
                            isCore = core;
                        }
                    }
                }
                else if (_selectedItem?.Tag is ModItemNode modItem)
                {
                    // Получаем FileFullPath и IsCore из IConfig или IGfx
                    if (modItem.Item is IConfig config)
                    {
                        fullPath = config.FileFullPath;
                        isCore = config.IsCore;
                    }
                    else if (modItem.Item is IGfx gfx)
                    {
                        // Для IGfx получаем из родительского файла
                        if (modItem.ParentFile != null)
                        {
                            var parentType = modItem.ParentFile.GetType();

                            var fullPathProp = parentType.GetProperty("FileFullPath");
                            if (fullPathProp != null)
                            {
                                fullPath = fullPathProp.GetValue(modItem.ParentFile) as string;
                            }

                            var isCoreProp = parentType.GetProperty("IsCore");
                            if (isCoreProp != null)
                            {
                                var isCoreValue = isCoreProp.GetValue(modItem.ParentFile);
                                if (isCoreValue is bool core)
                                {
                                    isCore = core;
                                }
                            }
                        }
                    }
                }

                // Проверяем IsCore
                if (isCore)
                {
                    MessageBox.Show(
                        "This object is from the core game files and does not have an editable file location.",
                        "Core Object",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                // Проверяем наличие пути
                if (string.IsNullOrEmpty(fullPath))
                {
                    MessageBox.Show("File path not available", "Info",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Проверяем существование файла
                if (!File.Exists(fullPath))
                {
                    MessageBox.Show($"File does not exist:\n{fullPath}", "File Not Found",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем директорию (без имени файла)
                string directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                {
                    MessageBox.Show($"Directory does not exist:\n{directory}", "Directory Not Found",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Открываем проводник с выделением файла
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{fullPath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening explorer: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработка нажатия клавиш в поле переименования
        /// </summary>
        private void RenameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FinishRenaming(true);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                FinishRenaming(false);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработка потери фокуса (клик вне поля)
        /// </summary>
        private void RenameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Проверяем, что фокус не перешел на контекстное меню
            if (_renameTextBox != null && !_renameTextBox.IsKeyboardFocusWithin)
            {
                FinishRenaming(true);
            }
        }
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedItem();
        }

        private void AddFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem?.Tag is ModCategoryNode category)
            {
                AddNewFileToCategory(category);
            }
        }
        #endregion
        #region Context Menu Implementations

        /// <summary>
        /// Переименование элемента
        /// </summary>
        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null)
                return;

            // Проверяем, что переименовываем не категорию (ModConfig свойство)
            if (_selectedItem.Tag is ModCategoryNode)
            {
                MessageBox.Show("Cannot rename categories", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StartRenaming(_selectedItem);
        }
        private List<SearchResult> _searchResults = new List<SearchResult>();
        private int _searchResultsDisplayed = 0;
        private const int SEARCH_BATCH_SIZE = 100;
        private CancellationTokenSource _searchCancellationToken;

        // Добавить класс для результатов поиска
        public class SearchResult
        {
            public string Path { get; set; }  // Путь к элементу (для уникальности)
            public string DisplayName { get; set; }
            public object Item { get; set; }
            public SearchResultType Type { get; set; }
            public ModCategoryNode Category { get; set; }
            public ConfigFileNode File { get; set; }
            public ModItemNode ModItem { get; set; }
        }

        public enum SearchResultType
        {
            Category,
            File,
            Item
        }
        #endregion
        #region Optimized Search with VS-Style Auto-Expand

        /// <summary>
        /// Оптимизированный поиск с пагинацией и автораскрытием как в Visual Studio
        /// </summary>
        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text;
            var placeholderText = "Поиск объектов мода...";

            // Отменяем предыдущий поиск
            _searchCancellationToken?.Cancel();
            _searchCancellationToken = new CancellationTokenSource();
            var token = _searchCancellationToken.Token;

            if (string.IsNullOrEmpty(searchText) || searchText == placeholderText)
            {
                _currentSearchText = "";
                _searchResults.Clear();
                _searchResultsDisplayed = 0;
                SaveExpansionState();
                FileTreeView.Items.Clear();
                LoadModData();
                return;
            }

            _currentSearchText = searchText;

            // Минимальная длина для поиска
            if (searchText.Length < 2)
                return;

            // Небольшая задержка перед поиском (debounce)
            await Task.Delay(300, token);

            if (token.IsCancellationRequested)
                return;

            // Выполняем поиск в фоновом потоке
            await Task.Run(() => PerformSearch(searchText, token), token);

            if (token.IsCancellationRequested)
                return;

            // Обновляем UI с первой порцией результатов в стиле VS
            DisplaySearchResultsVSStyle(0, SEARCH_BATCH_SIZE);
        }

        /// <summary>
        /// Выполнение поиска (в фоновом потоке)
        /// </summary>
        private void PerformSearch(string searchText, CancellationToken token)
        {
            _searchResults.Clear();

            if (ModDataStorage.Mod == null)
                return;

            var modConfig = ModDataStorage.Mod;
            var categories = GetModCategories(modConfig);

            foreach (var category in categories)
            {
                if (token.IsCancellationRequested)
                    return;

                SearchInCategory(category, searchText, token);
            }

            // Сортируем результаты по релевантности
            _searchResults = _searchResults
                .OrderBy(r => GetSearchRelevance(r.DisplayName, searchText))
                .ThenBy(r => r.Type)
                .ThenBy(r => r.DisplayName)
                .ToList();
        }

        /// <summary>
        /// Поиск внутри категории (оптимизированный)
        /// </summary>
        private void SearchInCategory(ModCategoryNode category, string searchText, CancellationToken token)
        {
            if (category.Items == null)
                return;

            // Проверяем совпадение с категорией
            bool categoryMatches = category.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

            if (categoryMatches)
            {
                _searchResults.Add(new SearchResult
                {
                    Path = $"Category_{category.Name}",
                    DisplayName = category.DisplayName,
                    Item = category,
                    Type = SearchResultType.Category,
                    Category = category
                });
            }

            // Поиск по файлам
            foreach (var obj in category.Items)
            {
                if (token.IsCancellationRequested)
                    return;

                if (obj == null)
                    continue;

                var fileType = obj.GetType();

                if (!fileType.IsGenericType)
                    continue;

                var genericDef = fileType.GetGenericTypeDefinition();
                if (genericDef != typeof(ConfigFile<>) && genericDef != typeof(GfxFile<>))
                    continue;

                Type configType = fileType.GetGenericArguments()[0];
                IList entities = null;
                string fileName = "Unknown";

                var entitiesProp = fileType.GetProperty("Entities");
                if (entitiesProp != null)
                {
                    entities = entitiesProp.GetValue(obj) as IList;
                }

                var fileNameProp = fileType.GetProperty("FileName");
                if (fileNameProp != null)
                {
                    fileName = fileNameProp.GetValue(obj) as string ?? "Unknown";
                }

                // Проверяем совпадение с именем файла
                bool fileMatches = fileName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                if (fileMatches)
                {
                    var fileNode = new ConfigFileNode
                    {
                        File = obj,
                        DisplayName = fileName,
                        ConfigType = configType,
                        EntityCount = entities?.Count ?? 0,
                        Entities = entities,
                        ParentProperty = category.PropertyInfo
                    };

                    _searchResults.Add(new SearchResult
                    {
                        Path = $"File_{fileName}_{configType?.Name}",
                        DisplayName = fileName,
                        Item = obj,
                        Type = SearchResultType.File,
                        Category = category,
                        File = fileNode
                    });
                }

                // Поиск по элементам внутри файла
                if (entities != null)
                {
                    SearchInEntities(entities, obj, fileName, configType, category, searchText, token);
                }
            }
        }

        /// <summary>
        /// Поиск по элементам (оптимизированный)
        /// </summary>
        private void SearchInEntities(IList entities, object parentFile, string fileName, Type configType,
                                      ModCategoryNode category, string searchText, CancellationToken token)
        {
            foreach (var entity in entities)
            {
                if (token.IsCancellationRequested)
                    return;

                if (entity == null)
                    continue;

                string displayName = GetItemDisplayName(entity);
                string id = GetItemId(entity);

                bool itemMatches = displayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  id.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                if (itemMatches)
                {
                    var modItem = new ModItemNode
                    {
                        Item = entity,
                        DisplayName = displayName,
                        Id = id,
                        ParentFile = parentFile
                    };

                    var fileNode = new ConfigFileNode
                    {
                        File = parentFile,
                        DisplayName = fileName,
                        ConfigType = configType,
                        EntityCount = entities.Count,
                        Entities = entities,
                        ParentProperty = category.PropertyInfo
                    };

                    _searchResults.Add(new SearchResult
                    {
                        Path = $"Item_{id}_{fileName}",
                        DisplayName = displayName,
                        Item = entity,
                        Type = SearchResultType.Item,
                        Category = category,
                        File = fileNode,
                        ModItem = modItem
                    });
                }
            }
        }

        /// <summary>
        /// Вычисление релевантности результата поиска
        /// </summary>
        private int GetSearchRelevance(string text, string searchText)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchText))
                return int.MaxValue;

            var lowerText = text.ToLowerInvariant();
            var lowerSearch = searchText.ToLowerInvariant();

            // Точное совпадение - наивысший приоритет
            if (lowerText == lowerSearch)
                return 0;

            // Начинается с поискового запроса
            if (lowerText.StartsWith(lowerSearch))
                return 1;

            // Содержит как отдельное слово
            if (lowerText.Contains(" " + lowerSearch) || lowerText.Contains("_" + lowerSearch))
                return 2;

            // Содержит в середине
            int index = lowerText.IndexOf(lowerSearch);
            if (index >= 0)
                return 3 + index;

            return int.MaxValue;
        }

        /// <summary>
        /// Отображение результатов поиска в стиле Visual Studio с автораскрытием
        /// </summary>
        private void DisplaySearchResultsVSStyle(int startIndex, int count)
        {
            Dispatcher.Invoke(() =>
            {
                if (startIndex == 0)
                {
                    SaveExpansionState();
                    FileTreeView.Items.Clear();
                    _searchResultsDisplayed = 0;
                }

                int endIndex = Math.Min(startIndex + count, _searchResults.Count);

                // Группируем результаты по категориям
                var resultsByCategory = _searchResults
                    .Skip(startIndex)
                    .Take(count)
                    .GroupBy(r => r.Category)
                    .ToList();

                foreach (var categoryGroup in resultsByCategory)
                {
                    var category = categoryGroup.Key;

                    // Находим или создаем узел категории
                    TreeViewItem categoryItem = FindOrCreateCategoryNode(category);

                    // Группируем по файлам внутри категории
                    var resultsByFile = categoryGroup.GroupBy(r => r.File?.DisplayName ?? "");

                    foreach (var fileGroup in resultsByFile)
                    {
                        var firstResult = fileGroup.First();

                        if (firstResult.Type == SearchResultType.Category)
                        {
                            // Результат - сама категория
                            continue;
                        }
                        else if (firstResult.Type == SearchResultType.File)
                        {
                            // Файл совпадает - создаем узел файла и раскрываем его
                            TreeViewItem fileItem = FindOrCreateFileNode(categoryItem, firstResult.File);
                            fileItem.IsExpanded = true;
                        }
                        else if (firstResult.Type == SearchResultType.Item)
                        {
                            // Элементы совпадают - создаем файл, добавляем элементы, раскрываем
                            TreeViewItem fileItem = FindOrCreateFileNode(categoryItem, firstResult.File);

                            foreach (var itemResult in fileGroup.Where(r => r.Type == SearchResultType.Item))
                            {
                                // Проверяем дубликаты
                                bool alreadyExists = false;
                                foreach (TreeViewItem existingItem in fileItem.Items)
                                {
                                    if (existingItem.Tag is ModItemNode existing &&
                                        existing.Id == itemResult.ModItem.Id)
                                    {
                                        alreadyExists = true;
                                        break;
                                    }
                                }

                                if (!alreadyExists)
                                {
                                    var itemNode = CreateModItemTreeViewItem(itemResult.ModItem);
                                    fileItem.Items.Add(itemNode);
                                }
                            }

                            // VS-Style: Раскрываем файл если в нем есть совпадения
                            fileItem.IsExpanded = true;
                        }
                    }

                    // VS-Style: Раскрываем категорию если в ней есть совпадения
                    categoryItem.IsExpanded = true;
                }

                _searchResultsDisplayed = endIndex;

                // Добавляем кнопку "Загрузить еще"
                if (_searchResultsDisplayed < _searchResults.Count)
                {
                    AddLoadMoreButton();
                }
            });
        }

        /// <summary>
        /// Найти или создать узел категории
        /// </summary>
        private TreeViewItem FindOrCreateCategoryNode(ModCategoryNode category)
        {
            // Ищем существующий узел
            foreach (TreeViewItem item in FileTreeView.Items)
            {
                if (item.Tag is ModCategoryNode existing && existing.Name == category.Name)
                {
                    return item;
                }
            }

            // Создаем новый
            var categoryItem = CreateCategoryTreeViewItem(category);
            categoryItem.Items.Clear(); // Убираем "Loading..."
            FileTreeView.Items.Add(categoryItem);
            return categoryItem;
        }

        /// <summary>
        /// Найти или создать узел файла
        /// </summary>
        private TreeViewItem FindOrCreateFileNode(TreeViewItem categoryItem, ConfigFileNode fileNode)
        {
            // Ищем существующий узел
            foreach (TreeViewItem item in categoryItem.Items)
            {
                if (item.Tag is ConfigFileNode existing &&
                    existing.DisplayName == fileNode.DisplayName &&
                    existing.ConfigType == fileNode.ConfigType)
                {
                    if (item.Items.Count == 1 && item.Items[0] is TreeViewItem loadingItem && !loadingItem.IsEnabled)
                    {
                        item.Items.Clear(); // Убираем "Loading..."
                    }
                    return item;
                }
            }

            // Создаем новый
            var fileItem = CreateFileTreeViewItem(fileNode);
            fileItem.Items.Clear(); // Убираем "Loading..."
            categoryItem.Items.Add(fileItem);
            return fileItem;
        }

        /// <summary>
        /// Добавление кнопки "Загрузить еще"
        /// </summary>
        private void AddLoadMoreButton()
        {
            // Удаляем старую кнопку если есть
            RemoveLoadMoreButton();

            var loadMoreItem = new TreeViewItem
            {
                IsEnabled = true,
                Focusable = true
            };

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 5, 0, 5)
            };

            var button = new Button
            {
                Content = $"Load more ({_searchResults.Count - _searchResultsDisplayed} remaining)...",
                Style = (Style)FindResource("ButtonDarkSecondary"),
                Padding = new Thickness(12, 6, 0,0),
                Cursor = Cursors.Hand
            };

            button.Click += (s, e) =>
            {
                RemoveLoadMoreButton();
                DisplaySearchResultsVSStyle(_searchResultsDisplayed, SEARCH_BATCH_SIZE);
            };

            panel.Children.Add(button);
            loadMoreItem.Header = panel;
            loadMoreItem.Tag = "LoadMoreButton";

            FileTreeView.Items.Add(loadMoreItem);
        }

        /// <summary>
        /// Удаление кнопки "Загрузить еще"
        /// </summary>
        private void RemoveLoadMoreButton()
        {
            TreeViewItem itemToRemove = null;

            foreach (TreeViewItem item in FileTreeView.Items)
            {
                if (item.Tag is string tag && tag == "LoadMoreButton")
                {
                    itemToRemove = item;
                    break;
                }
            }

            if (itemToRemove != null)
            {
                FileTreeView.Items.Remove(itemToRemove);
            }
        }

        #endregion
        /// <summary>
        /// Начало процесса переименования
        /// </summary>
        #region Context Menu and Other functions
        private void StartRenaming(TreeViewItem item)
        {
            if (item == null || !(item.Header is StackPanel panel))
                return;

            _renamingItem = item;

            // Находим TextBlock с именем в StackPanel
            TextBlock nameTextBlock = null;
            foreach (var child in panel.Children)
            {
                if (child is TextBlock tb && tb != panel.Children[0]) // Пропускаем иконку (первый элемент)
                {
                    nameTextBlock = tb;
                    break;
                }
            }

            if (nameTextBlock == null)
                return;

            // Получаем текущее имя
            string currentName = GetCurrentName(item);
            if (string.IsNullOrEmpty(currentName))
                return;

            // Создаем TextBox для редактирования
            _renameTextBox = new TextBox
            {
                Text = currentName,
                MinWidth = 100,
                VerticalAlignment = VerticalAlignment.Center,
                Background = (Brush)System.Windows.Application.Current.Resources["InputBackground"],
                Foreground = (Brush)System.Windows.Application.Current.Resources["InputText"],
                BorderBrush = (Brush)System.Windows.Application.Current.Resources["InputBorderFocus"],
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4, 2, 0,0)
            };

            // Обработчики событий
            _renameTextBox.KeyDown += RenameTextBox_KeyDown;
            _renameTextBox.LostFocus += RenameTextBox_LostFocus;
            _renameTextBox.PreviewMouseRightButtonDown += (s, ev) => { ev.Handled = true; FinishRenaming(false); };
            _renameTextBox.PreviewMouseLeftButtonDown += (s, ev) =>
            {
                ev.Handled = true;
            };


            // Заменяем TextBlock на TextBox
            int index = panel.Children.IndexOf(nameTextBlock);
            panel.Children.RemoveAt(index);
            panel.Children.Insert(index, _renameTextBox);

            // Фокусируемся и выделяем текст
            _renameTextBox.Focus();
            _renameTextBox.SelectAll();
        }

        /// <summary>
        /// Получить текущее имя элемента
        /// </summary>
        private string GetCurrentName(TreeViewItem item)
        {
            if (item.Tag is ConfigFileNode fileNode)
            {
                return fileNode.DisplayName;
            }
            else if (item.Tag is ModItemNode modItem)
            {
                return modItem.DisplayName;
            }
            return null;
        }


        private void OpenCreatorForItem(object item)
        {
            if (item == null) return;

            var itemType = item.GetType();
            var creatorAttribute = itemType.GetCustomAttribute<ConfigCreatorAttribute>();

            if (creatorAttribute == null)
            {
                CustomMessageBox.Show(
                    $"Для типа {itemType.Name} не указан атрибут ConfigCreatorAttribute",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                switch (creatorAttribute.CreatorType)
                {
                    case ConfigCreatorType.GenericCreator:
                        var viewer = new GenericViewer(itemType, item);
                        var parentWindow = Window.GetWindow(this);
                        if (parentWindow != null)
                        {
                            var dockManager = parentWindow.FindName("DockManager") as DockManager;

                            (dockManager as dynamic)?.SetContent(viewer);
                        }
                        return;

                    case ConfigCreatorType.CountryCreator:
                    case ConfigCreatorType.MapCreator:
                    case ConfigCreatorType.GenericGuiCreator:
                         CustomMessageBox.Show(
                            $"{creatorAttribute.CreatorType} пока не реализован",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;

                    default:
                        CustomMessageBox.Show(
                            $"Неизвестный тип Creator: {creatorAttribute.CreatorType}",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(
                    $"Ошибка при открытии Creator: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Завершение переименования
        /// </summary>
        private void FinishRenaming(bool applyChanges)
        {
            if (_renameTextBox == null || _renamingItem == null)
                return;

            string newName = _renameTextBox.Text?.Trim();
            bool success = false;

            if (applyChanges && !string.IsNullOrEmpty(newName))
            {
                success = ApplyRename(_renamingItem, newName);
            }

            // Восстанавливаем TextBlock
            if (_renamingItem.Header is StackPanel panel)
            {
                int index = panel.Children.IndexOf(_renameTextBox);
                if (index >= 0)
                {
                    panel.Children.RemoveAt(index);

                    // Создаем новый TextBlock с обновленным именем (если переименование успешно)
                    string displayName = success && applyChanges ? newName : GetCurrentName(_renamingItem);
                    var newTextBlock = CreateHighlightedTextBlock(displayName, _currentSearchText);
                    newTextBlock.VerticalAlignment = VerticalAlignment.Center;
                    newTextBlock.Foreground = (Brush)System.Windows.Application.Current.Resources["TextPrimary"];

                    panel.Children.Insert(index, newTextBlock);
                }
            }

            _renameTextBox = null;
            _renamingItem = null;

            if (applyChanges && success)
            {
                // Обновляем дерево с сохранением состояния
                LoadModData();
            }
        }

        /// <summary>
        /// Применить переименование к элементу
        /// </summary>
        private bool ApplyRename(TreeViewItem item, string newName)
        {
            try
            {
                if (item.Tag is ConfigFileNode fileNode)
                {
                    // Переименование файла
                    var fileType = fileNode.File.GetType();
                    var renameMethod = fileType.GetMethod("Rename");

                    if (renameMethod != null)
                    {
                        var result = renameMethod.Invoke(fileNode.File, new object[] { newName });
                        if (result is bool renamed && renamed)
                        {
                            fileNode.DisplayName = newName;
                            return true;
                        }
                        else
                        {
                            MessageBox.Show($"Failed to rename file to '{newName}'", "Rename Failed",
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Rename method not found on file object", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else if (item.Tag is ModItemNode modItem)
                {
                    // Переименование IConfig или IGfx (меняем Id)
                    if (modItem.Item is IConfig config)
                    {
                        // Создаем новый Identifier
                        var identifierType = config.Id?.GetType() ?? typeof(Models.Types.Utils.Identifier);
                        var newIdentifier = Activator.CreateInstance(identifierType, newName);

                        if (newIdentifier != null)
                        {
                            config.Id = (Models.Types.Utils.Identifier)newIdentifier;
                            modItem.DisplayName = newName;
                            modItem.Id = newName;
                            return true;
                        }
                    }
                    else if (modItem.Item is IGfx gfx)
                    {
                        var identifierType = gfx.Id?.GetType() ?? typeof(Models.Types.Utils.Identifier);
                        var newIdentifier = Activator.CreateInstance(identifierType, newName);

                        if (newIdentifier != null)
                        {
                            gfx.Id = (Models.Types.Utils.Identifier)newIdentifier;
                            modItem.DisplayName = newName;
                            modItem.Id = newName;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renaming: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return false;
        }

        #endregion
    }
}