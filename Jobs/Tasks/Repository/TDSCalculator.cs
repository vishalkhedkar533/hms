namespace Tasks.Repository
{
    public class TDSCalculator
    {
        private const decimal ThresholdLimit = 20000m;
        private const double StandardRate = 0.02; // 2%
        private const double HigherRateNoPan = 0.20; // 20%
        //totalPaymentInFY - Total commission paid to the person in the current Financial Year so far (including current bill)
        //currentBillAmount - The current invoice/bill amount (excluding GST)
        public static decimal Calculate194H(decimal totalPaymentInFY,
            decimal currentBillAmount, bool hasValidPan, int orgID, DateTime EffectiveFrom, DateTime EffectiveTo)
        {
            // Rule: TDS is only applicable if the total payment in the FY exceeds the threshold
            if (totalPaymentInFY <= ThresholdLimit)
            {
                return 0m;
            }

            // Rule: Determine rate based on PAN availability
            double applicableRate = hasValidPan ? StandardRate : HigherRateNoPan;

            // Rule: If this specific payment pushes the total over the threshold for the first time, 
            // you might need to deduct TDS on the entire amount paid so far. 
            // For simplicity, this calculates on the current bill.
            decimal tdsAmount = currentBillAmount * (decimal)applicableRate;

            return Math.Round(tdsAmount, 2);
        }
    }
}