using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.utils.structs
{
    public struct Var
    {
        public string name { get; set; }
        public string value { get; set; }
        public static string GetValueSafe(Var? v) => v?.value;
    }
}
