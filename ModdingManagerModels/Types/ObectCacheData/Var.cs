namespace ModdingManagerModels.Types.ObjectCacheData
{
    public class Var
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }
        public Type? PossibleCsType { get; set; }
        public bool IsCore { get; set; } = false;
    }
}