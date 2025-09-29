namespace ModdingManagerClassLib.Extentions
{
    public static class ListExtensions
    {
        public static T FindById<T>(this List<T> list, string targetName)
        {
            if (list == null || string.IsNullOrEmpty(targetName))
                return default;

            foreach (var item in list)
            {
                var nameProperty = typeof(T).GetProperty("Id");
                if (nameProperty != null)
                {
                    var value = nameProperty.GetValue(item) as string;
                    if (value == targetName)
                        return item;
                }
            }

            return default;
        }
        public static bool RemoveById<T>(this List<T> list, string targetId)
        {
            if (list == null || string.IsNullOrEmpty(targetId))
                return false;

            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                var value = idProperty.GetValue(list[i]) as string;
                if (value == targetId)
                {
                    list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        public static bool ReplaceById<T>(this List<T> list, string targetId, T newItem)
        {
            if (list == null || string.IsNullOrEmpty(targetId) || newItem == null)
                return false;

            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                var value = idProperty.GetValue(list[i]) as string;
                if (value == targetId)
                {
                    list[i] = newItem;
                    return true;
                }
            }

            return false;
        }
        public static void AddSafe<T>(this ICollection<T> collection, T item)
        {
            if (collection == null || item == null) return;
            collection.Add(item);
        }
        public static T Random<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("Список пуст или null.");
            Random _random = new Random();
            int index = _random.Next(list.Count);
            return list[index];
        }

    }
}
