using ModdingManagerModels.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;
namespace ModdingManagerModels
{
    public class TechTreeItemConfig : IConfig
    {
        public Identifier Id { get; set; }
        public string OldId { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public bool IsBig { get; set; }
        public int ModifCost { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LocName { get; set; }
        public string LocDescription { get; set; }
        public string Categories { get; set; }
        public List<string> Enables { get; set; }
        public int Cost { get; set; }
        public int StartYear { get; set; }
        public TechTreeConfig ChildOf { get; set; } = new ();
        public List<TechTreeConfig> Mutal { get; set; } = new List<TechTreeConfig>();
        public List<string> Allowed { get; set; }
        public List<string> AllowBranch { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
        public List<string> Effects { get; set; }
        public string AiWillDo { get; set; }
        public List<string> Dependencies { get; set; }
        [JsonIgnore]
        public Bitmap Image { get; set; }
    }
}
