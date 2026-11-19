namespace HealthcareAI.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public string PatientId { get; set; } = string.Empty;

        public string AppointmentDate { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public string Status { get; set; } = "Scheduled";
    }
}
