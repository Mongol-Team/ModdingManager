namespace ModdingManagerModels
{
    public class SupereventConfig : IModel
    {
        public string Id { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public System.Windows.Controls.Canvas EventConstructor { get; set; }
        public string SoundPath { get; set; }

    }
}
