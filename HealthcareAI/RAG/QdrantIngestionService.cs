namespace HealthcareAI.RAG
{
    public class QdrantIngestionService
    {
        private readonly TextChunker _textChunker;
        private readonly QdrantVectorStore _vectorStore;
        private readonly EmbeddingService _embeddingService;

        public QdrantIngestionService(
            TextChunker textChunker,
            QdrantVectorStore vectorStore,
            EmbeddingService embeddingService)
        {
            _textChunker = textChunker;
            _vectorStore = vectorStore;
            _embeddingService = embeddingService;
        }

        public async Task IngestDocumentAsync(
            string text,
            string documentName)
        {
            var chunks = _textChunker.SplitText(text);

            foreach (var chunk in chunks)
            {
                var embedding =
                    await _embeddingService.GenerateEmbeddingAsync(chunk);

                await _vectorStore.StoreChunkAsync(
                    chunk,
                    embedding,
                    documentName);
            }
        }
    }
}
