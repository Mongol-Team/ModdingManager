using Models.Types;
using System.Drawing;
using System.Globalization;

namespace RawDataWorker
{
    public struct HoiVarsConverter
    {
        public static bool TryParseDate(string text, out DateOnly result)
        {
            return DateOnly.TryParseExact(
                text,
                "yyyy.MM.dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out result
            );
        }
        public static bool TryParseBoolean(string text, out bool result)
        {
            switch (text)
            {
                case "yes": case "true": { result = true; return true; }
                case "no": case "false": { result = false; return true; }
                case "YES": case "TRUE": { result = true; return true; }
                case "NO": case "FALSE": { result = false; return true; }
                default: { result = default; return false; }
            }
        }
        public static bool TryParseDouble(string text, out double result)
        {
            // Разрешим и '.' и ',', но без аллокаций
            var span = text.AsSpan();
            // быстрый детект: если есть '.' — парсим инвариантно как '.'; если только ',' — заменим временно чтением посимвольно
            // Проще и быстрее: принять только '.' и потребовать Invariant
            return double.TryParse(span, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
        }

        public static bool TryParseInteger(string text, out int result)
        {
            return int.TryParse(text.AsSpan(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }
        public static bool TryParseColor(string text, out Color result)
        {
            var matches = Regexes.HoiColorPart.Matches(text);
            if (matches.Count == 3)
            {
                result = Color.FromArgb(
                    int.Parse(matches[0].Value, System.Globalization.CultureInfo.InvariantCulture),
                    int.Parse(matches[1].Value, System.Globalization.CultureInfo.InvariantCulture),
                    int.Parse(matches[2].Value, System.Globalization.CultureInfo.InvariantCulture));
                return true;
            }
            result = default;
            return false;
        }
        public static bool TryParseString(string text, out string result)
        {
            if (text.Length >= 2 && text[0] == '"' && text[^1] == '"')
            {
                // вернуть без кавычек — да, тут аллокация, но она нужна по смыслу
                result = text.Substring(1, text.Length - 2);
                return true;
            }
            result = default!;
            return false;
        }
        public static bool TryParseHoiReference(string text, out HoiReference result)
        {
            result = null;
            //if (text.Contains("\""))
            //    return false;
            result = new HoiReference() { Value = text };
            return true;
        }

        public static bool TryParseAny(string data, out object? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(data)) return false;

            ReadOnlySpan<char> s = data.AsSpan().Trim();

            // Быстрые префильтры по первому символу
            char c0 = s[0];

            // Строка в кавычках — распознать сразу, не дергать другие парсеры
            if (c0 == '"' && TryParseString(data, out var str)) { value = str; return true; }

            // Цвет — дешёвый предикат по фигурным скобкам / ключевому слову
            if (c0 == '{' || (c0 == 'r' || c0 == 'R'))
            {
                if (TryParseColor(data, out var color)) { value = color; return true; }
            }

            // Булево — дёшево, без аллокаций
            if (TryParseBoolean(data, out var b)) { value = b; return true; }

            // Дата: узнаётся по двум точкам и длине
            // yyyy.MM.dd -> 2 точки, длина 10
            if (s.Length == 10 && s[4] == '.' && s[7] == '.')
            {
                if (TryParseDate(data, out var d)) { value = d; return true; }
            }

            // Числа: сначала int, потом double (инвариантно)
            if (TryParseInteger(data, out var i)) { value = i; return true; }
            if (TryParseDouble(data, out var dbl)) { value = dbl; return true; }

            // Ссылка Хой — остаточный вариант (без кавычек)
            if (TryParseHoiReference(data, out var refVal)) { value = refVal; return true; }

            return false;
        }
    }
}
