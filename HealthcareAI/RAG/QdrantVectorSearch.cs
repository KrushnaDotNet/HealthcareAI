using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace HealthcareAI.RAG
{
    public class QdrantVectorSearch
    {
        private readonly QdrantClient _qdrantClient;

        private const string CollectionName = "healthcare_documents";

        public QdrantVectorSearch(QdrantClient qdrantClient)
        {
            _qdrantClient = qdrantClient;
        }

        public async Task<List<string>> SearchAsync(float[] queryEmbedding, int topK = 3)
        {
            var results = await _qdrantClient.SearchAsync(CollectionName, queryEmbedding, limit: (ulong)topK);

            return results
                .Select(result =>
                    result.Payload.TryGetValue("text", out var text)
                        ? text.StringValue
                        : string.Empty)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();
        }
    }
}
