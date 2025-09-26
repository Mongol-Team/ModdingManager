using ModdingManagerModels.Types.ObjectCacheData;
using System.Drawing;

namespace ModdingManagerClassLib.Extentions
{
    public static class BraketExtentions
    {
        public static string GetVarString(this Bracket gfxBracket, string name, string defaultValue = "Null")
        {
            return gfxBracket.SubVars.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value?.ToString() ?? defaultValue;
        }

        public static int GetVarInt(this Bracket gfxBracket, string name, int defaultValue = 0)
        {
            var val = gfxBracket.SubVars.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
            return val != null ? val.ToInt<object>() : defaultValue;
        }

        public static double GetVarDouble(this Bracket gfxBracket, string name, double defaultValue = 0)
        {
            var val = gfxBracket.SubVars.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
            return val != null ? val.ToDouble<object>() : defaultValue;
        }

        public static bool GetVarBool(this Bracket gfxBracket, string name, bool defaultValue = false)
        {
            var val = gfxBracket.SubVars.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
            return val != null ? val.Equals("yes", StringComparison.OrdinalIgnoreCase) : defaultValue;
        }

        public static int GetSubBracketVarInt(this Bracket bracket, string subBracketName, string varName, int defaultValue = 0)
        {
            var subBracket = bracket.SubBrackets
                .FirstOrDefault(b => b.Name.Equals(subBracketName, StringComparison.OrdinalIgnoreCase));

            if (subBracket == null)
                return defaultValue;

            var val = subBracket.SubVars
                .FirstOrDefault(v => v.Name.Equals(varName, StringComparison.OrdinalIgnoreCase))?.Value;

            if (val == null)
                return defaultValue;

            try
            {
                return Convert.ToInt32(val);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static Color GetArrayColor(this Bracket bracket, string arrayName, Color? defaultColor = null)
        {
            var arr = bracket.Arrays
                .FirstOrDefault(a => a.Name.Equals(arrayName, StringComparison.OrdinalIgnoreCase));

            if (arr == null || arr.Values.Count < 3)
                return defaultColor ?? Color.Transparent;

            try
            {
                int r = Convert.ToInt32(arr.Values[0]);
                int g = Convert.ToInt32(arr.Values[1]);
                int b = Convert.ToInt32(arr.Values[2]);
                int a = arr.Values.Count > 3 ? Convert.ToInt32(arr.Values[3]) : 255;

                return Color.FromArgb(a, r, g, b);
            }
            catch
            {
                return defaultColor ?? Color.Transparent;
            }
        }
    }
}
