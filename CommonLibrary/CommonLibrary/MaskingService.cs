namespace CommonLibrary
{
    public static class MaskingHelper
    {
        public static string? MaskPan(string? panNumber)
        {
            if (string.IsNullOrWhiteSpace(panNumber) || panNumber.Length <= 8)
                return panNumber; // too short to mask safely

            int visibleStart = 1; // keep 1st character
            int visibleEnd = 4;   // keep last 4 characters
            int maskLength = panNumber.Length - (visibleStart + visibleEnd);

            return panNumber.Substring(0, visibleStart) +
                   new string('X', maskLength) +
                   panNumber.Substring(panNumber.Length - visibleEnd);
        }
    }
}
