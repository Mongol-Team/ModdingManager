using Application.Composers;
using Application.Debugging;
using Data;
using System.Drawing;
using System.Reflection;

namespace Application.Extentions
{
    public static class ClassExtensions
    {
        public static TTarget CopyTo<TTarget>(this TTarget source, TTarget target)
       where TTarget : class
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var type = typeof(TTarget);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(source);
                    prop.SetValue(target, value);
                }
            }

            return target;
        }
       
        public static int ToInt<TClass>(this TClass id) where TClass : class
        {
            if (id == null)
                return -1;

            try
            {
                var str = id.ToString();
                return int.TryParse(str, out int result) ? result : -1;
            }
            catch (FormatException ex)
            {
                Logger.AddLog($"[ToInt] Cannot convert value '{id}' ({id?.GetType().FullName ?? "null"}) to int: {ex.Message}");
                return -1;
            }
        }

        public static double ToDouble<TClass>(this TClass id) where TClass : class
        {
            if (id == null)
                return -1;

            try
            {
                var str = id.ToString();
                return double.TryParse(str, out double result) ? result : -1;
            }
            catch (FormatException ex)
            {
                Logger.AddLog($"[ToDouble] Cannot convert value '{id}' ({id?.GetType().FullName ?? "null"}) to double: {ex.Message}");
                return -1;
            }
        }

        public static bool ToBool<TClass>(this TClass input) where TClass : class
        {
            if (input == null)
                return false;

            try
            {
                var str = input.ToString()?.Trim().ToLowerInvariant();
                if (str == "yes") return true;
                if (str == "no") return false;
                return bool.TryParse(str, out bool result) && result;
            }
            catch (FormatException ex)
            {
                Logger.AddLog($"[ToBool] Cannot convert value '{input}' ({input?.GetType().FullName ?? "null"}) to bool: {ex.Message}");
                return false;
            }
        }

        public static bool IsUndefined<TClass>(this TClass input) where TClass : class
        {
            if (input == null)
                return true;

            switch (input)
            {
                case string s:
                    return s == DataDefaultValues.Null
                        || s == DataDefaultValues.NaN
                        || s == DataDefaultValues.None;

                case int i:
                    return i == DataDefaultValues.NullInt;

                case double d:
                    return double.IsNaN(d) || d == DataDefaultValues.NullInt;

                case bool b:
                    // для bool можно считать "undefined" только false?
                    return !b;

                case KeyValuePair<string, string> kvp:
                    return kvp.Equals(DataDefaultValues.NullLocalistion);

                case Bitmap bmp:
                    return bmp == DataDefaultValues.NullImageSource
                        || bmp == DataDefaultValues.ItemWithNoGfxImage;
                case null:
                    return true;

                default:
                    // для остальных типов — считаем undefined только если ToString совпадает с "Null"
                    return input.ToString() == DataDefaultValues.Null;
            }
        }
    }
}
