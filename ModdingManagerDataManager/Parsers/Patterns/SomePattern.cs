using ModdingManagerDataManager.Interfaces;

namespace ModdingManagerDataManager.Parsers.Patterns
{
    public struct TxtPattern : IParsingPattern
    {
        public string OpenChar => "{";

        public string CloseChar => "}";

        public string CommentChar => "#";

        public string AssignChar => "=";

        public IReadOnlyList<Type> Types => throw new NotImplementedException();

        public string Separator => throw new NotImplementedException();

        public string Apply(string regex)
        {
            return regex.Replace(@"\{", $@"\{OpenChar}").Replace(@"\}", CloseChar).Replace("#", CommentChar).Replace("=", AssignChar);
        }
    }
}
