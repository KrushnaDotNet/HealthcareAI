using HealthcareAI.Models;

namespace HealthcareAI.Services
{
    public interface IPatientService
    {
        Task<Patient> CreatePatientAsync(string name, int age);

        Task<List<Patient>> GetAllPatientsAsync();

        Task<List<Patient>> SearchPatientsAsync(string patientName);

        Task<Patient> UpdatePatientAsync(string patientName, int newAge);
    }
}
