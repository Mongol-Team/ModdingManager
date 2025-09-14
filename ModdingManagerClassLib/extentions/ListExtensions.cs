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
