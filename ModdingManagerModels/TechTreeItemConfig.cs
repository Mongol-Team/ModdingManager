using System.Drawing;
using System.Text.Json.Serialization;
namespace ModdingManagerModels
{
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
        public Bitmap Image { get; set; }
        public byte[] ImageData { get; set; }
    }
}
