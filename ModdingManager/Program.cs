using ModdingManager.WPF.View;
using Application = System.Windows.Application;

namespace ModdingManager
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var app = new Application();
            var window = new Main();
            app.Run(window);
        }
    }
}