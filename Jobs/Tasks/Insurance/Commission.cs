using Quartz;

namespace Tasks.Insurance
{
    public class Commission
    {
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        public Commission(IJobExecutionContext jobExecutionContext)
        {
            _jobExecutionContext = jobExecutionContext;
            orgId = int.Parse(jobExecutionContext.JobDetail.JobDataMap.Values
                .Select((value, index) => new { value, index })
                .FirstOrDefault(x => x.index.Equals(
                    jobExecutionContext.JobDetail.JobDataMap.Keys
                    .Select((value, index) => new { value, index })
                    .FirstOrDefault(x => x.value == "orgId").index))
                .value.ToString());
            jobKey = jobExecutionContext.JobDetail.Key;
        }

        public void Calculate()
        {
            // Execution logic for commission calculation
            Console.WriteLine($"Calculating commissions as per job config: {_jobExecutionContext.JobDetail.Key}");
            /*
             using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

// 1. Define your data (These could come from a DB)
var pol = new Policy { Age = 25, Amount = 1000 };
var agent = new Agent { Level = "Senior", CommissionRate = 0.05 };
var premium = new Premium { TotalCollected = 1200 };

// 2. The formula entered by the user in the UI
// Example: "(pol.Age > 18 ? 0.5 : 0.1) * pol.Amount + (premium.TotalCollected * agent.CommissionRate)"
string userFormula = "(pol.Age > 18 ? 0.5 : 0.1) * pol.Amount + (premium.TotalCollected * agent.CommissionRate)";

// 3. Define the parameters for the engine
var parameters = new[] {
    Expression.Parameter(typeof(Policy), "pol"),
    Expression.Parameter(typeof(Agent), "agent"),
    Expression.Parameter(typeof(Premium), "premium")
};

// 4. Parse the formula into a compiled lambda
var e = DynamicExpressionParser.ParseLambda(parameters, typeof(double), userFormula);
var compiledFormula = e.Compile();

// 5. Execute by passing the actual object instances
var result = compiledFormula.DynamicInvoke(pol, agent, premium);

Console.WriteLine($"Calculated Result: {result}");
             */
        }
    }
}