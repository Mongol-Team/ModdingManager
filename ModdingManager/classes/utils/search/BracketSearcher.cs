using ModdingManager.classes.functional.search;
using ModdingManager.classes.utils.types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BracketSearcher : Searcher
{
    public BracketSearcher() { }
    public char OpenBracketChar { get; set; } = '{';
    public char CloseBracketChar { get; set; } = '}';
    public char CommentSymbol { get; set; } = '#';
    public char AssignSymbol { get; set; } = '=';

    /// <summary>
    /// Находит все скобки с указанным именем заголовка
    /// </summary>
    public List<Bracket> FindBracketsByName(string bracketName)
    {
        var brackets = new List<Bracket>();
        if (string.IsNullOrWhiteSpace(bracketName))
            return brackets;

        SearchPattern = bracketName.ToCharArray();
        int pos = 0;

        while (pos < CurrentString.Length)
        {
            pos = FindExactHeaderPosition(SearchPattern, pos);
            if (pos == -1) break;

            var bracket = GetBracketAtPosition(pos);
            if (bracket != null)
            {
                brackets.Add(bracket);
                pos = bracket.EndPosition + 1;
            }
            else
            {
                pos += SearchPattern.Length;
            }
        }

        return brackets;
    }

    /// <summary>
    /// Получает информацию о скобке по позиции заголовка
    /// </summary>
    private Bracket GetBracketAtPosition(int headerStart)
    {
        int headerEnd = headerStart + SearchPattern.Length;

        // Пропускаем пробелы после заголовка
        while (headerEnd < CurrentString.Length && char.IsWhiteSpace(CurrentString[headerEnd]))
            headerEnd++;

        // Проверяем наличие '=' после заголовка
        if (headerEnd < CurrentString.Length && CurrentString[headerEnd] == AssignSymbol)
        {
            headerEnd++;
            while (headerEnd < CurrentString.Length && char.IsWhiteSpace(CurrentString[headerEnd]))
                headerEnd++;
        }

        // Проверяем наличие открывающей скобки
        if (headerEnd >= CurrentString.Length || CurrentString[headerEnd] != OpenBracketChar)
            return null;

        int bracketStart = headerEnd;
        int bracketEnd = FindMatchingBracketEnd(bracketStart);
        if (bracketEnd == -1)
            return null;

        // Создаем объект Bracket
        var bracket = new Bracket
        {
            OpenChar = OpenBracketChar,
            CloseChar = CloseBracketChar,
            CommentSymbol = CommentSymbol,
            AssignSymbol = AssignSymbol,
            StartPosition = bracketStart,
            EndPosition = bracketEnd,
            Header = new string(CurrentString, headerStart, headerEnd - headerStart - 1).Trim()
        };

        // Парсим содержимое скобки
        ParseBracketContent(bracket, bracketStart + 1, bracketEnd - 1);

        return bracket;
    }

    /// <summary>
    /// Парсит содержимое скобки
    /// </summary>
    private void ParseBracketContent(Bracket bracket, int contentStart, int contentEnd)
    {
        int lineStart = contentStart;
        bool inSubBracket = false;
        int subBracketDepth = 0;
        int subBracketStart = 0;

        for (int i = contentStart; i <= contentEnd; i++)
        {
            if (CurrentString[i] == CommentSymbol)
            {
                // Пропускаем комментарии
                while (i <= contentEnd && CurrentString[i] != '\n' && CurrentString[i] != '\r')
                    i++;
                continue;
            }

            if (CurrentString[i] == OpenBracketChar && !inSubBracket)
            {
                inSubBracket = true;
                subBracketDepth = 1;
                subBracketStart = i;
                continue;
            }

            if (inSubBracket)
            {
                if (CurrentString[i] == OpenBracketChar) subBracketDepth++;
                if (CurrentString[i] == CloseBracketChar) subBracketDepth--;

                if (subBracketDepth == 0)
                {
                    inSubBracket = false;
                    var subBracket = GetBracketAtPosition(FindHeaderStart(subBracketStart));
                    if (subBracket != null)
                    {
                        bracket.AddSubBracket(subBracket);
                    }
                    lineStart = i + 1;
                }
                continue;
            }

            if (CurrentString[i] == '\n' || CurrentString[i] == '\r' || i == contentEnd)
            {
                if (lineStart < i)
                {
                    string line = new string(CurrentString, lineStart, i - lineStart).Trim();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        bracket.AddContent(line);

                        // Пытаемся распарсить как переменную
                        var var = TryParseVar(line);
                        if (var != null)
                        {
                            bracket.ContentVars.Add(var);
                        }
                    }
                }
                lineStart = i + 1;
            }
        }
    }

    /// <summary>
    /// Пытается распарсить строку как переменную
    /// </summary>
    private Var TryParseVar(string line)
    {
        int eqPos = line.IndexOf(AssignSymbol);
        if (eqPos <= 0) return null;

        string name = line.Substring(0, eqPos).Trim();
        string value = line.Substring(eqPos + 1).Trim();

        return new Var { Name = name, Value = value };
    }

    /// <summary>
    /// Находит начало заголовка для подскобки
    /// </summary>
    private int FindHeaderStart(int bracketPos)
    {
        int pos = bracketPos - 1;

        // Пропускаем пробелы перед скобкой
        while (pos >= 0 && char.IsWhiteSpace(CurrentString[pos]))
            pos--;

        // Ищем начало имени
        int end = pos;
        while (pos >= 0 && !char.IsWhiteSpace(CurrentString[pos]) &&
               CurrentString[pos] != '\n' && CurrentString[pos] != '\r')
            pos--;

        return pos + 1;
    }

    /// <summary>
    /// Находит все подскобки указанного уровня вложенности
    /// </summary>
    public List<Bracket> GetAllSubBrackets(int targetDepth = 1)
    {
        var results = new List<Bracket>();
        if (CurrentString == null || targetDepth < 1)
            return results;

        int bracketDepth = 0;
        int lineStart = 0;
        bool inComment = false;

        for (int i = 0; i < CurrentString.Length; i++)
        {
            if (CurrentString[i] == CommentSymbol)
            {
                inComment = true;
                continue;
            }

            if (CurrentString[i] == '\n')
            {
                inComment = false;
                lineStart = i + 1;
                continue;
            }

            if (inComment)
                continue;

            if (CurrentString[i] == OpenBracketChar)
            {
                if (bracketDepth == targetDepth - 1)
                {
                    int nameStart = lineStart;
                    while (nameStart < i && char.IsWhiteSpace(CurrentString[nameStart]))
                        nameStart++;

                    if (nameStart < i)
                    {
                        var bracket = GetBracketAtPosition(nameStart);
                        if (bracket != null)
                        {
                            results.Add(bracket);
                        }
                    }
                }
                bracketDepth++;
            }
            else if (CurrentString[i] == CloseBracketChar)
            {
                if (bracketDepth > 0)
                    bracketDepth--;
            }
        }

        return results;
    }

    #region Helper Methods
    private int FindMatchingBracketEnd(int startPos)
    {
        int depth = 1;
        for (int i = startPos + 1; i < CurrentString.Length; i++)
        {
            if (CurrentString[i] == OpenBracketChar)
                depth++;
            else if (CurrentString[i] == CloseBracketChar)
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }
        return -1;
    }

    private int FindExactHeaderPosition(char[] header, int startPos)
    {
        for (int i = startPos; i <= CurrentString.Length - header.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < header.Length; j++)
            {
                if (CurrentString[i + j] != header[j])
                {
                    match = false;
                    break;
                }
            }

            if (match && IsStandaloneWord(i, header.Length))
                return i;
        }
        return -1;
    }

    private bool IsStandaloneWord(int pos, int length)
    {
        if (pos > 0 && IsWordCharacter(CurrentString[pos - 1]))
            return false;

        if (pos + length < CurrentString.Length && IsWordCharacter(CurrentString[pos + length]))
            return false;

        return true;
    }

    private bool IsWordCharacter(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }
    #endregion
}