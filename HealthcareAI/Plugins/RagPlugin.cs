using HealthcareAI.Data;
using HealthcareAI.RAG;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace HealthcareAI.Plugins
{
    public class RagPlugin
    {
        private readonly HealthcareDbContext _dbContext;

        private readonly EmbeddingService _embeddingService;

        private readonly VectorSimilarityService _vectorSimilarityService;

        public RagPlugin(HealthcareDbContext dbContext, EmbeddingService embeddingService, VectorSimilarityService vectorSimilarityService)
        {
            _dbContext = dbContext;
            _embeddingService = embeddingService;
            _vectorSimilarityService = vectorSimilarityService;
        }

        /// <summary>
        /// What does this plugin do?
        /// It moves our existing RAG retrieval logic into a Semantic Kernel function.
        /// [KernelFunction] tells Semantic Kernel: This method is available as a tool that the AI can call.
        /// The Agent can now potentially call:SearchHospitalPolicyAsync(question)
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>

            [KernelFunction]
            [Description("Search hospital policy documents for appointment cancellation, late cancellation, appointment rules, arrival requirements, and other hospital policies.")]
            public async Task<string> SearchHospitalPolicyAsync(string question, string accessLevel)
            {
                var questionEmbedding =
                    await _embeddingService
                        .GenerateEmbeddingAsync(question);

            var chunks = await _dbContext.DocumentChunks
                        .Where(x => x.Category == "Appointment" && x.AccessLevel == accessLevel)
                        .ToListAsync();

            var topChunks =
                    _vectorSimilarityService
                        .FindTopKSimilarChunks(
                            questionEmbedding,
                            chunks,
                            topK: 3,
                            minScore: 0.70);

            foreach (var result in topChunks)
            {
                Console.WriteLine(
                    $"RAG Evaluation | Document: {result.Chunk.DocumentName} | " +
                    $"Category: {result.Chunk.Category} | " +
                    $"Access: {result.Chunk.AccessLevel} | " +
                    $"Score: {result.Score:F4}");
            }

            if (!topChunks.Any())
                {
                    return "No relevant hospital policy information was found.";
                }

                return string.Join(
                    "\n\n",
                    topChunks.Select(x => x.Chunk.ChunkText));
            }
        }
}
