using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public class TechTreeConfig : IConfig
    {
        public Identifier Id { get; set; }
        public string Orientation { get; set; }
        public List<TechTreeItemConfig> Items { get; set; } = new List<TechTreeItemConfig>();
        public List<List<string>> ChildOf { get; set; } = new List<List<string>>();
        public List<List<string>> Mutal { get; set; } = new List<List<string>>();
        public string Ledger { get; set; }
    }
}
