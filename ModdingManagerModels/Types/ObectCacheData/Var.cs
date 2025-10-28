using ModdingManagerModels.Types.Utils;
using DDF = ModdingManagerData.DataDefaultValues;

namespace ModdingManagerModels.Types.ObjectCacheData
{
    public class Var
    {
        public string Name { get; }

        public object? Value { get; }

        public Type? PossibleCsType { get; }
        public bool IsCore { get; }

        public Var(string name, object? value, Type? possibleCsType = null, bool isCore = false)
        {
            Name = name ?? string.Empty;
            Value = value ?? DDF.Null;
            PossibleCsType = possibleCsType;
            IsCore = isCore;
        }
    }
}