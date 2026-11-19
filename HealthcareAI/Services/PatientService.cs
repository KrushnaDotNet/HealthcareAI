using HealthcareAI.Data;
using HealthcareAI.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareAI.Services
{
    public class PatientService : IPatientService
    {
        private readonly HealthcareDbContext _context;

        public PatientService(HealthcareDbContext context)
        {
            _context = context;
        }

        public async Task<List<Patient>> SearchPatientsAsync(string patientName)
        {
            return await _context.Patients
                .AsNoTracking()
                .Where(p => p.Name == patientName)
                .ToListAsync();
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Patient> CreatePatientAsync(string name, int age)
        {
            var patient = new Patient
            {
                PatientId = $"P{Random.Shared.Next(1000, 9999)}",
                Name = name,
                Age = age
            };

            _context.Patients.Add(patient);

            await _context.SaveChangesAsync();

            return patient;
        }

        public async Task<Patient> UpdatePatientAsync(string patientName , int Newage)
        {
            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == patientName);

            if (patient == null)
                throw new InvalidOperationException("Patient not found.");

            patient.Age = Newage;

            _context.Patients.Update(patient);

            await _context.SaveChangesAsync();

            return patient;
        }
    }
}