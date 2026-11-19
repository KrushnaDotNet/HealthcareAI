namespace HealthcareAI.Models
{
    public class DocumentChunk
    {
        public int Id { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string AccessLevel { get; set; } = string.Empty;

        public string ChunkText { get; set; } = string.Empty;

        public string Embedding { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
