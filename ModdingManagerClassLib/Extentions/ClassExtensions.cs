using ModdingManagerClassLib.Composers;
using System.Reflection;

namespace ModdingManagerClassLib.Extentions
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

            var str = id.ToString();

            return int.TryParse(str, out int result) ? result : -1;
        }
        public static bool ToBool<TClass>(this TClass input) where TClass : class
        {
            if (input == null)
                return false;

            var str = input.ToString();

            return bool.TryParse(str, out bool result) && result;
        }

        public static double ToDouble<TClass>(this TClass id) where TClass : class
        {
            if (id == null)
                return -1;
            var str = id.ToString();
            return double.TryParse(str, out double result) ? result : -1;
        }
    }
}
