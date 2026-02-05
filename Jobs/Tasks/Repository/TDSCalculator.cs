using Tasks.Models;

namespace Tasks.Repository
{
    public class TDSCalculator
    {
        //private const decimal ThresholdLimit = 20000m;
        //private const double StandardRate = 0.02; // 2%
        //private const double HigherRateNoPan = 0.20; // 20%
        //totalPaymentInFY - Total commission paid to the person in the current Financial Year so far (including current bill)
        //currentBillAmount - The current invoice/bill amount (excluding GST)
        public decimal Calculate194H(decimal totalPaymentInFY,decimal currentBillAmount, 
            bool hasValidPan, int orgID,FinancialPeriod financialPeriod)
        {            
            // Rule: TDS is only applicable if the total payment in the FY exceeds the threshold
            if (totalPaymentInFY <= (financialPeriod.TdsThresholdLimit/100))
            {
                return 0m;
            }

            // Rule: Determine rate based on PAN availability
            decimal applicableRate = hasValidPan ? (financialPeriod.TdsStandardRate/100) : (financialPeriod.NoPanRate/100);

            // Rule: If this specific payment pushes the total over the threshold for the first time, 
            // you might need to deduct TDS on the entire amount paid so far. 
            // For simplicity, this calculates on the current bill.
            decimal tdsAmount = totalPaymentInFY * applicableRate;

            return Math.Round(tdsAmount, 2);
        }
    }
}