using ModdingManager.managers.@base;
using ModdingManagerClassLib.Composers;
using ModdingManagerClassLib.utils;
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
            w.Stop();

            //foreach (var item in fimoz)
            //{
            //    Console.WriteLine((item as StateConfig).Id);
            //}

            var reg = new LocalisationRegistry(fimoz.OfType<StateConfig>().ToList());
        }
    }
}
