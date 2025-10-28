using ModdingManagerModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels.Types.LocalizationData
{
    public class ConfigLocalisation : ILocalisation
    {
        public Language Language { get; set; } = Language.english;
        public Dictionary<string, string> Data { get; set; } = new();
        public IConfig Source { get; set; } = null;
        public bool ReplacebleResource { get; set; } = false;
        public bool IsConfigLocNull { get; set; } = false;
    }
}
