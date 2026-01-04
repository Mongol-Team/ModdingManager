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
    public class NavalLeaderCharacterType : ICharacterType
    {
        public string Visible { get; set; }
        public string Available { get; set; }
        public string Allowed { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public int Skill { get; set; } = 1;
        public int Attack { get; set; } = 1;
        public int Defense { get; set; } = 1;
        public int Maneuver { get; set; } = 1;
        public int Coordination { get; set; } = 1;
        public string Expire { get; set; }
        public List<CharacterTraitConfig> Traits { get; set; }
    }
}
