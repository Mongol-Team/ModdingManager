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
    public class TechCategoryConfig : IConfig
    {
        public Identifier Id { get ; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
    }
}
