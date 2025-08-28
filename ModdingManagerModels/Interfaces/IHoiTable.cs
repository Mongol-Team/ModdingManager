namespace ModdingManagerModels.Interfaces
{
    public interface IHoiTable
    {
        List<Type> ColumnTypes { get; }
        List<object> Values { get; set; }
    }
}
