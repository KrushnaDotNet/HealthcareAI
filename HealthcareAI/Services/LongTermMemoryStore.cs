using HealthcareAI.Data;
using HealthcareAI.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareAI.Services
{
    public class LongTermMemoryStore
    {
        //// LongTermMemoryStore Dictionary RAM 

        ////private readonly Dictionary<string, Dictionary<string, string>> _memories = new();

        ////public void SaveMemory(string userId, string key, string value)
        ////{
        ////    if (!_memories.TryGetValue(userId, out var userMemory))
        ////    {
        ////        userMemory = new Dictionary<string, string>();
        ////        _memories[userId] = userMemory;
        ////    }

        ////    userMemory[key] = value;
        ////}

        ////public string? GetMemory(string userId, string key)
        ////{
        ////    if (_memories.TryGetValue(userId, out var userMemory) && userMemory.TryGetValue(key, out var value))
        ////    {
        ////        return value;
        ////    }

        ////    return null;
        ////}

        //// LongTermMemoryStore SQL DB

        private readonly HealthcareDbContext _dbContext;

        public LongTermMemoryStore(HealthcareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveMemoryAsync(
            string userId,
            string key,
            string value)
        {
            var existingMemory = await _dbContext.UserMemories
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.MemoryKey == key);

            if (existingMemory == null)
            {
                var memory = new UserMemory
                {
                    UserId = userId,
                    MemoryKey = key,
                    MemoryValue = value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.UserMemories.Add(memory);
            }
            else
            {
                existingMemory.MemoryValue = value;
                existingMemory.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<string?> GetMemoryAsync(
            string userId,
            string key)
        {
            var memory = await _dbContext.UserMemories
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.MemoryKey == key);

            return memory?.MemoryValue;
        }
    }
}
