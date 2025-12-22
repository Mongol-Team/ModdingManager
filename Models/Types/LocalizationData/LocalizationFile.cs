using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;

namespace ModdingManagerModels.Types.LochalizationData
{
    public class LocalizationFile : IHoiData
    {
        public string FilePath { get; set; }
        public List<LocalizationBlock> Localizations { get; set; } = new List<LocalizationBlock>();
    }
}
