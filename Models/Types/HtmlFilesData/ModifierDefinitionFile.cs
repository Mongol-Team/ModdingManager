using ModdingManagerModels.Interfaces;

namespace ModdingManagerModels.Types.HtmlFilesData
{
    public class ModifierDefinitionFile : IHoiData
    {
        public string FilePath { get; set; }
        public List<ModifierDefinitionConfig> ModifierDefinitions { get; set; }
    }
}
