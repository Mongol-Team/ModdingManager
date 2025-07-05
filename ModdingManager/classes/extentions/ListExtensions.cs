namespace ModdingManager.classes.extentions
{
    public static class ListExtensions
    {
        public static T FindByName<T>(this List<T> list, string targetName)
        {
            if (list == null || string.IsNullOrEmpty(targetName))
                return default;

            foreach (var item in list)
            {
                var nameProperty = typeof(T).GetProperty("name");
                if (nameProperty != null)
                {
                    var value = nameProperty.GetValue(item) as string;
                    if (value == targetName)
                        return item;
                }
            }

            return default;
        }
    }
}
