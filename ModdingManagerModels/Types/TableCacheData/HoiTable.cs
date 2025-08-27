using ModdingManagerModels.Interfaces;

namespace ModdingManagerModels.Types.TableCacheData
{
    public class HoiTable : IHoiData, IHoiTable
    {
        public string FilePath { get; set; }
        public List<Type> ColumnTypes { get; set; } = new List<Type>();
        public Dictionary<Type, List<object>> Values { get; set; } = new Dictionary<Type, List<object>>();
    }
}
