using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;

namespace Models.Configs
{
    public class TemplateConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public List<SubUnitConfig>? SupportItems { get; set; } = new List<SubUnitConfig>();
        public List<SubUnitConfig> BrigadeItems { get; set; } = new List<SubUnitConfig>();
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public bool? IsLocked { get; set; }
        public string? Namespace { get; set; }
        public bool? AllowTraining { get; set; }
        public string? ModelName { get; set; }
        public int? DivisionCap { get; set; }
        public int? Priority { get; set; }
        public int? CustomIconId { get; set; }
        public string OOBFileName { get; set; }
        public int OOBFileYear { get; set; }
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
