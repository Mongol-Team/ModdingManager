namespace ModdingManagerModels.Args
{
    public class ProvinceTransferArg
    {
        public StateConfig? SourceState { get; set; }
        public int? ProvinceId { get; set; }
        public StateConfig? TargetState { get; set; }
        public StrategicRegionConfig? SourceRegion { get; set; }
        public StrategicRegionConfig? TargetRegion { get; set; }
        public string? TargetCountryTag { get; set; }
    }
}
