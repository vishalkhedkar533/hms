namespace Repository
{
    public interface IJobConfigRepository
    {
        /// <summary>
        /// Returns enabled job configs used by the scheduler at startup.
        /// </summary>
        Task<IEnumerable<Models.JobConfig>> GetEnabledAsync();

        /// <summary>
        /// Returns a single job config by id.
        /// </summary>
        Task<Models.JobConfig?> GetByIdAsync(int id);
    }
}
