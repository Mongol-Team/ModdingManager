
using Application.Debugging;
using Application.Extentions;
using Application.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Configs;

namespace Application.Composers
{
    public class IdeaGroupComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            string[] pathes =
            {
                GamePathes.IdeasPath,
                ModPathes.IdeasPath
            };

            List<IConfig> result = new List<IConfig>();
            foreach (string path in pathes)
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
            PaseDynamicModifierDefenitions(result);
            return result;
        }

        public static List<IConfig> ParseSingleFile(HoiFuncFile file)
        {
            List<IConfig> result = new List<IConfig>();
            foreach (Bracket br in file.Brackets)
            {
                if (br.Name == "ideas")
                {
                    foreach (Bracket slot in br.SubBrackets)
                    {
                        IdeaGroupConfig slotConfig = ModDataStorage.Mod.IdeaSlots.FindById(slot.Name);
                        if (slotConfig == null)
                        {
                            Var isLaw = slot.SubVars.FirstOrDefault(b => b.Name == "law");
                            Var useListView = slot.SubVars.FirstOrDefault(b => b.Name == "use_list_view");
                            Var designer = slot.SubVars.FirstOrDefault(b => b.Name == "designer");
                            slotConfig = new IdeaGroupConfig()
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
                                IdeaConfig parsedIdea = ModDataStorage.Mod.Ideas.FindById(idea.Name);
                                ModDataStorage.Mod.Ideas = new();
                                if (parsedIdea == null)
                                {
                                    parsedIdea = IdeaComposer.ParseSingleIdea(idea) as IdeaConfig;

                                    ModDataStorage.Mod.Ideas.Add(parsedIdea);
                                }
                                ;
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
                            //Logger.AddDbgLog($"Idea slot is added {slotConfig}");

                        }
                        //Logger.AddDbgLog($"Slot is already exist:{slot.Name}");
                    }
                }
            }
            return result;
        }

        public static void PaseDynamicModifierDefenitions(List<IConfig> ideaGroups)
        {
            foreach (IdeaGroupConfig ig in ideaGroups)
            {
                if (ig.Id != null)
                {
                    ModifierDefinitionConfig def = new ModifierDefinitionConfig()
                    {

                        ScopeType = ScopeTypes.Country,
                        ValueType = ModifierDefenitionValueType.Percent,
                        IsCore = true,
                        ColorType = ModifierDefenitionColorType.Bad,
                        Precision = 2,
                        FilePath = DataDefaultValues.ItemCreatedDynamically,
                        Cathegory = ModifierDefinitionCathegoryType.Country,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                    };

                    def.Id = new Identifier($"{ig.Id}_cost_factor");
                    def.Localisation = new ConfigLocalisation()
                    {
                        Language = ModdingManagerSettings.Instance.CurrentLanguage,
                    };
                    def.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(def.Id.ToString()));
                    ModDataStorage.Mod.ModifierDefinitions.Add(def);
                }
            }
        }
    }
}
