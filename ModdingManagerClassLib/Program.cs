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
            ModManager.Mod.Map.Provinces = mifoz.Cast<ProvinceConfig>().ToList();

            List<IConfig> fimoz = StateComposer.Parse();
            sw.Stop();


            var reg = new LocalisationRegistry(fimoz.OfType<StateConfig>().ToList());
        }
    }
}
