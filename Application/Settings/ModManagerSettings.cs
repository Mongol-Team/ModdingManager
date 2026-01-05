using Models.Enums;

namespace Application.Settings
{
    public static class ModManagerSettings
    {
        public static string ModDirectory { get; set; }
        public static bool IsDebugRunning { get; set; }
        public static int MaxPercentForParallelUsage { get; set; }
        public static string GameDirectory { get; set; }
        public static Language CurrentLanguage { get; set; }
        public static List<RecentProject> RecentProjects { get; set; }

        static ModManagerSettings()
        {
            ModDirectory = string.Empty;
            GameDirectory = string.Empty;
            IsDebugRunning = false;
            MaxPercentForParallelUsage = 50;
            CurrentLanguage = Language.english;
            RecentProjects = new List<RecentProject>();
        }
    }
}
