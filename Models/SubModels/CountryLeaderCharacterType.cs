using Models.Configs;
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
    public class CountryLeaderCharacterType : ICharacterType
    {
        public List<CharacterTraitConfig> Traits { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public string Visible { get; set; }
        public string Available { get; set; }
        public string Allowed { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public IdeologyType Ideology { get; set; }
        public string Expire { get; set; }
    }
}
