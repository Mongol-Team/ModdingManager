using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Controls.Args
{
    // ─── Добавление файла в категорию ───────────────────────────────────────────

    public class AddFileRequestedEventArgs : RoutedEventArgs
    {
        public ModCategoryNode Category { get; }

        public AddFileRequestedEventArgs(RoutedEvent routedEvent, ModCategoryNode category)
            : base(routedEvent)
        {
            Category = category;
        }
    }

    // ─── Добавление entity в файл ────────────────────────────────────────────────

    public class AddEntityRequestedEventArgs : RoutedEventArgs
    {
        public ConfigFileNode FileNode { get; }

        public AddEntityRequestedEventArgs(RoutedEvent routedEvent, ConfigFileNode fileNode)
            : base(routedEvent)
        {
            FileNode = fileNode;
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
}