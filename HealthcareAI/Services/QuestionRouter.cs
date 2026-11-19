namespace HealthcareAI.Services
{
    public class QuestionRouter
    {
        /// <summary>
        /// This class decides:Does this question need RAG or not?
        /// It accepts the user's question and returns:true  → use RAG false → don't use RAG
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        
        public string GetIntent(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "None";

            bool requiresMemory =
                question.Contains("preferred language",
                    StringComparison.OrdinalIgnoreCase);

            bool requiresRag =
                question.Contains("hospital policy",
                    StringComparison.OrdinalIgnoreCase)
                || question.Contains("appointment policy",
                    StringComparison.OrdinalIgnoreCase)
                || question.Contains("cancel",
                    StringComparison.OrdinalIgnoreCase)
                || question.Contains("cancellation",
                    StringComparison.OrdinalIgnoreCase)
                || question.Contains("appointment rules",
                    StringComparison.OrdinalIgnoreCase)
                || question.Contains("arrive",
                    StringComparison.OrdinalIgnoreCase)
                || question.Contains("arrival",
                    StringComparison.OrdinalIgnoreCase)
                || question.Contains("24 hours",
                    StringComparison.OrdinalIgnoreCase)
                || question.Contains("emergency process",
                    StringComparison.OrdinalIgnoreCase);

            if (requiresMemory && requiresRag)
                return "MemoryAndRag";

            if (requiresMemory)
                return "Memory";

            if (requiresRag)
                return "Rag";

            return "None";
        }
    }
}