using Application.Settings;
using Application.utils;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace View
{
    public partial class GameDirectorySettingsWindow : BaseWindow
    {
        public GameDirectorySettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            GameDirBox.Text = ModManagerSettings.GameDirectory ?? string.Empty;
        }

        private void BrowseGameButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = StaticLocalisation.GetString("Label.GameDirectory");
                dialog.ShowNewFolderButton = false;
                if (!string.IsNullOrEmpty(GameDirBox.Text))
                {
                    dialog.SelectedPath = GameDirBox.Text;
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    GameDirBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GameDirBox.Text))
            {
                ShowWarning(
                    StaticLocalisation.GetString("Error.PleaseSpecifyDirectory"),
                    NotificationCorner.TopRight);
                return;
            }

            if (!Directory.Exists(GameDirBox.Text))
            {
                ShowWarning(
                    StaticLocalisation.GetString("Error.DirectoryNotExists"),
                    NotificationCorner.TopRight);
                return;
            }

            ModManagerSettingsLoader.SaveGameDirectory(GameDirBox.Text);
            ShowSuccess(
                "Настройки сохранены",
                NotificationCorner.TopRight);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

