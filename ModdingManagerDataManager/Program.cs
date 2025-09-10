using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using System.Diagnostics;

namespace ModdingManagerDataManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            YmlParser parser = new YmlParser(new TxtPattern());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = parser.Parse("C:\\Users\\timpf\\Downloads\\HOIlocal\\afr_countries_l_russian.yml");
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
