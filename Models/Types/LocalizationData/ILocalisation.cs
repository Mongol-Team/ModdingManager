using Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Types.LocalizationData
{
    public interface ILocalisation
    {
        public Language Language { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public bool ReplacebleResource { get; set; }
    }
}
