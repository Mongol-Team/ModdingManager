using ModdingManagerModels.Types;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ModdingManagerDataManager
{
    public struct HoiVarsConverter
    {
        public static bool TryParseDate(string text, out DateOnly result)
        {
            try
            {
                result = DateOnly.ParseExact(text, "yyyy.MM.dd");
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
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
            text = text.Replace('.', ',');

            if (double.TryParse(text, out result))
                return true;
            else
                return false;
        }
        public static bool TryParseInteger(string text, out int result)
        {
            if (int.TryParse(text, out result))
                return true;
            else
                return false;
        }
        public static bool TryParseColor(string text, out Color result)
        {
            if (!Regexes.HoiColorContent.IsMatch(text))
            {
                result = default;
                return false;
            }

            MatchCollection RGB = Regexes.HoiColorPart.Matches(text);

            result = Color.FromArgb(int.Parse(RGB[0].Value), int.Parse(RGB[1].Value), int.Parse(RGB[2].Value));
            return true;
        }
        public static bool TryParseString(string text, out string result)
        {
            if (text.Contains("\""))
            {
                result = text.Replace("\"", "");
                return true;
            }
            result = default;
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
        //ПИЗДЕЦ ТЯЖЕЛАЯ ДУРА, ВЫЗЫВАТЬ ТОЛЬКО ОТ БЕЗИСХОДНОСТИ
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
                value = null;
                return false;
            }
            return true;
        }
    }
}
