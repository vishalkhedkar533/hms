namespace CommonLibrary
{
    public class FileService
    {
        private readonly string _templateRoot;

        public FileService(string templateRoot)
        {
            if (string.IsNullOrWhiteSpace(templateRoot))
                throw new ArgumentNullException(nameof(templateRoot));

            _templateRoot = templateRoot;
        }

        public string GetTemplate(string fileFolder, string fileName)
        {
            string filePath = Path.Combine(_templateRoot, fileFolder, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Template not found: {filePath}");

            return File.ReadAllText(filePath);
        }
    }
}
