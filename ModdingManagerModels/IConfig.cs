using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public interface IConfig 
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
    }
}
