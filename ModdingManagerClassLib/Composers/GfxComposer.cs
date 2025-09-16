using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.ObjectCacheData;

namespace ModdingManagerClassLib.Composers
{
    public static class GfxComposer
    {
        public static List<IGfx> Parse()
        {
            string[] files = Directory.GetFiles(GamePathes.InterfacePath);
            var parser = new TxtParser(new TxtPattern());
            List<HoiFuncFile> data = new List<HoiFuncFile>();

            foreach (string file in files)
            {
                try { data.Add((HoiFuncFile)parser.Parse(file)); } catch (Exception e) { Console.WriteLine(e.Message); }
            }

            throw new NotImplementedException();
        }
    }
}
