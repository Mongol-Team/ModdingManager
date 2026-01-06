using System.Diagnostics;

namespace Application.Utils
{
    public static class AppPaths
    {
        public const string Company = "SME";
        public const string AppName = "ModdingManager";

        public static string BaseDirectory => AppContext.BaseDirectory;

        private static bool IsDevelopmentEnvironment()
        {
            if (Debugger.IsAttached)
                return true;

            var baseDir = BaseDirectory;
            var dirName = Path.GetFileName(Path.GetDirectoryName(baseDir))?.ToLowerInvariant();

            if (dirName is "debug" or "release")
                return true;

            var parentDir = Path.GetDirectoryName(Path.GetDirectoryName(baseDir));
            var parentDirName = Path.GetFileName(parentDir)?.ToLowerInvariant();

            if (parentDirName == "bin")
                return true;

            return false;
        }

        public static string DataDirectory
        {
            get
            {
                if (IsDevelopmentEnvironment())
                    return Ensure(Path.Combine(BaseDirectory, "AppData"));

                return Ensure(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Company,
                    AppName
                ));
            }
        }

        public static string ConfigDirectory => Ensure(Path.Combine(DataDirectory, "Configs"));
        public static string LogsDirectory => Ensure(Path.Combine(DataDirectory, "Logs"));
        public static string ProfilesDirectory => Ensure(Path.Combine(DataDirectory, "Profiles"));
        public static string CacheDirectory => Ensure(Path.Combine(DataDirectory, "Cache"));

        public static string GetConfigPath(string name) => Path.Combine(ConfigDirectory, name);

        public static string ProgramCfgPath => GetConfigPath("program.cfg");

        public static void EnsureProgramCfgExists()
        {
            if (!File.Exists(ProgramCfgPath))
                throw new FileNotFoundException($"Configuration file not found: {ProgramCfgPath}");
        }

        private static string Ensure(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }
    }
}

