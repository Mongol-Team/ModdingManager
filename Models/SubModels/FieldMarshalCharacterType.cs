using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels.SubModels
{
    public class FieldMarshalCharacterType : ICharacterType
    {
        public string Visible { get; set; }
        public string Available { get; set; }
        public string Allowed { get; set; }
        public Identifier Id { get; set; }
        public Types.LocalizationData.ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public int Skill { get; set; } = 2;
        public int Attack { get; set; } = 2;
        public int Defense { get; set; } = 2;
        public int Supply { get; set; } = 2;
        public int Planning { get; set; } = 2;
        public string Expire { get; set; }
        public List<CharacterTraitConfig> Traits { get; set; }
    }
}
