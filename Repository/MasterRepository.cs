// File: MasterRepository.cs
using Database;
using MetaDataLoader;
using Models.DB;

namespace Repository
{

    public interface IMasterRepository
    {
        /// <summary>
        /// Fetches KeyValueEntry records based on the organization ID and entry category code.
        /// This uses the 'getKeyValueEntries' script defined in the metadata.
        /// </summary>
        /// <param name="orgId">The organization ID to filter by (@orgid).</param>
        /// <param name="entryCategory">The category code to filter by (@EntryCategory).</param>
        /// <returns>A collection of matching KeyValueEntry objects.</returns>
        Task<IEnumerable<KeyValueEntry>> GetKeyValueEntriesAsync(int orgId);
        Task<IEnumerable<KeyValueEntry>> GetKeyValueEntriesAsync(int orgId, string entryCategory);
    }
    public class MasterRepository : DapperRepositoryBase, IMasterRepository
    {
        // Constructor passes the Entity name "Master" to the base class
        public MasterRepository(IConnectionFactory connectionFactory, IMetadataLoader metadataLoader)
            : base(connectionFactory, metadataLoader, "Master") { }

        /// <summary>
        /// Implements the 'getKeyValueEntries' query defined in the metadata.
        /// </summary>
        /// <param name="orgId">The organization ID to filter by.</param>
        /// <param name="entryCategory">The category code to filter by.</param>
        /// <returns>A collection of matching KeyValueEntry objects.</returns>
        public async Task<IEnumerable<KeyValueEntry>> GetKeyValueEntriesAsync(int orgId)
        {
            var parameters = new
            {
                orgid = orgId,
            };
            return await QueryAsync<KeyValueEntry>(
                "getKeyValueEntriesByOrgId",
                parameters
            );
        }
        public async Task<IEnumerable<KeyValueEntry>> GetKeyValueEntriesAsync(int orgId, string entryCategory)
        {
            var parameters = new
            {
                orgid = orgId,
                EntryCategory = entryCategory
            };
            return await QueryAsync<KeyValueEntry>(
                "getKeyValueEntriesByOrgIdCategory",
                parameters
            );
        }
    }
}
