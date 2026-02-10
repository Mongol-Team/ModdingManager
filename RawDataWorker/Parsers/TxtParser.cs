using Models.Enums;
using Models.Interfaces;
using Models.Interfaces.RawDataWorkerInterfaces;
using Models.Types.ObectCacheData;
using Models.Types.ObjectCacheData;
using RawDataWorker.Parsers.Errors;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Rx = RawDataWorker.Regexes;
using Data;
using RawDataWorker.Healers;

namespace RawDataWorker.Parsers
{
    public class TxtParser : Parser
    {
        public readonly IHealer? healer;

        public TxtParser(IParsingPattern pattern, IHealer? healer = null) : base(pattern)
        {
            if(healer != null)
                this.healer = healer;
            else
                this.healer = new TxtHealer();
        }

        protected override IHoiData ParseRealization(string content)
        {
            Normalize(ref content);

            var result = new HoiFuncFile();

            // Проверка баланса скобок — если не совпадает, это Fatal → не лечим
            var (openCount, closeCount) = CountBraces(content, pattern.OpenChar[0], pattern.CloseChar[0]);
            if (openCount != closeCount)
            {
                var error = new UnbalancedBracketsError(openCount, closeCount);
                if (healer != null)
                {
                    var (fixedContent, success) = healer.HealError(error, content);
                    if (!success)
                    {
                        throw new Exception($"[TxtParser] Fatal: {error.Message}");
                    }
                    content = fixedContent;
                }
                else
                {
                    throw new Exception($"[TxtParser] Fatal: {error.Message}");
                }
            }

            // Парсим брекеты
            content = Rx.FindBracket.Replace(content, m =>
            {
                var bracket = ParseBracketWithHealing(m.Value);
                if (bracket != null)
                {
                    result.Brackets.Add(bracket);
                }
                return string.Empty;
            });

            // Парсим переменные (Var)
            var varMatches = Rx.FindVar.Matches(content);
            foreach (Match match in varMatches)
            {
                var varItem = ParseVarWithHealing(match.Value);
                if (varItem != null)
                {
                    result.Vars.Add(varItem);
                }
            }

            // Парсим массивы
            var arrayMatches = Rx.Array.Matches(content);
            foreach (Match match in arrayMatches)
            {
                var array = ParseArrayWithHealing(match.Value);
                result.Arrays.Add(array);
            }

            return result;
        }

        protected override void Normalize(ref string content)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            content = content.Replace("\r\n", "\n");
            content = Rx.FileComment.Replace(content, "");
            content = Rx.EscapeCharsAroundAssignChar.Replace(content, "=");
            content = Rx.EmptyLine.Replace(content, "");


            stopwatch.Stop();
        }

        private Bracket? ParseBracketWithHealing(string originalContent)
        {
            string current = originalContent;
            int retries = 0;
            const int MAX_RETRIES = 5;

            while (retries < MAX_RETRIES)
            {
                var localErrors = healer.Errors;
                var bracket = ParseBracketInternal(current, localErrors);

                if (bracket != null && !localErrors.Any(e => e.Type == ErrorType.Fatal))
                {
                    // Warn-ошибки оставляем, но не прерываем
                    foreach (var warn in localErrors.Where(e => e.Type == ErrorType.Warn))
                    {

                    }
                    return bracket;
                }

                // Обрабатываем только Critical-ошибки через хилер
                bool healed = false;
                foreach (var error in localErrors.Where(e => e.Type == ErrorType.Critical))
                {
                    if (healer == null) return null;

                    var (fixedContent, success) = healer.HealError(error, current);
                    if (success)
                    {
                        current = fixedContent;
                        healed = true;
                    }
                    else
                    {
                        throw new Exception($"[TxtParser] Не удалось исправить критическую ошибку: {error.Message}");
                    }
                }

                if (!healed)
                {
                    // если не было исправлений → дальше бессмысленно
                    return null;
                }

                retries++;
            }

            throw new Exception($"[TxtParser] Не удалось исправить брекет после {MAX_RETRIES} попыток: {originalContent}");
        }

        private Bracket? ParseBracketInternal(string content, List<IError> errors)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            var bracket = new Bracket();

            var nameMatch = Rx.BracketName.Match(content);
            bracket.Name = nameMatch.Success ? nameMatch.Value : DataDefaultValues.Null;

            var contentMatch = Rx.BracketContent.Match(content);
            var innerContent = contentMatch.Success ? contentMatch.Value : string.Empty;

            if (nameMatch.Success)
            {
                // ищем позицию имени и смотрим, есть ли = после имени и перед {
                string beforeOpening = content.Substring(0, content.IndexOf(pattern.OpenChar));
                if (!beforeOpening.Contains("="))
                {
                    errors.Add(new BracketWithoutAssignmentError(bracket.Name));
                }
            }

            // Рекурсивно парсим вложенные брекеты
            innerContent = Rx.FindBracket.Replace(innerContent, m =>
            {
                var subBracket = ParseBracketWithHealing(m.Value);
                if (subBracket != null)
                {
                    bracket.SubBrackets.Add(subBracket);
                }
                return string.Empty;
            });

            // Проверка на {{ ... }} без имени
            if (Rx.BracketInBracket.IsMatch(innerContent))
            {
                errors.Add(new BracketInBracketError());
            }

            // Парсим Var внутри
            var varMatches = Rx.FindVar.Matches(innerContent);
            foreach (Match match in varMatches)
            {
                var varItem = ParseVarWithHealing(match.Value);
                if (varItem != null)
                {
                    bracket.SubVars.Add(varItem);
                }
            }

            // Парсим массивы внутри
            var arrayMatches = Rx.Array.Matches(innerContent);
            foreach (Match match in arrayMatches)
            {
                var array = ParseArrayWithHealing(match.Value);
                bracket.Arrays.Add(array);
            }

            // Пустой брекет
            if (!bracket.SubBrackets.Any() && !bracket.SubVars.Any() && !bracket.Arrays.Any())
            {
                errors.Add(new EmptyBracketError(bracket.Name));
            }

            return bracket;
        }

        private Var? ParseVarWithHealing(string content)
        {
            var localErrors = healer.Errors;
            var parts = content.Split(pattern.AssignChar, 2);

            if (parts.Length != 2)
            {
                localErrors.Add(InvalidAssignmentSyntaxError.TripleEquals(content, -1));
            }
            else
            {
                string name = parts[0].Trim();
                string rawValue = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(rawValue))
                {
                    localErrors.Add(InvalidAssignmentSyntaxError.EmptyValueAfterEquals(content, -1));
                }
                else if (!HoiVarsConverter.TryParseAny(rawValue, out object? value) || value == null)
                {
                    localErrors.Add(new ValueParsingError(name, rawValue, "any"));
                }
                else
                {
                    return new Var
                    {
                        Name = name,
                        Value = value,
                        PossibleCsType = value.GetType()
                    };
                }
            }

            // Если есть критические ошибки — пытаемся починить
            foreach (var error in localErrors.Where(e => e.Type == ErrorType.Critical))
            {
                if (healer == null) return null;
                var (fixedLine, success) = healer.HealError(error, content);
                if (success)
                {
                    // после исправления — пробуем заново
                    return ParseVarWithHealing(fixedLine);
                }
            }

            return null;
        }

        private HoiArray ParseArrayWithHealing(string content)
        {
            var localErrors = healer.Errors;
            var array = new HoiArray();

            if (string.IsNullOrWhiteSpace(content))
                return array;

            array.Name = Rx.ArrayName.Match(content).Value;

            var contentMatch = Rx.ArrayContent.Match(content);
            var innerContent = contentMatch.Success ? contentMatch.Value : string.Empty;

            var valueMatches = Rx.ArrayElement.Matches(innerContent);

            Type? previousType = null;

            foreach (Match match in valueMatches)
            {
                string rawValue = match.Value.Trim();

                if (!HoiVarsConverter.TryParseAny(rawValue, out object? value) || value == null)
                {
                    localErrors.Add(new ValueParsingError(array.Name, rawValue, "any"));
                    continue;
                }

                if (previousType != null && previousType != value.GetType())
                {
                    localErrors.Add(new HeterogeneousArrayError(
                        array.Name,
                        previousType.Name ?? "unknown",
                        value.GetType().Name ?? "unknown"));
                }

                array.Values.Add(value);
                array.PossibleCsType ??= value.GetType();
                previousType = value.GetType();
            }


            return array;
        }

        private static (int open, int close) CountBraces(string s, char open, char close)
        {
            int o = 0, c = 0;
            foreach (char ch in s)
            {
                if (ch == open) o++;
                else if (ch == close) c++;
            }
            return (o, c);
        }
    }
}