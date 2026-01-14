using RawDataWorker.Interfaces;
using Models.Interfaces;

namespace RawDataWorker.Parsers
{
    public abstract class Parser()
    {
        protected readonly IParsingPattern pattern;
        public Parser(IParsingPattern _pattern) : this() { pattern = _pattern; }
        protected abstract IHoiData ParseRealization(string content);
        protected abstract void Normalize(ref string content);
        public virtual IHoiData Parse(string content)
        {
            if (content.Length < 300 && File.Exists(content))
            {
                string FileFullPath = content;
                content = File.ReadAllText(content);
                var result = ParseRealization(content);
                result.FileFullPath = FileFullPath;
                return result;
            }

            return ParseRealization(content);
        }
    }
}