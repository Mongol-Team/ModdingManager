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
            List<IConfig> mifoz = ProvinceComposer.Parse();
            ModManager.CurrentConfig.Map.Provinces = mifoz.Cast<ProvinceConfig>().ToList();
            var w = new Stopwatch();
            w.Start();
            List<IConfig> fimoz = StateComposer.Parse();
            w.Stop();

            foreach (var item in fimoz)
            {
                Console.WriteLine((item as StateConfig).Id);
            }
        }
    }
}
