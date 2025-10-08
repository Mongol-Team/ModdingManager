using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public class TechTreeConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public string Orientation { get; set; }
        public List<TechTreeItemConfig> Items { get; set; } = new List<TechTreeItemConfig>();
        public string Ledger { get; set; }
    }
}
