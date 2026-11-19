namespace HealthcareAI.Models
{
    public class UserMemory
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string MemoryKey { get; set; } = string.Empty;

        public string MemoryValue { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
