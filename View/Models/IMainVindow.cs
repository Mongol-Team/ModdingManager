using Controls;
using Controls.Docking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace View.Models
{
    /// <summary>
    /// Контракт View для MainWindow. Presenter работает только через этот интерфейс.
    /// </summary>
    public interface IMainWindow
    {
        /// <summary>Добавить кнопку в топбар</summary>
        void AddTopbarButton(System.Windows.Controls.Button button, PanelSide side);

        /// <summary>Найти панель в DockManager по заголовку</summary>
        DockPanelInfo FindPanelWithTitle(string title);

        /// <summary>Добавить панель в DockManager</summary>
        void AddDockPanel(DockPanelInfo panel, DockSide side);

        /// <summary>Установить контент существующей панели</summary>
        void SetPanelContent(DockPanelInfo panel, object content);

        /// <summary>Открыть контент в основной зоне DockManager (центральная область)</summary>
        void OpenInDockZone(System.Windows.UIElement content);

        /// <summary>Получить все панели DockManager</summary>
        System.Collections.Generic.IEnumerable<DockPanelInfo> GetAllDockPanels();

        /// <summary>Сериализовать/десериализовать layout через LayoutSerializer</summary>
        void LoadLayout(string layoutPath);
        void SaveLayout(string layoutPath);

    }
}
