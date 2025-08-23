namespace ModdingManagerClassLib.Extentions
{
    public static class CharExtensions
    {
        public static string AsString(this IEnumerable<char> chars)
        {
            // если chars уже char[], то ToArray() практически ничего не копирует
            return new string(chars.ToArray());
        }
    }
}
