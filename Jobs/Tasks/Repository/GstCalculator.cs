namespace Tasks.Repository
{
    /*
     * var localSale = CalculateGst(productPrice, gstRate, false);
     */
    public class GstResult
    {
        public decimal BaseAmount { get; set; }
        public decimal TotalGst { get; set; }
        public decimal Cgst { get; set; }
        public decimal Sgst { get; set; }
        public decimal Igst { get; set; }
        public decimal FinalAmount { get; set; }
        public string TransactionType { get; set; }

    }

    public class GstCalculator
    {
        public GstResult CalculateGst(decimal amount, decimal ratePercent, bool isInterState)
        {
            //UGST it yet to be implemented, as it is applicable only for Union Territories without legislature
            decimal totalGst = amount * (ratePercent / 100);

            var result = new GstResult
            {
                BaseAmount = amount,
                TotalGst = totalGst,
                FinalAmount = amount + totalGst
            };

            if (isInterState)
            {
                // For Inter-state, 100% of GST goes to IGST
                result.Igst = totalGst;
                result.Cgst = 0;
                result.Sgst = 0;
                result.TransactionType = "Inter-state (IGST)";
            }
            else
            {
                // For Intra-state, GST is split 50-50 between CGST and SGST
                decimal splitTax = totalGst / 2;
                result.Cgst = splitTax;
                result.Sgst = splitTax;
                result.Igst = 0;
                result.TransactionType = "Intra-state (CGST + SGST)";
            }

            return result;
        }
    }
}