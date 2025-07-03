using ModdingManager.classes.functional.search;
using System.Collections.Generic;
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

    public static string SearchFileInFolder(string folderPath, string filePattern)
    {
        if (string.IsNullOrEmpty(folderPath) ||
            string.IsNullOrEmpty(filePattern) ||
            !Directory.Exists(folderPath))
        {
            return null;
        }

        // Delimiters that break words
        char[] separators = { '.', '\\', '/', '_', '+', '-' };

        // Prepare comparison
        var pattern = filePattern.Trim();
        var cmp = StringComparison.OrdinalIgnoreCase;

        var candidates = new List<(string Path, string Name, int Score)>();

        foreach (var filePath in Directory.EnumerateFiles(folderPath))
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            int score = 0;

            // 1) exact segment match
            var segments = name
                .Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Any(s => s.Equals(pattern, cmp)))
            {
                score = 100;
            }
            // 2) full-name equality
            else if (name.Equals(pattern, cmp))
            {
                score = 90;
            }
            // 3) prefix + delimiter
            else if (name.StartsWith(pattern, cmp) && name.Length > pattern.Length)
            {
                char next = name[pattern.Length];
                if (separators.Contains(next))
                {
                    score = 80;
                }
                else
                {
                    // a prefix but not broken by delimiter
                    score = 50;
                }
            }
            // 4) anywhere inside
            else if (name.IndexOf(pattern, cmp) >= 0)
            {
                score = 10;
            }

            if (score > 0)
            {
                candidates.Add((filePath, name, score));
            }
        }

        if (!candidates.Any())
        {
            return null;
        }

        // pick highest score, then shortest base-name
        var best = candidates
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Name.Length)
            .First();

        return best.Path;
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