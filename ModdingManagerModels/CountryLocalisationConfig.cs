using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class CountryLocalisationConfig
    {
        public string NameValue { get; set; }
        public string AdjValue { get; set; }
        public string DefValue { get; set; }
        public Tuple<IdeologyConfig, string, string> IdeologyValues { get; set; }

    }
}
