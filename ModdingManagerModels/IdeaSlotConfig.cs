using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class IdeaSlotConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public bool IsLaw { get; set; }
        public bool UseListView { get; set; }
        public bool IsDesigner { get; set; }
        public List<IdeaConfig> Ideas { get; set; } = new List<IdeaConfig>();
        public ConfigLocalisation Localisation { get; set; }
    }
}
