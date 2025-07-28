using ModdingManager.classes.utils.search;
using ModdingManager.classes.utils.types;
using ModdingManager.managers.utils;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
namespace ModdingManager.classes.cache
{
    public class LocalisationCache
    {
        public List<Var> AllCache { get; private set; } = new();
        public List<Var> VictoryPointsLocalisation { get; private set; } = new();
        public List<Var> IdeologyLocalisation { get; private set; } = new();
        public List<Var> CountryLocalisation { get; private set; } = new();
        public List<Var> StateLocalisation { get; private set; } = new();

        /// <summary>
        /// Параллельно загружает все переменные из .yml файлов в AllCache (с учётом приоритетов)
        /// </summary>
        public void LoadCache()
        {
            string[] searchPaths = new[]
            {
                Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage),                // самый низкий приоритет
                Path.Combine(ModManager.GameDirectory, "localisation", ModManager.CurrentLanguage, "replace"),
                Path.Combine(ModManager.Directory, "localisation", ModManager.CurrentLanguage),
                Path.Combine(ModManager.Directory, "localisation", ModManager.CurrentLanguage, "replace")          // самый высокий приоритет
            };

            var cache = new ConcurrentDictionary<string, Var>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in searchPaths)
            {
                if (!Directory.Exists(path)) continue;

                var files = Directory.GetFiles(path, "*.yml", SearchOption.AllDirectories);

                Parallel.ForEach(files, file =>
                {
                    string content = File.ReadAllText(file, new UTF8Encoding(true));
                    var vars = VarSearcher.ParseAssignments(content, ':');

                    foreach (var variable in vars)
                    {
                        cache[variable.Name] = variable;
                        variable.AddProperty("sourcePath", file);
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