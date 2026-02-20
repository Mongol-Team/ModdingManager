using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.extentions
{
    public static class TypeExtentions
    {
        public static bool IsInterface(this Type type)
        {
            return type?.IsInterface ?? false;
        }

        /// <summary>
        /// Проверяет, есть ли зарегистрированные реализации для данного типа
        /// </summary>
        public static bool HasImplementations(this Type interfaceType)
        {
            var implementations = GetImplementationsForType(interfaceType);
            return implementations.Count > 0;
        }
        /// <summary>
        /// Получает список реализаций для указанного типа интерфейса
        /// </summary>
        /// <param name="interfaceType">Тип интерфейса</param>
        /// <returns>Список TypeInfo с информацией о реализациях</returns>
        public static List<TypeInfo> GetImplementationsForType(this Type interfaceType)
        {
            if (interfaceType == null)
                return new List<TypeInfo>();

            try
            {
                // Получаем generic метод GetImplementationsWithNames<T>
                var method = typeof(TypeRegistry)
                    .GetMethod(nameof(TypeRegistry.GetImplementationsWithNames))
                    ?.MakeGenericMethod(interfaceType);

                if (method == null)
                    return new List<TypeInfo>();

                // Вызываем метод
                var result = method.Invoke(null, null) as List<TypeInfo>;
                return result ?? new List<TypeInfo>();
            }
            catch (Exception ex)
            {
                Debugging.Logger.AddDbgLog($"Ошибка при получении реализаций для типа {interfaceType.Name}: {ex.Message}");
                return new List<TypeInfo>();
            }
        }
    }
}
