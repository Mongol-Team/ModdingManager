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
    public class OpinionModifierConfig : IModifier, IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsTrade { get; set; }
        public int Value { get; set; }
        public int Decay { get; set; }
        public Dictionary<TimeUnit, int> RemovalTime { get; set; }
        public int MinTrust { get; set; }
        public int MaxTrust { get; set; }
    }
}
