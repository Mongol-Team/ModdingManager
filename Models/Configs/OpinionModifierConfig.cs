using Data;
using Models.Attributes;
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
    [ConfigCreator(ConfigCreatorType.GenericCreator)]
    public class OpinionModifierConfig : IModifier, IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsTrade { get; set; }
        public int Value { get; set; }
        public int Decay { get; set; }
        public Dictionary<TimeUnit, int> RemovalTime { get; set; } = new Dictionary<TimeUnit, int> { { TimeUnit.Day, DataDefaultValues.NullInt } };
        public int MinTrust { get; set; }
        public int MaxTrust { get; set; }
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
