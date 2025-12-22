namespace ModdingManagerModels.Types.ObectCacheData
{
    public class HoiArray
    {
        public string Name { get; set; }
        public List<object> Values { get; set; } = new List<object>();
        public Type PossibleCsType { get; set; }
        public bool IsHoiReference { get; set; }
    }
}
