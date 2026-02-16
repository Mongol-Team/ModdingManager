namespace Application.Utils
{
    public static class AppPaths
    {
        public const string Company = "SME";
        public const string AppName = "ModdingManager";

        public const string ProgramConfigName = "program.cfg";

        public static string MyBaseDirectory => AppContext.BaseDirectory;

        private static bool IsDevelopmentEnvironment()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public static string DataDirectory
        {
            get
            {
                if (IsDevelopmentEnvironment())
                    return EnsureOrCreate(Path.Combine(MyBaseDirectory, "AppData"));

                return EnsureOrCreate(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Company,
                    AppName
                ));
            }
        }
        // Directories , we use "EnsureOrCreate" for them
        public static string ConfigDirectory => EnsureOrCreate(Path.Combine(DataDirectory, "Configs"));
        public static string LogsDirectory => EnsureOrCreate(Path.Combine(DataDirectory, "Logs"));
        public static string ProfilesDirectory => EnsureOrCreate(Path.Combine(DataDirectory, "Profiles"));
        public static string CacheDirectory => EnsureOrCreate(Path.Combine(DataDirectory, "Cache"));
        // Files , we use "Ensure" for them
        public static string ProgramConfigPath => Path.Combine(ConfigDirectory, ProgramConfigName);

        public static void Ensure(string dir)  { if (!File.Exists(dir)) throw new FileNotFoundException($"Configuration file not found: {dir}"); }

        private static string EnsureOrCreate(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }
    }
}

