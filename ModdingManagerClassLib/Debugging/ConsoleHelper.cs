using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;

namespace ModdingManagerClassLib.Debugging
{

    public static class ConsoleHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        public static void ShowConsole()
        {
            AllocConsole();
        }

        public static void HideConsole()
        {
            FreeConsole();
        }
    }

}
