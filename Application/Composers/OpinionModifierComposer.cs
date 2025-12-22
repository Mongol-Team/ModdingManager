using Application.utils.Pathes;
using Models;
using Models.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Composers
{
    public class OpinionModifierComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new List<IConfig>();
            string[] possiblePathes =
            {
                ModPathes.OpinionModifiersPath,
                GamePathes.OpinionModifiersPath
            };
            foreach (string path in possiblePathes)
            {
                string[] files = Directory.GetFiles(path);

            }
            return configs;

        }

        //public static List<IConfig> ParseFile()
        //{

        //}

        //public static IConfig ParseSingleOpModif(Bracket modbr)
        //{

        //}
    }
}
