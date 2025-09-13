using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels.Types.Utils
{
    public class Identifier
    {
        public Identifier(object rawItendifier)
        {
            RawItendifier = rawItendifier;
        }
        public object RawItendifier { get; set; }
        public string AsString()
        {
            return RawItendifier.ToString();
        }
        public int AsInt()
        {
            return int.Parse(RawItendifier.ToString());
        }
    }
}
