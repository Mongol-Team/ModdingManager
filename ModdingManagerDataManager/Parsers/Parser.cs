using ModdingManagerDataManager.Interfaces;
using ModdingManagerModels.Interfaces;

namespace ModdingManagerDataManager.Parsers
{
    public abstract class Parser
    {
        protected abstract IHoiData ParseRealization(string content, IParsingPattern pattern);

        public virtual IHoiData Parse(string content, IParsingPattern pattern)
        {
            if (File.Exists(content))
                content = File.ReadAllText(content);

            return ParseRealization(content, pattern);
        }
    }
}