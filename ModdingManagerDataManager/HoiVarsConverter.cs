using ModdingManagerModels.Types;
using System.Drawing;
using System.Globalization;

namespace ModdingManagerDataManager
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
            if (text.Contains("\""))
                return false;
            result = new HoiReference() { Value = text };
            return true;
        }

        public static bool TryParseAny(string data, out object? value)
        {
            if (string.IsNullOrEmpty(data))
                value = null;
            else if (TryParseColor(data, out var color))
                value = color;
            else if (TryParseDate(data, out var date))
                value = date;
            else if (TryParseBoolean(data, out var b))
                value = b;
            else if (TryParseInteger(data, out var i))
                value = i;
            else if (TryParseDouble(data, out var d))
                value = d;
            else if (TryParseString(data, out var s))
                value = s;
            else if (TryParseHoiReference(data, out var r))
                value = r;
            else
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
