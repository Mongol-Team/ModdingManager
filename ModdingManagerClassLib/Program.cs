using ModdingManager.managers.@base;
using ModdingManagerClassLib.Composers;

namespace ModdingManagerDataManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var g = new ModManager();
            var r = GfxComposer.Parse();
        }
    }
}
