using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class TypeRegistry
    {
        private static readonly Dictionary<Type, List<Type>> _registeredTypes = new Dictionary<Type, List<Type>>();

        public static void RegisterType<TInterface, TImplementation>()
            where TImplementation : TInterface
        {
            var interfaceType = typeof(TInterface);

            if (!_registeredTypes.ContainsKey(interfaceType))
            {
                _registeredTypes[interfaceType] = new List<Type>();
            }

            if (!_registeredTypes[interfaceType].Contains(typeof(TImplementation)))
            {
                _registeredTypes[interfaceType].Add(typeof(TImplementation));
            }
        }

        public static List<Type> GetImplementations<T>()
        {
            var interfaceType = typeof(T);

            if (_registeredTypes.TryGetValue(interfaceType, out var implementations))
            {
                return new List<Type>(implementations);
            }

            return new List<Type>();
        }

        public static List<TypeInfo> GetImplementationsWithNames<T>()
        {
            var implementations = GetImplementations<T>();
            var result = new List<TypeInfo>();

            foreach (var type in implementations)
            {
                var attr = type.GetCustomAttribute<CreatableTypeAttribute>();
                var displayName = attr?.DisplayName ?? type.Name;
                var localizationKey = attr?.LocalizationKey;

                result.Add(new TypeInfo
                {
                    Type = type,
                    DisplayName = displayName,
                    LocalizationKey = localizationKey
                });
            }

            return result;
        }
    }

    public class TypeInfo
    {
        public Type Type { get; set; }
        public string DisplayName { get; set; }
        public string LocalizationKey { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CreatableTypeAttribute : Attribute
    {
        public string DisplayName { get; }
        public string LocalizationKey { get; }

        public CreatableTypeAttribute(string displayName, string localizationKey = null)
        {
            DisplayName = displayName;
            LocalizationKey = localizationKey;
        }
    }
}
