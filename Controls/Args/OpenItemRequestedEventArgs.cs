using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Controls.Args
{
    /// <summary>
    /// Аргументы события запроса открытия элемента мода в рабочей зоне.
    /// FileExplorer только сигнализирует — решение принимает Presenter.
    /// </summary>
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
