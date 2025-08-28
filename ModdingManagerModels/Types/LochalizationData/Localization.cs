using ModdingManagerModels.Enums;

namespace ModdingManagerModels.Types.LocalizationData
{
    public class Localization
    {
        public Language Language { get; set; }
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }
}
