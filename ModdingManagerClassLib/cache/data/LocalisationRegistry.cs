using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerModels.Types.ObjectCacheData;
using System.Collections.Concurrent;
using System.Text;
namespace ModdingManager.classes.cache.data
{
    public class LocalisationRegistry
    {
        public List<Var> AllCache { get; private set; } = new();
        public List<Var> VictoryPointsLocalisation { get; private set; } = new();
        public List<Var> IdeologyLocalisation { get; private set; } = new();
        public List<Var> CountryLocalisation { get; private set; } = new();
        public List<Var> StateLocalisation { get; private set; } = new();

        public LocalisationRegistry()
        {
            LoadLocalisation();
        }
        /// <summary>
        /// Параллельно загружает все переменные из .yml файлов в AllCache (с учётом приоритетов)
        /// </summary>
        public void LoadCache()
        {
            string[] searchPaths = new[]
            {
                Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage),                // самый низкий приоритет
                Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace"),
                Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage),
                Path.Combine(ModManager.ModDirectory, "localisation", ModManager.CurrentLanguage, "replace")          // самый высокий приоритет
            };

            var cache = new ConcurrentDictionary<string, Var>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in searchPaths)
            {
                if (!Directory.Exists(path)) continue;

                var files = Directory.GetFiles(path, "*.yml", SearchOption.AllDirectories);

                Parallel.ForEach(files, file =>
                {
                    try
                    {
                        string content = File.ReadAllText(file, new UTF8Encoding(true));
                        
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog("Ошибка при загрузке файлов локализации: " + ex.Message);
                    }
                });
            }

            AllCache = cache.Values.ToList();
        }

        public void LoadLocalisation()
        {
            LoadCache();
            LoadVictoryPoints();
            LoadIdeologies();
            LoadCountries();
            LoadStates();
        }

        public void LoadVictoryPoints() =>
            VictoryPointsLocalisation = AllCache
                .Where(v => v.Name.StartsWith("VICTORY_POINTS_", StringComparison.OrdinalIgnoreCase))
                .ToList();

        public void LoadIdeologies() =>
            IdeologyLocalisation = AllCache
                .Where(v => v.Name.StartsWith("IDEOLOGY_", StringComparison.OrdinalIgnoreCase))
                .ToList();

        public void LoadCountries() =>
            CountryLocalisation = AllCache
                .Where(v => v.Name.Length == 3 && v.Name.All(char.IsLetter))
                .ToList();

        public void LoadStates() =>
            StateLocalisation = AllCache
                .Where(v => v.Name.StartsWith("STATE_", StringComparison.OrdinalIgnoreCase))
                .ToList();
    }
}