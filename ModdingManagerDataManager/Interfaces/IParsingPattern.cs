namespace ModdingManagerDataManager.Interfaces
{
    public interface IParsingPattern
    {
        string OpenChar { get; }
        string CloseChar { get; }
        string CommentChar { get; }
        string AssignChar { get; }
        IReadOnlyList<Type> Types { get; }
        string Separator { get; }
        string Apply(string regex);

    }
}