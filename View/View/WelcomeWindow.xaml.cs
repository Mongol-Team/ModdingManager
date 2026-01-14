using Application;
using Application.Settings;
using Models.Enums;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using View.Utils;

namespace View
{
    public partial class WelcomeWindow : BaseWindow
    {
        public string SelectedProjectPath { get; private set; } = string.Empty;
        private List<RecentProject> _allProjects = new();

        public WelcomeWindow()
        {
            InitializeComponent();
            LoadRecentProjects();
            SetupSearchPlaceholder();
            UpdateGameDirectoryDisplay();
            LoadSettings();
        }

        private void WelcomeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != true && string.IsNullOrEmpty(SelectedProjectPath))
            {
                DialogResult = false;
            }
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
                var format = UILocalization.GetString("Message.CurrentGameDirectory");
                GameDirectoryTextBlock.Text = string.Format(format, gameDir);
            }
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

        private async void ProjectItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is RecentProject project)
            {
                SelectedProjectPath = project.Path;
                try
                {
                    await LoadModData();
                    MainWindow mainWindow = new MainWindow();
                    System.Windows.Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    mainWindow.Activate();
                    this.Close();
                }
                catch (Exception ex)
                {
                    ShowError(
                        string.Format(UILocalization.GetString("Error.LoadModDataFailed"), ex.Message),
                        NotificationCorner.TopRight);
                }
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

        private async void OpenProjectButton_Click(object sender, RoutedEventArgs e)
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
            ParallelismSlider.Value = ModManagerSettings.MaxPercentForParallelUsage;
            ParallelismValueText.Text = $"{ModManagerSettings.MaxPercentForParallelUsage}%";
            DebugModeCheckBox.IsChecked = ModManagerSettings.IsDebugRunning;

            LanguageComboBox.ItemsSource = Enum.GetValues(typeof(Language)).Cast<Language>();
            LanguageComboBox.SelectedItem = ModManagerSettings.CurrentLanguage;
        }

        private void ParallelismSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ParallelismValueText != null)
            {
                ParallelismValueText.Text = $"{(int)e.NewValue}%";
            }
        }

        private void DebugModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ModManagerSettings.IsDebugRunning = true;
        }

        private void DebugModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ModManagerSettings.IsDebugRunning = false;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is Language language)
            {
                ModManagerSettings.CurrentLanguage = language;
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ModManagerSettings.MaxPercentForParallelUsage = (int)ParallelismSlider.Value;
            ModManagerSettings.IsDebugRunning = DebugModeCheckBox.IsChecked ?? false;

            if (LanguageComboBox.SelectedItem is Language language)
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
