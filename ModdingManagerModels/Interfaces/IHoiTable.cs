namespace ModdingManagerModels.Interfaces
{
    public interface IHoiTable
    {
        List<Type> ColumnTypes { get; }
        List<List<object>> Values { get; set; }
    }
}
