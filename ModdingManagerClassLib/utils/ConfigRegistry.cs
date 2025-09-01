
using ModdingManager.classes.managers.gfx;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerModels;
using System.Drawing;
using System.Text;
using System.Windows.Media;

namespace ModdingManager.classes.utils
{
    public class ConfigRegistry
    {
        private ConfigRegistry() { }
        private static ConfigRegistry _instance = new();
        public static ConfigRegistry Instance => _instance ??= new ConfigRegistry();
        public List<RuleConfig> Rules { get; set; }
        public List<StateCathegoryConfig> StateCathegories { get; set; }
        public List<RegimentConfig> Regiments { get; set; }
        public List<CountryConfig> Countries { get; set; }
        public List<IdeaConfig> Ideas { get; set; }
        public List<StaticModifierConfig> StaticModifiers { get; set; }
        public List<OpinionModifierConfig> OpinionModifiers { get; set; }
        public List<DynamicModifierConfig> DynamicModifiers { get; set; }
        public List<ModifierDefenitionConfig> ModifierDefenitions { get; set; }
        public MapConfig Map { get; set; }
        public List<CountryCharacterConfig> Characters { get; set; }
        public List<IdeologyConfig> Ideologies { get; set; }
        
    }
}
