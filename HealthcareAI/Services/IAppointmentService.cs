using HealthcareAI.Models;

namespace HealthcareAI.Services
{
    public interface IAppointmentService
    {
        Task<Appointment> CreateAppointmentAsync(string patientId, string appointmentDate, string doctorName);

        Task<AppointmentResponse> CreateAppointmentForPatientAsync(string patientName, string appointmentDate, string doctorName);
    }
}
