using Application.Settings;
using System.IO;
using System.Windows;
using View.Utils;

namespace View
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            ModManagerSettingsLoader.Load();

            var gameDirectory = ModManagerSettings.GameDirectory ?? string.Empty;

            if (string.IsNullOrWhiteSpace(gameDirectory) || !Directory.Exists(gameDirectory))
            {
                System.Windows.MessageBox.Show(
                    UILocalization.GetString("Error.GameDirectoryNotFound"),
                    UILocalization.GetString("Error.Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = UILocalization.GetString("Label.GameDirectory");
                    dialog.ShowNewFolderButton = false;

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        gameDirectory = dialog.SelectedPath;
                        ModManagerSettingsLoader.SaveGameDirectory(gameDirectory);
                        ModManagerSettingsLoader.Load();
                        gameDirectory = ModManagerSettings.GameDirectory ?? gameDirectory;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            UILocalization.GetString("Error.GameDirectoryNotSelected"),
                            UILocalization.GetString("Error.Error"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Shutdown();
                        return;
                    }
                }
            }

            var welcomeWindow = new WelcomeWindow();
            welcomeWindow.Show();
            
            welcomeWindow.Closed += (s, e) =>
            {
                if (string.IsNullOrEmpty(welcomeWindow.SelectedProjectPath))
                {
                    Shutdown();
                }
            };
        }
    }
}

