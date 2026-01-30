using Application;
using Models.Attributes;
using Models.Configs;
using Models.Interfaces;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using View.Utils;
using Brush = System.Windows.Media.Brush;
using Clipboard = System.Windows.Clipboard;
using FontFamily = System.Windows.Media.FontFamily;
using Orientation = System.Windows.Controls.Orientation;
using TreeView = System.Windows.Controls.TreeView;
using TreeViewItem = System.Windows.Controls.TreeViewItem;

namespace ViewControls
{
    public class ModCategoryNode
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public IList Items { get; set; }
        public Type ItemType { get; set; }
    }

    public class ModItemNode
    {
        public object Item { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
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

        public FileExplorer()
        {
            InitializeComponent();
            InitializeContextMenu();
            LoadContextMenuLocalization();
            SetupSearchPlaceholder();
            UpdateTitle();
            LoadModData();
        }

        private void InitializeContextMenu()
        {
            _fileContextMenu = new ContextMenu();

            var openMenuItem = new MenuItem { Name = "OpenMenuItem" };
            openMenuItem.Click += OpenMenuItem_Click;
            _fileContextMenu.Items.Add(openMenuItem);

            var openInExplorerMenuItem = new MenuItem { Name = "OpenInExplorerMenuItem" };
            openInExplorerMenuItem.Click += OpenInExplorerMenuItem_Click;
            _fileContextMenu.Items.Add(openInExplorerMenuItem);

            _fileContextMenu.Items.Add(new Separator());

            var copyPathMenuItem = new MenuItem { Name = "CopyPathMenuItem" };
            copyPathMenuItem.Click += CopyPathMenuItem_Click;
            _fileContextMenu.Items.Add(copyPathMenuItem);

            var renameMenuItem = new MenuItem { Name = "RenameMenuItem" };
            renameMenuItem.Click += RenameMenuItem_Click;
            _fileContextMenu.Items.Add(renameMenuItem);

            var deleteMenuItem = new MenuItem { Name = "DeleteMenuItem" };
            deleteMenuItem.Click += DeleteMenuItem_Click;
            _fileContextMenu.Items.Add(deleteMenuItem);
        }

        private void UpdateTitle()
        {
            //TitleTextBlock.Text = Title?.ToUpperInvariant() ?? "FILE EXPLORER";
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
                    openMenuItem.Header = UILocalization.GetString("Menu.Open");
                if (_fileContextMenu.Items[1] is MenuItem openInExplorerMenuItem)
                    openInExplorerMenuItem.Header = UILocalization.GetString("Menu.OpenInExplorer");
                if (_fileContextMenu.Items[3] is MenuItem copyPathMenuItem)
                    copyPathMenuItem.Header = UILocalization.GetString("Menu.CopyFullPath");
                if (_fileContextMenu.Items[4] is MenuItem renameMenuItem)
                    renameMenuItem.Header = UILocalization.GetString("Menu.Rename");
                if (_fileContextMenu.Items[5] is MenuItem deleteMenuItem)
                    deleteMenuItem.Header = UILocalization.GetString("Menu.Delete");
            }
        }


        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileExplorer explorer)
            {
                explorer.UpdateTitle();
            }
        }

        public void LoadModData()
        {
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
        }

        private List<ModCategoryNode> GetModCategories(ModConfig modConfig)
        {
            var categories = new List<ModCategoryNode>();

            // Получаем все типы конфигов из сборки Models
            var configTypes = GetConfigTypesFromAssembly();
            var modType = typeof(ModConfig);
            var properties = modType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Создаем словарь для быстрого поиска свойств по типу элемента
            var propertyMap = new Dictionary<Type, PropertyInfo>();
            foreach (var prop in properties)
            {
                if (prop.PropertyType.IsGenericType)
                {
                    var genericDef = prop.PropertyType.GetGenericTypeDefinition();
                    if (genericDef == typeof(List<>) || genericDef == typeof(ObservableCollection<>))
                    {
                        var itemType = prop.PropertyType.GetGenericArguments()[0];
                        propertyMap[itemType] = prop;
                    }
                }
                else if (prop.PropertyType == typeof(MapConfig))
                {
                    propertyMap[typeof(MapConfig)] = prop;
                }
            }

            // Обрабатываем только те конфиги, которые есть в списке типов
            foreach (var configType in configTypes)
            {
                if (propertyMap.TryGetValue(configType, out var prop))
                {
                    if (prop.PropertyType.IsGenericType)
                    {
                        var genericDef = prop.PropertyType.GetGenericTypeDefinition();
                        if (genericDef == typeof(List<>) || genericDef == typeof(ObservableCollection<>))
                        {
                            if (prop.GetValue(modConfig) is IList value && value.Count > 0)
                            {
                                categories.Add(new ModCategoryNode
                                {
                                    Name = prop.Name,
                                    DisplayName = FormatCategoryName(prop.Name),
                                    Items = value,
                                    ItemType = configType
                                });
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
                                ItemType = typeof(MapConfig)
                            };
                            categories.Add(mapCategory);
                        }
                    }
                }
            }

            return categories.OrderBy(c => c.DisplayName).ToList();
        }

        private List<Type> GetConfigTypesFromAssembly()
        {
            var configTypes = new List<Type>();

            foreach (var prop in typeof(ModConfig).GetProperties())
            {
                var type = prop.PropertyType;

                // Проверяем, является ли тип открытым generic перед вызовом
                if (type.IsGenericType)
                {
                    try
                    {
                        var genericDef = type.GetGenericTypeDefinition();
                        if (genericDef == typeof(List<>) || genericDef == typeof(ObservableCollection<>))
                        {
                            var elementType = type.GetGenericArguments()[0];
                            if (typeof(IGfx).IsAssignableFrom(elementType) ||
                                typeof(IConfig).IsAssignableFrom(elementType))
                            {
                                configTypes.Add(elementType);
                            }
                        }
                    }
                    catch
                    {
                        // игнорируем, если тип не generic
                    }
                }
                else
                {
                    // не-generic типы
                    if (typeof(IGfx).IsAssignableFrom(type) ||
                        typeof(IConfig).IsAssignableFrom(type))
                    {
                        configTypes.Add(type);
                    }
                }
            }

            return configTypes.Distinct().ToList(); // на всякий случай убираем дубли
        }

        private string FormatCategoryName(string name)
        {
            // Преобразуем "IdeaSlots" в "Idea Slots" и т.д.
            var result = System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim();
            return result;
        }

        private TreeViewItem CreateCategoryTreeViewItem(ModCategoryNode category)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader(category.DisplayName, true),
                Tag = category,
                IsExpanded = false
            };

            item.Items.Add(new TreeViewItem { Header = "Loading...", IsEnabled = false });
            item.Expanded += CategoryItem_Expanded;

            return item;
        }

        private TreeViewItem CreateModItemTreeViewItem(ModItemNode modItem)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader(modItem.DisplayName, false),
                Tag = modItem,
                IsExpanded = false
            };

            return item;
        }

        private StackPanel CreateHeader(string name, bool isDirectory)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var icon = new TextBlock
            {
                Text = isDirectory ? "\uE8B7" : GetFileIcon(name),
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0),
                Foreground = isDirectory
                    ? (Brush)System.Windows.Application.Current.Resources["FolderLayer1"]
                    : (Brush)System.Windows.Application.Current.Resources["TextTertiary"]
            };

            var textBlock = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)System.Windows.Application.Current.Resources["TextPrimary"]
            };

            panel.Children.Add(icon);
            panel.Children.Add(textBlock);

            return panel;
        }

        private string GetFileIcon(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".cs" => "\uE7C3",
                ".xaml" => "\uE7C3",
                ".csproj" => "\uE7C3",
                ".json" => "\uE7C3",
                ".xml" => "\uE7C3",
                ".txt" => "\uE8A5",
                ".png" => "\uEB9F",
                ".jpg" => "\uEB9F",
                ".jpeg" => "\uEB9F",
                ".bmp" => "\uEB9F",
                ".gif" => "\uEB9F",
                ".cfg" => "\uE7C3",
                ".loc" => "\uE7C3",
                _ => "\uE8A5"
            };
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

        private void LoadCategoryContents(TreeViewItem parentItem, ModCategoryNode category)
        {
            try
            {
                if (category.Items == null)
                    return;

                // Особый случай для MapConfig - показываем вложенные списки
                if (category.ItemType == typeof(MapConfig) && category.Items.Count > 0 && category.Items[0] is MapConfig map)
                {
                    LoadMapContents(parentItem, map);
                    return;
                }

                var items = new List<ModItemNode>();

                foreach (var obj in category.Items)
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
                            Id = id
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
            }
        }

        private void LoadMapContents(TreeViewItem parentItem, MapConfig map)
        {
            // Provinces
            if (map.Provinces != null && map.Provinces.Count > 0)
            {
                var provincesCategory = CreateCategoryTreeViewItem(new ModCategoryNode
                {
                    Name = "Provinces",
                    DisplayName = "Provinces",
                    Items = map.Provinces.Cast<object>().ToList(),
                    ItemType = typeof(ProvinceConfig)
                });
                parentItem.Items.Add(provincesCategory);
            }

            // States
            if (map.States != null && map.States.Count > 0)
            {
                var statesCategory = CreateCategoryTreeViewItem(new ModCategoryNode
                {
                    Name = "States",
                    DisplayName = "States",
                    Items = map.States.Cast<object>().ToList(),
                    ItemType = typeof(StateConfig)
                });
                parentItem.Items.Add(statesCategory);
            }

            // StrategicRegions
            if (map.StrategicRegions != null && map.StrategicRegions.Count > 0)
            {
                var regionsCategory = CreateCategoryTreeViewItem(new ModCategoryNode
                {
                    Name = "StrategicRegions",
                    DisplayName = "Strategic Regions",
                    Items = map.StrategicRegions.Cast<object>().ToList(),
                    ItemType = typeof(StrategicRegionConfig)
                });
                parentItem.Items.Add(regionsCategory);
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
            else if (item is MapConfig map)
            {
                return "Map";
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

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text;
            var placeholderText = "Поиск объектов мода...";

            if (string.IsNullOrEmpty(searchText) || searchText == placeholderText)
            {
                FileTreeView.Items.Clear();
                LoadModData();
                return;
            }

            FileTreeView.Items.Clear();
            LoadModData();
            ExpandAllMatchingItems(FileTreeView.Items, searchText);
        }

        private void ExpandAllMatchingItems(ItemCollection items, string searchText)
        {
            foreach (TreeViewItem item in items)
            {
                if (item.Tag is ModCategoryNode category)
                {
                    var name = category.DisplayName;
                    if (name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        item.IsExpanded = true;
                    }

                    if (item.Items.Count > 0)
                    {
                        ExpandAllMatchingItems(item.Items, searchText);
                    }
                }
                else if (item.Tag is ModItemNode modItem)
                {
                    var name = modItem.DisplayName;
                    if (name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        item.IsExpanded = true;
                        var parent = item.Parent as TreeViewItem;
                        while (parent != null)
                        {
                            parent.IsExpanded = true;
                            parent = parent.Parent as TreeViewItem;
                        }
                    }
                }
            }
        }

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
                else if (item.Tag is ModCategoryNode category)
                {
                    SelectedItem = category;
                    RaiseEvent(new RoutedEventArgs(ItemSelectedEvent));
                }
            }
        }

        private void FileTreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                var hit = treeView.InputHitTest(e.GetPosition(treeView)) as DependencyObject;
                var item = FindAncestor<TreeViewItem>(hit);
                if (item != null)
                {
                    item.IsSelected = true;
                    item.ContextMenu = _fileContextMenu;
                }
            }
        }

        private void FileTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var item = FindAncestor<TreeViewItem>(source);
                if (item != null && item.Tag is ModItemNode modItem)
                {
                    OpenCreatorForItem(modItem.Item);
                }
            }
        }

        private void FileTreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is TreeView treeView && treeView.SelectedItem is TreeViewItem selectedItem)
            {
                selectedItem.ContextMenu = _fileContextMenu;
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                    return ancestor;
                current = VisualTreeHelper.GetParent(current);
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
                        var mainWindow = System.Windows.Application.Current.MainWindow as View.MainWindow;
                        if (mainWindow?.DockManager != null)
                        {
                            mainWindow.DockManager.SetContent(viewer);
                        }
                        return;

                    case ConfigCreatorType.CountryCreator:
                        CustomMessageBox.Show(
                            "CountryCreator пока не реализован",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;

                    case ConfigCreatorType.MapCreator:
                        CustomMessageBox.Show(
                            "MapCreator пока не реализован",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;

                    case ConfigCreatorType.GenericGuiCreator:
                        CustomMessageBox.Show(
                            $"GenericGuiCreator для типа {itemType.Name} пока не реализован",
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

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Можно добавить логику открытия объекта мода
        }

        private void OpenInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Можно добавить логику открытия файла объекта в проводнике
        }

        private void CopyPathMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null)
            {
                var id = GetItemId(SelectedItem);
                Clipboard.SetText(id);
            }
        }

        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Можно добавить логику переименования объекта
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Можно добавить логику удаления объекта из мода
        }
    }
}


