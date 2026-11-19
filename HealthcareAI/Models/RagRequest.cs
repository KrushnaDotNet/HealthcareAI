namespace HealthcareAI.Models
{
    /// <summary>
    /// Create RagRequest
    /// </summary>

    public class RagRequest
    {
        public string ConversationId { get; set; } = string.Empty;

        public string Question { get; set; } = string.Empty;
    }
}
