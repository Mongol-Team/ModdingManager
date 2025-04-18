using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Text.Json.Serialization;
namespace ModdingManager.configs
{
    public class TechTreeConfig
    {
        public string Name { get; set; } // Имя древа технологий
        public string Orientation { get; set; } // Ориентация древа: "vertical" или "horizontal"
        public List<TechTreeItemConfig> Items { get; set; } = new List<TechTreeItemConfig>();
        public List<List<string>> ChildOf { get; set; } = new List<List<string>>(); // Связи типа "родитель-ребенок"
        public List<List<string>> Mutal { get; set; } = new List<List<string>>(); // Взаимные связи
    }

    public class TechTreeItemConfig
    {
        public string Id { get; set; }
        public string OldId { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public bool IsBig { get; set; }
        public int ModifCost { get; set; }
        public string LocName { get; set; }
        public string LocDescription { get; set; }
        public List<string> Categories { get; set; }
        public string Enable { get; set; }
        public string EnableType { get; set; }
        public string Cost { get; set; }
        public string Folder { get; set; }
        public string StartYear { get; set; }
        public string Allowed { get; set; }
        public string Modifiers { get; set; }
        public string Effects { get; set; }
        public string AiWillDo { get; set; }
        public string Dependencies { get; set; }
        [JsonIgnore]
        public ImageSource Image { get; set; }
        public byte[] ImageData { get; set; }
    }
}
