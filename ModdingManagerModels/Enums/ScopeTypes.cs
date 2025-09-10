namespace ModdingManagerModels.Enums
{
    using System.ComponentModel;

    public enum ScopeTypes
    {
        [Description("Aggressive")]
        Aggressive,

        [Description("AI")]
        Ai,

        [Description("Air")]
        Air,

        [Description("Army")]
        Army,

        [Description("Autonomy")]
        Autonomy,

        [Description("Country")]
        Country,

        [Description("Defensive")]
        Defensive,

        [Description("Government In Exile")]
        GovernmentInExile,

        [Description("Intelligence Agency")]
        IntelligenceAgency,

        [Description("Military Advancements")]
        MilitaryAdvancements,

        [Description("Naval")]
        Naval,

        [Description("Peace")]
        Peace,

        [Description("Politics")]
        Politics,

        [Description("State")]
        State,

        [Description("Unit Leader")]
        UnitLeader,

        [Description("War Production")]
        WarProduction
    }
}
