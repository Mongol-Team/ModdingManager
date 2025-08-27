using ModdingManagerDataManager.Interfaces;
using ModdingManagerModels.Interfaces;

namespace ModdingManagerDataManager.Parsers
{
    public abstract class Parser
    {
        protected abstract IHoiData ParseRealization(string content, IParsingPattern pattern);

        public virtual IHoiData Parse(string content, IParsingPattern pattern)
        {
            if (content.Length < 300 && File.Exists(content))
            {
                string filePath = content;
                content = File.ReadAllText(content);
                var result = ParseRealization(content, pattern);
                result.FilePath = filePath;
                return result;
            }

            return ParseRealization(content, pattern);
        }
    }
}