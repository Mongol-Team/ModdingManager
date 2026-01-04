using RawDataWorker.Interfaces;
using Models.Enums;
using Models.Types;
using System.Drawing;

namespace RawDataWorker.Parsers.Patterns
{
    public struct RulesDataPattern : IParsingPattern
    {
        public string OpenChar => throw new NotImplementedException();

        public string CloseChar => throw new NotImplementedException();

        public string CommentChar => throw new NotImplementedException();

        public string AssignChar => throw new NotImplementedException();

        public IReadOnlyList<Type> Types =>
        [typeof(string)];


        public string Separator => ";";
        public string Apply(string regex)
        {
            return regex.Replace(";", Separator);
        }
    }
}
