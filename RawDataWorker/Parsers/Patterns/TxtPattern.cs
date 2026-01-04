using RawDataWorker.Interfaces;

namespace RawDataWorker.Parsers.Patterns
{
    public struct TxtPattern : IParsingPattern
    {
        public string OpenChar => "{";

        public string CloseChar => "}";

        public string CommentChar => "#";

        public string AssignChar => "=";

        public IReadOnlyList<Type> Types => throw new NotImplementedException();

        public string Separator => throw new NotImplementedException();

    }
}
