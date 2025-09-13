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
            
            ModManager.Mod.Map.Provinces = ProvinceComposer.Parse().OfType<ProvinceConfig>().ToList();
            List<IConfig> mifoz = SRegionComposer.Parse();
            sw.Stop();
            foreach (var item in mifoz)
            {
                Console.WriteLine((item as StrategicRegionConfig).FilePath);
                
            }
            Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        }
    }
}
