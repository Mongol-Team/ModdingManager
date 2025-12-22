using ModdingManagerModels.Enums;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels.SubModels
{
    public class AdvisorCharacterType : ICharacterType
    {
        public string Visible { get; set; }
        public string Available { get; set; }
        public string Allowed { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IdeaSlotConfig AdvisorSlot { get; set; }
        public int AdvisorCost { get; set; } = 100;
        public IGfx Gfx { get; set; }
        public IdeaSlotConfig Slot { get; set; }
        public IdeaConfig Idea { get; set; }
        public IdeaLedgerType IdeaLedgerType { get; set; }
        public string Expire { get; set; }
        public List<CharacterTraitConfig> Traits { get; set; }
    }
}
