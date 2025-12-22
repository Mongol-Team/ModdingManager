using System.Drawing;

namespace ModdingManagerModels
{
    public class IdeologyType
    {
        public string Parrent { get; set; }
        public bool CanBeRandomlySelected { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public override string ToString()
        {
            return $"{Name} (Random: {CanBeRandomlySelected}, Color: {Color.ToString()}, Parrent: {Parrent})";
        }
    }
}
