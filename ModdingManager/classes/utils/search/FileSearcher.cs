using ModdingManager.classes.functional.search;
using ModdingManager.classes.utils.types;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.Windows.Forms.DataFormats;

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

        foreach (var pattern in patterns)
        {
            if (fileParts.Contains(pattern))
            {
                count++;
                matchedPatterns.Add(pattern);
                remainingPatterns.Remove(pattern);
            }
        }

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
    #region Var Methods
    public void SetVar(Var variable, int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= Files.Count || variable == null || variable.IsEmpty()) return;

        var stream = Files[fileIndex];
        stream.Seek(0, SeekOrigin.Begin);

        var encoding = variable.Format == Var.VarFormat.Localisation ? new UTF8Encoding(true) : new UTF8Encoding(false);
        var lines = new List<string>();

        using (var reader = new StreamReader(stream, encoding, true, 1024, true))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }

        bool replaced = false;
        string formatted = variable.ToString().TrimEnd();
        string name = variable.Name.Trim('"');
        string pattern = $@"^\s*{name}\s*{variable.AssignSymbol}\s*""[^""]*""\s*$";


        for (int i = 0; i < lines.Count; i++)
        {
            var match = Regex.Match(lines[i], pattern);
            if (match.Success)
            {
                if (variable.IsStringAreVar(lines[i]))
                {
                    string prefix = lines[i].Substring(0, lines[i].IndexOf(variable.AssignSymbol) + 1);
                    lines[i] = $"{prefix} {variable.Value}";
                }
                else
                {
                    lines[i] = formatted;
                }

                replaced = true;
                break;
            }
        }


        if (!replaced)
        {
            AddVar(variable, fileIndex, null);
            return;
        }

        stream.Seek(0, SeekOrigin.Begin);
        stream.SetLength(0);

        using (var writer = new StreamWriter(stream, encoding, 1024, true))
        {
            foreach (var line in lines)
            {
                writer.WriteLine(line);
            }
        }
    }

    public Var? GetVar(string name, int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= Files.Count || string.IsNullOrWhiteSpace(name)) return null;

        var stream = Files[fileIndex];
        stream.Seek(0, SeekOrigin.Begin);

        using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
        {
            string pattern = $@"^\s*{Regex.Escape(name)}\s*(.)\s*(.+)$";

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    char assignSymbol = match.Groups[1].Value[0];
                    string value = match.Groups[2].Value.Trim();
                    return new Var { Name = name, Value = value, AssignSymbol = assignSymbol };
                }
            }
        }

        return null;
    }

    public void AddVar(Var variable, int fileIndex, int? lineIndex = null)
    {
        if (fileIndex < 0 || fileIndex >= Files.Count || variable == null) return;

        var stream = Files[fileIndex];
        stream.Seek(0, SeekOrigin.Begin);

        var encoding = variable.Format == Var.VarFormat.Localisation ? new UTF8Encoding(true) : new UTF8Encoding(false);
        var lines = new List<string>();

        using (var reader = new StreamReader(stream, encoding, true, 1024, true))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }

        string formatted = FormatVar(variable);
        if (lineIndex.HasValue && lineIndex.Value >= 0 && lineIndex.Value < lines.Count)
        {
            lines.Insert(lineIndex.Value, formatted);
        }
        else
        {
            lines.Add(formatted);
        }

        stream.Seek(0, SeekOrigin.Begin);
        stream.SetLength(0);

        using (var writer = new StreamWriter(stream, encoding, 1024, true))
        {
            foreach (var line in lines)
            {
                writer.WriteLine(line);
            }
        }
    }

    public void RemoveVar(Var variable, int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= Files.Count || variable == null) return;

        var stream = Files[fileIndex];
        stream.Seek(0, SeekOrigin.Begin);

        var encoding = variable.Format == Var.VarFormat.Localisation ? new UTF8Encoding(true) : new UTF8Encoding(false);
        var lines = new List<string>();

        using (var reader = new StreamReader(stream, encoding, true, 1024, true))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }

        lines.RemoveAll(line => Regex.IsMatch(line, $@"^\s*{Regex.Escape(variable.Name)}\s*{Regex.Escape(variable.AssignSymbol.ToString())}", RegexOptions.IgnoreCase));

        stream.Seek(0, SeekOrigin.Begin);
        stream.SetLength(0);

        using (var writer = new StreamWriter(stream, encoding, 1024, true))
        {
            foreach (var line in lines)
            {
                writer.WriteLine(line);
            }
        }
    }
    #endregion

    #region Bracket Methods

    public Bracket? GetBracket(string header, int fileIndex,
    char openChar = '{', char closeChar = '}', char assignSymbol = '=', char commentSymbol = '#')
    {
        if (fileIndex < 0 || fileIndex >= Files.Count || string.IsNullOrWhiteSpace(header)) return null;

        string path = Files[fileIndex].Name;
        var lines = File.ReadAllLines(path).ToList();

        string pattern = $@"^\s*{Regex.Escape(header)}\s*{Regex.Escape(assignSymbol.ToString())}";
        int start = -1, end = -1, openCount = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            if (start == -1 && Regex.IsMatch(lines[i], pattern, RegexOptions.IgnoreCase))
            {
                start = i;
                openCount = lines[i].Count(c => c == openChar) - lines[i].Count(c => c == closeChar);
                if (openCount <= 0) openCount = 1;
            }
            else if (start != -1)
            {
                openCount += lines[i].Count(c => c == openChar);
                openCount -= lines[i].Count(c => c == closeChar);

                if (openCount <= 0)
                {
                    end = i;
                    break;
                }
            }
        }

        if (start == -1 || end == -1) return null;

        var blockLines = lines.GetRange(start, end - start + 1);
        return ParseBracketBlock(blockLines, start, header, openChar, closeChar, assignSymbol, commentSymbol);
    }

    /// <summary>
    /// Рекурсивный парсер для построения дерева Bracket.
    /// </summary>
    private Bracket ParseBracketBlock(List<string> blockLines, int startPosition, string header,
        char openChar, char closeChar, char assignSymbol, char commentSymbol)
    {
        var bracket = new Bracket
        {
            Header = header,
            OpenChar = openChar,
            CloseChar = closeChar,
            AssignSymbol = assignSymbol,
            CommentSymbol = commentSymbol,
            StartPosition = startPosition,
            EndPosition = startPosition + blockLines.Count - 1
        };

        // Убираем первую и последнюю строку (заголовок и закрывающую скобку)
        var innerLines = blockLines.Skip(1).Take(blockLines.Count - 2).ToList();
        bracket.Content = innerLines;

        for (int i = 0; i < innerLines.Count; i++)
        {
            string line = innerLines[i];
            string trimmed = line.Split(commentSymbol)[0].Trim();

            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            // Проверяем: это начало подскобки?
            if (trimmed.Contains(openChar))
            {
                // Получаем имя подскобки до AssignSymbol
                string subHeader = trimmed.Split(assignSymbol)[0].Trim();

                // Собираем блок подскобки
                int start = i;
                int openCount = line.Count(c => c == openChar) - line.Count(c => c == closeChar);
                if (openCount <= 0) openCount = 1;

                for (int j = i + 1; j < innerLines.Count; j++)
                {
                    openCount += innerLines[j].Count(c => c == openChar);
                    openCount -= innerLines[j].Count(c => c == closeChar);

                    if (openCount <= 0)
                    {
                        var subBlock = innerLines.GetRange(start, j - start + 1);
                        var subBracket = ParseBracketBlock(subBlock, start + 1, subHeader,
                            openChar, closeChar, assignSymbol, commentSymbol);

                        bracket.SubBrackets.Add(subBracket);
                        i = j;
                        break;
                    }
                }
            }
            else if (trimmed.Contains(assignSymbol))
            {
                // Это Var
                var parts = trimmed.Split(assignSymbol);
                if (parts.Length == 2)
                {
                    bracket.SubVars.Add(new Var
                    {
                        Name = parts[0].Trim(),
                        Value = parts[1].Trim()
                    });
                }
            }
        }

        return bracket;
    }

    public void SetBracket(Bracket bracket, int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= Files.Count || bracket == null) return;

        string path = Files[fileIndex].Name;
        var lines = File.ReadAllLines(path).ToList();

        // Если известны позиции блока, заменяем его
        if (bracket.StartPosition >= 0 && bracket.EndPosition < lines.Count)
        {
            lines.RemoveRange(bracket.StartPosition, bracket.EndPosition - bracket.StartPosition + 1);
            lines.InsertRange(bracket.StartPosition, bracket.ToString().Split('\n'));
        }
        else
        {
            // Иначе ищем по Header
            string pattern = $@"^\s*{Regex.Escape(bracket.Header)}\s*{Regex.Escape(bracket.AssignSymbol.ToString())}";
            int start = lines.FindIndex(l => Regex.IsMatch(l, pattern, RegexOptions.IgnoreCase));

            if (start != -1)
            {
                // Заменяем старый блок
                var existing = GetBracket(bracket.Header, fileIndex, bracket.OpenChar,
                    bracket.CloseChar, bracket.AssignSymbol, bracket.CommentSymbol);

                if (existing != null)
                {
                    lines.RemoveRange(existing.StartPosition, existing.EndPosition - existing.StartPosition + 1);
                    lines.InsertRange(existing.StartPosition, bracket.ToString().Split('\n'));
                }
            }
            else
            {
                // Добавляем в конец
                lines.AddRange(bracket.ToString().Split('\n'));
            }
        }

        File.WriteAllLines(path, lines);
    }

    public void AddBracket(Bracket bracket, int fileIndex, int? lineIndex = null)
    {
        if (fileIndex < 0 || fileIndex >= Files.Count || bracket == null) return;

        string path = Files[fileIndex].Name;
        var lines = File.ReadAllLines(path).ToList();
        var blockLines = bracket.ToString().Split('\n');

        if (lineIndex.HasValue && lineIndex.Value >= 0 && lineIndex.Value < lines.Count)
        {
            lines.InsertRange(lineIndex.Value, blockLines);
        }
        else
        {
            lines.AddRange(blockLines);
        }

        File.WriteAllLines(path, lines);
    }

    public void RemoveBracket(Bracket bracket, int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= Files.Count || bracket == null) return;

        string path = Files[fileIndex].Name;
        var lines = File.ReadAllLines(path).ToList();

        if (bracket.StartPosition >= 0 && bracket.EndPosition < lines.Count)
        {
            lines.RemoveRange(bracket.StartPosition, bracket.EndPosition - bracket.StartPosition + 1);
            File.WriteAllLines(path, lines);
        }
        else
        {
            // Если позиции неизвестны – ищем по Header
            var existing = GetBracket(bracket.Header, fileIndex, bracket.OpenChar,
                bracket.CloseChar, bracket.AssignSymbol, bracket.CommentSymbol);

            if (existing != null)
            {
                lines.RemoveRange(existing.StartPosition, existing.EndPosition - existing.StartPosition + 1);
                File.WriteAllLines(path, lines);
            }
        }
    }
    #endregion
    #region Helper methods
    private Encoding GetEncoding(string format)
    {
        return format == "localisation" ? new UTF8Encoding(true) : Encoding.UTF8;
    }
    private string FormatVar(Var variable) => variable.ToString();

    #endregion
}