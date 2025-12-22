using Models;
using Models.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Composers
{
    public interface IComposer
    {
        public static abstract List<IConfig> Parse();
        
    }
}
