using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;

namespace ModdingManagerDataManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TxtParser p = new TxtParser(new TxtPattern());
            p.Parse("C:\\Users\\timpf\\Downloads\\123321.txt");
        }
    }
}
