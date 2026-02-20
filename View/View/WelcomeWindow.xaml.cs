using Application;
using Application.Settings;
using Application.utils;
using Microsoft.Win32;
using Models.Enums;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace View
{
    public partial class WelcomeWindow : BaseWindow
    {
        public string SelectedProjectPath { get; private set; } = string.Empty;
        private List<RecentProject> _allProjects = new();

        public ICommand RemoveProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }

        public WelcomeWindow()
        {
            RemoveProjectCommand = new RelayCommand(RemoveProjectExecute, p => p is RecentProject);
            OpenProjectCommand = new RelayCommand(OpenProjectExecute, p => p is RecentProject);
            InitializeComponent();
            LoadRecentProjects();
            SetupSearchPlaceholder();
            UpdateGameDirectoryDisplay();
            LoadSettings();
        }

        private void WelcomeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (DialogResult != true && string.IsNullOrEmpty(SelectedProjectPath))
            //{
            //    DialogResult = false;
            //}
        }

        private void UpdateGameDirectoryDisplay()
        {
            var gameDir = ModManagerSettings.GameDirectory;
            if (string.IsNullOrEmpty(gameDir))
            {
                GameDirectoryTextBlock.Text = "";
            }
            else
            {
                var format = StaticLocalisation.GetString("Message.CurrentGameDirectory");
                GameDirectoryTextBlock.Text = string.Format(format, gameDir);
            }
        }

        private void SetupSearchPlaceholder()
        {
            var placeholderText = StaticLocalisation.GetString("Message.SearchRecent");
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
                grouped.Add(new ProjectGroup { Key = StaticLocalisation.GetString("Message.Today"), Value = todayProjects });
            }
            if (yesterdayProjects.Any())
            {
                grouped.Add(new ProjectGroup { Key = StaticLocalisation.GetString("Message.Yesterday"), Value = yesterdayProjects });
            }
            if (thisMonthProjects.Any())
            {
                grouped.Add(new ProjectGroup { Key = StaticLocalisation.GetString("Message.ThisMonth"), Value = thisMonthProjects });
            }
            if (olderProjects.Any())
            {
                grouped.Add(new ProjectGroup { Key = StaticLocalisation.GetString("Message.Older"), Value = olderProjects });
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
            var placeholderText = StaticLocalisation.GetString("Message.SearchRecent");

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

        private void RemoveProjectExecute(object? parameter)
        {
            if (parameter is not RecentProject project) return;
            var list = ModManagerSettings.RecentProjects ?? new List<RecentProject>();
            list.RemoveAll(p => string.Equals(p.Path, project.Path, StringComparison.OrdinalIgnoreCase));
            ModManagerSettings.RecentProjects = list;
            ModManagerSettingsLoader.Save(ModManagerSettings.ModDirectory ?? string.Empty, ModManagerSettings.GameDirectory ?? string.Empty);
            _allProjects = _allProjects.Where(p => !string.Equals(p.Path, project.Path, StringComparison.OrdinalIgnoreCase)).ToList();
            UpdateProjectsDisplay(_allProjects);
        }

        private async void OpenProjectExecute(object? parameter)
        {
            if (parameter is not RecentProject project) return;
            SelectedProjectPath = project.Path;
            try
            {
                await LoadModData();
                var mainWindow = new MainWindow();
                System.Windows.Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                mainWindow.Activate();
                Close();
            }
            catch (Exception ex)
            {
                ShowError(
                    string.Format(StaticLocalisation.GetString("Error.LoadModDataFailed"), ex.Message),
                    NotificationCorner.TopRight);
            }
        }

        public void UpdateLoadingProgress(int current, int total, string message)
        {
            Dispatcher.Invoke(() =>
            {
                LoadingProgressBar.Visibility = Visibility.Visible;
                LoadingStatusText.Visibility = Visibility.Visible;
                LoadingProgressBar.Maximum = total;
                LoadingProgressBar.Value = current;
                LoadingStatusText.Text = message;
            });
        }

        public void HideLoadingProgress()
        {
            Dispatcher.Invoke(() =>
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
                LoadingStatusText.Visibility = Visibility.Collapsed;
            });
        }

        private async System.Threading.Tasks.Task LoadModData()
        {
            if (!string.IsNullOrEmpty(SelectedProjectPath))
            {
                ModManagerSettings.ModDirectory = SelectedProjectPath;
                ModManagerSettingsLoader.Save(SelectedProjectPath, ModManagerSettings.GameDirectory);
            }

            await System.Threading.Tasks.Task.Run(() =>
            {
                ModDataStorage.RegisterTypes();
                ModDataStorage.ComposeMod((current, total, message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        int progress = (int)((current / (double)total) * 100);
                        UpdateLoadingProgress(progress, 100, message);
                    });
                });
            });

            await Dispatcher.InvokeAsync(async () =>
            {
                UpdateLoadingProgress(100, 100, "Загрузка завершена");
                await System.Threading.Tasks.Task.Delay(300);
                HideLoadingProgress();
            });
        }

        private async void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = StaticLocalisation.GetString("Button.CreateNewProject"),
                // InitialDirectory = ... // можно задать стартовую папку
            };

            if (dialog.ShowDialog() == true)
            {
                var projectPath = dialog.FolderName;
                var projectName = Path.GetFileName(projectPath);

                if (string.IsNullOrEmpty(projectName))
                    projectName = projectPath;

                ModManagerSettingsLoader.AddRecentProject(projectPath, projectName);
                SelectedProjectPath = projectPath;

                try
                {
                    await LoadModData();

                    DialogResult = true;

                    var mainWindow = new MainWindow();
                    System.Windows.Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();

                    Close();
                }
                catch (Exception ex)
                {
                    ShowError(
                        $"Ошибка при загрузке данных мода: {ex.Message}",
                        NotificationCorner.TopRight);

                    DialogResult = false;
                }
            }
        }

        private async void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = StaticLocalisation.GetString("Button.OpenProject");
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var projectPath = dialog.SelectedPath;
                    var projectName = Path.GetFileName(projectPath);
                    if (string.IsNullOrEmpty(projectName))
                        projectName = projectPath;

                    ModManagerSettingsLoader.AddRecentProject(projectPath, projectName);
                    SelectedProjectPath = projectPath;
                    try
                    {
                        await LoadModData();
                        ((Window)this).DialogResult = true;
                        var mainWindow = new MainWindow();
                        System.Windows.Application.Current.MainWindow = (Window)mainWindow;
                        mainWindow.Show();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        ShowError(
                            $"Ошибка при загрузке данных мода: {ex.Message}",
                            NotificationCorner.TopRight);
                        ((Window)this).DialogResult = false;
                    }
                }
            }
        }

        private void LoadSettings()
        {
            WelcomeSettingsControl.GameDirectory = ModManagerSettings.GameDirectory ?? string.Empty;
            WelcomeSettingsControl.ParallelismPercent = ModManagerSettings.MaxPercentForParallelUsage;
            WelcomeSettingsControl.IsDebugMode = ModManagerSettings.IsDebugRunning;
            WelcomeSettingsControl.LanguageItemsSource = Enum.GetValues(typeof(Language)).Cast<Language>();
            WelcomeSettingsControl.EffectiveLanguage = ModManagerSettings.CurrentLanguage;
            WelcomeSettingsControl.SelectedLanguage = ModManagerSettings.CurrentLanguage;
        }

        private void WelcomeSettingsControl_SaveClicked(object sender, RoutedEventArgs e)
        {
            ModManagerSettings.GameDirectory = (WelcomeSettingsControl.GameDirectory ?? string.Empty).Trim();
            ModManagerSettings.MaxPercentForParallelUsage = WelcomeSettingsControl.ParallelismPercent;
            ModManagerSettings.IsDebugRunning = WelcomeSettingsControl.IsDebugMode;

            if (WelcomeSettingsControl.SelectedLanguage is Language language)
            {
                ModManagerSettings.CurrentLanguage = language;
            }

            ModManagerSettingsLoader.Save(ModManagerSettings.ModDirectory ?? string.Empty, ModManagerSettings.GameDirectory ?? string.Empty);

            ShowSuccess(
                "Настройки сохранены",
                NotificationCorner.TopRight);
        }

    }

    public class ProjectGroup
    {
        public string Key { get; set; }
        public List<RecentProject> Value { get; set; }
    }
}
