using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.args
{
    public class StateTransferArg
    {
        public int? StateId { get; set; }
        public string? SourceCountryTag { get; set; }
        public string? TargetCountryTag { get; set; }
    }
}
