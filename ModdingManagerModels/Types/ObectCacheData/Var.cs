namespace ModdingManagerModels.Types.ObjectCacheData
{
    public class Var
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Type PossibleCsType { get; set; }
        public bool IsHoiReference { get; set; }
    }
}