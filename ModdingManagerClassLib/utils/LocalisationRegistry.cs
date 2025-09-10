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

        public LocalisationRegistry(List<StateConfig>? states = null)
        {
            LoadLocalisation(states);
        }
        /// <summary>
        /// Параллельно загружает все переменные из .yml файлов в AllCache (с учётом приоритетов)
        /// </summary>
        /// 


        private async void LoadLocalisation(List<StateConfig>? states)
        {
            string[] searchPaths = new[]
            {
                GamePathes.LocalisationPath,
                GamePathes.LocalisationReplacePath,
            };
            var files = Directory.EnumerateFiles(GamePathes.LocalisationPath, "*.yml", SearchOption.AllDirectories).ToList();
            files = files.OrderBy(s => s).ToList();

            // Потокобезопасные накопители по категориям.
            var vpDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var ideologyDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var countryDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var stateDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var otherDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            bool IsVictoryPoint(string k) => k.StartsWith("VICTORY_POINTS_", StringComparison.OrdinalIgnoreCase);
            bool IsIdeology(string k) => k.StartsWith("IDEOLOGY_", StringComparison.OrdinalIgnoreCase);
            bool IsState(string k) =>
                states == null ? k.StartsWith("STATE_", StringComparison.OrdinalIgnoreCase) : states.Count(s => s.LocalizationKey == k) != 0;
            bool IsCountry(string k) => k.Length == 3 && k.All(char.IsLetter);

            YmlParser parser = new YmlParser(new TxtPattern());
            Console.WriteLine($"files  - {files.Count}");
            Console.WriteLine($"fimoz - {files.Count(f => f == "C:\\Users\\timpf\\Downloads\\Telegram Desktop\\SME\\SME\\localisation\\russian\\state_names_l_russian.yml")}");
            var f = 0;
            Parallel.ForEach(files, file =>
            {
                try
                {
                    var text = File.ReadAllText(file);
                    if (file == "C:\\Users\\timpf\\Downloads\\Telegram Desktop\\SME\\SME\\localisation\\russian\\state_names_l_russian.yml")
                        Console.WriteLine("HOI DEV HUESOS");
                    var parsed = (LocalizationFile)parser.Parse(text);
                    if (parsed.localizations.Count < 1)
                    {
                        Console.WriteLine(file.Split("\\").Last());
                        return;

                    }
                    // Пробегаем все блоки и все пары ключ/значение внутри файла
                    foreach (var block in parsed.localizations)
                    {
                        foreach (var kvp in block.Data)
                        {
                            var key = kvp.Key;
                            var val = kvp.Value;
                            if (file == "C:\\Users\\timpf\\Downloads\\Telegram Desktop\\SME\\SME\\localisation\\russian\\state_names_l_russian.yml")
                                Console.WriteLine(key + " " + val);
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
                    f++;
                }
                catch (Exception)
                {
                    //Logger.AddLog("Ошибка при загрузке файлов локализации: " + ex.Message);
                }
            });
            Console.WriteLine($" real files  - {f}");
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