using ModdingManagerModels.Interfaces;
using ModdingManagerClassLib.Enums;

namespace ModdingManagerClassLib.Parsers
{
    public abstract class Parser
    {
        protected abstract IHoiData ParseRealisation(string content);

        public virtual IHoiData Parse(string content, ParsingArgType type)
        {
            if(type == ParsingArgType.Path && File.Exists(content))
                content = File.ReadAllText(content);
            

            return ParseRealisation(content);
        }
    }
}