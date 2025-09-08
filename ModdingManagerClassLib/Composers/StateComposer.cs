using ModdingManager.classes.utils;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types;
using ModdingManagerModels.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class StateComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            var result = new List<IConfig>();

            string[] priorityFolders = {
                ModPathes.StatesPath,
                GamePathes.StatesPath,
            };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder))
                    continue;
                foreach (string file in Directory.GetFiles(folder))
                {
                    StateConfig stateConfig = ParseStateConfig(file);
                    result.Add(stateConfig);
                }
                if (result.Count > 0)
                    break;
                else
                {
                    Logger.AddLog($"[⚠️] No state files found in mod folder: {folder}");
                }
            }

            return result;
        }
        public static StateConfig ParseStateConfig(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Logger.AddLog($"Файл не найден: {filePath}");
                return null;
            }

            HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(filePath) as HoiFuncFile;
            if (file == null || file.Brackets.Count == 0)
            {
                Logger.AddLog($"Не удалось распарсить файл: {filePath}");
                return null;
            }

            var stateBracket = file.Brackets.First();

            var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
            if (idVar == null || !int.TryParse(idVar.Value.ToString(), out int id))
            {
                Logger.AddLog($"Не удалось извлечь ID из файла: {filePath}");
                return null;
            }

            var provincesArray = stateBracket.Arrays.FirstOrDefault(a => a.Name == "provinces");
            var provinceIds = provincesArray?.Values
                .OfType<object>()
                .Select(v => v is HoiReference hr ? hr.Value : v)
                .OfType<int>()
                .ToList() ?? new List<int>();
            var matchedProvinces = ModManager.CurrentConfig.Map.Provinces
                .Where(p => provinceIds.Contains(p.Id))
                .ToList();

            return new StateConfig
            {
                Id = id,
                Provinces = matchedProvinces,
                FilePath = file.FilePath,
                Color = ModManager.GenerateColorFromId(id),
                //Cathegory = ModManager.CurrentConfig.StateCathegories.Where(s => s.Id == stateBracket.SubVars.FirstOrDefault(v => v.Name == "category")?.Value as string).First(),
            };
        }
    }
}
