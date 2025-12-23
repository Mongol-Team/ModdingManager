using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utils.Pathes
{
    public static class ProgramPathes
    {
        private static string GetDataDirectory()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            var dataDirectory = Path.Combine(assemblyDirectory, "..", "..", "..", "Data", "Configs");
            return Path.GetFullPath(dataDirectory);
        }
        
        public static readonly string ConfigsDirectory = GetDataDirectory();
        public static readonly string ConfigFilePath = Path.Combine(ConfigsDirectory, "Program.cfg");
        public static readonly string DirConfigPath = Path.Combine(ConfigsDirectory, "dir.cfg");
    }
}
