using ModdingManagerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels.SubModels
{
    public interface ICharacterType : IConfig
    {
        public List<CharacterTraitConfig> Traits { get; set; }
        public string Visible { get; set; }
        public string Available { get; set; }
        public string Allowed { get; set; }
        public string Expire { get; set; }
    }
}
