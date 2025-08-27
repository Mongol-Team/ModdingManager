using ModdingManagerDataManager.Interfaces;

namespace ModdingManagerDataManager.Parsers.Patterns
{
    public struct CsvAdjacenciesPattern : IParsingPattern
    {
        public string OpenChar => throw new NotImplementedException();

        public string CloseChar => throw new NotImplementedException();

        public string CommentChar => throw new NotImplementedException();

        public string AssignChar => throw new NotImplementedException();

        public IReadOnlyList<Type> Types =>
            [];


        public string Separator => ";";
        public string Apply(string regex)
        {
            return regex.Replace(";", Separator);
        }
    }
}
