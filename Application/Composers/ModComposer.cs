using Application.Debugging;
using Application.Extentions;
using Application.utils;
using Application.utils.Pathes;
using Models.Configs.HoiConfigs;
using Models.Enums;
using Models.Types.ObjectCacheData;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using View.Interfaces;

namespace Application.Composers
{
    public static class ModComposer
    {
        public static IMod ParseMod()
        {
            string[] files = Directory.GetFiles(ModPathes.BaseDirectory);
            string descriptorFile = files.FirstOrDefault(f => Path.GetFileName(f).Equals("descriptor.mod", StringComparison.OrdinalIgnoreCase));
            FuncFile func = new TxtParser(new TxtPattern()).Parse(descriptorFile) as FuncFile;

            IMod mod = new HoiModConfig();
            if (func != null)
            {
                if (func.Vars == null)
                {
                    Logger.AddLog("No variables found in mod descriptor.", ConsoleColor.Yellow);
                }
                else
                {

                    foreach (Var variable in func.Vars)
                    {
                        switch (variable.Name.ToLower())
                        {
                            case "name":
                                mod.Name = variable.Value.ToString();
                                break;
                            case "replace_path":
                                mod.ReplacePathes.Add(variable.Value.ToString());
                                break;
                            case "version":
                                mod.ModVersion = variable.Value.ToString();
                                break;
                            case "supported_version":
                                mod.GameVersion = variable.Value.ToString();
                                break;
                            case "picture":
                                string imagePath = null;
                                try
                                {
                                    imagePath = files.FirstOrDefault(f => f.EndsWith(
                                        variable.Value.ToString().Replace("\"", ""),
                                        StringComparison.OrdinalIgnoreCase));

                                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                                    {
                                        using (var bmp = Bitmap.FromFile(imagePath))
                                        {
                                            mod.Image = new Bitmap(bmp);
                                        }
                                    }
                                    else
                                    {
                                        mod.Image = null;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.AddLog(StaticLocalisation.GetString("Log.ModImageNotFound", imagePath));
                                    mod.Image = null;
                                }

                                break;
                            case "type":
                                if (Enum.TryParse<ModTypes>(variable.Value.ToString().SnakeToPascal(), out var parsedType))
                                {
                                    mod.Type = parsedType;
                                }
                                break;


                        }
                    }
                }
                if (func.Arrays == null)
                {
                    Logger.AddDbgLog("No arrays found in mod descriptor.");
                }
                else
                {
                   foreach (var array in func.Arrays)
                   {
                        if (array.Name.Equals("authors", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var author in array.Values)
                            {
                                mod.Authors.Add(author.ToString());
                            }
                        }
                   }
                }
            
                Logger.AddDbgLog($"Parsed mod descriptor: Name={mod.Name}, ModVersion={mod.ModVersion}, GameVersion={mod.GameVersion}, ReplacePathes={string.Join(", ", mod.ReplacePathes)}");
            }
            else
            {
                Logger.AddDbgLog("Failed to parse mod descriptor.");
            }

            return mod;
        }
    }
}
