using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;

namespace RawDataWorker
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
