using Application.Debugging;
using Application.Utils;
using Models.Enums;

namespace Application.Settings
{
    public static class ModManagerSettingsLoader
    {
        public static void Load()
        {
            try
            {
                string configFileFullPath = AppPaths.ProgramConfigPath;
                AppPaths.Ensure(configFileFullPath);
                Dictionary<string, string> config = ConfigFileParser.ParseConfigFile(configFileFullPath);
                ModManagerSettings.ModDirectory = config.TryGetValue("ModDirectory", out var modDir) ? modDir : string.Empty;
                ModManagerSettings.GameDirectory = config.TryGetValue("GameDirectory", out var gameDir) ? gameDir : string.Empty;
                ModManagerSettings.IsDebugRunning = config.TryGetValue("IsDebugRunning", out var debug) && bool.TryParse(debug, out var debugVal) && debugVal;
                ModManagerSettings.MaxPercentForParallelUsage = config.TryGetValue("MaxPercentForParallelUsage", out var maxPercent) && int.TryParse(maxPercent, out var maxVal) ? maxVal : 50;
                ModManagerSettings.CurrentLanguage = config.TryGetValue("CurrentLanguage", out var lang) && Enum.TryParse<Language>(lang, true, out var langVal) ? langVal : Language.english;
                ModManagerSettings.RecentProjects = config.TryGetValue("RecentProjects", out var projects) ? ConfigFileParser.ParseRecentProjects(projects) : new List<RecentProject>();
                ModManagerSettings.ClassDebugNames = config.TryGetValue("ClassDebugNames", out var debugNames) ? ConfigFileParser.ParseList(debugNames) : new List<string>();

            }
            catch
            {
                ModManagerSettings.ModDirectory = string.Empty;
                ModManagerSettings.GameDirectory = string.Empty;
                ModManagerSettings.IsDebugRunning = false;
                ModManagerSettings.MaxPercentForParallelUsage = 50;
                ModManagerSettings.CurrentLanguage = Language.english;
                ModManagerSettings.RecentProjects = new List<RecentProject>();
                ModManagerSettings.ClassDebugNames = new List<string>();
            }
        }

        public static void Save(string modDirectory, string gameDirectory)
        {
            ModManagerSettings.ModDirectory = modDirectory;
            ModManagerSettings.GameDirectory = gameDirectory;

            var config = new Dictionary<string, string>
            {
                ["ModDirectory"] = ModManagerSettings.ModDirectory ?? string.Empty,
                ["GameDirectory"] = ModManagerSettings.GameDirectory ?? string.Empty,
                ["IsDebugRunning"] = ModManagerSettings.IsDebugRunning.ToString(),
                ["MaxPercentForParallelUsage"] = ModManagerSettings.MaxPercentForParallelUsage.ToString(),
                ["CurrentLanguage"] = ModManagerSettings.CurrentLanguage.ToString(),
                ["RecentProjects"] = ConfigFileParser.SerializeRecentProjects(ModManagerSettings.RecentProjects ?? new List<RecentProject>()),
                ["ClassDebugNames"] = ConfigFileParser.SerializeList(ModManagerSettings.ClassDebugNames ?? new List<string>())
            };

            ConfigFileParser.WriteConfigFile(AppPaths.ProgramConfigPath, config);
        }

        public static void AddRecentProject(string projectPath, string projectName = null)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                projectName = Path.GetFileName(projectPath);
                if (string.IsNullOrEmpty(projectName))
                    projectName = projectPath;
            }

            var recentProjects = ModManagerSettings.RecentProjects ?? new List<RecentProject>();
            var existingProject = recentProjects.FirstOrDefault(p => p.Path.Equals(projectPath, StringComparison.OrdinalIgnoreCase));
            if (existingProject != null)
            {
                recentProjects.Remove(existingProject);
            }

            recentProjects.Insert(0, new RecentProject(projectPath, projectName));

            if (recentProjects.Count > 10)
            {
                recentProjects.RemoveAt(recentProjects.Count - 1);
            }

            ModManagerSettings.RecentProjects = recentProjects;
            Save(ModManagerSettings.ModDirectory, ModManagerSettings.GameDirectory);
        }

        public static void SaveGameDirectory(string gameDirectory)
        {
            Save(ModManagerSettings.ModDirectory ?? string.Empty, gameDirectory);
        }
    }
}
