using ModdingManagerModels;

namespace ModdingManagerDataManager.Parsers
{
    public abstract class HtmlParser
    {
        protected abstract List<IConfig> ParseRealization(string content);
        protected abstract void Normalize(ref string content);
        public virtual List<IConfig> Parse(string content)
        {
            if (content.Length < 300 && File.Exists(content))
            {
                string filePath = content;
                content = File.ReadAllText(content);
                var result = ParseRealization(content);
                return result;
            }

            return ParseRealization(content);
        }
    }
}
