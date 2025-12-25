using CommisionCalc.Insurance.Entities;
using CommisionCalc.Insurance.Interfaces;
using System.Data;

namespace CommisionCalc.Infrastructure.Repository.Insurance
{
    public class CommissionConfig : ICommissionConfig
    {
        private readonly IDbConnection _dbConnection;
        Task<IEnumerable<CommisionConfigRecord>> ICommissionConfig.GetCommissionConfigAsync(int batchSize)
        {
            throw new NotImplementedException();
        }
    }
}
