namespace HealthcareAI.Models
{
    public class Patient
    {
        public int Id { get; set; }

        public string PatientId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }
}
