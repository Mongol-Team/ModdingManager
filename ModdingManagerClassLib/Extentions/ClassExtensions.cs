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
    }
}
