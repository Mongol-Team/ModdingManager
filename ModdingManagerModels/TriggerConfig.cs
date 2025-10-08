using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.ObectCacheData;
using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public class TriggerBlockConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public Dictionary<TriggerDefenitionConfig, bool> Triggers { get; set; } = new();
        public IGfx Gfx { get; set; }
    }
}
