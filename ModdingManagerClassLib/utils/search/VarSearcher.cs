using ModdingManagerModels.Types;
using System.Drawing;
using System.Globalization;

namespace ModdingManager.classes.utils.search
{
    /// <summary>
    /// Класс для поиска и парсинга переменных в текстовых файлах.
    /// </summary>
    public class VarSearcher
    {
        /// <summary>
        /// Ищет строковое значение переменной по её имени.
        /// </summary>
        /// <param name="lines">Массив строк файла.</param>
        /// <param name="targetName">Имя переменной.</param>
        /// <param name="assignSymbol">Символ присваивания (по умолчанию "=").</param>
        /// <returns>Значение переменной в виде строки или null, если не найдено.</returns>
        public static string SearchString(string[] lines, string targetName, string assignSymbol = "=")
        {
            foreach (var rawLine in lines)
            {
                string line = rawLine.Split('#')[0].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                int index = line.IndexOf(assignSymbol);
                if (index <= 0) continue;

                string name = line.Substring(0, index).Trim();
                if (!name.Equals(targetName, StringComparison.OrdinalIgnoreCase)) continue;

                string value = line.Substring(index + assignSymbol.Length).Trim();
                return value;
            }
            return null;
        }

        /// <summary>
        /// Обновляет значение переменной по ключу в строках с указанным символом присваивания.
        /// </summary>
        /// <param name="lines">Массив строк, в котором нужно произвести замену.</param>
        /// <param name="targetName">Имя переменной.</param>
        /// <param name="newValue">Новое значение переменной.</param>
        /// <param name="delimiter">Символ разделения (например, ':').</param>
        /// <returns>Обновлённый массив строк.</returns>
        /// <summary>
        /// Обновляет значение переменной по ключу в строках с указанным символом разделения.
        /// Возвращает обновлённый массив строк, либо null, если ключ не найден.
        /// </summary>
        public static string[]? SetSourceValue(string[] lines, Var var, string delimiter = ":", string tabstr = "")
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string rawLine = lines[i];
                string line = rawLine.Split('#')[0].Trim(); // Убираем комментарии

                int delimiterIndex = line.IndexOf(delimiter);
                if (delimiterIndex <= 0) continue;

                string name = line.Substring(0, delimiterIndex).Trim();
                if (!name.Equals(var.Name, StringComparison.OrdinalIgnoreCase)) continue;

                // Сохраняем отступ в начале строки и возможный комментарий
                int indentIndex = rawLine.IndexOf(line);
                string indentation = indentIndex >= 0 ? rawLine.Substring(0, indentIndex) : "";
                string comment = rawLine.Contains("#") ? rawLine.Substring(rawLine.IndexOf("#")) : "";

                // Формируем новую строку
                string formattedValue = $"\"{var.Value}\"";
                lines[i] = $"{tabstr}{indentation}{var.Name}{delimiter} {formattedValue} {comment}".Trim();
                return lines;
            }

            return null; // Если не найдено
        }


        /// <summary>
        /// Ищет переменную по имени и возвращает структуру Var (имя и значение).
        /// </summary>
        public static Var? SearchVar(string[] lines, string targetName, string assignSymbol = "=")
        {
            foreach (var rawLine in lines)
            {
                string line = rawLine.Split('#')[0].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                int index = line.IndexOf(assignSymbol);
                if (index <= 0) continue;

                string name = line.Substring(0, index).Trim();
                if (!name.Equals(targetName, StringComparison.OrdinalIgnoreCase)) continue;

                string value = line.Substring(index + assignSymbol.Length).Trim();
                return new Var { Name = name, Value = value };
            }
            return null;
        }

        /// <summary>
        /// Ищет целое число по имени переменной.
        /// </summary>
        public static int? SearchInt(string[] lines, string targetName, string assignSymbol = "=")
        {
            string valueStr = SearchString(lines, targetName, assignSymbol);
            return int.TryParse(valueStr, out var result) ? result : null;
        }

        /// <summary>
        /// Ищет десятичное число по имени переменной.
        /// </summary>
        public static decimal? SearchDecimal(string[] lines, string targetName, string assignSymbol = "=")
        {
            string valueStr = SearchString(lines, targetName, assignSymbol);
            return decimal.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        /// <summary>
        /// Ищет булево значение по имени переменной (yes/no).
        /// </summary>
        public static bool? SearchBool(string[] lines, string targetName, string assignSymbol = "=")
        {
            string valueStr = SearchString(lines, targetName, assignSymbol);
            if (valueStr == null) return null;
            return valueStr.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Парсит присваивания переменных из текста и возвращает список Var.
        /// Игнорирует строки с фигурными скобками.
        /// </summary>
        public static List<Var> ParseAssignments(string content, char assignSymbol = '=')
        {
            List<Var> varList = new();
            HashSet<string> seenNames = new();
            string[] lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string trimmed = line.Split('#')[0].Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                int eqIndex = trimmed.IndexOf(assignSymbol);
                if (eqIndex > 0 && !trimmed.Contains('{') && !trimmed.Contains('}'))
                {
                    string key = trimmed.Substring(0, eqIndex).Trim();
                    string value = trimmed.Substring(eqIndex + 1).Trim();

                    if (seenNames.Contains(key)) continue;

                    seenNames.Add(key);
                    varList.Add(new Var { Name = key, Value = value, AssignSymbol = assignSymbol });
                }
            }
            return varList;
        }


        /// <summary>
        /// Извлекает все строки в кавычках из текста.
        /// </summary>
        public static List<string> ParseQuotedStrings(string content)
        {
            var names = new List<string>();
            int startIndex = 0;

            while ((startIndex = content.IndexOf('\"', startIndex)) != -1)
            {
                int endIndex = content.IndexOf('\"', startIndex + 1);
                if (endIndex == -1) break;

                names.Add(content.Substring(startIndex + 1, endIndex - startIndex - 1));
                startIndex = endIndex + 1;
            }
            return names;
        }

        /// <summary>
        /// Парсит цвет из строки вида {R G B}.
        /// </summary>
        public static Color ParseColor(string content)
        {
            string[] parts = content.Trim('{', '}').Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return Color.Black;

            return Color.FromArgb(
                byte.Parse(parts[0]),
                byte.Parse(parts[1]),
                byte.Parse(parts[2])
            );
        }

        /// <summary>
        /// Сканирует папку и собирает все переменные из текстовых файлов.
        /// </summary>
        public static List<Var> TryGetCountryTags(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return new List<Var>();

            var result = new List<Var>();
            foreach (var file in Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(file);
                var vars = ParseAssignments(content);
                result.AddRange(vars);
            }
            return result;
        }

        /// <summary>
        /// Конвертирует строку "yes"/"no" в булево значение.
        /// </summary>
        public static bool ParseBool(string value) =>
            value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}
