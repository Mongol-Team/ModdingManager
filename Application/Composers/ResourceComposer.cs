using Application.Debugging;
using Application.Extentions;
using Application.utils;
using Application.utils.Pathes;
using Models.Configs;
using Models.Types.ObjectCacheData;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Application.Composers
{
    public class ResourceComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new List<IConfig>();
            string[] possiblePathes =
            {
                ModPathes.ResourcesPath,
                GamePathes.ResourcesPath
            };
            foreach (string path in possiblePathes)
            {
                try
                {
                    string[] files = Directory.GetFiles(path);
                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            string fileContent = File.ReadAllText(file);
                            HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(fileContent);
                            List<ResourceConfig> resourceConfigs = ParseFile(hoiFuncFile, file);
                            foreach (ResourceConfig resourceConfig in resourceConfigs)
                            {
                                if (!configs.Any(c => c.Id == resourceConfig.Id))
                                {
                                    configs.Add(resourceConfig);
                                }
                            }
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    Logger.AddLog($"No resouces found using: {ModPathes.ResourcesPath + " " + GamePathes.ResourcesPath}", LogLevel.Info);
                }
            }
            return configs;
        }

        public static List<ResourceConfig> ParseFile(HoiFuncFile hoiFuncFile, string path)
        {
            List<ResourceConfig> configs = new();
            foreach (var bracket in hoiFuncFile.Brackets.Where(b => b.Name == "resources"))
            {
                foreach (var cfgBr in bracket.SubBrackets)
                {
                    ResourceConfig config = new ResourceConfig();
                    config.Id = new Models.Types.Utils.Identifier(cfgBr.Name);
                    config.Gfx = ModDataStorage.Mod.Gfxes.FirstOrDefault(g => g.Id.ToString() == "GFX_resources_strip");
                    foreach (var var in cfgBr.SubVars)
                    {
                        switch (var.Name)
                        {
                            case "cic":
                                config.Cost = var.Value.ToDouble();
                                break;
                            case "convoys":
                                config.Convoys = var.Value.ToDouble();
                                break;
                            case "icon_frame":
                                config.IconIndex = var.Value.ToInt();
                                break;
                        }
                    }
                    config.FileFullPath = path;
                    configs.AddSafe(config);
                }
            }
            return configs;
        }
    }
}
