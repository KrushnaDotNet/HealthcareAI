using Microsoft.SemanticKernel.ChatCompletion;

namespace HealthcareAI.Services
{
    public class ChatHistoryStore
    {
        private readonly Dictionary<string, ChatHistory> _histories = new();

        public ChatHistory GetOrCreateChat(string conversationId)
        {
            if (!_histories.TryGetValue(conversationId, out var history))
            {
                history = new ChatHistory();
                _histories[conversationId] = history;
            }

            return history;
        }
    }
}
