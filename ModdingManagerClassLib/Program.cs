using ModdingManager.managers.@base;
using ModdingManagerClassLib.Composers;
using ModdingManagerModels;
using System.Diagnostics;

namespace ModdingManagerDataManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var mm = new ModManager();
            var sw = Stopwatch.StartNew();
            List<IConfig> mifoz = ProvinceComposer.Parse();
            ModManager.CurrentConfig.Map.Provinces = mifoz.Cast<ProvinceConfig>().ToList();
           
            List<IConfig> fimoz = StateComposer.Parse();
            sw.Stop();
            foreach (var item in fimoz)
            {
                Console.WriteLine((item as StateConfig).Id);
                
            }
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        }
    }
}
