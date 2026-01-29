namespace Models.Interfaces.RawDataWorkerInterfaces
{
    public interface IHealer
    {
        public (string fixedLine, bool success) HealError(IError error, string originalLine);
        public List<IError> Errors { get; set; }
    }
}