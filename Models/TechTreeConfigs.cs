using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;

namespace Models
{
    public class TechTreeConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; } = new ConfigLocalisation();
        public TechTreeOrientationType Orientation { get; set; }
        public string Available { get; set; }
        public List<TechTreeItemConfig> Items { get; set; } = new List<TechTreeItemConfig>();
        public TechTreeLedgerType Ledger { get; set; }
    }
}
