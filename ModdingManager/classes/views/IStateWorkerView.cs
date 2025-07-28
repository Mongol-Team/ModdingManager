using ModdingManager.classes.args;
using ModdingManager.classes.configs;
using ModdingManager.classes.controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Zoombox;
using Control = System.Windows.Controls.Control;

namespace ModdingManager.classes.views
{
    public interface IStateWorkerView
    {
        public string CurrentMapState { get; set; }
        public Canvas Display { get; set; }
        public Canvas CountryLayer { get; set; }
        public Canvas StateLayer { get; set; }
        public Canvas StrategicLayer { get; set; }
        public Canvas ProvinceLayer { get; set; }
        public Canvas ProvinceRenderLayer { get; set; }
        public SceneViewer Scene { get; set; }
        public Canvas ProvinceIDLayer { get; set; }
        public Canvas StrategicRenderLayer { get; set; }
        public Canvas StrategicIDLayer { get; set; }
        public Canvas StateRenderLayer { get; set; }
        public Canvas StateIDLayer { get; set; }
        public Canvas CountryRenderLayer { get; set; }
        public Canvas CountryIDLayer { get; set; }
        public event RoutedEventHandler Loaded;
        public event Action<bool, string> ShowIdsChanged;
        public bool IsShowIdsChecked { get; }
        public event Action<string> MapLayerChanged;
        event Action<ProvinceTransferArg> ProvinceTransferRequested;
        event Action<StateTransferArg> StateTransferRequested;
        public event Action<string> MapChanged;
        public event Action<string, int> SearchElement;
        public event Action<MarkEventArg> MarkEvent;
        public StackPanel Menu { get; set; }
    }
}
