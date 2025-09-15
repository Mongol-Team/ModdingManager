using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.ObjectCacheData;

namespace ModdingManagerClassLib.Composers
{
    public class GfxComposer 
    {
        public static List<IGfx> Parse()
        {
            string[] files = Directory.GetFiles(GamePathes.InterfacePath);
            var parser = new TxtParser(new TxtPattern());
            List<HoiFuncFile> data = new List<HoiFuncFile>();

            foreach (string file in files)
            {
                data.Add((HoiFuncFile)parser.Parse(file));
            }

            throw new NotImplementedException();
        }
    }
}
