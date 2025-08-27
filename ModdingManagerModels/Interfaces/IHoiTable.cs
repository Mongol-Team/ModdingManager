namespace ModdingManagerModels.Interfaces
{
    public interface IHoiTable
    {
        List<Type> ColumnTypes { get; }
        Dictionary<Type, List<object>> Values { get; set; }
    }
}
