using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.LochalizationData;
using System.Collections.Concurrent;

namespace ModdingManagerClassLib.utils
{
    public class LocalisationRegistry
    {
        public LocalizationBlock VictoryPointsLocalisation { get; private set; }
        public LocalizationBlock IdeologyLocalisation { get; private set; }
        public LocalizationBlock CountryLocalisation { get; private set; }
        public LocalizationBlock StateLocalisation { get; private set; }
        public LocalizationBlock OtherLocalisation { get; private set; }
        public List<string> FailedFiles { get; private set; }
        public LocalisationRegistry() { LoadLocalisation(); }
        public KeyValuePair<string, string> GetLocalisationByKey(string key)
        {
            if (VictoryPointsLocalisation.Data.TryGetValue(key, out var val))
                return new KeyValuePair<string, string>(key, val);
            if (IdeologyLocalisation.Data.TryGetValue(key, out val))
                return new KeyValuePair<string, string>(key, val);
            if (CountryLocalisation.Data.TryGetValue(key, out val))
                return new KeyValuePair<string, string>(key, val);
            if (StateLocalisation.Data.TryGetValue(key, out val))
                return new KeyValuePair<string, string>(key, val);
            if (OtherLocalisation.Data.TryGetValue(key, out val))
                return new KeyValuePair<string, string>(key, val);
            return new KeyValuePair<string, string>(key, val);
        }

        /// <summary>
        /// Параллельно загружает все переменные из .yml файлов в AllCache (с учётом приоритетов)
        /// </summary>
        /// 
        public void LoadLocalisation()
        {
            string[] searchPaths = new[]
            {
                GamePathes.LocalisationPath,
                GamePathes.LocalisationReplacePath,
            };
            var files = Directory.EnumerateFiles(GamePathes.LocalisationPath, "*.yml", SearchOption.AllDirectories).ToList();
            files = files.OrderBy(s => s).ToList();
            var vpDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var ideologyDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var countryDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var stateDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var otherDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            bool IsVictoryPoint(string k) => k.StartsWith("VICTORY_POINTS_", StringComparison.OrdinalIgnoreCase);
            bool IsIdeology(string k) => k.StartsWith("IDEOLOGY_", StringComparison.OrdinalIgnoreCase);
            bool IsState(string k) => ModManager.Mod.Map.States == null ? k.StartsWith("STATE_", StringComparison.OrdinalIgnoreCase) : ModManager.Mod.Map.States.Count(s => s.LocalizationKey == k) != 0;
            bool IsCountry(string k) => k.Length == 3 && k.All(char.IsLetter);

            YmlParser parser = new YmlParser(new TxtPattern());

            var failedFiles = new ConcurrentBag<string>();

            Parallel.ForEach(files, file =>
            {
                try
                {
                    var text = File.ReadAllText(file);
                    var parsed = (LocalizationFile)parser.Parse(text);
                    if (parsed.Localizations.Count < 1)
                    {
                        failedFiles.Add(file);
                        return;
                    }
                    foreach (var block in parsed.Localizations)
                    {
                        foreach (var kvp in block.Data)
                        {
                            var key = kvp.Key;
                            var val = kvp.Value;

                            ConcurrentDictionary<string, string> target =
                                IsVictoryPoint(key) ? vpDict :
                                IsIdeology(key) ? ideologyDict :
                                IsState(key) ? stateDict :
                                IsCountry(key) ? countryDict :
                                                      otherDict;

                            target.AddOrUpdate(key, val, (_, __) => val);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.AddDbgLog("Ошибка при загрузке файлов локализации: " + ex.Message);
                }
            });

            VictoryPointsLocalisation = new LocalizationBlock
            {
                Data = new Dictionary<string, string>(vpDict, StringComparer.OrdinalIgnoreCase)
            };

            IdeologyLocalisation = new LocalizationBlock
            {
                Data = new Dictionary<string, string>(ideologyDict, StringComparer.OrdinalIgnoreCase)
            };

            CountryLocalisation = new LocalizationBlock
            {
                Data = new Dictionary<string, string>(countryDict, StringComparer.OrdinalIgnoreCase)
            };

            StateLocalisation = new LocalizationBlock
            {
                Data = new Dictionary<string, string>(stateDict, StringComparer.OrdinalIgnoreCase)
            };

            OtherLocalisation = new LocalizationBlock
            {
                Data = new Dictionary<string, string>(otherDict, StringComparer.OrdinalIgnoreCase)
            };

            FailedFiles = failedFiles.ToList();
        }
    }
}