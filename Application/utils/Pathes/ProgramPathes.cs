using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utils.Pathes
{
    public static class ProgramPathes
    {
        public static readonly string ConfigFilePath = Path.Combine(AppContext.BaseDirectory, "Program.json");

    }
}
