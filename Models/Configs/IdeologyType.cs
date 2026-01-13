using System.Drawing;

namespace Models.Configs
{
    public class IdeologyType
    {
        public string Parrent { get; set; }
        public bool CanBeRandomlySelected { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public override string ToString()
        {
            return $"{Name} (Random: {CanBeRandomlySelected}, Color: {Color.ToString()}, Parrent: {Parrent})";
        }
    }
}
