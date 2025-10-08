using ModdingManagerModels.Enums;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class IdeaTagConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public List<IdeaSlotConfig> Slots { get; set; } = new List<IdeaSlotConfig>();
        public List<string> CharacterSlots { get; set; } = new List<string>();
        public int Cost { get; set; }
        public int RemovalCost { get; set; }
        public IdeaLedgerType Ledger { get; set; }
        public IdeaType Type { get; set; }
        public bool Hidden { get; set; }
        public bool PoliticsTab { get; set; }
    }
}
