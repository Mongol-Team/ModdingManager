namespace Models.Args
{
    public class StateTransferArg
    {
        public int? StateId { get; set; }
        public string? SourceCountryTag { get; set; }
        public string? TargetCountryTag { get; set; }
    }
}
