using Application.Debugging;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.Enums;
using Models.GfxTypes;
using Models.Types.LocalizationData;
using Models.Types.ObectCacheData;
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
                ModPathes.EquipmentsPath,
                GamePathes.EquipmentsPath
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
                        List<IConfig> equipmentConfigs = ParseFile(hoiFuncFile);
                        foreach (EquipmentConfig equipmentConfig in equipmentConfigs)
                        {
                            equipmentConfig.FileFullPath = file;
                            if (!configs.Any(c => c.Id == equipmentConfig.Id))
                            {
                                configs.Add(equipmentConfig);
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
            foreach (var bracket in hoiFuncFile.Brackets.Where(b => b.Name == "equipments"))
            {
                foreach(Bracket br in bracket.SubBrackets)
                {
                    EquipmentConfig config = new EquipmentConfig();
                    config.Id = new(br.Name);
                    foreach (Bracket b in br.SubBrackets)
                    {
                        switch (b.Name)
                        {
                            case "resources":
                                foreach (Var v in b.SubVars)
                                {
                                    ResourceConfig? res = ModDataStorage.Mod.Resources.FirstOrDefault(r => r.Id.ToString() == v.Name);
                                    if (res != null)
                                    {
                                        config.Cost.AddSafe(res, v.Value.ToInt());
                                    }
                                    else
                                    {
                                        Logger.AddDbgLog($"Неверное имя ресурса в стоимости снаряжения {br.Name}, в {hoiFuncFile.FileFullPath}");
                                    }
                                }
                                break;
                            case "can_be_produced":
                                config.CanBeProduced = b.ToString(); //todo: raw trigger data
                                break;

                        }
                    }
                    foreach (HoiArray a in br.Arrays)
                    {
                        switch (a.Name)
                        {
                            case "type":
                                foreach (var item in a.Values)
                                {
                                    config.Type.AddSafe(Enum.Parse<IternalUnitType>(item.ToString().SnakeToPascal()));
                                }
                                break;
                        }
                    }
                    foreach (Var v in br.SubVars)
                    {
                        switch (v.Name)
                        {
                            case "year":
                                config.Year = v.Value.ToInt();
                                break;
                            case "is_archetype":
                                config.IsArchetype = v.Value.ToBool();
                                break;
                            case "is_buildable":
                                config.IsBuidable = v.Value.ToBool();
                                break;
                            case "is_active":
                                config.IsActive = v.Value.ToBool();
                                break;
                            case "type":
                                config.Type.AddSafe(Enum.Parse<IternalUnitType>(v.Value.ToString().SnakeToPascal()));
                                break;
                            case "picture":
                                config.Gfx = ModDataStorage.Mod.Gfxes.FirstOrDefault(g =>
                                    g.Id.ToString() == (
                                        v.Value != null
                                            ? $"GFX_{v.Value.ToString()}_medium"
                                            : $"GFX_{v.Value?.ToString() ?? "_small"}"
                                    )
                                );
                                break;
                            case "archetype":
                                config.Archetype = ModDataStorage.Mod.Equipments.FirstOrDefault(e => e.Id.ToString() == v.Value.ToString()) ?? configs.FirstOrDefault(e => e.Id.ToString() == v.Value.ToString()) as EquipmentConfig;
                                break;
                            case "interface_category":
                                config.InterfaceType = Enum.Parse<EquipmentInterfaceCategory>(v.Value.ToString().SnakeToPascal());
                                break;//todo: snake case to pascal case

                        }
                    }
                    configs.Add(config);
                }
            }
            return configs;
        }
    }
}
