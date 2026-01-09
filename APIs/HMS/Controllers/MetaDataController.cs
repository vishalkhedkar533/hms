using HMS.Security;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using SharedModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetaDataController : Controller
    {
        private readonly ILogger<MetaDataController> _logger;
        public MetaDataController(ILogger<MetaDataController> logger)
        {
            _logger = logger;
        }
        [HttpGet("Fetch")]
        [MenuAuthorize(1001)]
        public async Task<IActionResult> GetHMSMetaData()
        {
            try
            {
                HmsResponse hMSResponse = new HmsResponse();
                var metaDataResponse = new MetaDataResponse();
                var metadataList = new List<MetaProperty>();
                var agentProperties = typeof(SharedModels.BackEndCalculation.Agent).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var policyProperties = typeof(SharedModels.BackEndCalculation.Ins_Policy).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var insuredProperties = typeof(SharedModels.BackEndCalculation.Insured).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var ownerProperties = typeof(SharedModels.BackEndCalculation.Owner).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var commrateProperties = typeof(SharedModels.BackEndCalculation.CommRate).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var premiumProperties = typeof(SharedModels.BackEndCalculation.PremiumCollected).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                metaDataResponse.agent = GetMetaDataForType<SharedModels.BackEndCalculation.Agent>(agentProperties);
                metaDataResponse.policy = GetMetaDataForType<SharedModels.BackEndCalculation.Ins_Policy>(policyProperties);
                metaDataResponse.insured = GetMetaDataForType<SharedModels.BackEndCalculation.Insured>(insuredProperties);
                metaDataResponse.owner = GetMetaDataForType<SharedModels.BackEndCalculation.Owner>(ownerProperties);
                metaDataResponse.commrate = GetMetaDataForType<SharedModels.BackEndCalculation.CommRate>(commrateProperties);
                metaDataResponse.premium = GetMetaDataForType<SharedModels.BackEndCalculation.PremiumCollected>(premiumProperties);
                hMSResponse.responseBody.metaDataResponse = metaDataResponse;
                return Ok(hMSResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meta Data Retrival at {UtcNow} Exception {message}", DateTime.UtcNow, ex.Message);
                return StatusCode(503, new
                {
                    status = "Failed",
                    database = "File Retrival",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
        private List<MetaProperty> GetMetaDataForType<T>(PropertyInfo[] propertyInfo)
        {
            var metadataList = new List<MetaProperty>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // Extract the [Column] attribute
                var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                // Extract the [Description] attribute
                var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                // Only include if a Description exists (as per your request)
                if (descAttr != null)
                {
                    metadataList.Add(new MetaProperty
                    {
                        PropertyName = prop.Name,
                        // Fallback to Property Name if [Column] attribute is missing
                        ColumnName = columnAttr?.Name ?? prop.Name,
                        Description = descAttr.Description,
                        DataType = (Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType).FullName,
                        IsNullable = Nullable.GetUnderlyingType(prop.PropertyType) != null || !prop.PropertyType.IsValueType
                    }
                    );
                }
            }
            return metadataList;
        }
    }


}