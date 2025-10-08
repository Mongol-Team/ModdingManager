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

        public int ToInt()
        {
            return int.Parse(RawItendifier.ToString());
        }
        public string ToString()
        {
            return RawItendifier.ToString();
        }
        public bool HasValue()
        {
            if (RawItendifier != null)
            {
                return true;
            }
            else return false;
        }
    }
}
