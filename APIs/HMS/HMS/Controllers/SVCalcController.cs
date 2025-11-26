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
        public async Task<IActionResult> CalculateSV(SVCalcRequest svCalcRequest)
        {
            SVCalcResponse svCalcResponse = new SVCalcResponse();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(svCalcResponse);
        }
    }

    public class SVCalcRequest
    {
        public string? policyNo { get; set; }
        public string? policyStatus { get; set; }
        public string? premiumStatus { get; set; }
        public int? PT { get; set; }
        public int? PPT { get; set; }
        public int? premium { get; set; }
        public string? Product { get; set; }
        public DateTime? RCD { get; set; }
        public int? totalPremiumPaid { get; set; }
        public int? pendingInstallments { get; set; }
        public Customer? Proposer { get; set; }
        public Customer? Insured { get; set; }
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
