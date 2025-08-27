using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using System.Diagnostics;

namespace ModdingManagerDataManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var one = File.ReadAllText("C:\\Users\\timpf\\Downloads\\Telegram Desktop\\06_bftb_on_actions.txt");
            //var one = File.ReadAllText();
            //var one = File.ReadAllText("C:\\Users\\timpf\\Downloads\\HOIlocal\\adjacencies.csv");
            CsvFileParser parser = new CsvFileParser();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = parser.Parse("C:\\Users\\timpf\\Downloads\\HOIlocal\\definition.csv", new CsvDefinitionsPattern());
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
