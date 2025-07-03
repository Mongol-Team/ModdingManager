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
        public string Name { get; set; } 
        public string Orientation { get; set; }
        public List<TechTreeItemConfig> Items { get; set; } = new List<TechTreeItemConfig>();
        public List<List<string>> ChildOf { get; set; } = new List<List<string>>(); 
        public List<List<string>> Mutal { get; set; } = new List<List<string>>(); 
        public string Ledger { get; set; }
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
        public string Categories { get; set; }
        public List<string> Enables { get; set; }
        public int Cost { get; set; }
        public int StartYear { get; set; }
        public List<string> Allowed { get; set; }
        public List<string> Modifiers { get; set; }
        public List<string> Effects { get; set; }
        public string AiWillDo { get; set; }
        public List<string> Dependencies { get; set; }
        [JsonIgnore]
        public ImageSource Image { get; set; }
        public byte[] ImageData { get; set; }
    }
}
