using ModdingManager.View;

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
            var app = new System.Windows.Application();
            var view = new MainWindow();
            app.Run(view);
        }
    }
}