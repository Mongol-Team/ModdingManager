using Application.utils.Pathes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.utils
{
    public class OverridePathComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            string relX = GetRelativePath(x);
            string relY = GetRelativePath(y);

            if (relX == relY)
            {
                bool xIsMod = x.Contains(ModPathes.BaseDirectory);
                bool yIsMod = y.Contains(ModPathes.BaseDirectory);
                if (xIsMod && !yIsMod) return 1;
                if (!xIsMod && yIsMod) return -1;
            }

            return string.Compare(x, y, StringComparison.Ordinal);
        }

        private string GetRelativePath(string fullPath)
        {
            if (fullPath.Contains(ModPathes.BaseDirectory))
                fullPath = fullPath.Replace(ModPathes.BaseDirectory, "").TrimStart('\\', '/');
            else if (fullPath.Contains(GamePathes.BaseDirectory))
                fullPath = fullPath.Replace(GamePathes.BaseDirectory, "").TrimStart('\\', '/');

            return fullPath.ToLowerInvariant();
        }
    }
}
