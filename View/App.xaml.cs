using Application.Settings;
using System.IO;
using System.Windows;

namespace View
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            
            ModManagerSettings.Load();
            
            var gameDirectory = ModManagerSettings.Instance?.GameDirectory ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(gameDirectory) || !Directory.Exists(gameDirectory))
            {
                System.Windows.MessageBox.Show(
                    "Директория игры не найдена. Пожалуйста, настройте путь к игре через меню 'Настройки' -> 'Путь к игре'.",
                    "Игра не найдена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            
            if (settingsWindow.DialogResult == true && !string.IsNullOrEmpty(settingsWindow.SelectedProjectPath))
            {
                ModManagerSettings.Save(settingsWindow.SelectedProjectPath, gameDirectory);
            }
            
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}

