using ModdingManager.managers.@base;
using ModdingManagerClassLib.Composers;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
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
            List<IConfig> fimoz = StateComposer.Parse();
            
            foreach (var item in fimoz)
            {
                Debug.WriteLine((item as StateConfig).Id);
            }
        }
    }
}
