using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class StateCathegoryConfig
    {
        public Color Color { get; set; }
        public string Id { get; set; }
        public Dictionary<ModifierDefenitionConfig, object> Modifiers { get; set; }
    }
}
