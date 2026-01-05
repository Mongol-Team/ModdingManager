using View.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Application.Settings;

namespace View
{
    public partial class SettingsWindow : Window
    {
        public string SelectedProjectPath { get; private set; } = string.Empty;
        private List<RecentProject> _allProjects = new List<RecentProject>();

        public SettingsWindow()
        {
            InitializeComponent();
            LoadRecentProjects();
            SetupSearchPlaceholder();
        }

        private void SetupSearchPlaceholder()
        {
            var placeholderText = UILocalization.GetString("Message.SearchRecent");
            var placeholderColor = System.Windows.Media.Color.FromRgb(133, 133, 133);
            var normalColor = System.Windows.Media.Color.FromRgb(204, 204, 204);

            SearchTextBox.Text = placeholderText;
            SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush(placeholderColor);

            SearchTextBox.GotFocus += (s, e) =>
            {
                if (SearchTextBox.Text == placeholderText)
                {
                    SearchTextBox.Text = "";
                    SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush(normalColor);
                }
            };

            SearchTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    SearchTextBox.Text = placeholderText;
                    SearchTextBox.Foreground = new System.Windows.Media.SolidColorBrush(placeholderColor);
                }
            };
        }

        private void LoadRecentProjects()
        {
            if (ModManagerSettings.RecentProjects != null)
            {
                _allProjects = ModManagerSettings.RecentProjects
                    .Where(p => Directory.Exists(p.Path))
                    .Select(p => new RecentProject(p.Path, p.Name))
                    .ToList();

                UpdateProjectsDisplay(_allProjects);
            }
        }

        private void UpdateProjectsDisplay(List<RecentProject> projects)
        {
            var grouped = new List<ProjectGroup>();

            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var todayProjects = projects.Where(p => GetLastModified(p.Path) >= today).ToList();
            var yesterdayProjects = projects.Where(p => GetLastModified(p.Path) >= yesterday && GetLastModified(p.Path) < today).ToList();
            var thisMonthProjects = projects.Where(p => GetLastModified(p.Path) >= thisMonth && GetLastModified(p.Path) < yesterday).ToList();
            var olderProjects = projects.Where(p => GetLastModified(p.Path) < thisMonth).ToList();

            if (todayProjects.Any())
            {
                grouped.Add(new ProjectGroup { Key = UILocalization.GetString("Message.Today"), Value = todayProjects });
            }
            if (yesterdayProjects.Any())
            {
                grouped.Add(new ProjectGroup { Key = UILocalization.GetString("Message.Yesterday"), Value = yesterdayProjects });
            }
            if (thisMonthProjects.Any())
            {
                grouped.Add(new ProjectGroup { Key = UILocalization.GetString("Message.ThisMonth"), Value = thisMonthProjects });
            }
            if (olderProjects.Any())
            {
                grouped.Add(new ProjectGroup { Key = UILocalization.GetString("Message.Older"), Value = olderProjects });
            }

            ProjectsItemsControl.ItemsSource = grouped;
        }

        private DateTime GetLastModified(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    return Directory.GetLastWriteTime(path);
                }
            }
            catch
            {
            }
            return DateTime.MinValue;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text;
            var placeholderText = UILocalization.GetString("Message.SearchRecent");
            
            if (searchText == placeholderText || string.IsNullOrWhiteSpace(searchText))
            {
                UpdateProjectsDisplay(_allProjects);
                return;
            }

            var filtered = _allProjects.Where(p =>
                p.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                p.Path.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();

            UpdateProjectsDisplay(filtered);
        }

        private void ProjectItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is RecentProject project)
            {
                SelectedProjectPath = project.Path;
                DialogResult = true;
                Close();
            }
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = UILocalization.GetString("Button.CreateNewProject");
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var projectPath = dialog.SelectedPath;
                    var projectName = Path.GetFileName(projectPath);
                    if (string.IsNullOrEmpty(projectName))
                        projectName = projectPath;

                    ModManagerSettingsLoader.AddRecentProject(projectPath, projectName);
                    SelectedProjectPath = projectPath;
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = UILocalization.GetString("Button.OpenProject");
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var projectPath = dialog.SelectedPath;
                    var projectName = Path.GetFileName(projectPath);
                    if (string.IsNullOrEmpty(projectName))
                        projectName = projectPath;

                    ModManagerSettingsLoader.AddRecentProject(projectPath, projectName);
                    SelectedProjectPath = projectPath;
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = UILocalization.GetString("Button.OpenFolder");
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var projectPath = dialog.SelectedPath;
                    var projectName = Path.GetFileName(projectPath);
                    if (string.IsNullOrEmpty(projectName))
                        projectName = projectPath;

                    ModManagerSettingsLoader.AddRecentProject(projectPath, projectName);
                    SelectedProjectPath = projectPath;
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void CloneRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            PlaceholderWindow.ShowPlaceholder(
                UILocalization.GetString("Message.PageNotReady") + " " + UILocalization.GetString("Button.CloneRepository"),
                this);
        }
    }

    public class ProjectGroup
    {
        public string Key { get; set; }
        public List<RecentProject> Value { get; set; }
    }
}
