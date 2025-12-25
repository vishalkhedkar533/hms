using CommisionCalc.Insurance.Entities;

namespace CommisionCalc.Insurance.Interfaces
{
    public interface ICommissionConfig
    {
        Task<IEnumerable<CommisionConfigRecord>> GetCommissionConfigAsync(int batchSize);
    }
}
