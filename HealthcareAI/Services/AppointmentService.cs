using HealthcareAI.Data;
using HealthcareAI.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareAI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly HealthcareDbContext _context;

        public AppointmentService(HealthcareDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment> CreateAppointmentAsync(string patientId, string appointmentDate, string doctorName)
        {
            var appointment = new Appointment
            {
                PatientId = patientId,
                AppointmentDate = appointmentDate,
                DoctorName = doctorName,
                Status = "Scheduled"
            };

            _context.Appointments.Add(appointment);

            await _context.SaveChangesAsync();

            return appointment;
        }
        
        public async Task<AppointmentResponse> CreateAppointmentForPatientAsync(string patientName, string appointmentDate, string doctorName)
        {

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Name == patientName);

            if (patient == null)
                throw new InvalidOperationException("Patient not found.");

            var doctorAlreadyBooked = await _context.Appointments
                                    .AnyAsync(a =>
                                        a.DoctorName == doctorName &&
                                        a.AppointmentDate == appointmentDate);

            if (doctorAlreadyBooked)
            {
                return new AppointmentResponse
                {
                    Success = false,
                    PatientId = patient.PatientId,
                    DoctorName = doctorName,
                    AppointmentDate = appointmentDate,
                    Status = "Conflict",
                    Message = $"Dr. {doctorName} already has an appointment at this time."
                };
            }   

            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                AppointmentDate = appointmentDate,
                DoctorName = doctorName,
                Status = "Scheduled"
            };

            _context.Appointments.Add(appointment);

            await _context.SaveChangesAsync();

            return new AppointmentResponse
            {
                Success = true,
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                DoctorName = appointment.DoctorName,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Message = "Appointment created successfully."
            };
        }
    }
}
