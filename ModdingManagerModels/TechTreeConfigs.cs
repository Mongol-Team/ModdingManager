using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public class TechTreeConfig : IConfig
    {
        public Identifier Id { get; set; }
        public string Orientation { get; set; }
        public List<TechTreeItemConfig> Items { get; set; } = new List<TechTreeItemConfig>();
        public string Ledger { get; set; }
    }
}
