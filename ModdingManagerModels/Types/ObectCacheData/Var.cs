using ModdingManagerModels.Types.Utils;
using DDF = ModdingManagerData.DataDefaultValues;

namespace ModdingManagerModels.Types.ObjectCacheData
{
    public class Var
    {
        public string Name { get; set; }

        public object? Value { get; set; }

        public Type? PossibleCsType { get; set; }
        public bool IsCore { get; set; }

    }
}