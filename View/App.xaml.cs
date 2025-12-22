using Application.Settings;

namespace View
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            
            ModManagerSettings.Load();
            
            var settingsWindow = new SettingsWindow();
            settingsWindow.LoadSettings(
                ModManagerSettings.Instance?.GameDirectory ?? string.Empty,
                ModManagerSettings.Instance?.ModDirectory ?? string.Empty);
            
            settingsWindow.ShowDialog();
            
            if (settingsWindow.SettingsSaved)
            {
                var gameDir = settingsWindow.GetGameDirectory();
                var modDir = settingsWindow.GetModDirectory();
                
                ModManagerSettings.Save(modDir, gameDir);
                
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}

