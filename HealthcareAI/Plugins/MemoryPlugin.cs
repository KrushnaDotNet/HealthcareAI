using HealthcareAI.Services;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace HealthcareAI.Plugins
{
    public class MemoryPlugin
    {
        private readonly LongTermMemoryStore _longTermMemoryStore;

        private readonly string _userId;

        public MemoryPlugin(LongTermMemoryStore longTermMemoryStore, string userId)
        {
            _longTermMemoryStore = longTermMemoryStore;
            _userId = userId;
        }

        /// <summary>
        /// This exposes your existing SQL memory method as a Semantic Kernel tool/function.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>

        [KernelFunction]
        [Description("""
        Saves useful non-sensitive information about the current user
        into long-term memory.
        Use this when the user provides a personal preference or useful
        information that should be remembered for future conversations.
        Do not save temporary or sensitive healthcare information.
        """)]
        public async Task<string> SaveUserMemoryAsync(string key, string value)
        {
            await _longTermMemoryStore.SaveMemoryAsync(
                _userId,
                key,
                value);

            return $"Memory saved successfully: {key} = {value}";
        }

        [KernelFunction]
        [Description("""
                        Retrieves a previously saved non-sensitive memory about the current user.
                        Use this when information from long-term memory is needed to answer the user's question.
                        """)]
        public async Task<string> GetUserMemoryAsync(string key)
        {
            var value = await _longTermMemoryStore.GetMemoryAsync(
                _userId,
                key);

            if (value == null)
            {
                return $"No memory found for key: {key}";
            }

            return value;
        }
    }
}
