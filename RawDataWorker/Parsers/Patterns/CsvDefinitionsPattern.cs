
using Models.Enums;
using System.Drawing;
using Models.Interfaces.RawDataWorkerInterfaces;

namespace RawDataWorker.Parsers.Patterns
{
    public struct CsvDefinitionsPattern : IParsingPattern
    {
        public string OpenChar => throw new NotImplementedException();

        public string CloseChar => throw new NotImplementedException();

        public string CommentChar => throw new NotImplementedException();

        public string AssignChar => throw new NotImplementedException();

        public IReadOnlyList<Type> Types =>
        [typeof(int), typeof(Color), typeof(ProvinceType), typeof(bool), typeof(ProvinceTerrain), typeof(int)];

        public string Separator => ";";
    }
}
