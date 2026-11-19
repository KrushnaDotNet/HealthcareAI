namespace HealthcareAI.RAG
{
    public class DocumentLoader
    {
        public string LoadDocument(string filePath)
        {
            // Implement logic to load the document from the specified file path
            // For example, read the file content and return it as a string
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file at path {filePath} was not found.");
            }

            return File.ReadAllText(filePath);
        }
    }
}
