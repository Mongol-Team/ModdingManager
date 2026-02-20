using Application;
using Application.Debugging;
using Application.extentions;
using Application.Extentions;
using Application.utils;
using Controls.Args;
using Controls.Docking;
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
        // ─── Dependency Properties ───────────────────────────────────────────────

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(FileExplorer),
                new PropertyMetadata("Mod Explorer"));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(FileExplorer),
                new PropertyMetadata(null));

        // ─── Routed Events ───────────────────────────────────────────────────────

        public static readonly RoutedEvent ItemSelectedEvent =
            EventManager.RegisterRoutedEvent(nameof(ItemSelected), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(FileExplorer));

        public static readonly RoutedEvent OpenItemRequestedEvent =
            EventManager.RegisterRoutedEvent(nameof(OpenItemRequested), RoutingStrategy.Bubble,
                typeof(EventHandler<OpenItemRequestedEventArgs>), typeof(FileExplorer));

        public static readonly RoutedEvent AddFileRequestedEvent =
            EventManager.RegisterRoutedEvent(nameof(AddFileRequested), RoutingStrategy.Bubble,
                typeof(EventHandler<AddFileRequestedEventArgs>), typeof(FileExplorer));

        public static readonly RoutedEvent AddEntityRequestedEvent =
            EventManager.RegisterRoutedEvent(nameof(AddEntityRequested), RoutingStrategy.Bubble,
                typeof(EventHandler<AddEntityRequestedEventArgs>), typeof(FileExplorer));

        public static readonly RoutedEvent DeleteItemRequestedEvent =
            EventManager.RegisterRoutedEvent(nameof(DeleteItemRequested), RoutingStrategy.Bubble,
                typeof(EventHandler<DeleteItemRequestedEventArgs>), typeof(FileExplorer));

        public static readonly RoutedEvent MoveFileRequestedEvent =
            EventManager.RegisterRoutedEvent(nameof(MoveFileRequested), RoutingStrategy.Bubble,
                typeof(EventHandler<MoveFileRequestedEventArgs>), typeof(FileExplorer));

        public static readonly RoutedEvent MoveEntityRequestedEvent =
            EventManager.RegisterRoutedEvent(nameof(MoveEntityRequested), RoutingStrategy.Bubble,
                typeof(EventHandler<MoveEntityRequestedEventArgs>), typeof(FileExplorer));

        public static readonly RoutedEvent RenameRequestedEvent =
      EventManager.RegisterRoutedEvent(nameof(RenameRequested), RoutingStrategy.Bubble,
          typeof(EventHandler<RenameRequestedEventArgs>), typeof(FileExplorer));

        // ─── Public API ──────────────────────────────────────────────────────────

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
        public event EventHandler<OpenItemRequestedEventArgs> OpenItemRequested
        {
            add => AddHandler(OpenItemRequestedEvent, value);
            remove => RemoveHandler(OpenItemRequestedEvent, value);
        }
        public event EventHandler<AddFileRequestedEventArgs> AddFileRequested
        {
            add => AddHandler(AddFileRequestedEvent, value);
            remove => RemoveHandler(AddFileRequestedEvent, value);
        }
        public event EventHandler<AddEntityRequestedEventArgs> AddEntityRequested
        {
            add => AddHandler(AddEntityRequestedEvent, value);
            remove => RemoveHandler(AddEntityRequestedEvent, value);
        }
        public event EventHandler<DeleteItemRequestedEventArgs> DeleteItemRequested
        {
            add => AddHandler(DeleteItemRequestedEvent, value);
            remove => RemoveHandler(DeleteItemRequestedEvent, value);
        }
        public event EventHandler<MoveFileRequestedEventArgs> MoveFileRequested
        {
            add => AddHandler(MoveFileRequestedEvent, value);
            remove => RemoveHandler(MoveFileRequestedEvent, value);
        }
        public event EventHandler<MoveEntityRequestedEventArgs> MoveEntityRequested
        {
            add => AddHandler(MoveEntityRequestedEvent, value);
            remove => RemoveHandler(MoveEntityRequestedEvent, value);
        }
        public event EventHandler<RenameRequestedEventArgs> RenameRequested
        {
            add => AddHandler(RenameRequestedEvent, value);
            remove => RemoveHandler(RenameRequestedEvent, value);
        }

        // ─── Private State ───────────────────────────────────────────────────────

        private TreeViewItem _selectedItem;
        private ContextMenu _fileContextMenu;
        private ContextMenu _categoryContextMenu;
        private Point _dragStartPoint;
        private bool _isDragging;
        private readonly Dictionary<string, bool> _expansionState = new();
        private string _currentSearchText = "";
        private TreeViewItem _renamingItem;
        private TextBox _renameTextBox;

        // ─── Поиск ──────────────────────────────────────────────────────────────

        private List<SearchResult> _searchResults = new();
        private int _searchResultsDisplayed;
        private const int SearchBatchSize = 100;
        private CancellationTokenSource _searchCts;

        public class SearchResult
        {
            public string Path { get; set; }
            public string DisplayName { get; set; }
            public object Item { get; set; }
            public SearchResultType Type { get; set; }
            public ModCategoryNode Category { get; set; }
            public ConfigFileNode File { get; set; }
            public ModItemNode ModItem { get; set; }
        }

        public enum SearchResultType { Category, File, Item }

        // ─── Constructor ─────────────────────────────────────────────────────────

        public FileExplorer()
        {
            InitializeComponent();
            InitializeContextMenus();
            LoadContextMenuLocalization();
            SetupSearchPlaceholder();
            LoadModData();
        }

        // ─── Инициализация ───────────────────────────────────────────────────────

        private void InitializeContextMenus()
        {
            _fileContextMenu = new ContextMenu();

            var openItem = CreateMenuItem("OpenMenuItem", "Open", OpenMenuItem_Click);
            var openInExplorerItem = CreateMenuItem("OpenInExplorerMenuItem", "Open in Explorer", OpenInExplorerMenuItem_Click);
            var copyPathItem = CreateMenuItem("CopyPathMenuItem", "Copy Path", CopyPathMenuItem_Click);
            var renameItem = CreateMenuItem("RenameMenuItem", "Rename", RenameMenuItem_Click);
            var deleteItem = CreateMenuItem("DeleteMenuItem", "Delete", DeleteMenuItem_Click);

            _fileContextMenu.Items.Add(openItem);
            _fileContextMenu.Items.Add(openInExplorerItem);
            _fileContextMenu.Items.Add(new Separator());
            _fileContextMenu.Items.Add(copyPathItem);
            _fileContextMenu.Items.Add(renameItem);
            _fileContextMenu.Items.Add(deleteItem);

            _categoryContextMenu = new ContextMenu();
            var addFileItem = CreateMenuItem("AddFileMenuItem", "Add File", AddFileMenuItem_Click);
            _categoryContextMenu.Items.Add(addFileItem);
        }

        private static MenuItem CreateMenuItem(string name, string header, RoutedEventHandler handler)
        {
            var item = new MenuItem { Name = name, Header = header };
            item.Click += handler;
            return item;
        }

        private void LoadContextMenuLocalization()
        {
            SetMenuItemHeader(_fileContextMenu, 0, "Menu.Open");
            SetMenuItemHeader(_fileContextMenu, 1, "Menu.OpenInExplorer");
            SetMenuItemHeader(_fileContextMenu, 3, "Menu.CopyFullPath");
            SetMenuItemHeader(_fileContextMenu, 4, "Menu.Rename");
            SetMenuItemHeader(_fileContextMenu, 5, "Menu.Delete");
            SetMenuItemHeader(_categoryContextMenu, 0, "Menu.AddFile");
        }

        private static void SetMenuItemHeader(ContextMenu menu, int index, string key)
        {
            if (menu?.Items.Count > index && menu.Items[index] is MenuItem item)
                item.Header = StaticLocalisation.GetString(key);
        }

        private void SetupSearchPlaceholder()
        {
            var placeholder = StaticLocalisation.GetString("FileExplorer.SearchPlaceholder");
            var placeholderBrush = new SolidColorBrush(Color.FromRgb(133, 133, 133));
            var normalBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204));

            SearchTextBox.Text = placeholder;
            SearchTextBox.Foreground = placeholderBrush;

            SearchTextBox.GotFocus += (s, e) =>
            {
                if (SearchTextBox.Text == placeholder)
                {
                    SearchTextBox.Text = "";
                    SearchTextBox.Foreground = normalBrush;
                }
            };
            SearchTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    SearchTextBox.Text = placeholder;
                    SearchTextBox.Foreground = placeholderBrush;
                }
            };
        }

        // ─── Загрузка данных ─────────────────────────────────────────────────────

        public void LoadModData()
        {
            SaveExpansionState();
            FileTreeView.Items.Clear();

            if (ModDataStorage.Mod == null)
                return;

            foreach (var category in GetModCategories(ModDataStorage.Mod))
            {
                if (category.Items?.Count > 0)
                    FileTreeView.Items.Add(CreateCategoryTreeViewItem(category));
            }

            RestoreExpansionState();
        }

        private List<ModCategoryNode> GetModCategories(ModConfig modConfig)
        {
            var categories = new List<ModCategoryNode>();

            foreach (var prop in typeof(ModConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType.IsGenericType)
                {
                    var genericDef = prop.PropertyType.GetGenericTypeDefinition();
                    if (genericDef == typeof(ObservableCollection<>))
                    {
                        var itemType = prop.PropertyType.GetGenericArguments()[0];
                        if (IsFileContainerType(itemType) &&
                            prop.GetValue(modConfig) is IList list && list.Count > 0)
                        {
                            categories.Add(new ModCategoryNode
                            {
                                Name = prop.Name,
                                DisplayName = FormatCategoryName(prop.Name),
                                Items = list,
                                ItemType = itemType,
                                PropertyInfo = prop
                            });
                        }
                    }
                }
                else if (prop.PropertyType == typeof(MapConfig) &&
                         prop.GetValue(modConfig) is MapConfig map)
                {
                    categories.Add(new ModCategoryNode
                    {
                        Name = "Map",
                        DisplayName = "Map",
                        Items = new List<object> { map },
                        ItemType = typeof(MapConfig),
                        PropertyInfo = prop
                    });
                }
            }

            return categories.OrderBy(c => c.DisplayName).ToList();
        }

        private static bool IsFileContainerType(Type t) =>
            t.IsGenericType &&
            (t.GetGenericTypeDefinition() == typeof(ConfigFile<>) ||
             t.GetGenericTypeDefinition() == typeof(GfxFile<>));

        private static string FormatCategoryName(string name) =>
            System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim();

        // ─── Создание TreeViewItem'ов ────────────────────────────────────────────

        private TreeViewItem CreateCategoryTreeViewItem(ModCategoryNode category)
        {
            var item = new TreeViewItem
            {
                Header = CreateCategoryHeader(category),
                Tag = category
            };
            item.Items.Add(CreateLoadingItem());
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
                AllowDrop = true
            };
            item.Items.Add(CreateLoadingItem());
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
                AllowDrop = true
            };
            item.MouseMove += TreeViewItem_MouseMove;
            item.PreviewMouseLeftButtonDown += TreeViewItem_PreviewMouseLeftButtonDown;
            item.MouseRightButtonDown += ModItem_MouseRightButtonDown;
            return item;
        }

        private static TreeViewItem CreateLoadingItem() =>
            new() { Header = "Loading...", IsEnabled = false };

        private static bool IsLoadingItem(TreeViewItem item) =>
            item.Items.Count == 1 && item.Items[0] is TreeViewItem t && !t.IsEnabled;

        // ─── Создание заголовков ─────────────────────────────────────────────────

       
      
        private UIElement CreateItemHeader(string name)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(new TextBlock
            {
                Text = "\uE7C3",
                Style = (Style)FindResource("FileIconStyle"),
                Foreground = (Brush)System.Windows.Application.Current.Resources["TextTertiary"]
            });
            panel.Children.Add(CreateHighlightedTextBlock(name));
            return panel;
        }

        private Button CreateHeaderButton(string text, string tooltip, Action<object> onClick)
        {
            var btn = new Button
            {
                Content = new TextBlock { Text = text, FontSize = 12, FontWeight = FontWeights.Bold },
                Style = (Style)FindResource("FileControlButton"),
                ToolTip = tooltip
            };
            btn.Click += (s, e) => { e.Handled = true; onClick(s); };
            return btn;
        }

        private TextBlock CreateHighlightedTextBlock(string text)
        {
            var tb = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)System.Windows.Application.Current.Resources["TextPrimary"]
            };

            var search = _currentSearchText;
            if (string.IsNullOrEmpty(search))
            {
                tb.Text = text;
                return tb;
            }

            var idx = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                tb.Text = text;
                return tb;
            }

            if (idx > 0) tb.Inlines.Add(new Run(text[..idx]));
            tb.Inlines.Add(new Run(text.Substring(idx, search.Length))
            {
                Background = new SolidColorBrush(Color.FromRgb(74, 144, 226)),
                Foreground = Brushes.White
            });
            if (idx + search.Length < text.Length)
                tb.Inlines.Add(new Run(text[(idx + search.Length)..]));

            return tb;
        }

        private static string GetShortTypeName(Type type) =>
            type?.Name.EndsWith("Config") == true ? type.Name[..^6] : type?.Name ?? "Unknown";

        // ─── Загрузка содержимого узлов (lazy) ──────────────────────────────────

        private void CategoryItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item && item.Tag is ModCategoryNode category && IsLoadingItem(item))
            {
                item.Items.Clear();
                LoadCategoryContents(item, category);
            }
        }

        private void FileItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item && item.Tag is ConfigFileNode fileNode && IsLoadingItem(item))
            {
                item.Items.Clear();
                LoadFileContents(item, fileNode);
            }
        }

        private void LoadCategoryContents(TreeViewItem parentItem, ModCategoryNode category)
        {
            if (category.Items == null) return;

            foreach (var obj in category.Items)
            {
                if (obj == null || !TryExtractFileInfo(obj, out var configType, out var entities, out var fileName))
                    continue;

                if (!ShouldShowItem(fileName)) continue;

                var fileNode = new ConfigFileNode
                {
                    File = obj,
                    DisplayName = fileName,
                    ConfigType = configType,
                    EntityCount = entities?.Count ?? 0,
                    Entities = entities,
                    ParentProperty = category.PropertyInfo
                };

                parentItem.Items.Add(CreateFileTreeViewItem(fileNode));
            }
        }

        private void LoadFileContents(TreeViewItem parentItem, ConfigFileNode fileNode)
        {
            if (fileNode.Entities == null) return;

            foreach (var entity in fileNode.Entities.Cast<object>().Where(e => e != null))
            {
                var displayName = GetItemId(entity);
                if (!ShouldShowItem(displayName)) continue;

                parentItem.Items.Add(CreateModItemTreeViewItem(new ModItemNode
                {
                    Item = entity,
                    DisplayName = displayName,
                    Id = displayName,
                    ParentFile = fileNode.File
                }));
            }
        }

        /// <summary>
        /// Извлекает метаданные из объекта ConfigFile&lt;T&gt; или GfxFile&lt;T&gt; через рефлексию.
        /// </summary>
        private static bool TryExtractFileInfo(object obj, out Type configType, out IList entities, out string fileName)
        {
            configType = null; entities = null; fileName = "Unknown";

            var fileType = obj.GetType();
            if (!fileType.IsGenericType) return false;

            var genericDef = fileType.GetGenericTypeDefinition();
            if (genericDef != typeof(ConfigFile<>) && genericDef != typeof(GfxFile<>))
                return false;

            configType = fileType.GetGenericArguments()[0];
            entities = fileType.GetProperty("Entities")?.GetValue(obj) as IList;
            fileName = fileType.GetProperty("FileName")?.GetValue(obj) as string ?? "Unknown";
            return true;
        }

        /// <summary>
        /// Возвращает строковый идентификатор объекта (IConfig.Id, IGfx.Id или имя типа).
        /// </summary>
        private static string GetItemId(object item) => item switch
        {
            IConfig c when c.Id != null => c.Id.ToString(),
            IGfx g when g.Id != null => g.Id.ToString(),
            _ => item.GetType().Name
        };

        private bool ShouldShowItem(string name)
        {
            if (string.IsNullOrEmpty(_currentSearchText)) return true;
            if (string.IsNullOrEmpty(name)) return false;
            return name.IndexOf(_currentSearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // ─── Expansion State ─────────────────────────────────────────────────────

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
                if (key != null) _expansionState[key] = item.IsExpanded;
                if (item.Items.Count > 0) SaveExpansionStateRecursive(item.Items);
            }
        }

        private void RestoreExpansionState() =>
            RestoreExpansionStateRecursive(FileTreeView.Items);

        private void RestoreExpansionStateRecursive(ItemCollection items)
        {
            foreach (TreeViewItem item in items)
            {
                var key = GetNodeKey(item);
                if (key != null && _expansionState.TryGetValue(key, out var expanded))
                    item.IsExpanded = expanded;
                if (item.Items.Count > 0) RestoreExpansionStateRecursive(item.Items);
            }
        }

        private static string GetNodeKey(TreeViewItem item) => item.Tag switch
        {
            ModCategoryNode c => $"Category_{c.Name}",
            ConfigFileNode f => $"File_{f.DisplayName}_{f.ConfigType?.Name}",
            ModItemNode m => $"Item_{m.Id}",
            _ => null
        };

        // ─── Drag & Drop ─────────────────────────────────────────────────────────

        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _isDragging) return;

            var diff = _dragStartPoint - e.GetPosition(null);
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is TreeViewItem item)
                {
                    _isDragging = true;
                    StartDragOperation(item);
                }
            }
        }

        private void StartDragOperation(TreeViewItem item)
        {
            var data = new DataObject();

            if (item.Tag is ConfigFileNode fileNode)
            {
                data.SetData("ConfigFile", fileNode.File);
                data.SetData("ConfigFileType", fileNode.ConfigType);
                DragDrop.DoDragDrop(item, data, DragDropEffects.Move | DragDropEffects.Copy);
            }
            else if (item.Tag is ModItemNode modItem)
            {
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
            if (targetItem == null) return;

            if (e.Data.GetDataPresent("ConfigFile") && targetItem.Tag is ModCategoryNode category)
            {
                var sourceType = e.Data.GetData("ConfigFileType") as Type;
                var targetType = GetConfigTypeFromCategory(category);
                if (sourceType != null && sourceType == targetType)
                    e.Effects = DragDropEffects.Move;
            }
            else if (e.Data.GetDataPresent("ConfigItem") && targetItem.Tag is ConfigFileNode fileNode)
            {
                var sourceItem = e.Data.GetData("ConfigItem");
                if (sourceItem != null && fileNode.ConfigType == sourceItem.GetType())
                    e.Effects = DragDropEffects.Move;
            }
        }

        private void FileTreeView_Drop(object sender, DragEventArgs e)
        {
            var targetItem = GetTreeViewItemFromPoint(e.GetPosition(FileTreeView));
            if (targetItem == null) return;

            if (e.Data.GetDataPresent("ConfigFile") && targetItem.Tag is ModCategoryNode category)
            {
                var sourceFile = e.Data.GetData("ConfigFile");
                RaiseEvent(new MoveFileRequestedEventArgs(MoveFileRequestedEvent, sourceFile, category));
            }
            else if (e.Data.GetDataPresent("ConfigItem") && targetItem.Tag is ConfigFileNode fileNode)
            {
                var sourceItem = e.Data.GetData("ConfigItem");
                var sourceParentFile = e.Data.GetData("ParentFile");
                RaiseEvent(new MoveEntityRequestedEventArgs(MoveEntityRequestedEvent,
                    sourceItem, sourceParentFile, fileNode.File));
            }
        }

        private TreeViewItem GetTreeViewItemFromPoint(Point point)
        {
            var hit = VisualTreeHelper.HitTest(FileTreeView, point)?.VisualHit;
            while (hit != null && hit is not TreeViewItem)
                hit = VisualTreeHelper.GetParent(hit);
            return hit as TreeViewItem;
        }

        private static Type GetConfigTypeFromCategory(ModCategoryNode category)
        {
            if (!IsFileContainerType(category.ItemType)) return null;
            return category.ItemType.GetGenericArguments()[0];
        }

        // ─── Event Handlers ──────────────────────────────────────────────────────

        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is not TreeViewItem item) return;

            _selectedItem = item;
            SelectedItem = item.Tag switch
            {
                ModItemNode m => m.Item,
                ConfigFileNode f => f.File,
                ModCategoryNode c => c,
                _ => null
            };

            if (SelectedItem != null)
                RaiseEvent(new RoutedEventArgs(ItemSelectedEvent));
        }

        private void FileTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var item = source.FindAncestor<TreeViewItem>();
                if (item?.Tag is ModItemNode modItem)
                {
                    RaiseEvent(new OpenItemRequestedEventArgs(OpenItemRequestedEvent, modItem.Item));
                    Logger.AddDbgLog(StaticLocalisation.GetString(
                        "Log.FileExplorer.OpenItemRequested", modItem.Item?.GetType().Name ?? "null"));
                }
            }
        }

        private void CategoryItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TreeViewItem item) return;
            item.IsSelected = true;
            _selectedItem = item;
            item.ContextMenu = _categoryContextMenu;
            e.Handled = true;
        }

        private void FileItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e) =>
            AssignContextMenu(sender, _fileContextMenu, e);

        private void ModItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e) =>
            AssignContextMenu(sender, _fileContextMenu, e);

        private void AssignContextMenu(object sender, ContextMenu menu, MouseButtonEventArgs e)
        {
            if (sender is not TreeViewItem item) return;
            item.IsSelected = true;
            _selectedItem = item;
            item.ContextMenu = menu;
            e.Handled = true;
        }

        // ─── Context Menu Handlers ───────────────────────────────────────────────

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem?.Tag is ModItemNode modItem)
                RaiseEvent(new OpenItemRequestedEventArgs(OpenItemRequestedEvent, modItem.Item));
        }

        private void CopyPathMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var path = GetSelectedItemPath();
            if (!string.IsNullOrEmpty(path))
                Clipboard.SetText(path);
            else
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("FileExplorer.PathNotAvailable"),
                    StaticLocalisation.GetString("Dialog.Info"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var path = GetSelectedItemPath();

            if (string.IsNullOrEmpty(path))
            {
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("FileExplorer.PathNotAvailable"),
                    StaticLocalisation.GetString("Dialog.Info"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!File.Exists(path))
            {
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("FileExplorer.FileNotFound", path),
                    StaticLocalisation.GetString("Dialog.Warning"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
        }

        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem?.Tag is ModCategoryNode)
            {
                CustomMessageBox.Show(
                    StaticLocalisation.GetString("FileExplorer.CannotRenameCategory"),
                    StaticLocalisation.GetString("Dialog.Info"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            StartRenaming(_selectedItem);
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e) =>
            DeleteSelectedItem();

       

        // ─── Вспомогательные методы ──────────────────────────────────────────────

        /// <summary>
        /// Получить путь к файлу для выбранного элемента дерева.
        /// Поддерживает ConfigFileNode и ModItemNode (IConfig / IGfx через родительский файл).
        /// </summary>
        private string GetSelectedItemPath()
        {
            if (_selectedItem == null) return null;

            if (_selectedItem.Tag is ConfigFileNode fileNode)
                return GetPathFromFile(fileNode.File);

            if (_selectedItem.Tag is ModItemNode modItem)
            {
                if (modItem.Item is IConfig config)
                    return config.FileFullPath;

                // IGfx — берём путь из родительского файла
                return GetPathFromFile(modItem.ParentFile);
            }

            return null;
        }

        private static string GetPathFromFile(object file)
        {
            if (file == null) return null;
            return file.GetType().GetProperty("FileFullPath")?.GetValue(file) as string;
        }

        private void RequestDelete(object node, string displayName)
        {
            var args = new DeleteItemRequestedEventArgs(DeleteItemRequestedEvent, node, displayName);
            RaiseEvent(args);

            // Если Presenter подтвердил — перезагружаем дерево
            if (args.Confirmed)
                LoadModData();
        }

        private void DeleteSelectedItem()
        {
            if (_selectedItem == null) return;

            if (_selectedItem.Tag is ModItemNode modItem)
                RequestDelete(modItem, modItem.DisplayName);
            else if (_selectedItem.Tag is ConfigFileNode fileNode)
                RequestDelete(fileNode, fileNode.DisplayName);
        }

        // ─── Переименование (inline) ─────────────────────────────────────────────

        private void StartRenaming(TreeViewItem item)
        {
            if (item?.Header is not StackPanel panel) return;

            // Ищем первый TextBlock после иконки
            TextBlock nameTextBlock = panel.Children.OfType<TextBlock>().Skip(1).FirstOrDefault();
            if (nameTextBlock == null) return;

            var currentName = item.Tag switch
            {
                ConfigFileNode f => f.DisplayName,
                ModItemNode m => m.DisplayName,
                _ => null
            };
            if (string.IsNullOrEmpty(currentName)) return;

            _renamingItem = item;
            _renameTextBox = new TextBox
            {
                Text = currentName,
                MinWidth = 100,
                VerticalAlignment = VerticalAlignment.Center,
                Background = (Brush)System.Windows.Application.Current.Resources["InputBackground"],
                Foreground = (Brush)System.Windows.Application.Current.Resources["InputText"],
                BorderBrush = (Brush)System.Windows.Application.Current.Resources["InputBorderFocus"],
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4, 2, 0, 0)
            };
            _renameTextBox.KeyDown += RenameTextBox_KeyDown;
            _renameTextBox.LostFocus += RenameTextBox_LostFocus;
            _renameTextBox.PreviewMouseRightButtonDown += (s, ev) => { ev.Handled = true; FinishRenaming(false); };

            var idx = panel.Children.IndexOf(nameTextBlock);
            panel.Children.RemoveAt(idx);
            panel.Children.Insert(idx, _renameTextBox);

            _renameTextBox.Focus();
            _renameTextBox.SelectAll();
        }

        private void RenameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { FinishRenaming(true); e.Handled = true; }
            else if (e.Key == Key.Escape) { FinishRenaming(false); e.Handled = true; }
        }

        private void RenameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_renameTextBox != null && !_renameTextBox.IsKeyboardFocusWithin)
                FinishRenaming(true);
        }

        private void FinishRenaming(bool apply)
        {
            if (_renameTextBox == null || _renamingItem == null) return;

            var newName = _renameTextBox.Text?.Trim();
            bool success = false;

            if (apply && !string.IsNullOrEmpty(newName))
            {
                var args = new RenameRequestedEventArgs(RenameRequestedEvent, _renamingItem.Tag, newName);
                RaiseEvent(args);
            }

            // Восстанавливаем TextBlock
            if (_renamingItem.Header is StackPanel panel)
            {
                var idx = panel.Children.IndexOf(_renameTextBox);
                if (idx >= 0)
                {
                    panel.Children.RemoveAt(idx);
                    var displayName = success ? newName : (_renamingItem.Tag switch
                    {
                        ConfigFileNode f => f.DisplayName,
                        ModItemNode m => m.DisplayName,
                        _ => newName
                    });
                    panel.Children.Insert(idx, CreateHighlightedTextBlock(displayName));
                }
            }

            _renameTextBox = null;
            _renamingItem = null;

            if (success) LoadModData();
        }
        // Добавить в FileExplorer.xaml.cs

        // ─── Методы для работы с выбором типа ────────────────────────────────────────

        /// <summary>
        /// Обработка нажатия кнопки "+" для категории
        /// </summary>
        private void HandleAddFileRequest(ModCategoryNode category)
        {
            // Получаем тип из generic аргумента ItemType (ConfigFile<T> или GfxFile<T>)
            var fileContainerType = category.ItemType;
            if (!fileContainerType.IsGenericType)
            {
                RaiseEvent(new AddFileRequestedEventArgs(AddFileRequestedEvent, category));
                return;
            }

            Type entityType = fileContainerType.GetGenericArguments()[0];

            // Проверяем, является ли T интерфейсом
            if (entityType.IsInterface())
            {
                var implementations = entityType.GetImplementationsForType();

                if (implementations.Count == 0)
                {
                    // Нет зарегистрированных реализаций - показываем ошибку
                    CustomMessageBox.Show(
                        StaticLocalisation.GetString("FileExplorer.NoImplementationsFound", entityType.Name),
                        StaticLocalisation.GetString("Dialog.Error"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.FileExplorer.NoImplementations", entityType.Name));
                    return;
                }

                if (implementations.Count == 1)
                {
                    // Только одна реализация - создаем сразу
                    RaiseEvent(new AddFileRequestedEventArgs(
                        AddFileRequestedEvent,
                        category,
                        implementations[0].Type));
                }
                else
                {
                    // Несколько реализаций - показываем меню выбора
                    ShowFileTypeSelectionMenu(category, implementations);
                }
            }
            else
            {
                // T - это обычный класс, создаем как раньше
                RaiseEvent(new AddFileRequestedEventArgs(AddFileRequestedEvent, category));
            }
        }

        /// <summary>
        /// Обработка нажатия кнопки "+" для файла (добавление сущности)
        /// </summary>
        private void HandleAddEntityRequest(ConfigFileNode fileNode)
        {
            var entityType = fileNode.ConfigType;

            // Проверяем, является ли T интерфейсом
            if (entityType.IsInterface())
            {
                var implementations = entityType.GetImplementationsForType();

                if (implementations.Count == 0)
                {
                    // Нет зарегистрированных реализаций - показываем ошибку
                    CustomMessageBox.Show(
                        StaticLocalisation.GetString("FileExplorer.NoImplementationsFound", entityType.Name),
                        StaticLocalisation.GetString("Dialog.Error"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Logger.AddDbgLog(StaticLocalisation.GetString("Log.FileExplorer.NoImplementations", entityType.Name));
                    return;
                }

                if (implementations.Count == 1)
                {
                    // Только одна реализация - создаем сразу
                    RaiseEvent(new AddEntityRequestedEventArgs(
                        AddEntityRequestedEvent,
                        fileNode,
                        implementations[0].Type));
                }
                else
                {
                    // Несколько реализаций - показываем меню выбора
                    ShowEntityTypeSelectionMenu(fileNode, implementations);
                }
            }
            else
            {
                // T - это обычный класс, создаем как раньше
                RaiseEvent(new AddEntityRequestedEventArgs(AddEntityRequestedEvent, fileNode));
            }
        }

        /// <summary>
        /// Показывает контекстное меню для выбора типа файла
        /// </summary>
        private void ShowFileTypeSelectionMenu(ModCategoryNode category, List<Application.TypeInfo> implementations)
        {
            var menu = new ContextMenu();

            foreach (var typeInfo in implementations)
            {
                var displayName = typeInfo.LocalizationKey != null
                    ? StaticLocalisation.GetString(typeInfo.LocalizationKey)
                    : typeInfo.DisplayName;

                var menuItem = new MenuItem
                {
                    Header = displayName,
                    Tag = typeInfo.Type
                };

                menuItem.Click += (s, e) =>
                {
                    var selectedType = (Type)((MenuItem)s).Tag;
                    RaiseEvent(new AddFileRequestedEventArgs(
                        AddFileRequestedEvent,
                        category,
                        selectedType));
                    Logger.AddDbgLog(StaticLocalisation.GetString(
                        "Log.FileExplorer.FileTypeSelected",
                        selectedType.Name,
                        category.DisplayName));
                };

                menu.Items.Add(menuItem);
            }

            menu.IsOpen = true;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
        }

        /// <summary>
        /// Показывает контекстное меню для выбора типа сущности
        /// </summary>
        private void ShowEntityTypeSelectionMenu(ConfigFileNode fileNode, List<Application.TypeInfo> implementations)
        {
            var menu = new ContextMenu();

            foreach (var typeInfo in implementations)
            {
                var displayName = typeInfo.LocalizationKey != null
                    ? StaticLocalisation.GetString(typeInfo.LocalizationKey)
                    : typeInfo.DisplayName;

                var menuItem = new MenuItem
                {
                    Header = displayName,
                    Tag = typeInfo.Type
                };

                menuItem.Click += (s, e) =>
                {
                    var selectedType = (Type)((MenuItem)s).Tag;
                    RaiseEvent(new AddEntityRequestedEventArgs(
                        AddEntityRequestedEvent,
                        fileNode,
                        selectedType));
                    Logger.AddDbgLog(StaticLocalisation.GetString(
                        "Log.FileExplorer.EntityTypeSelected",
                        selectedType.Name,
                        fileNode.DisplayName));
                };

                menu.Items.Add(menuItem);
            }

            menu.IsOpen = true;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
        }

        // ─── Обновляем существующие методы создания заголовков ───────────────────────

        private StackPanel CreateCategoryHeader(ModCategoryNode category)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            panel.Children.Add(new TextBlock
            {
                Text = "\uE8B7",
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0),
                Foreground = (Brush)System.Windows.Application.Current.Resources["FolderLayer1"]
            });
            panel.Children.Add(CreateHighlightedTextBlock(category.DisplayName));

            var addBtn = CreateHeaderButton("+", StaticLocalisation.GetString("Tooltip.AddFile"),
                _ => HandleAddFileRequest(category)); // Изменено!
            panel.Children.Add(addBtn);

            return panel;
        }

        private StackPanel CreateFileHeader(ConfigFileNode fileNode)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            panel.Children.Add(new TextBlock
            {
                Text = "\uE8A5",
                Style = (Style)FindResource("FileIconStyle"),
                Foreground = (Brush)System.Windows.Application.Current.Resources["Link"]
            });
            panel.Children.Add(CreateHighlightedTextBlock(fileNode.DisplayName));
            panel.Children.Add(new TextBlock
            {
                Text = $"<{GetShortTypeName(fileNode.ConfigType)}>",
                Style = (Style)FindResource("FileTypeStyle")
            });
            panel.Children.Add(new TextBlock
            {
                Text = $"({fileNode.EntityCount})",
                Style = (Style)FindResource("ItemCountStyle")
            });

            var addBtn = CreateHeaderButton("+", StaticLocalisation.GetString("Tooltip.AddEntity"),
                _ => HandleAddEntityRequest(fileNode)); // Изменено!
            addBtn.Margin = new Thickness(8, 0, 0, 0);
            panel.Children.Add(addBtn);

            var delBtn = CreateHeaderButton("−", StaticLocalisation.GetString("Tooltip.RemoveFile"),
                _ => RequestDelete(fileNode, fileNode.DisplayName));
            delBtn.Margin = new Thickness(4, 0, 0, 0);
            panel.Children.Add(delBtn);

            return panel;
        }

        // ─── Обновляем контекстное меню ──────────────────────────────────────────────

        private void AddFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem?.Tag is ModCategoryNode category)
                HandleAddFileRequest(category); // Изменено!
        }

        // ─── Поиск ───────────────────────────────────────────────────────────────

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text;
            var placeholder = StaticLocalisation.GetString("FileExplorer.SearchPlaceholder");

            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            if (string.IsNullOrEmpty(searchText) || searchText == placeholder)
            {
                _currentSearchText = "";
                _searchResults.Clear();
                _searchResultsDisplayed = 0;
                FileTreeView.Items.Clear();
                LoadModData();
                return;
            }

            _currentSearchText = searchText;
            if (searchText.Length < 2) return;

            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;

            await Task.Run(() => PerformSearch(searchText, token), token);
            if (token.IsCancellationRequested) return;

            DisplaySearchResultsVSStyle(0, SearchBatchSize);
        }

        private void PerformSearch(string searchText, CancellationToken token)
        {
            _searchResults.Clear();
            if (ModDataStorage.Mod == null) return;

            foreach (var category in GetModCategories(ModDataStorage.Mod))
            {
                if (token.IsCancellationRequested) return;
                SearchInCategory(category, searchText, token);
            }

            _searchResults = _searchResults
                .OrderBy(r => GetSearchRelevance(r.DisplayName, searchText))
                .ThenBy(r => r.Type)
                .ThenBy(r => r.DisplayName)
                .ToList();
        }

        private void SearchInCategory(ModCategoryNode category, string searchText, CancellationToken token)
        {
            if (category.Items == null) return;

            if (category.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
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

            foreach (var obj in category.Items)
            {
                if (token.IsCancellationRequested) return;
                if (obj == null || !TryExtractFileInfo(obj, out var configType, out var entities, out var fileName))
                    continue;

                if (fileName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    _searchResults.Add(new SearchResult
                    {
                        Path = $"File_{fileName}_{configType?.Name}",
                        DisplayName = fileName,
                        Item = obj,
                        Type = SearchResultType.File,
                        Category = category,
                        File = new ConfigFileNode
                        {
                            File = obj,
                            DisplayName = fileName,
                            ConfigType = configType,
                            EntityCount = entities?.Count ?? 0,
                            Entities = entities,
                            ParentProperty = category.PropertyInfo
                        }
                    });
                }

                if (entities != null)
                    SearchInEntities(entities, obj, fileName, configType, category, searchText, token);
            }
        }

        private void SearchInEntities(IList entities, object parentFile, string fileName,
            Type configType, ModCategoryNode category, string searchText, CancellationToken token)
        {
            var fileNode = new ConfigFileNode
            {
                File = parentFile,
                DisplayName = fileName,
                ConfigType = configType,
                EntityCount = entities.Count,
                Entities = entities,
                ParentProperty = category.PropertyInfo
            };

            foreach (var entity in entities)
            {
                if (token.IsCancellationRequested) return;
                if (entity == null) continue;

                var id = GetItemId(entity);
                if (!id.Contains(searchText, StringComparison.OrdinalIgnoreCase)) continue;

                _searchResults.Add(new SearchResult
                {
                    Path = $"Item_{id}_{fileName}",
                    DisplayName = id,
                    Item = entity,
                    Type = SearchResultType.Item,
                    Category = category,
                    File = fileNode,
                    ModItem = new ModItemNode { Item = entity, DisplayName = id, Id = id, ParentFile = parentFile }
                });
            }
        }

        private static int GetSearchRelevance(string text, string searchText)
        {
            if (string.IsNullOrEmpty(text)) return int.MaxValue;
            var t = text.ToLowerInvariant();
            var s = searchText.ToLowerInvariant();
            if (t == s) return 0;
            if (t.StartsWith(s)) return 1;
            if (t.Contains(" " + s) || t.Contains("_" + s)) return 2;
            var idx = t.IndexOf(s, StringComparison.Ordinal);
            return idx >= 0 ? 3 + idx : int.MaxValue;
        }

        private void DisplaySearchResultsVSStyle(int startIndex, int count)
        {
            Dispatcher.Invoke(() =>
            {
                if (startIndex == 0)
                {
                    FileTreeView.Items.Clear();
                    _searchResultsDisplayed = 0;
                }

                var grouped = _searchResults.Skip(startIndex).Take(count)
                    .GroupBy(r => r.Category);

                foreach (var categoryGroup in grouped)
                {
                    var categoryItem = FindOrCreateCategoryNode(categoryGroup.Key);

                    foreach (var fileGroup in categoryGroup.GroupBy(r => r.File?.DisplayName ?? ""))
                    {
                        var first = fileGroup.First();
                        if (first.Type == SearchResultType.Category) continue;

                        var fileItem = FindOrCreateFileNode(categoryItem, first.File);

                        foreach (var result in fileGroup.Where(r => r.Type == SearchResultType.Item))
                        {
                            if (fileItem.Items.Cast<TreeViewItem>()
                                .Any(i => i.Tag is ModItemNode m && m.Id == result.ModItem.Id))
                                continue;
                            fileItem.Items.Add(CreateModItemTreeViewItem(result.ModItem));
                        }

                        fileItem.IsExpanded = true;
                    }

                    categoryItem.IsExpanded = true;
                }

                _searchResultsDisplayed = Math.Min(startIndex + count, _searchResults.Count);

                if (_searchResultsDisplayed < _searchResults.Count)
                    AddLoadMoreButton();
            });
        }

        private TreeViewItem FindOrCreateCategoryNode(ModCategoryNode category)
        {
            foreach (TreeViewItem item in FileTreeView.Items)
                if (item.Tag is ModCategoryNode c && c.Name == category.Name) return item;

            var node = CreateCategoryTreeViewItem(category);
            node.Items.Clear();
            FileTreeView.Items.Add(node);
            return node;
        }

        private TreeViewItem FindOrCreateFileNode(TreeViewItem categoryItem, ConfigFileNode fileNode)
        {
            foreach (TreeViewItem item in categoryItem.Items)
            {
                if (item.Tag is ConfigFileNode f &&
                    f.DisplayName == fileNode.DisplayName &&
                    f.ConfigType == fileNode.ConfigType)
                {
                    if (IsLoadingItem(item)) item.Items.Clear();
                    return item;
                }
            }

            var node = CreateFileTreeViewItem(fileNode);
            node.Items.Clear();
            categoryItem.Items.Add(node);
            return node;
        }

        private void AddLoadMoreButton()
        {
            RemoveLoadMoreButton();

            var btn = new Button
            {
                Content = StaticLocalisation.GetString("FileExplorer.LoadMore",
                    _searchResults.Count - _searchResultsDisplayed),
                Style = (Style)FindResource("ButtonDarkSecondary"),
                Padding = new Thickness(12, 6, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.Click += (s, e) => { RemoveLoadMoreButton(); DisplaySearchResultsVSStyle(_searchResultsDisplayed, SearchBatchSize); };

            var loadMoreItem = new TreeViewItem
            {
                Header = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(20, 5, 0, 5),
                    Children = { btn }
                },
                Tag = "LoadMoreButton"
            };
            FileTreeView.Items.Add(loadMoreItem);
        }

        private void RemoveLoadMoreButton()
        {
            var toRemove = FileTreeView.Items.Cast<TreeViewItem>()
                .FirstOrDefault(i => i.Tag is string s && s == "LoadMoreButton");
            if (toRemove != null) FileTreeView.Items.Remove(toRemove);
        }
    }
}