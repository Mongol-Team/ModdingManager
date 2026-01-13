using Models.Configs;
using Models.Interfaces;

namespace Models.Types.HtmlFilesData
{
    public class ModifierDefinitionFile : IHoiData
    {
        public string FileFullPath { get; set; }
        public List<ModifierDefinitionConfig> ModifierDefinitions { get; set; }
    }
}
