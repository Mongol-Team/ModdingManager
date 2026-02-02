using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.Enums;
using Models.GfxTypes;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;

namespace Application.Composers
{
    public class EquipmentComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new();
            string[] possiblePathes =
            {
                ModPathes.BuildingsPath,
                GamePathes.BuildingsPath
            };
            foreach (string path in possiblePathes)
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        string fileContent = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(fileContent);
                        List<IConfig> buildingConfigs = ParseFile(hoiFuncFile);
                        foreach (BuildingConfig buildingConfig in buildingConfigs)
                        {
                            buildingConfig.FileFullPath = file;
                            if (!configs.Any(c => c.Id == buildingConfig.Id))
                            {
                                configs.Add(buildingConfig);
                            }
                        }
                    }
                }
            }
            return configs;
        }

        public static List<IConfig> ParseFile(HoiFuncFile hoiFuncFile)
        {
            List<IConfig> configs = new();
            foreach (var bracket in hoiFuncFile.Brackets)
            {
                
            }
            return configs;
        }
    }
}
