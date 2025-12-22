using System;
using System.Windows;
using System.Windows.Forms;

namespace View
{
    public partial class SettingsWindow : Window
    {
        public bool SettingsSaved { get; private set; } = false;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public void LoadSettings(string gameDirectory, string modDirectory)
        {
            GameDirBox.Text = gameDirectory ?? string.Empty;
            ModDirBox.Text = modDirectory ?? string.Empty;
        }

        public string GetGameDirectory() => GameDirBox.Text;
        public string GetModDirectory() => ModDirBox.Text;

        private void BrowseGameButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите директорию игры";
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

        private void BrowseModButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите директорию мода";
                dialog.ShowNewFolderButton = true;
                
                if (!string.IsNullOrEmpty(ModDirBox.Text))
                {
                    dialog.SelectedPath = ModDirBox.Text;
                }
                else if (!string.IsNullOrEmpty(GameDirBox.Text))
                {
                    var modsPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "Paradox Interactive",
                        "Hearts of Iron IV",
                        "mod");
                    if (System.IO.Directory.Exists(modsPath))
                    {
                        dialog.SelectedPath = modsPath;
                    }
                    else
                    {
                        dialog.SelectedPath = GameDirBox.Text;
                    }
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ModDirBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GameDirBox.Text) || string.IsNullOrWhiteSpace(ModDirBox.Text))
            {
                System.Windows.MessageBox.Show(
                    "Пожалуйста, укажите обе директории.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SettingsSaved = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsSaved = false;
            Close();
        }
    }
}

