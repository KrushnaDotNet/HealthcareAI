using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace HealthcareAI.RAG
{
    public class QdrantVectorStore
    {
        private readonly QdrantClient _qdrantClient;

        private const string CollectionName = "healthcare_documents";

        public QdrantVectorStore(QdrantClient qdrantClient)
        {
            _qdrantClient = qdrantClient;
        }

        public async Task StoreChunkAsync(
            string chunk,
            float[] embedding,
            string documentName)
        {
            var point = new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = embedding,
                Payload =
            {
                ["text"] = chunk,
                ["document"] = documentName
            }
            };

            await _qdrantClient.UpsertAsync(
                CollectionName,
                new[] { point });
        }
    }
}
