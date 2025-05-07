using ModdingManager.classes.functional;
using System.IO;

public class FileSearcher : Searcher
{
    public List<FileStream> Files { get; set; } = new List<FileStream>();
    public List<string> PatternsList { get; set; } = new List<string>();
    public List<string> AllowedExtensions { get; set; } = new List<string>();

    
    private static readonly Dictionary<HashSet<string>, HashSet<string>> PatternEquivalents =
       new Dictionary<HashSet<string>, HashSet<string>>(HashSet<string>.CreateSetComparer())
       {
           [new HashSet<string> { "aa" }] = new HashSet<string> { "anti_air", "antiair" },
           [new HashSet<string> { "at" }] = new HashSet<string> { "anti_tank", "antitank" },
           [new HashSet<string> { "light", "tank" }] = new HashSet<string> { "armored" },
           [new HashSet<string> { "armored" }] = new HashSet<string> { "light", "tank" },
           [new HashSet<string> { "mot" }] = new HashSet<string> { "motorized" },
           [new HashSet<string> { "anti", "tank" }] = new HashSet<string> { "at", "anti_tank" }
       };

    public FileStream SearchFile()
    {
        if (Files == null || !Files.Any() || PatternsList == null || !PatternsList.Any())
            return null;

        var matchedFiles = new List<(FileStream file, int matchCount)>();

        foreach (var file in Files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            var fileParts = fileName.Split('_').ToHashSet(StringComparer.OrdinalIgnoreCase);

            int matchedPatterns = CountMatchedPatterns(PatternsList, fileParts);

            if (matchedPatterns > 0)
            {
                matchedFiles.Add((file, matchedPatterns));
            }
        }

        return matchedFiles
            .OrderByDescending(x => x.matchCount)
            .ThenBy(x => Path.GetFileNameWithoutExtension(x.file.Name).Length)
            .FirstOrDefault().file;
    }

    private int CountMatchedPatterns(List<string> patterns, HashSet<string> fileParts)
    {
        int count = 0;
        var remainingPatterns = new HashSet<string>(patterns, StringComparer.OrdinalIgnoreCase);
        var matchedPatterns = new HashSet<string>();

        // Проверяем прямые совпадения
        foreach (var pattern in patterns)
        {
            if (fileParts.Contains(pattern))
            {
                count++;
                matchedPatterns.Add(pattern);
                remainingPatterns.Remove(pattern);
            }
        }

        // Проверяем эквиваленты для оставшихся паттернов
        foreach (var equiv in PatternEquivalents)
        {
            // Если все паттерны из ключа еще не найдены
            if (equiv.Key.IsSubsetOf(remainingPatterns))
            {
                // Проверяем есть ли в файле хотя бы один эквивалент
                foreach (var replacement in equiv.Value)
                {
                    if (fileParts.Contains(replacement))
                    {
                        count += equiv.Key.Count; // Засчитываем все паттерны из ключа
                        foreach (var p in equiv.Key)
                        {
                            matchedPatterns.Add(p);
                            remainingPatterns.Remove(p);
                        }
                        break;
                    }
                }
            }
        }

        return count;
    }
    
    private List<HashSet<string>> GetAllPatternVariants(List<string> originalPatterns)
    {
        var variants = new List<HashSet<string>> { originalPatterns.ToHashSet() };

        // Генерируем варианты замены через эквиваленты
        foreach (var equiv in PatternEquivalents)
        {
            if (equiv.Key.IsSubsetOf(originalPatterns.ToHashSet()))
            {
                foreach (var replacement in equiv.Value)
                {
                    var newVariant = originalPatterns
                        .Where(p => !equiv.Key.Contains(p))
                        .Concat(new[] { replacement })
                        .ToHashSet();
                    variants.Add(newVariant);
                }
            }
        }

        return variants;
    }

    private int CountMatchedPatterns(HashSet<string> patterns, HashSet<string> fileParts)
    {
        int count = 0;
        foreach (var pattern in patterns)
        {
            if (fileParts.Contains(pattern)
                || PatternEquivalents.Any(e =>
                    e.Key.Contains(pattern) &&
                    e.Value.Any(v => fileParts.Contains(v))))
            {
                count++;
            }
        }
        return count;
    }
}