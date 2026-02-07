using System.Text.Json;

namespace Tasks.Repository
{

    /*
     * var ptService = new PTService(@"Tasks\ProfTaxStructure\PTStructure.json");
    // Example: Calculate for Maharashtra, Male, earning 15,000 in February
    decimal tax = ptService.CalculateTax("Maharashtra", 15000, "Male", 2); 
    // Returns: 300
     */
    public class ProfTaxCalculator
    {
        private readonly Dictionary<string, StateLaw> _stateLaws;

        public ProfTaxCalculator(string jsonPath)
        {
            string jsonString = File.ReadAllText(jsonPath);
            _stateLaws = JsonSerializer.Deserialize<Dictionary<string, StateLaw>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public decimal CalculateTax(string state, decimal monthlyIncome, string gender = "Male", int? month = null)
        {
            if (!_stateLaws.ContainsKey(state)) return 0;

            var law = _stateLaws[state];
            int calcMonth = month ?? DateTime.Now.Month;

            // 1. Check for specific Income Slabs
            var slab = law.Slabs.FirstOrDefault(s =>
                monthlyIncome >= s.MinIncome &&
                (s.MaxIncome == null || monthlyIncome <= s.MaxIncome));

            if (slab == null) return 0;

            // 2. Handle Maharashtra Female Exception (No tax if income <= threshold)
            if (state == "Maharashtra" && gender == "Female" && slab.SpecialRates?.FemaleThreshold != null)
            {
                if (monthlyIncome <= slab.SpecialRates.FemaleThreshold) return 0;
            }

            // 3. Handle the "February Jump" (₹300 instead of ₹200)
            if (calcMonth == 2 && slab.SpecialRates?.February != null)
            {
                return slab.SpecialRates.February.Value;
            }

            return slab.Rate;
        }
    }

    // Model classes to match your JSON structure
    public class StateLaw
    {
        public string CalculationPeriod { get; set; }
        public decimal? FixedAnnualRateForProfessionals { get; set; }
        public List<Slab> Slabs { get; set; }
    }

    public class Slab
    {
        public decimal MinIncome { get; set; }
        public decimal? MaxIncome { get; set; }
        public decimal Rate { get; set; }
        public SpecialRates SpecialRates { get; set; }
    }

    public class SpecialRates
    {
        public decimal? February { get; set; }
        public decimal? FemaleThreshold { get; set; }
    }
}