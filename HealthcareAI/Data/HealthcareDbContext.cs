using HealthcareAI.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareAI.Data
{
    public class HealthcareDbContext : DbContext
    {
        public HealthcareDbContext(
            DbContextOptions<HealthcareDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<UserMemory> UserMemories { get; set; }

        public DbSet<DocumentChunk> DocumentChunks { get; set; }
    }
}
