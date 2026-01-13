using Models.Interfaces;

namespace Models.Types.TableCacheData
{
    public class HoiTable : IHoiData, IHoiTable
    {
        public string FileFullPath { get; set; }
        public List<Type> ColumnTypes { get; set; } = new List<Type>();
        public List<List<object>> Values { get; set; } = new List<List<object>>();
    }
}
