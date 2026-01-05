using Models.Enums;
using Models.Interfaces;
using Models.Types.Utils;

namespace Models.Configs
{
    public class CharacterTraitConfig : IConfig
    {
        public Identifier Id { get; set; }
        public Types.LocalizationData.ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public List<CharacterType> CharacterTypes { get; set; } = new List<CharacterType>();
        public TraitType TraitType { get; set; }
        public bool ShowInCombat { get; set; }
        public string Allowed { get; set; } //todo: implement trigger handling
        public string AiWillDo { get; set; } //todo: implement ai will do type handling, check https://hoi4.paradoxwikis.com/AI_modding#AI_will_do
        public IdeaSlotConfig CharacterSlot { get; set; }
        public CharacterTraitConfig SpecialistAdvisorTrait { get; set; }
        public CharacterTraitConfig ExpertAdvisorTrait { get; set; }
        public CharacterTraitConfig GeniusAdvisorTrait { get; set; }
        public List<SubUnitConfig> UnitType { get; set; }
        public string UnitTrigger { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; } = new Dictionary<ModifierDefinitionConfig, object>();
        public Dictionary<ModifierDefinitionConfig, object> ArmyComanderModifiers { get; set; } = new Dictionary<ModifierDefinitionConfig, object>();
        public Dictionary<ModifierDefinitionConfig, object> NonSharedModifiers { get; set; } = new Dictionary<ModifierDefinitionConfig, object>();
        public Dictionary<ModifierDefinitionConfig, object> CorpsCommanderModifiers { get; set; } = new Dictionary<ModifierDefinitionConfig, object>();
        public Dictionary<ModifierDefinitionConfig, object> FieldMarshalModifiers { get; set; } = new Dictionary<ModifierDefinitionConfig, object>();
        public Dictionary<ModifierDefinitionConfig, object> SubUnitModifiers { get; set; } = new Dictionary<ModifierDefinitionConfig, object>();
        public Dictionary<CharacterSkillType, int> SkillTypes { get; set; } = [];
        public Dictionary<CharacterSkillType, double> SkillTypesFactors { get; set; } = [];
        //todo: implement abilities https://hoi4.paradoxwikis.com/Character_modding#skill
        //todo: implement effects
        public string OnAdd { get; set; }
        public string OnRemove { get; set; }
        public string DailyEffect { get; set; }
        public List<CharacterTraitConfig> MutuallyExclusives { get; set; }
        public List<CharacterTraitConfig> Parents { get; set; }
        public int NumParentsRequired { get; set; }
        public int GuiRow { get; set; }
        public int GuiColumn { get; set; }
        public bool Random { get; set; }
        public string Prerequisites { get; set; } //triggers
        public double Cost { get; set; }
        public string GainXp { get; set; } //triggers
        public string GainXpLeader { get; set; } //triggers
        public double GainXpOnSpotting { get; set; }
        public string TraitXpFactor { get; set; } //triggers
    }
}
