using Models.Enums;
using Models.Interfaces;
using Models.Interfaces.RawDataWorkerInterfaces;
using RawDataWorker.Parsers.Errors;
using System.Text.RegularExpressions;

namespace RawDataWorker.Healers
{
    public class TxtHealer : IHealer
    {
        public TxtHealer()
        {
        }
        public List<IError> Errors { get; set; } = new();

        public (string fixedLine, bool success) HealError(IError error, string originalContent)
        {
            Errors.Add(error);

            switch (error)
            {
                case BracketInBracketError _:
                    // удаляем лишние пустые скобки {{ }}
                    string fixed1 = Regex.Replace(originalContent, @"\{\s*\}", "");
                    return (fixed1, true);

                case BracketWithoutAssignmentError _:
                    // вставляем = перед {
                    string fixed2 = Regex.Replace(originalContent, @"(\w+)\s*\{", "$1 = {");
                    return (fixed2, true);

                case UnexpectedContentAfterValueError ue:
                    // удаляем мусор после значения внутри брекета
                    // (нужна более точная логика в зависимости от того, как ты определяешь "мусор")
                    string fixed3 = Regex.Replace(originalContent, @"([^\s=]+)\s+[^={\s][^}]*", "$1");
                    return (fixed3, true);

                case ValueParsingError _:
                case InvalidAssignmentSyntaxError _ when error.Type == ErrorType.Critical:
                    // для Critical можно пробовать подставить дефолтное значение
                    // но это уже сложнее — требует знания контекста
                    // пока оставляем как есть или возвращаем false
                    return (originalContent, false);
                case UnbalancedBracketsError:
                    {
                        // Пытаемся автоматически подправить несбалансированные скобки в скриптах Paradox
                        if (string.IsNullOrWhiteSpace(originalContent))
                            return (originalContent, false);

                        var lines = originalContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
                        if (lines.Count < 3)
                            return (originalContent, false); // слишком маленький контент — чинить бессмысленно

                        var fixedLines = new List<string>(lines);
                        var openBracketStack = new Stack<int>(); // храним индексы строк с {
                        bool wasModified = false;

                        // Слова, после которых часто забывают открывающую скобку
                        var likelyBlockStarters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        {
                            "if", "else_if", "else", "and", "or", "not", "limit", "XOR", 
                            "any", "every", "random", "random_list",
                            "limit", "trigger", "effect", "immediate", "option", "ai_chance",
                            "create_equipment_variant", "add_equipment_production", "set_variable"
                        };

                        for (int i = 0; i < lines.Count; i++)
                        {
                            string line = lines[i];
                            string trimmed = line.TrimStart();

                            // Пропускаем пустые строки и комментарии
                            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                                continue;

                            // ─── Случай 1: забыли { после ключевого слова = ───────────────────────
                            if (trimmed.Contains('=') && !trimmed.Contains('{') && !trimmed.Contains('}'))
                            {
                                var match = Regex.Match(trimmed, @"^\s*([\w\.]+)\s*=");
                                if (match.Success)
                                {
                                    string keyword = match.Groups[1].Value.Trim();

                                    if (likelyBlockStarters.Contains(keyword) ||
                                        keyword.StartsWith("any_") ||
                                        keyword.StartsWith("every_") ||
                                        keyword.StartsWith("random_"))
                                    {
                                        // Проверяем, похоже ли дальше на содержимое блока
                                        bool seemsLikeBlock = false;
                                        for (int lookahead = 1; lookahead <= 7 && i + lookahead < lines.Count; lookahead++)
                                        {
                                            string nextLine = lines[i + lookahead].Trim();
                                            if (string.IsNullOrWhiteSpace(nextLine) || nextLine.StartsWith("#"))
                                                continue;

                                            if (nextLine.StartsWith("limit") ||
                                                nextLine.StartsWith("effect") ||
                                                nextLine.StartsWith("trigger") ||
                                                nextLine.Contains("=") ||
                                                Regex.IsMatch(nextLine, @"^\w+\s*=\s*[\w""{]"))
                                            {
                                                seemsLikeBlock = true;
                                                break;
                                            }

                                            // прерываем если видим else или закрытие
                                            if (nextLine.StartsWith("else") || nextLine.StartsWith("}"))
                                                break;
                                        }

                                        if (seemsLikeBlock)
                                        {
                                            // Вставляем { сразу после =
                                            int eqIndex = line.IndexOf('=');
                                            if (eqIndex >= 0)
                                            {
                                                string beforeEq = line.Substring(0, eqIndex + 1);
                                                string afterEq = line.Substring(eqIndex + 1);
                                                fixedLines[i] = beforeEq + " {" + afterEq;
                                                openBracketStack.Push(i);
                                                wasModified = true;
                                                continue; // переходим к следующей строке
                                            }
                                        }
                                    }
                                }
                            }

                            // ─── Подсчёт скобок в текущей строке ─────────────────────────────────
                            foreach (char c in trimmed)
                            {
                                if (c == '{')
                                {
                                    openBracketStack.Push(i);
                                }
                                else if (c == '}')
                                {
                                    if (openBracketStack.Count == 0)
                                    {
                                        // Лишняя закрывающая → закомментируем
                                        int closePos = line.IndexOf('}');
                                        if (closePos >= 0)
                                        {
                                            fixedLines[i] = line.Substring(0, closePos) +
                                                            "# }" +
                                                            line.Substring(closePos + 1);
                                            wasModified = true;
                                        }
                                    }
                                    else
                                    {
                                        openBracketStack.Pop();
                                    }
                                }
                            }
                        }

                        // ─── Случай 2: остались открытые блоки → закрываем в конце файла ───────
                        if (openBracketStack.Count > 0)
                        {
                            // Ищем позицию для вставки (после последней содержательной строки)
                            int insertPosition = lines.Count;
                            while (insertPosition > 0 &&
                                   string.IsNullOrWhiteSpace(fixedLines[insertPosition - 1].Trim()))
                            {
                                insertPosition--;
                            }

                            // Примерный отступ (можно улучшить, анализируя предыдущие строки)
                            string baseIndent = openBracketStack.Count > 2 ? "    " : "";
                            string closing = string.Join(Environment.NewLine,
                                Enumerable.Repeat(baseIndent + "}", openBracketStack.Count));

                            fixedLines.Insert(insertPosition, closing);
                            wasModified = true;
                        }

                        if (wasModified)
                        {
                            string repairedContent = string.Join(Environment.NewLine, fixedLines);
                            return (repairedContent, true); // удалось починить
                        }

                        // Не получилось безопасно починить
                        return (originalContent, false);
                    }
                default:
                    // Fatal и Warn не лечим
                    return (originalContent, false);
            }
        }
    }
}