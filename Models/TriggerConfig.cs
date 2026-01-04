using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.ObectCacheData;
using Models.Types.Utils;

namespace Models
{
    public class TriggerBlockConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public Dictionary<TriggerDefenitionConfig, bool> Triggers { get; set; } = new();
        public IGfx Gfx { get; set; }
    }
}
