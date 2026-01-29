using Models.Enums;
using Models.Interfaces;
using Models.Interfaces.RawDataWorkerInterfaces;
using Models.Types.TableCacheData;
using RawDataWorker.Errors;
using System.Drawing;
using System.Text.RegularExpressions;
using Rx = RawDataWorker.Regexes;

namespace RawDataWorker.Parsers
{
    public class CsvParser : Parser
    {
        private readonly IHealer healer;

        public CsvParser(IParsingPattern pattern, IHealer healer = null) : base(pattern)
        {
            this.healer = healer;
        }

        protected override IHoiData ParseRealization(string content)
        {
            Normalize(ref content);

            var result = new HoiTable();

            int expectedValuesPerLine = CalculateExpectedValueCount();

            MatchCollection lines = Rx.CsvLine.Matches(content);

            // Проверка на полностью пустой файл после нормализации
            if (lines.Count == 0)
            {
                var error = new InvalidFormatError("No valid lines found after normalization")
                {
                    Line = 0  // специальный случай — ошибка всего файла
                };

                if (healer != null)
                {
                    var (fixedContent, success) = healer.HealError(error, content);
                    if (!success)
                        throw new Exception(error.Message);
                    content = fixedContent;
                    lines = Rx.CsvLine.Matches(content);
                }
                else
                {
                    throw new Exception(error.Message);
                }
            }

            for (int q = 0; q < lines.Count; q++)
            {
                string line = lines[q].Value;
                var values = ParseLine(line, expectedValuesPerLine, q + 1, null);
                if (values != null)
                {
                    result.Values.Add(values);
                }
            }

            return result;
        }

        private int CalculateExpectedValueCount()
        {
            int count = 0;
            foreach (var type in pattern.Types)
            {
                if (type.Name == "Color") count += 3;
                else if (type.Name == "Point") count += 2;
                else count++;
            }
            return count;
        }

        protected override void Normalize(ref string content)
        {
            content = content.Replace("\r\n", "\n");
            if (pattern.Separator == " ")
                content = content.Replace(" ", ";");
            else
                content = content.Replace(" ", "");

            content = Rx.FileComment.Replace(content, "");
            content = Rx.EmptyLine.Replace(content, "");
            content = content.Replace($"{pattern.Separator}{pattern.Separator}", $"{pattern.Separator} {pattern.Separator}");
        }

        private List<object>? ParseLine(string originalLine, int expectedValuesPerLine, int lineNum, string? path)
        {
            string line = originalLine;
            int retries = 0;
            const int MAX_RETRIES = 5;

            while (retries < MAX_RETRIES)
            {
                bool hadError = false;
                var parts = line.Split(pattern.Separator);

                // Пустая строка после сплита
                if (parts.Length == 0)
                {
                    var error = new InvalidFormatError("Empty line after splitting", path)
                    {
                        Line = lineNum
                    };
                    hadError = true;
                    if (healer == null) return null;

                    var (fixedLine, success) = healer.HealError(error, line);
                    if (!success) throw new Exception(error.Message);
                    line = fixedLine;
                    retries++;
                    continue;
                }

                // Недостаточно столбцов → Fatal
                if (parts.Length < expectedValuesPerLine)
                {
                    var error = new WrongColumnCountError(lineNum, parts.Length, expectedValuesPerLine, path)
                    {
                        Line = lineNum
                    };
                    hadError = true;
                    if (healer == null) return null;

                    var (fixedLine, success) = healer.HealError(error, line);
                    if (!success) throw new Exception(error.Message);
                    line = fixedLine;
                    retries++;
                    continue;
                }

                // Лишние столбцы → Warning
                if (parts.Length > expectedValuesPerLine)
                {
                    var error = new ExtraColumnsWarning(lineNum, expectedValuesPerLine, parts.Length, path)
                    {
                        Line = lineNum
                    };
                    hadError = true;
                    if (healer == null) return null;

                    var (fixedLine, success) = healer.HealError(error, line);
                    if (!success) throw new Exception(error.Message);
                    line = fixedLine;
                    retries++;
                    continue;
                }

                var result = new List<object>();
                int index = 0;

                foreach (var type in pattern.Types)
                {
                    if (index >= parts.Length) break;

                    string raw = parts[index]?.Trim() ?? "";

                    // Пустое обязательное значение → Warning
                    if (string.IsNullOrEmpty(raw))
                    {
                        var error = new MissingRequiredValueWarning(lineNum, index, null, path)
                        {
                            Line = lineNum
                        };
                        hadError = true;
                        if (healer == null) return null;

                        var (fixedLine, success) = healer.HealError(error, line);
                        if (!success) throw new Exception(error.Message);
                        line = fixedLine;
                        break; // после исправления нужно начать заново
                    }

                    object? value = null;
                    bool parsed = false;

                    switch (type.Name)
                    {
                        case "Color":
                            if (index + 2 < parts.Length)
                            {
                                string colorTxt = $"{parts[index]} {parts[index + 1]} {parts[index + 2]}";
                                parsed = HoiVarsConverter.TryParseColor(colorTxt, out var color);
                                if (parsed) value = color;
                                index += 2;
                            }
                            break;

                        case "Point":
                            if (index + 1 < parts.Length &&
                                int.TryParse(parts[index], out var x) &&
                                int.TryParse(parts[index + 1], out var y))
                            {
                                value = new Point(x, y);
                                parsed = true;
                                index++;
                            }
                            break;

                        case "ProvincesBiome":
                            parsed = Enum.TryParse<ProvinceTerrain>(raw, ignoreCase: true, out var biome);
                            if (parsed) value = biome;
                            break;

                        case "ProvinceType":
                            parsed = Enum.TryParse<ProvinceType>(raw, ignoreCase: true, out var provType);
                            if (parsed) value = provType;
                            break;

                        case "AdjacencyType":
                            parsed = Enum.TryParse<AdjacencyType>(raw, ignoreCase: true, out var adjType);
                            if (parsed) value = adjType;
                            break;

                        case "Int32":
                        case "int":
                            parsed = HoiVarsConverter.TryParseInteger(raw, out var intVal);
                            if (parsed) value = intVal;
                            break;

                        case "Double":
                        case "double":
                            parsed = HoiVarsConverter.TryParseDouble(raw, out var dblVal);
                            if (parsed) value = dblVal;
                            break;

                        case "Boolean":
                        case "bool":
                            parsed = HoiVarsConverter.TryParseBoolean(raw, out var boolVal);
                            if (parsed) value = boolVal;
                            break;

                        case "DateOnly":
                            parsed = HoiVarsConverter.TryParseDate(raw, out var dateVal);
                            if (parsed) value = dateVal;
                            break;

                        case "String":
                        case "string":
                            parsed = HoiVarsConverter.TryParseString(raw, out var strVal);
                            if (parsed) value = strVal;
                            break;

                        case "HoiReference":
                            parsed = HoiVarsConverter.TryParseHoiReference(raw, out var refVal);
                            if (parsed) value = refVal;
                            break;

                        default:
                            value = raw;
                            parsed = true;
                            break;
                    }

                    if (!parsed)
                    {
                        hadError = true;
                        var error = new TypeConversionError(lineNum, index, raw, type.Name, null, path)
                        {
                            Line = lineNum
                        };
                        if (healer == null) return null;

                        var (fixedLine, success) = healer.HealError(error, line);
                        if (!success) throw new Exception(error.Message);
                        line = fixedLine;
                        break; // после исправления — заново парсим строку
                    }

                    if (value != null)
                        result.Add(value);

                    index++;
                }

                // Успешный парсинг всей строки
                if (!hadError && result.Count == pattern.Types.Count)
                {
                    return result;
                }

                retries++;
            }

            // Если после всех попыток исправления всё ещё не получилось
            throw new Exception($"[CSV Parser]: Failed to parse line {lineNum} after {MAX_RETRIES} healing attempts: {originalLine}");
        }
    }
}