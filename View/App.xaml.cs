using Application.Settings;
using Application.utils;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace View
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ModManagerSettingsLoader.Load();

            var gameDirectory = ModManagerSettings.GameDirectory ?? string.Empty;

            if (string.IsNullOrWhiteSpace(gameDirectory) || !Directory.Exists(gameDirectory))
            {
                MessageBox.Show(
                    StaticLocalisation.GetString("Error.GameDirectoryNotFound"),
                    StaticLocalisation.GetString("Error.Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                var dialog = new OpenFolderDialog
                {
                    Title = StaticLocalisation.GetString("Label.GameDirectory")
                };

                if (dialog.ShowDialog() == true)
                {
                    gameDirectory = dialog.FolderName;

                    ModManagerSettingsLoader.SaveGameDirectory(gameDirectory);
                    ModManagerSettingsLoader.Load();

                    gameDirectory = ModManagerSettings.GameDirectory ?? gameDirectory;
                }
                else
                {
                    MessageBox.Show(
                        StaticLocalisation.GetString("Error.GameDirectoryNotSelected"),
                        StaticLocalisation.GetString("Error.Error"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    Shutdown();
                    return;
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

