namespace HealthcareAI.Models
{
    public class AppointmentResponse
    {
        public bool Success { get; set; }

        public int AppointmentId { get; set; }

        public string PatientId { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public string AppointmentDate { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
