using Application.utils;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace Controls.Docking
{
    public class LayoutSerializer
    {
        public class LayoutInfo
        {
            public List<SerializedPanelInfo> LeftPanels { get; set; } = new List<SerializedPanelInfo>();
            public List<SerializedPanelInfo> RightPanels { get; set; } = new List<SerializedPanelInfo>();
            public List<SerializedPanelInfo> TopPanels { get; set; } = new List<SerializedPanelInfo>();
            public List<SerializedPanelInfo> BottomPanels { get; set; } = new List<SerializedPanelInfo>();
            public string CenterPageType { get; set; }
            public Dictionary<string, double> SplitterPositions { get; set; } = new Dictionary<string, double>();
        }

        public class SerializedPanelInfo
        {
            public string Type { get; set; }
            public string Title { get; set; }
            public bool IsPinned { get; set; }
            public bool IsVisible { get; set; }
        }

        public static LayoutInfo Serialize(DockManager manager)
        {
            var layout = new LayoutInfo();

            var leftZone = manager.GetZone(DockSide.Left);
            if (leftZone != null)
            {
                layout.LeftPanels = leftZone.Panels.Select(p => new SerializedPanelInfo
                {
                    Type = p.Content?.GetType().Name ?? "Unknown",
                    Title = p.Title,
                    IsPinned = p.IsPinned,
                    IsVisible = p.Content?.Visibility == Visibility.Visible
                }).ToList();
            }

            var rightZone = manager.GetZone(DockSide.Right);
            if (rightZone != null)
            {
                layout.RightPanels = rightZone.Panels.Select(p => new SerializedPanelInfo
                {
                    Type = p.Content?.GetType().Name ?? "Unknown",
                    Title = p.Title,
                    IsPinned = p.IsPinned,
                    IsVisible = p.Content?.Visibility == Visibility.Visible
                }).ToList();
            }

            var topZone = manager.GetZone(DockSide.Top);
            if (topZone != null)
            {
                layout.TopPanels = topZone.Panels.Select(p => new SerializedPanelInfo
                {
                    Type = p.Content?.GetType().Name ?? "Unknown",
                    Title = p.Title,
                    IsPinned = p.IsPinned,
                    IsVisible = p.Content?.Visibility == Visibility.Visible
                }).ToList();
            }

            var bottomZone = manager.GetZone(DockSide.Bottom);
            if (bottomZone != null)
            {
                layout.BottomPanels = bottomZone.Panels.Select(p => new SerializedPanelInfo
                {
                    Type = p.Content?.GetType().Name ?? "Unknown",
                    Title = p.Title,
                    IsPinned = p.IsPinned,
                    IsVisible = p.Content?.Visibility == Visibility.Visible
                }).ToList();
            }

            return layout;
        }

        public static void Deserialize(DockManager manager, LayoutInfo layout)
        {
            if (layout == null) return;

            foreach (var panelInfo in layout.LeftPanels)
            {
                var panel = CreatePanelFromInfo(panelInfo);
                if (panel != null)
                {
                    manager.AddPanel(panel, DockSide.Left);
                }
            }

            foreach (var panelInfo in layout.RightPanels)
            {
                var panel = CreatePanelFromInfo(panelInfo);
                if (panel != null)
                {
                    manager.AddPanel(panel, DockSide.Right);
                }
            }

            foreach (var panelInfo in layout.TopPanels)
            {
                var panel = CreatePanelFromInfo(panelInfo);
                if (panel != null)
                {
                    manager.AddPanel(panel, DockSide.Top);
                }
            }

            foreach (var panelInfo in layout.BottomPanels)
            {
                var panel = CreatePanelFromInfo(panelInfo);
                if (panel != null)
                {
                    manager.AddPanel(panel, DockSide.Bottom);
                }
            }
        }

        private static DockPanelInfo CreatePanelFromInfo(SerializedPanelInfo info)
        {
            UIElement content = null;

            var solutionExplorerTitle = StaticLocalisation.GetString("Window.EntityExplorer");
            if (info.Title == solutionExplorerTitle || info.Title == "Обозреватель решений" || info.Title.Contains("Solution Explorer"))
            {
                var fileExplorer = new FileExplorer
                {
                    Title = solutionExplorerTitle
                };
                fileExplorer.LoadModData();
                content = fileExplorer;
            }

            return new DockPanelInfo
            {
                Title = info.Title,
                Content = content,
                IsPinned = info.IsPinned,
                CanClose = true,
                CanPin = true
            };
        }

        public static void SaveToFile(LayoutInfo layout, string path)
        {
            try
            {
                var json = JsonConvert.SerializeObject(layout, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving layout: {ex.Message}");
            }
        }

        public static LayoutInfo LoadFromFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return null;

                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<LayoutInfo>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading layout: {ex.Message}");
                return null;
            }
        }
    }
}

