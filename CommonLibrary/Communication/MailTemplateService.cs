namespace Communication
{
    public class MailTemplateService
    {
        private readonly string _templateRoot;

        public MailTemplateService(string templateRoot)
        {
            if (string.IsNullOrWhiteSpace(templateRoot))
                throw new ArgumentNullException(nameof(templateRoot));

            _templateRoot = templateRoot;
        }

        public string GetTemplate(string fileName)
        {
            string filePath = Path.Combine(_templateRoot, "Templates", "Mail", fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Template not found: {filePath}");

            return File.ReadAllText(filePath);
        }
    }
}
