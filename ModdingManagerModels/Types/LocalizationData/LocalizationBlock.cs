using ModdingManagerModels.Enums;

namespace ModdingManagerModels.Types.LocalizationData
{
    public class LocalizationBlock : ILocalisation
    {
        public Language Language { get; set; }
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        public bool ReplacebleResource { get; set; } = false;
    }
}
