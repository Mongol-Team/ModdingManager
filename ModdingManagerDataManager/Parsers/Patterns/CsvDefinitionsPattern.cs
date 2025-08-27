using ModdingManagerDataManager.Interfaces;
using ModdingManagerModels.Enums;
using System.Drawing;

namespace ModdingManagerDataManager.Parsers.Patterns
{
    public struct CsvDefinitionsPattern : IParsingPattern
    {
        public string OpenChar => throw new NotImplementedException();

        public string CloseChar => throw new NotImplementedException();

        public string CommentChar => throw new NotImplementedException();

        public string AssignChar => throw new NotImplementedException();

        public IReadOnlyList<Type> Types =>
    [typeof(int), typeof(Color), typeof(ProvinceTypes), typeof(bool), typeof(ProvincesBiomes), typeof(int)];

        public string Separator => ";";

        public string Apply(string regex)
        {
            return regex.Replace(";", Separator);
        }
    }
}
