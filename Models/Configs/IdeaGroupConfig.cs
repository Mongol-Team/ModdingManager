using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;

namespace Models.Configs
{
    public class IdeaGroupConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public bool IsLaw { get; set; }
        public bool UseListView { get; set; }
        public bool IsDesigner { get; set; }
        public List<IdeaConfig> Ideas { get; set; } = new List<IdeaConfig>();
        public ConfigLocalisation Localisation { get; set; }
    }
}
