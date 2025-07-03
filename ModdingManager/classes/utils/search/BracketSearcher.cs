using ModdingManager.classes.functional.search;
using System;
using System.Collections.Generic;
using System.Linq;

public class BracketSearcher : Searcher
{
    public BracketSearcher() { }
    public char OpenBracketChar { get; set; } = '{';
    public char CloseBracketChar { get; set; } = '}';
    public char CommentSymbol { get; set; } = '#';
    public List<string> GetBracketContentByHeaderName(char[] header)
    {
        var results = new List<string>();
        if (CurrentString == null || header == null || header.Length == 0)
            return results;

        int pos = 0;
        int headerLength = header.Length;

        while (pos < CurrentString.Length)
        {
            // Ищем начало заголовка
            pos = FindExactHeaderPosition(header, pos);
            if (pos == -1) break;

            // Проверяем, что это отдельное слово
            if (!IsStandaloneWord(pos, headerLength))
            {
                pos += headerLength;
                continue;
            }

            // Пропускаем пробелы после заголовка
            pos += headerLength;
            while (pos < CurrentString.Length && char.IsWhiteSpace(CurrentString[pos]))
                pos++;

            // Проверяем, что после заголовка идет '=' или открывающая скобка
            if (pos >= CurrentString.Length ||
                (CurrentString[pos] != '=' && CurrentString[pos] != OpenBracketChar))
            {
                pos++;
                continue;
            }

            // Ищем открывающую скобку
            int bracketStart = FindNextBracketStart(pos);
            if (bracketStart == -1) break;

            // Находим соответствующую закрывающую скобку
            int bracketEnd = FindMatchingBracketEnd(bracketStart);
            if (bracketEnd == -1) break;

            // Извлекаем содержимое
            var content = new char[bracketEnd - bracketStart - 1];
            Array.Copy(CurrentString, bracketStart + 1, content, 0, content.Length);
            results.Add(new string(content).Trim());

            pos = bracketEnd + 1;
        }

        return results;
    }

    private bool IsStandaloneWord(int pos, int length)
    {
        // Проверяем символ перед заголовком
        if (pos > 0 && IsWordCharacter(CurrentString[pos - 1]))
            return false;

        // Проверяем символ после заголовка
        if (pos + length < CurrentString.Length && IsWordCharacter(CurrentString[pos + length]))
            return false;

        return true;
    }

   

    public List<string> GetAllBracketSubbracketsNames(int targetDepth = 1)
    {
        var results = new List<string>();
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
                        int nameEnd = i - 1;
                        while (nameEnd > nameStart && char.IsWhiteSpace(CurrentString[nameEnd]))
                            nameEnd--;

                        if (nameEnd >= nameStart && CurrentString[nameEnd] == '=')
                            nameEnd--;

                        if (nameEnd >= nameStart)
                        {
                            var name = new string(CurrentString, nameStart, nameEnd - nameStart + 1);
                            if (!string.IsNullOrWhiteSpace(name))
                                results.Add(name.Trim());
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

    public List<string> GetAllSubbracetContent(char[] brackedHeader, char[] subbrackedHeader)
    {
        var results = new List<string>();
        var mainContents = GetBracketContentByHeaderName(brackedHeader);

        foreach (var content in mainContents)
        {
            var tempSearcher = new BracketSearcher
            {
                OpenBracketChar = this.OpenBracketChar,
                CloseBracketChar = this.CloseBracketChar,
                CurrentString = content.ToCharArray(),
            };

            results.AddRange(tempSearcher.GetBracketContentByHeaderName(subbrackedHeader));
        }

        return results;
    }

    #region Improved Helper Methods
    private int SearchFullPatternFromPosition(int startPos)
    {
        if (startPos >= CurrentString.Length) return -1;

        for (int i = startPos; i <= CurrentString.Length - SearchPattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < SearchPattern.Length; j++)
            {
                if (CurrentString[i + j] != SearchPattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return i;
        }
        return -1;
    }

    private int FindNextBracketStart(int startPos)
    {
        for (int i = startPos; i < CurrentString.Length; i++)
        {
            if (CurrentString[i] == OpenBracketChar)
                return i;
        }
        return -1;
    }

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

    private int FindNearestHeaderBeforeBracket(int bracketPos, int depthLevel)
    {
        int currentDepth = 0;
        int position = bracketPos - 1;

        // Ищем начало строки перед скобкой
        while (position >= 0)
        {
            if (CurrentString[position] == '\n' || CurrentString[position] == '\r')
            {
                currentDepth++;
                if (currentDepth > depthLevel)
                    return -1;
            }

            if (!char.IsWhiteSpace(CurrentString[position]) &&
                CurrentString[position] != '\n' &&
                CurrentString[position] != '\r')
            {
                break;
            }

            position--;
        }

        if (position < 0) return -1;

        // Ищем начало строки
        int headerStart = position;
        while (headerStart > 0 && !IsNewLine(CurrentString[headerStart - 1]))
            headerStart--;

        return headerStart;
    }

    private int FindHeaderEnd(int startPos)
    {
        int endPos = startPos;
        while (endPos < CurrentString.Length && !IsNewLine(CurrentString[endPos]))
            endPos++;
        return endPos;
    }
    private int FindHeaderEnd(int startPos, int maxPos)
    {
        int endPos = startPos;
        while (endPos < maxPos &&
               !IsNewLine(CurrentString[endPos]) &&
               CurrentString[endPos] != '=' &&
               CurrentString[endPos] != OpenBracketChar)
        {
            endPos++;
        }
        return endPos;
    }
    private bool IsNewLine(char c) => c == '\n' || c == '\r';

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

            if (match)
                return i;
        }
        return -1;
    }

    private bool IsWordCharacter(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }
    #endregion
}