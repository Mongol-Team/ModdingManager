namespace ModdingManagerClassLib.Interfaces
{
    public interface IParsingPattern
    {
        string OpenChar { get; }
        string CloseChar { get; }
        string CommentChar { get; }
        string AssignChar { get; }
    }
}