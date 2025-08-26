using ModdingManagerClassLib.Interfaces;

namespace ModdingManagerClassLib.Parsers.Patterns
{
    public struct SomePattern : IParsingPattern
    {
        public string OpenChar => "{";

        public string CloseChar => "}";

        public string CommentChar => "#";

        public string AssignChar => "=";
    }
}
