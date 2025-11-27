using AutoMapper;
using HMS.Data;
using HMS.Security;
using HMS.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;

namespace HMS.Controllers
{
    public class SVCalcController : Controller
    {
        private readonly HMSContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly DatabaseService _db;

        public SVCalcController(HMSContext context, IConfiguration config, IMapper mapper, DatabaseService db)
        {
        }
        [HttpPost("CalculateSV")]
        //[MenuAuthorize(1001)]
        public async Task<IActionResult> CalculateSV(PolicySearchResponse policySearchResponse)
        {
            SVCalcResponse svCalcResponse = new SVCalcResponse();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(svCalcResponse);
        }


        [HttpPost("SearchPolicy")]
        //[MenuAuthorize(1001)]
        public async Task<IActionResult> SearchPolicy(PolicySearchRequest policySearchRequest)
        {
            PolicySearchResponse policySearchResponse = new PolicySearchResponse();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(policySearchResponse);
        }
    }

    public class PolicySearchRequest
    {
        public string? policyNo { get; set; } = "POL12345";
    }

    public class PolicySearchResponse
    {
        public string? policyNo { get; set; } = "POL12345";
        public string? policyStatus { get; set; } = "Inforce";
        public string? premiumStatus { get; set; } = "Premium Paying";
        public int? PT { get; set; } = 20;
        public int? PPT { get; set; } = 20;
        public int? premium { get; set; } = 2000;
        public string? Product { get; set; } = "Star Union Dai-ichi Life Saral Jeevan Bima";
        public DateTime? RCD { get; set; } = DateTime.Now.AddYears(-10);
        public int? totalPremiumPaid { get; set; } = 20;
        public int? pendingInstallments { get; set; } = 5;
        public Customer? Proposer { get; set; } = new Customer { Name = "John Doe", DoB = DateTime.Now.AddYears(-30), Age = 30 };
        public Customer? Insured { get; set; }= new Customer { Name = "Jane Doe", DoB = DateTime.Now.AddYears(-25), Age = 25 };
        public string? UIN { get; set; } ="UIN12345";
        public string? ProductOption { get; set; } ="Option1";
    }

    public class Customer
    {
        public string? Name { get; set; }
        public DateTime? DoB { get; set; }
        public int? Age { get; set; }
    }
    public class SVCalcResponse
    {
        public DateTime SurvivalBenStartDt { get; set; } = DateTime.Now;
        public int SurvivalBenInstall { get; set; } = 10;
        public decimal SurvivalBenPaid { get; set; } = 1000;
        public decimal BonusAccured { get; set; } = 1001;
        public decimal UnclaimedFund { get; set; } = 1002;
        public decimal ValueTillDt { get; set; } = 1003;
        public decimal ValueOnMaturity { get; set; } = 1004;
        public decimal RevivalAmt { get; set; } = 1005;
        public decimal BalancePayable { get; set; } = 1005;
        public decimal SurrenderValue { get; set; } = 1005;
        public decimal BonusValue { get; set; } = 1005;
        public decimal TotalPremiumPaid { get; set; } = 1005;
        public DateTime CalculationDt { get; set; } = DateTime.Now;
    }
}
