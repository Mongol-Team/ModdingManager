using Controls;
using Models.Args;
using System.Windows;
using System.Windows.Controls;

namespace View.Models
{
    public interface IStateWorkerView
    {
        string CurrentMapState { get; set; }
        Canvas Display { get; set; }
        Canvas CountryLayer { get; set; }
        Canvas StateLayer { get; set; }
        Canvas StrategicLayer { get; set; }
        Canvas ProvinceLayer { get; set; }
        Canvas ProvinceRenderLayer { get; set; }
        SceneViewer Scene { get; set; }
        Canvas ProvinceIDLayer { get; set; }
        Canvas StrategicRenderLayer { get; set; }
        Canvas StrategicIDLayer { get; set; }
        Canvas StateRenderLayer { get; set; }
        Canvas StateIDLayer { get; set; }
        Canvas CountryRenderLayer { get; set; }
        Canvas CountryIDLayer { get; set; }
        event RoutedEventHandler Loaded;
        event Action<bool, string> ShowIdsChanged;
        bool IsShowIdsChecked { get; }
        event Action<string> MapLayerChanged;
        event Action<ProvinceTransferArg> ProvinceTransferRequested;
        event Action<StateTransferArg> StateTransferRequested;
        event Action<string> MapChanged;
        event Action<string, int> SearchElement;
        event Action<MarkEventArg> MarkEvent;
        StackPanel Menu { get; set; }
    }
}
