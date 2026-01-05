using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using View.Utils;
using Orientation = System.Windows.Controls.Orientation;
using TreeView = System.Windows.Controls.TreeView;
using TreeViewItem = System.Windows.Controls.TreeViewItem;
using MessageBox = System.Windows.MessageBox;
using Clipboard = System.Windows.Clipboard;
using FontFamily = System.Windows.Media.FontFamily;
using Brush = System.Windows.Media.Brush;

namespace ViewControls
{
    public partial class FileExplorer : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(FileExplorer),
                new PropertyMetadata("File Explorer", OnTitleChanged));

        public static readonly DependencyProperty RootPathProperty =
            DependencyProperty.Register(nameof(RootPath), typeof(string), typeof(FileExplorer),
                new PropertyMetadata(null, OnRootPathChanged));

        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register(nameof(SelectedPath), typeof(string), typeof(FileExplorer),
                new PropertyMetadata(null));

        public static readonly RoutedEvent PathSelectedEvent =
            EventManager.RegisterRoutedEvent(nameof(PathSelected), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(FileExplorer));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string RootPath
        {
            get => (string)GetValue(RootPathProperty);
            set => SetValue(RootPathProperty, value);
        }

        public string SelectedPath
        {
            get => (string)GetValue(SelectedPathProperty);
            private set => SetValue(SelectedPathProperty, value);
        }

        public event RoutedEventHandler PathSelected
        {
            add => AddHandler(PathSelectedEvent, value);
            remove => RemoveHandler(PathSelectedEvent, value);
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
            TitleTextBlock.Text = Title?.ToUpperInvariant() ?? "FILE EXPLORER";
        }

        private void SetupSearchPlaceholder()
        {
            var placeholderText = UILocalization.GetString("Message.SearchFiles");
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

        private static void OnRootPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileExplorer explorer)
            {
                explorer.LoadDirectory(e.NewValue as string);
            }
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileExplorer explorer)
            {
                explorer.UpdateTitle();
            }
        }

        public void LoadDirectory(string path)
        {
            FileTreeView.Items.Clear();

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return;
            }

            var rootItem = CreateTreeViewItem(path, Path.GetFileName(path) ?? path, true);
            FileTreeView.Items.Add(rootItem);
            rootItem.IsExpanded = true;
        }

        private TreeViewItem CreateTreeViewItem(string fullPath, string displayName, bool isDirectory)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader(displayName, isDirectory),
                Tag = fullPath,
                IsExpanded = false
            };

            if (isDirectory)
            {
                item.Items.Add(new TreeViewItem { Header = "Loading...", IsEnabled = false });
                item.Expanded += DirectoryItem_Expanded;
            }

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

        private void DirectoryItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item && item.Tag is string path)
            {
                if (item.Items.Count == 1 && item.Items[0] is TreeViewItem loadingItem && !loadingItem.IsEnabled)
                {
                    item.Items.Clear();
                    LoadDirectoryContents(item, path);
                }
            }
        }

        private void LoadDirectoryContents(TreeViewItem parentItem, string directoryPath)
        {
            try
            {
                var directories = Directory.GetDirectories(directoryPath)
                    .OrderBy(d => Path.GetFileName(d))
                    .ToList();

                var files = Directory.GetFiles(directoryPath)
                    .OrderBy(f => Path.GetFileName(f))
                    .ToList();

                foreach (var dir in directories)
                {
                    var dirName = Path.GetFileName(dir);
                    if (ShouldShowItem(dirName))
                    {
                        var dirItem = CreateTreeViewItem(dir, dirName, true);
                        parentItem.Items.Add(dirItem);
                    }
                }

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    if (ShouldShowItem(fileName))
                    {
                        var fileItem = CreateTreeViewItem(file, fileName, false);
                        parentItem.Items.Add(fileItem);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        private bool ShouldShowItem(string name)
        {
            var searchText = SearchTextBox.Text;
            var placeholderText = UILocalization.GetString("Message.SearchFiles");

            if (string.IsNullOrEmpty(searchText) || searchText == placeholderText)
                return true;

            return name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(RootPath))
                return;

            var searchText = SearchTextBox.Text;
            var placeholderText = UILocalization.GetString("Message.SearchFiles");

            if (string.IsNullOrEmpty(searchText) || searchText == placeholderText)
            {
                FileTreeView.Items.Clear();
                LoadDirectory(RootPath);
                return;
            }

            FileTreeView.Items.Clear();
            LoadDirectory(RootPath);
            ExpandAllMatchingItems(FileTreeView.Items, searchText);
        }

        private void ExpandAllMatchingItems(ItemCollection items, string searchText)
        {
            foreach (TreeViewItem item in items)
            {
                if (item.Tag is string path)
                {
                    var name = Path.GetFileName(path);
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

                    if (item.Items.Count > 0)
                    {
                        ExpandAllMatchingItems(item.Items, searchText);
                    }
                }
            }
        }

        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is string path)
            {
                _selectedItem = item;
                SelectedPath = path;
                RaiseEvent(new RoutedEventArgs(PathSelectedEvent));
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

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedPath))
            {
                if (File.Exists(SelectedPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = SelectedPath,
                        UseShellExecute = true
                    });
                }
            }
        }

        private void OpenInExplorerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedPath))
            {
                var path = File.Exists(SelectedPath) ? SelectedPath : Path.GetDirectoryName(SelectedPath);
                if (!string.IsNullOrEmpty(path))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
                }
            }
        }

        private void CopyPathMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedPath))
            {
                Clipboard.SetText(SelectedPath);
            }
        }

        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedPath))
            {
                var result = MessageBox.Show(
                    string.Format(UILocalization.GetString("Message.ConfirmDelete"), Path.GetFileName(SelectedPath)),
                    UILocalization.GetString("Message.ConfirmDeleteTitle"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (File.Exists(SelectedPath))
                        {
                            File.Delete(SelectedPath);
                        }
                        else if (Directory.Exists(SelectedPath))
                        {
                            Directory.Delete(SelectedPath, true);
                        }

                        if (_selectedItem?.Parent is TreeViewItem parent)
                        {
                            parent.Items.Remove(_selectedItem);
                        }
                        else if (_selectedItem != null)
                        {
                            FileTreeView.Items.Remove(_selectedItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            string.Format(UILocalization.GetString("Error.DeleteFailed"), ex.Message),
                            UILocalization.GetString("Error.Error"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}

