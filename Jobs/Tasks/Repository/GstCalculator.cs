using Tasks.Models;

namespace Tasks.Repository
{
    /*
     * var localSale = CalculateGst(productPrice, gstRate, false);
     */
    public class GstResult
    {
        public decimal BaseAmount { get; set; }
        public decimal TotalGst { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal FinalAmount { get; set; }
        public string TransactionType { get; set; }

    }

    public class GstCalculator
    {
        public GstResult CalculateGst(decimal amount, FinancialPeriod financialPeriod, bool isInterState)
        {
            //UGST it yet to be implemented, as it is applicable only for Union Territories without legislature
            //decimal totalGst = amount * (ratePercent / 100);

            var result = new GstResult
            {
                BaseAmount = amount,
                TotalGst = 0,
                FinalAmount = amount
            };

            if (isInterState)
            {
                // For Inter-state, 100% of GST goes to IGST
                result.IGST = (financialPeriod.IGST /100);
                result.CGST = 0;
                result.SGST = 0;
                result.TransactionType = "Inter-state (IGST)";
            }
            else
            {
                result.CGST = financialPeriod.CGST /100;
                result.SGST = financialPeriod.SGST /100;
                result.IGST = 0;
                result.TransactionType = "Intra-state (CGST + SGST)";
            }

            return result;
        }
    }
}