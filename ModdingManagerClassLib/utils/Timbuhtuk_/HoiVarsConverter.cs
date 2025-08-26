using System.Drawing;
using System.Text.RegularExpressions;

namespace ModdingManagerClassLib.utils.Timbuhtuk_
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
                case "yes": { result = true; return true; }
                case "no": { result = false; return true; }
                default: { result = default; return false; }
            }
        }
        public static bool TryParseDouble(string text, out double result)
        {
            text = text.Replace('.', ',');

            if (Double.TryParse(text, out result))
                return true;
            else
                return false;
        }
        public static bool TryParseInteger(string text, out int result)
        {
            if (Int32.TryParse(text, out result))
                return true;
            else
                return false;
        }
        public static bool TryParseColor(string text, out Color result)
        {
            if (!Regex.IsMatch(text, Regexes.hoiColorVar))
            {
                result = default;
                return false;
            }

            MatchCollection RGB = Regex.Matches(text, Regexes.hoiColorPart);

            result = Color.FromArgb(Int32.Parse(RGB[0].Value), Int32.Parse(RGB[0].Value), Int32.Parse(RGB[0].Value));
            return true;
        }
        public static bool TryParseString(string text, out string result)
        {
            result = text.Replace("\"", "");
            return true;
        }
        public static bool TryParseAny(string data, out object? value)
        {
            if (string.IsNullOrEmpty(data))
                value = null;
            else if (HoiVarsConverter.TryParseColor(data, out var color))
                value = color;
            else if (HoiVarsConverter.TryParseDate(data, out var date))
                value = date;
            else if (HoiVarsConverter.TryParseBoolean(data, out var b))
                value = b;
            else if (HoiVarsConverter.TryParseInteger(data, out var i))
                value = i;
            else if (HoiVarsConverter.TryParseDouble(data, out var d))
                value = d;
            else if (HoiVarsConverter.TryParseString(data, out var s))
                value = s;
            else
            {
                value = null;
                return false;
            }
            return true;
        }
    }
}
