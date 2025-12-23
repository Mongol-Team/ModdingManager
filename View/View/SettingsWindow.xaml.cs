using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Application.Settings;

namespace View
{
    public partial class SettingsWindow : Window
    {
        public string SelectedProjectPath { get; private set; } = string.Empty;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadRecentProjects();
        }

        private void LoadRecentProjects()
        {
            if (ModManagerSettings.Instance?.RecentProjects != null)
            {
                ProjectsListBox.ItemsSource = ModManagerSettings.Instance.RecentProjects
                    .Where(p => Directory.Exists(p))
                    .ToList();
            }
        }

        private void ProjectsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProjectsListBox.SelectedItem != null)
            {
                SelectedProjectPath = ProjectsListBox.SelectedItem.ToString();
            }
        }

        private void ProjectsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProjectsListBox.SelectedItem != null)
            {
                SelectedProjectPath = ProjectsListBox.SelectedItem.ToString();
                DialogResult = true;
                Close();
            }
        }

        private void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите директорию проекта мода";
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var projectPath = dialog.SelectedPath;
                    ModManagerSettings.AddRecentProject(projectPath);
                    SelectedProjectPath = projectPath;
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsListBox.SelectedItem != null)
            {
                var selectedPath = ProjectsListBox.SelectedItem.ToString();
                var recentProjects = ModManagerSettings.Instance?.RecentProjects?.ToList() ?? new List<string>();
                recentProjects.Remove(selectedPath);
                
                typeof(ModManagerSettings).GetProperty(nameof(ModManagerSettings.RecentProjects))
                    ?.SetValue(ModManagerSettings.Instance, recentProjects);
                ModManagerSettings.Save(
                    ModManagerSettings.Instance?.ModDirectory ?? string.Empty,
                    ModManagerSettings.Instance?.GameDirectory ?? string.Empty);
                
                LoadRecentProjects();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

