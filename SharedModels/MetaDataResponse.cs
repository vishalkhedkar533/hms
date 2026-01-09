namespace SharedModels
{
    public class MetaDataResponse
    {
        public List<MetaProperty> policy { get; set; }
        public List<MetaProperty> agent { get; set; }
        public List<MetaProperty> premium { get; set; }
        public List<MetaProperty> insured { get; set; }
        public List<MetaProperty> owner { get; set; }
        public List<MetaProperty> commrate { get; set; }
    }
}
