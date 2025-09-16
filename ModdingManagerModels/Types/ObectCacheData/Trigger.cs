using ModdingManagerModels.Enums;
using ModdingManagerModels.Types.ObjectCacheData;

namespace ModdingManagerModels.Types.ObectCacheData
{
    public class Trigger : Var
    {
        public OperatorType Condition { get; set; }
    }
}
