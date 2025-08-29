using ModdingManagerModels.Enums;

namespace ModdingManagerModels.Types.LocalizationData
{
    public class LocalizationBlock
    {
        public Language Language { get; set; }
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }
}
