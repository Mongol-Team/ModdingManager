namespace ModdingManagerModels.Types
{
    public class HoiReference
    {
        public string Value { get; set; } = string.Empty;
        public bool IsCore { get; set; } = false;
        public override string ToString()
        {
            return Value;
        }
    }
}
