using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.LochalizationData;
using System.Collections.Concurrent;
using System.Text;
namespace ModdingManagerClassLib.utils
{
    public class LocalisationRegistry
    {
        public LocalizationBlock VictoryPointsLocalisation { get; private set; }
        public LocalizationBlock IdeologyLocalisation { get; private set; }
        public LocalizationBlock CountryLocalisation { get; private set; }
        public LocalizationBlock StateLocalisation { get; private set; }
        public LocalizationBlock OtherLocalisation { get; private set; }

        public LocalisationRegistry()
        {
            LoadLocalisation();
        }
        /// <summary>
        /// Параллельно загружает все переменные из .yml файлов в AllCache (с учётом приоритетов)
        /// </summary>
        /// 


        private void LoadLocalisation()
        {
            string[] searchPaths = new[]
            {
                GamePathes.LocalisationPath,   
                GamePathes.LocalisationReplacePath,
            };
            var files = Directory.EnumerateFiles(GamePathes.LocalisationPath, "*.yml", SearchOption.AllDirectories)
    .Concat(Directory.EnumerateFiles(GamePathes.LocalisationReplacePath, "*.yml", SearchOption.AllDirectories))
    .ToList();

            // Потокобезопасные накопители по категориям.
            var vpDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var ideologyDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var countryDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var stateDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var otherDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            bool IsVictoryPoint(string k) => k.StartsWith("VICTORY_POINTS_", StringComparison.OrdinalIgnoreCase);
            bool IsIdeology(string k) => k.StartsWith("IDEOLOGY_", StringComparison.OrdinalIgnoreCase);
            bool IsState(string k) => k.StartsWith("STATE_", StringComparison.OrdinalIgnoreCase);
            bool IsCountry(string k) => k.Length == 3 && k.All(char.IsLetter);

            YmlParser parser = new YmlParser(new TxtPattern());

            Parallel.ForEach(files, file =>
            {
                try
                {
                    var text = File.ReadAllText(file, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                    var parsed = (LocalizationFile)parser.Parse(text);

                    // Пробегаем все блоки и все пары ключ/значение внутри файла
                    foreach (var block in parsed.localizations)
                    {
                        foreach (var kvp in block.Data)
                        {
                            var key = kvp.Key;
                            var val = kvp.Value;

                            // Определяем целевой словарь
                            ConcurrentDictionary<string, string> target =
                                IsVictoryPoint(key) ? vpDict :
                                IsIdeology(key) ? ideologyDict :
                                IsState(key) ? stateDict :
                                IsCountry(key) ? countryDict :
                                                      otherDict;

                            // Политика при коллизиях ключей: ПОСЛЕДНЕЕ значение побеждает.
                            target.AddOrUpdate(key, val, (_, __) => val);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.AddLog("Ошибка при загрузке файлов локализации: " + ex.Message);
                }
            });

            // Заполняем итоговые блоки (язык — на твое усмотрение; можно оставить по умолчанию)
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
        }
    }
}