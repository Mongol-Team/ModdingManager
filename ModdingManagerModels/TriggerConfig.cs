using ModdingManagerModels.Types.ObectCacheData;
using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public class ScriptedTriggerConfig : IConfig
    {
        public Identifier Id { get; set; }
        public List<Trigger> Triggers { get; set; } = new List<Trigger>();
    }
}
