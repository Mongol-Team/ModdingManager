using Application.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models;
using Models.GfxTypes;
using Models.Interfaces;
using Models.Types.ObectCacheData;
using Models.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDF = Data.DataDefaultValues;
using Models.Configs;
namespace Application.Composers
{
    public class TechCategoryComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new List<IConfig>();
            HashSet<string> seenIds = new HashSet<string>();

            string[] possiblePathes =
            {
                ModPathes.TechDefPath,
                GamePathes.TechDefPath
            };

            foreach (string path in possiblePathes)
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    HoiFuncFile funcFile = new TxtParser(new TxtPattern()).Parse(file) as HoiFuncFile;
                    if (funcFile == null) continue;

                    foreach (HoiArray harr in funcFile.Arrays)
                    {
                        foreach (object name in harr.Values)
                        {
                            string id = name.ToString();
                            if (seenIds.Contains(id)) continue;

                            seenIds.Add(id);
                            configs.Add(new TechCategoryConfig
                            {
                                Id = new(id),
                                Gfx = new SpriteType(DDF.ItemWithNoGfxImage, DDF.Null),
                                FileFullPath = file,
                            });
                        }
                    }
                }
            }

            return configs;
        }

    }
}
