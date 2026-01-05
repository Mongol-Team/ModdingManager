using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Configs
{
    public class IdeaTagConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public List<IdeaGroupConfig> Slots { get; set; } = new List<IdeaGroupConfig>();
        public List<string> CharacterSlots { get; set; } = new List<string>();
        public int Cost { get; set; }
        public int RemovalCost { get; set; }
        public IdeaLedgerType Ledger { get; set; }
        public IdeaType Type { get; set; }
        public bool Hidden { get; set; }
        public bool PoliticsTab { get; set; }
    }
}
