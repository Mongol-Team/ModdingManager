using ModdingManager.classes.functional.search;
using ModdingManagerModels.Types.ObectCacheData;

public class BracketSearcher : Searcher
{
    public BracketSearcher() { }
    public char OpenBracketChar { get; set; } = '{';
    public char CloseBracketChar { get; set; } = '}';
    public char CommentSymbol { get; set; } = '#';
    public char AssignSymbol { get; set; } = '=';
    private int maxRecursionDepth = 50;
    private int currentRecursionDepth = 0;

    /// <summary>
    /// Находит все скобки с указанным именем заголовка
    /// </summary>
    public List<Bracket> FindBracketsByName(string bracketName, string prefixToIgnore = null)
    {
        currentRecursionDepth = 0;
        var brackets = new List<Bracket>();
        if (string.IsNullOrWhiteSpace(bracketName))
            return brackets;

        SearchPattern = bracketName.ToCharArray();
        int pos = 0;

        while (pos < CurrentString.Length)
        {
            pos = FindExactHeaderPosition(SearchPattern, pos);
            if (pos == -1) break;

            var bracket = GetBracketAtPosition(pos, prefixToIgnore);
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
    private Bracket GetBracketAtPosition(int headerStart, string prefixToIgnore = null)
    {
        if (currentRecursionDepth > maxRecursionDepth)
            return null;

        currentRecursionDepth++;

        try
        {
            if (headerStart < 0 || headerStart >= CurrentString.Length)
                return null;

            int headerEnd = headerStart;
            bool hasAssign = false;

            // Собираем заголовок до пробела, '=' или '{'
            while (headerEnd < CurrentString.Length)
            {
                char c = CurrentString[headerEnd];
                if (char.IsWhiteSpace(c) || c == AssignSymbol || c == OpenBracketChar)
                    break;

                headerEnd++;
            }

            // Пропускаем пробелы после заголовка
            int posAfterHeader = headerEnd;
            while (posAfterHeader < CurrentString.Length && char.IsWhiteSpace(CurrentString[posAfterHeader]))
                posAfterHeader++;

            // Обработка присваивания
            if (posAfterHeader < CurrentString.Length && CurrentString[posAfterHeader] == AssignSymbol)
            {
                hasAssign = true;
                posAfterHeader++;
                while (posAfterHeader < CurrentString.Length && char.IsWhiteSpace(CurrentString[posAfterHeader]))
                    posAfterHeader++;
            }

            // 🔹 Если есть префикс (например rgb), пропускаем его
            if (!string.IsNullOrEmpty(prefixToIgnore))
            {
                int prefixEnd = posAfterHeader;
                while (prefixEnd < CurrentString.Length && !char.IsWhiteSpace(CurrentString[prefixEnd]) && CurrentString[prefixEnd] != OpenBracketChar)
                    prefixEnd++;

                string foundPrefix = new string(CurrentString, posAfterHeader, prefixEnd - posAfterHeader);
                if (string.Equals(foundPrefix, prefixToIgnore, StringComparison.OrdinalIgnoreCase))
                {
                    posAfterHeader = prefixEnd;
                    while (posAfterHeader < CurrentString.Length && char.IsWhiteSpace(CurrentString[posAfterHeader]))
                        posAfterHeader++;
                }
            }

            // Проверка открывающей скобки
            if (posAfterHeader >= CurrentString.Length || CurrentString[posAfterHeader] != OpenBracketChar)
                return null;

            int bracketStart = posAfterHeader;
            int bracketEnd = FindMatchingBracketEnd(bracketStart);
            if (bracketEnd == -1)
                return null;

            // Извлекаем заголовок
            string headerName = new string(CurrentString, headerStart, headerEnd - headerStart).Trim();

            if (hasAssign && headerName.EndsWith("="))
                headerName = headerName.Substring(0, headerName.Length - 1).Trim();

            var bracket = new Bracket
            {
                OpenChar = OpenBracketChar,
                CloseChar = CloseBracketChar,
                CommentSymbol = CommentSymbol,
                AssignSymbol = AssignSymbol,
                StartPosition = bracketStart,
                EndPosition = bracketEnd,
                Header = headerName
            };

            // Рекурсивный парсинг содержимого
            ParseBracketContent(bracket, bracketStart + 1, bracketEnd - 1);
            return bracket;
        }
        finally
        {
            currentRecursionDepth--;
        }
    }


    /// <summary>
    /// Парсит содержимое скобки без рекурсивных вызовов GetBracketAtPosition
    /// </summary>
    private void ParseBracketContentNonRecursive(Bracket bracket, int contentStart, int contentEnd)
    {
        int lineStart = contentStart;
        int depth = 0;
        var bracketStack = new Stack<Bracket>();

        for (int i = contentStart; i <= contentEnd; i++)
        {
            char c = CurrentString[i];

            // Обработка комментариев только на нулевом уровне
            if (c == CommentSymbol && depth == 0)
            {
                // Добавляем содержимое до комментария
                if (i > lineStart)
                {
                    string cleanLine = new string(CurrentString, lineStart, i - lineStart).Trim();
                    if (!string.IsNullOrWhiteSpace(cleanLine))
                    {
                        AddContentWithVar(bracket, cleanLine);
                    }
                }

                // Пропускаем комментарий
                while (i <= contentEnd && CurrentString[i] != '\n' && CurrentString[i] != '\r')
                    i++;

                lineStart = i + 1;
                continue;
            }

            // Обработка вложенных скобок
            if (c == OpenBracketChar)
            {
                if (depth == 0)
                {
                    // Нашли начало новой подскобки
                    int subHeaderStart = FindHeaderStart(i);
                    if (subHeaderStart >= 0 && subHeaderStart < i)
                    {
                        // Создаем новую подскобку
                        var subBracket = new Bracket
                        {
                            OpenChar = OpenBracketChar,
                            CloseChar = CloseBracketChar,
                            StartPosition = i
                        };

                        // Определяем заголовок
                        int subHeaderEnd = i - 1;
                        while (subHeaderEnd >= subHeaderStart && char.IsWhiteSpace(CurrentString[subHeaderEnd]))
                            subHeaderEnd--;

                        subBracket.Header = new string(CurrentString, subHeaderStart, subHeaderEnd - subHeaderStart + 1)
                            .Trim().TrimEnd(AssignSymbol).Trim();

                        bracketStack.Push(subBracket);
                    }
                }
                depth++;
            }
            else if (c == CloseBracketChar)
            {
                depth--;

                if (depth == 0 && bracketStack.Count > 0)
                {
                    // Завершаем подскобку
                    var subBracket = bracketStack.Pop();
                    subBracket.EndPosition = i;

                    // Парсим содержимое подскобки (без рекурсии)
                    ParseSubBracketContent(subBracket, subBracket.StartPosition + 1, i - 1);

                    // Добавляем подскобку в родительскую
                    bracket.AddSubBracket(subBracket);

                    // Добавляем всю подскобку как единый элемент контента
                    string fullBracketContent = new string(CurrentString,
                        subBracket.StartPosition - subBracket.Header.Length - (subBracket.Header.Length > 0 ? 1 : 0),
                        i - (subBracket.StartPosition - subBracket.Header.Length - (subBracket.Header.Length > 0 ? 1 : 0)) + 1);

                    bracket.AddContent(fullBracketContent);
                    lineStart = i + 1;
                }
            }

            // Обработка конца строки или контента
            if (depth == 0 && (c == '\n' || c == '\r' || i == contentEnd))
            {
                if (i >= lineStart)
                {
                    int len = (i == contentEnd) ? (i - lineStart + 1) : (i - lineStart);
                    if (len > 0)
                    {
                        string line = new string(CurrentString, lineStart, len).Trim();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            AddContentWithVar(bracket, line);
                        }
                    }
                }
                lineStart = i + 1;
            }
        }
    }

    /// <summary>
    /// Добавляет строку контента и пытается распарсить ее как переменную
    /// </summary>
    private void AddContentWithVar(Bracket bracket, string line)
    {
        // Пробуем распарсить как переменную
        var variable = TryParseVar(line);
        if (variable != null)
        {
            bracket.AddVar(variable);

            // Удаляем оригинальное представление переменной
            bracket.Content.RemoveAll(l => l == variable.ToString());
        }
        else
        {
            bracket.AddContent(line);
        }
    }

    /// <summary>
    /// Парсит содержимое подскобки (упрощенная версия без вложенных скобок)
    /// </summary>
    private void ParseSubBracketContent(Bracket bracket, int contentStart, int contentEnd)
    {
        int lineStart = contentStart;
        int depth = 0;

        for (int i = contentStart; i <= contentEnd; i++)
        {
            char c = CurrentString[i];

            if (c == OpenBracketChar)
            {
                depth++;
            }
            else if (c == CloseBracketChar)
            {
                depth--;
            }

            // Обработка конца строки
            if (c == '\n' || c == '\r' || i == contentEnd || depth > 0)
            {
                if (depth == 0 && i >= lineStart)
                {
                    int len = (i == contentEnd) ? (i - lineStart + 1) : (i - lineStart);
                    if (len > 0)
                    {
                        string line = new string(CurrentString, lineStart, len).Trim();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            AddContentWithVar(bracket, line);
                        }
                    }
                    lineStart = i + 1;
                }
            }
        }
    }
    /// <summary>
    /// Парсит содержимое скобки
    /// </summary>
    private void ParseBracketContent(Bracket bracket, int contentStart, int contentEnd)
    {
        int lineStart = contentStart;
        int depth = 0;  // Глубина для обработки "голых" скобок без заголовков

        for (int i = contentStart; i <= contentEnd; i++)
        {
            char c = CurrentString[i];

            // Обработка комментариев только на нулевом уровне
            if (c == CommentSymbol && depth == 0)
            {
                // Добавляем содержимое до комментария
                if (i > lineStart)
                {
                    string cleanLine = new string(CurrentString, lineStart, i - lineStart).Trim();
                    if (!string.IsNullOrWhiteSpace(cleanLine))
                    {
                        AddContentWithVar(bracket, cleanLine);
                    }
                }

                // Пропускаем комментарий
                while (i <= contentEnd && CurrentString[i] != '\n' && CurrentString[i] != '\r')
                    i++;

                lineStart = i + 1;
                continue;
            }

            // Обработка вложенных скобок
            if (c == OpenBracketChar)
            {
                // Если на нулевой глубине - пытаемся распарсить как подскобку
                if (depth == 0)
                {
                    int subHeaderStart = FindHeaderStart(i);
                    if (subHeaderStart >= 0 && subHeaderStart < i)
                    {
                        var subBracket = GetBracketAtPosition(subHeaderStart);
                        if (subBracket != null)
                        {
                            // Добавляем подскобку и переходим к её концу
                            bracket.AddSubBracket(subBracket);
                            i = subBracket.EndPosition;
                            lineStart = i + 1;
                            continue;
                        }
                    }
                }

                // Увеличиваем глубину если не нашли валидную подскобку
                depth++;
            }
            if (c == OpenBracketChar && depth == 0)
            {
                int subHeaderStart = FindHeaderStart(i);
                if (subHeaderStart >= 0 && subHeaderStart < i)
                {
                    var subBracket = GetBracketAtPosition(subHeaderStart);
                    if (subBracket != null)
                    {
                        bracket.AddSubBracket(subBracket);
                        i = subBracket.EndPosition;
                        lineStart = i + 1;

                        // Удаляем оригинальное представление из Content
                        string bracketRepresentation = subBracket.ToString();
                        bracket.Content.RemoveAll(line => line == bracketRepresentation);
                        continue;
                    }
                }
                depth++;
            }
            else if (c == CloseBracketChar && depth > 0)
            {
                depth--;
            }

            // Обработка конца строки или контента
            if (depth == 0 && (c == '\n' || c == '\r' || i == contentEnd))
            {
                if (i >= lineStart)
                {
                    int len = (i == contentEnd) ? (i - lineStart + 1) : (i - lineStart);
                    if (len > 0)
                    {
                        string line = new string(CurrentString, lineStart, len).Trim();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            AddContentWithVar(bracket, line);
                        }
                    }
                }

                if (i < contentEnd)
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

        // Если встретили '=', пропускаем его и пробелы перед ним
        if (pos >= 0 && CurrentString[pos] == AssignSymbol)
        {
            pos--; // Переходим перед '='
            while (pos >= 0 && char.IsWhiteSpace(CurrentString[pos]))
                pos--;
        }

        // Теперь ищем начало токена (числа/даты/слова)
        int end = pos;
        while (pos >= 0 && !char.IsWhiteSpace(CurrentString[pos]) &&
               CurrentString[pos] != '\n' && CurrentString[pos] != '\r' &&
               CurrentString[pos] != AssignSymbol)
        {
            pos--;
        }

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

            if (match)
            {
                // Проверка границ слова
                bool validStart = (i == 0) ||
                    !IsWordCharacter(CurrentString[i - 1]) ||
                    (CurrentString[i - 1] == CloseBracketChar);

                bool validEnd = (i + header.Length == CurrentString.Length) ||
                    !IsWordCharacter(CurrentString[i + header.Length]) ||
                    (CurrentString[i + header.Length] == OpenBracketChar);

                if (validStart && validEnd)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private bool IsWordCharacter(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }
    #endregion
}