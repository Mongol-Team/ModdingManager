using Models.Configs;
using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.SubModels
{
    public class AdvisorCharacterType : ICharacterType
    {
        public string Visible { get; set; }
        public string Available { get; set; }
        public string Allowed { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IdeaGroupConfig AdvisorSlot { get; set; }
        public int AdvisorCost { get; set; } = 100;
        public IGfx Gfx { get; set; }
        public IdeaGroupConfig Slot { get; set; }
        public IdeaConfig Idea { get; set; }
        public IdeaLedgerType IdeaLedgerType { get; set; }
        public string Expire { get; set; }
        public List<CharacterTraitConfig> Traits { get; set; }
    }
}
