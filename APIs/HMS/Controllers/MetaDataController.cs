using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetaDataController : Controller
    {
        [HttpGet("MetaData")]
        public IActionResult GetHMSMetaData()
        {
            var metaDataResponse = new MetaDataResponse();
            var metadataList = new List<MetaProperty>();
            var agentProperties = typeof(SharedModels.BackEndCalculation.Agent).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var policyProperties = typeof(SharedModels.BackEndCalculation.Ins_Policy).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var insuredProperties = typeof(SharedModels.BackEndCalculation.Insured).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var ownerProperties = typeof(SharedModels.BackEndCalculation.Owner).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var commrateProperties = typeof(SharedModels.BackEndCalculation.CommRate).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            metaDataResponse.agent = GetMetaDataForType<SharedModels.BackEndCalculation.Agent>(agentProperties);
            metaDataResponse.policy = GetMetaDataForType<SharedModels.BackEndCalculation.Ins_Policy>(policyProperties);
            metaDataResponse.insured = GetMetaDataForType<SharedModels.BackEndCalculation.Insured>(insuredProperties);
            metaDataResponse.owner = GetMetaDataForType<SharedModels.BackEndCalculation.Owner>(ownerProperties);
            metaDataResponse.commrate = GetMetaDataForType<SharedModels.BackEndCalculation.CommRate>(commrateProperties);

            return Ok(metaDataResponse);
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
                    }
                    );
                }
            }
            return metadataList;
        }
    }

    public class MetaDataResponse
    {
        public List<MetaProperty> policy { get; set; }
        public List<MetaProperty> agent { get; set; }
        public List<MetaProperty> premium { get; set; }
        public List<MetaProperty> insured { get; set; }
        public List<MetaProperty> owner { get; set; }
        public List<MetaProperty> commrate { get; set; }

    }

    public class MetaProperty
    {
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
        public string Description { get; set; }
    }

}
