namespace ModdingManagerModels
{
    public class IdeaConfig : IModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public string Modifiers { get; set; }
        public string RemovalCost { get; set; }
        public string Available { get; set; }
        public string AvailableCivilWar { get; set; }
        public string OnAdd { get; set; }
    }
}
