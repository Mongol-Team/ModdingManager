using Models.Attributes;
using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;

namespace Models.Configs
{
    [ConfigCreator(ConfigCreatorType.GenericGuiCreator)]
    public class TechTreeConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; } = new ConfigLocalisation();
        public TechTreeOrientationType Orientation { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public string Available { get; set; }
        public List<TechTreeItemConfig> Items { get; set; } = new List<TechTreeItemConfig>();
        public TechTreeLedgerType Ledger { get; set; }
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
