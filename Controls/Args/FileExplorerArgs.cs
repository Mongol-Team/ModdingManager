// Controls/Args/FileExplorerEventArgs.cs
using System;
using System.Windows;

namespace Controls.Args
{
    // ─── Добавление файла в категорию ───────────────────────────────────────────

    public class AddFileRequestedEventArgs : RoutedEventArgs
    {
        public ModCategoryNode Category { get; }
        public Type SpecificType { get; }  // Новое свойство

        public AddFileRequestedEventArgs(
            RoutedEvent routedEvent,
            ModCategoryNode category,
            Type specificType = null)  // Опциональный параметр
            : base(routedEvent)
        {
            Category = category;
            SpecificType = specificType;
        }
    }

    // ─── Добавление entity в файл ────────────────────────────────────────────────

    public class AddEntityRequestedEventArgs : RoutedEventArgs
    {
        public ConfigFileNode FileNode { get; }
        public Type SpecificType { get; }  // Новое свойство

        public AddEntityRequestedEventArgs(
            RoutedEvent routedEvent,
            ConfigFileNode fileNode,
            Type specificType = null)  // Опциональный параметр
            : base(routedEvent)
        {
            FileNode = fileNode;
            SpecificType = specificType;
        }
    }

    // ─── Удаление элемента (файла или entity) ───────────────────────────────────

    public class DeleteItemRequestedEventArgs : RoutedEventArgs
    {
        public object Item { get; }          // ConfigFileNode или ModItemNode
        public string DisplayName { get; }

        /// <summary>
        /// Presenter выставляет Confirmed = true чтобы разрешить удаление.
        /// </summary>
        public bool Confirmed { get; set; }

        public DeleteItemRequestedEventArgs(RoutedEvent routedEvent, object item, string displayName)
            : base(routedEvent)
        {
            Item = item;
            DisplayName = displayName;
        }
    }

    // ─── Перемещение файла между категориями ────────────────────────────────────

    public class MoveFileRequestedEventArgs : RoutedEventArgs
    {
        public object SourceFile { get; }
        public ModCategoryNode TargetCategory { get; }

        public MoveFileRequestedEventArgs(RoutedEvent routedEvent, object sourceFile, ModCategoryNode targetCategory)
            : base(routedEvent)
        {
            SourceFile = sourceFile;
            TargetCategory = targetCategory;
        }
    }

    // ─── Перемещение entity между файлами ───────────────────────────────────────

    public class MoveEntityRequestedEventArgs : RoutedEventArgs
    {
        public object SourceItem { get; }
        public object SourceFile { get; }
        public object TargetFile { get; }

        public MoveEntityRequestedEventArgs(RoutedEvent routedEvent,
            object sourceItem, object sourceFile, object targetFile)
            : base(routedEvent)
        {
            SourceItem = sourceItem;
            SourceFile = sourceFile;
            TargetFile = targetFile;
        }
    }

    // ─── Переименование ─────────────────────────────────────────────────────────

    public class RenameRequestedEventArgs : RoutedEventArgs
    {
        public object Item { get; }          // ConfigFileNode или ModItemNode
        public string NewName { get; }

        /// <summary>
        /// Presenter выставляет Success = true если переименование применено.
        /// </summary>
        public bool Success { get; set; }

        public RenameRequestedEventArgs(RoutedEvent routedEvent, object item, string newName)
            : base(routedEvent)
        {
            Item = item;
            NewName = newName;
        }
    }

    // ─── Открытие элемента ──────────────────────────────────────────────────────

    public class OpenItemRequestedEventArgs : RoutedEventArgs
    {
        public object Item { get; }

        public OpenItemRequestedEventArgs(RoutedEvent routedEvent, object item)
            : base(routedEvent)
        {
            Item = item;
        }
    }
}