using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class IdeaSlotComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            string[] pathes =
            {
                GamePathes.IdeasPath,
                ModPathes.IdeasPath
            };

            List<IConfig> result = new List<IConfig>();
            foreach(string path in pathes)
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        HoiFuncFile hoifile = new TxtParser(new TxtPattern()).Parse(file) as HoiFuncFile;
                        List<IConfig> parsed = ParseSingleFile(hoifile);
                        result.AddRange(parsed);
                    }
                }
                
            }
            return result;
        }

        public static List<IConfig> ParseSingleFile(HoiFuncFile file)
        {
            List<IConfig> result = new List<IConfig>();
            foreach(Bracket br in file.Brackets)
            {
                if (br.Name == "ideas")
                {
                    foreach (Bracket slot in br.SubBrackets)
                    {
                        IdeaSlotConfig slotConfig = ModManager.Mod.IdeaSlots.FindById(slot.Name);
                        if (slotConfig == null)
                        {
                            Var isLaw = slot.SubVars.FirstOrDefault(b => b.Name == "law");
                            Var useListView = slot.SubVars.FirstOrDefault(b => b.Name == "use_list_view");
                            Var designer = slot.SubVars.FirstOrDefault(b => b.Name == "designer");
                            slotConfig = new IdeaSlotConfig()
                            {
                                Id = slot?.Name != null ? new(slot.Name) : throw new ArgumentNullException(nameof(slot.Name)),
                                IsLaw = isLaw != null ? (isLaw.Value as bool?) ?? false : false,
                                UseListView = useListView != null ? (useListView.Value as bool?) ?? false : false,
                                IsDesigner = designer != null ? (designer.Value as bool?) ?? false : false,
                                Ideas = new List<IdeaConfig>(),
                                Localisation = new ConfigLocalisation()
                            };


                            foreach (Bracket idea in slot.SubBrackets)
                            {
                                IdeaConfig parsedIdea = ModManager.Mod.Ideas.FindById(idea.Name);
                                ModManager.Mod.Ideas = new();
                                if (parsedIdea == null)
                                {
                                    parsedIdea = IdeaComposer.ParseSingleIdea(idea) as IdeaConfig;
                                    
                                    ModManager.Mod.Ideas.Add(parsedIdea);
                                };
                                try
                                {
                                    slotConfig.Ideas.Add(parsedIdea);
                                }
                                catch (Exception ex)
                                {
                                    Logger.AddLog($"[⚠️]Error adding idea {parsedIdea.Id} to slot {slotConfig.Id}: {ex.Message}");
                                }
                            }
                            result.Add(slotConfig);
                            Logger.AddDbgLog($"Idea slot is added {slotConfig}");

                        }
                        Logger.AddDbgLog($"Slot is already exist:{slot.Name}");
                    }
                }
            }
            return result;
        }
    }
}
