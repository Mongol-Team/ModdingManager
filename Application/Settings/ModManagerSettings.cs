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
        public static List<string> ClassDebugNames { get; set; }
        public static List<RecentProject> RecentProjects { get; set; }
    }
}
