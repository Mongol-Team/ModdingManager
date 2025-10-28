using ModdingManager.View;
using ModdingManagerClassLib;
using ModdingManagerClassLib.Settings;
using System.Runtime.InteropServices;

namespace ModdingManager
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
     
        [STAThread]
        public static void Main()
        {
           
            var app = new System.Windows.Application();
            var view = new MainWindow();
            app.Run(view);
        }
    }
}