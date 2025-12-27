namespace Repository
{
    public interface IJobConfigRepository
    {
        /// <summary>
        /// Returns enabled job configs used by the scheduler at startup.
        /// </summary>
        Task<IEnumerable<Models.JobConfig>> GetEnabledAsync();
    }
}
