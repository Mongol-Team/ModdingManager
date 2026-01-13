using Models.Interfaces;
using Models.Types.LocalizationData;

namespace Models.Types.LochalizationData
{
    public class LocalizationFile : IHoiData
    {
        public string FileFullPath { get; set; }
        public List<LocalizationBlock> Localizations { get; set; } = new List<LocalizationBlock>();
    }
}
