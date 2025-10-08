using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public class TemplateConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public List<RegimentConfig>? SupportItems { get; set; } = new List<RegimentConfig>();
        public List<RegimentConfig> BrigadeItems { get; set; } = new List<RegimentConfig>();
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public bool? IsLocked { get; set; }
        public string? Namespace { get; set; }
        public bool? AllowTraining { get; set; }
        public string? ModelName { get; set; }
        public int? DivisionCap { get; set; }
        public int? Priority { get; set; }
        public int? CustomIconId { get; set; }
        public string OOBFileName { get; set; }
        public int OOBFileYear { get; set; }
    }
}
