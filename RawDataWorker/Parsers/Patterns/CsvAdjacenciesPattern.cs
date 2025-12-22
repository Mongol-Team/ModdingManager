using RawDataWorker.Interfaces;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Types;
using System.Drawing;

namespace RawDataWorker.Parsers.Patterns
{
    public struct CsvAdjacenciesPattern : IParsingPattern
    {
        public string OpenChar => throw new NotImplementedException();

        public string CloseChar => throw new NotImplementedException();

        public string CommentChar => throw new NotImplementedException();

        public string AssignChar => throw new NotImplementedException();

        public IReadOnlyList<Type> Types =>
            [typeof(HoiReference), typeof(HoiReference), typeof(AdjacencyType), typeof(HoiReference), typeof(Point), typeof(Point), typeof(string), typeof(string)];


        public string Separator => ";";
        public string Apply(string regex)
        {
            return regex.Replace(";", Separator);
        }
    }
}
