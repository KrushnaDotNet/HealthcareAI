using HealthcareAI.RAG;
using HealthcareAI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HealthcareAI.Data;
using HealthcareAI.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthcareController : ControllerBase
    {
        private readonly SemanticKernelService _semanticKernelService;

        private readonly LongTermMemoryStore _longTermMemoryStore;

        private readonly DocumentLoader _documentLoader;

        private readonly TextChunker _textChunker;

        private readonly EmbeddingService _embeddingService;

        private readonly HealthcareDbContext _dbContext;

        private readonly VectorSimilarityService _vectorSimilarityService;

        public class AskRequest
        {
            public string ConversationId { get; set; } = string.Empty;

            public string Question { get; set; } = string.Empty;

            public string UserId { get; set; } = string.Empty;
        }

        public HealthcareController(SemanticKernelService semanticKernelService, LongTermMemoryStore longTermMemoryStore,
            DocumentLoader documentLoader, TextChunker textChunker, EmbeddingService embeddingService, HealthcareDbContext dbContext,
            VectorSimilarityService vectorSimilarityService)
        {
            _semanticKernelService = semanticKernelService;
            _longTermMemoryStore = longTermMemoryStore;
            _documentLoader = documentLoader;
            _textChunker = textChunker;
            _embeddingService = embeddingService;
            _dbContext = dbContext;
            _vectorSimilarityService = vectorSimilarityService;
        }

        ////[HttpPost("memory")]
        ////public IActionResult SaveMemory(string user, string key, string value)
        ////{
        ////    _longTermMemoryStore.SaveMemory(user, key, value);

        ////    return Ok(new 
        ////    { 
        ////        Message = "Memory saved successfully." 
        ////    });
        ////}

        ////[HttpGet("memory")]
        ////public IActionResult GetMemory(string userId, string key)
        ////{
        ////    var value = _longTermMemoryStore.GetMemory(userId, key);

        ////    if (value == null)
        ////    {
        ////        return NotFound(new
        ////        {
        ////            Message = "Memory not found."
        ////        });
        ////    }

        ////    return Ok(new
        ////    {
        ////        userId,
        ////        key,
        ////        value
        ////    });
        ////}

        /// <summary>
        /// Saves a memory for a specific user with SQL DB.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        
        [HttpPost("memory")]
        public async Task<IActionResult> SaveMemory(string userId, string key, string value)
        {
            await _longTermMemoryStore.SaveMemoryAsync(userId, key, value);

            return Ok(new
            {
                Message = "Memory saved successfully."
            });
        }

        [HttpGet("memory")]
        public async Task<IActionResult> GetMemory(string userId, string key)
        {
            var value = await _longTermMemoryStore.GetMemoryAsync(userId, key);

            if (value == null)
            {
                return NotFound(new
                {
                    Message = "Memory not found."
                });
            }

            return Ok(new
            {
                userId,
                key,
                value
            });
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            var response = await _semanticKernelService.AskAsync(request.ConversationId, request.UserId, request.Question);

            return Ok(response);
        }

        [HttpGet("rag/document")]
        public IActionResult ReadRagDocument()
        {
            var filePath = @"C:\Users\krush\source\repos\HealthcareAI\RAG\Documents\AppointmentPolicy.txt";

            var content = _documentLoader.LoadDocument(filePath);

            return Ok(new
            {
                FilePath = filePath,
                Content = content
            });
        }

        [HttpGet("rag/chunks")]
        public IActionResult GetDocumentChunks()
        {
            var filePath = @"C:\Users\krush\source\repos\HealthcareAI\RAG\Documents\AppointmentPolicy.txt";

            var content = _documentLoader.LoadDocument(filePath);

            ////var chunks = _textChunker.SplitText(
            ////    content,
            ////    chunkSize: 30,
            ////    overlap: 5);

            var chunks = _textChunker.SplitText(content, sentencesPerChunk: 2);

            return Ok(new
            {
                TotalChunks = chunks.Count,
                Chunks = chunks
            });
        }

        [HttpGet("rag/embedding")]
        public async Task<IActionResult> TestEmbedding()
        {
            var text = "Appointments should be cancelled at least 24 hours before the scheduled appointment.";

            var embedding = await _embeddingService
                .GenerateEmbeddingAsync(text);

            return Ok(new
            {
                Text = text,
                Dimension = embedding.Length,
                FirstValues = embedding.Take(10)
            });
        }

        [HttpPost("rag/index")]
        public async Task<IActionResult> IndexDocument()
        {
            var filePath = @"C:\Users\krush\source\repos\HealthcareAI\RAG\Documents\AppointmentPolicy.txt";

            var content = _documentLoader.LoadDocument(filePath);

            ////var chunks = _textChunker.SplitText(
            ////    content,
            ////    chunkSize: 30,
            ////    overlap: 5);


            var chunks = _textChunker.SplitText(content, sentencesPerChunk: 2);

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingService
                    .GenerateEmbeddingAsync(chunk);

                var documentChunk = new DocumentChunk
                {
                    DocumentName = "AppointmentPolicy.txt",
                    Category = "Appointment",
                    AccessLevel = "Patient",
                    ChunkText = chunk,
                    Embedding = JsonSerializer.Serialize(embedding)
                };

                _dbContext.DocumentChunks.Add(documentChunk);
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Document indexed successfully.",
                TotalChunks = chunks.Count
            });
        }

        /// <summary>
        /// we need to retrieve the stored chunks from DocumentChunks DB
        /// </summary>
        /// <returns></returns>

        [HttpGet("rag/chunks-db")]
        public async Task<IActionResult> GetChunksFromDb()
        {
            var chunks = await _dbContext.DocumentChunks
                .ToListAsync();

            return Ok(chunks);
        }

        /// <summary>
        /// Generate the user's question embedding
        /// Now we need to create vectorA from the user's question.
        /// endpoint controller - rag/question-embedding
        /// This question embedding is not stored in DB yet. We generate it when searching.
        /// </summary>
        /// <returns></returns>

        [HttpGet("rag/question-embedding")]
        public async Task<IActionResult> GenerateQuestionEmbedding()
        {
            var question = "When should an appointment be cancelled?";

            var embedding = await _embeddingService
                .GenerateEmbeddingAsync(question);

            return Ok(new
            {
                Question = question,
                Dimension = embedding.Length,
                Embedding = embedding
            });
        }

        /// <summary>
        /// Connect Question → DB Chunks → Similarity Search
        /// </summary>
        /// <returns></returns>

        [HttpGet("rag/search")]
        public async Task<IActionResult> SearchRelevantChunk()
        {
            var question = "When should an appointment be cancelled?";

            // 1. Generate question embedding
            var questionEmbedding = await _embeddingService
                .GenerateEmbeddingAsync(question);

            // 2. Load stored chunks from DB
            var chunks = await _dbContext.DocumentChunks
                .ToListAsync();

            // 3. Find most similar chunk
            var bestChunk = await _vectorSimilarityService
                .FindMostSimilarChunkAsync(
                    questionEmbedding,
                    chunks);

            // from the bestChunk 
            //The embedding is only used for matching; ChunkText is the human-readable content returned.

            return Ok(new
            {
                Question = question,
                RelevantChunk = bestChunk?.ChunkText
            });
        }

        /// <summary>
        /// Send retrieved chunk to the LLM
        /// Now we connect retrieval → LLM.
        /// </summary>
        /// <returns></returns>

        [HttpPost("rag/answer")]
        public async Task<IActionResult> GetRagAnswer([FromBody] RagRequest request)
        {
            var question = request.Question;

            if (string.IsNullOrWhiteSpace(question))
                return BadRequest("Question is required.");

            if (string.IsNullOrWhiteSpace(request.ConversationId))
                return BadRequest("ConversationId is required.");


            // 1. Generate question embedding
            var questionEmbedding = await _embeddingService
                .GenerateEmbeddingAsync(question);

            // 2. Load stored chunks
            var chunks = await _dbContext.DocumentChunks
                .ToListAsync();

            // 3. Find relevant chunk

            ////var bestChunk = await _vectorSimilarityService
            ////    .FindMostSimilarChunkAsync(
            ////        questionEmbedding,
            ////        chunks);

            //// Top-K Retrieval

            var topChunks = _vectorSimilarityService
                            .FindTopKSimilarChunks(
                                questionEmbedding,
                                chunks,
                                topK: 3);

            //// Now we need to combine the Top-K chunks into one context for the LLM.

            var context = string.Join(
                                    "\n\n",
                                    topChunks.Select(x => x.Chunk.ChunkText));

            ////if (bestChunk == null)
            ////    return NotFound("No relevant information found.");

            if (topChunks.Count == 0)
                return NotFound("No relevant information found.");

            var bestChunk = topChunks[0].Chunk;

            // 4. Create prompt using question + retrieved context
            var prompt = $"""
                            Answer the question using only the provided context.

                            Context:
                            {bestChunk.ChunkText}

                            Question:
                            {question}

                            If the answer is not available in the context, say:
                            "I don't have enough information in the provided policy."
                            """;

            // 5. Send prompt to Semantic Kernel / LLM
            ////var answer = await _semanticKernelService.AskRagAsync(question, bestChunk.ChunkText);

            // Now we send the combined context to the LLM for answering.
            // var answer = await _semanticKernelService.AskRagAsync(question, context);

            // RAG with Chat History

            var answer = await _semanticKernelService.AskRagConversationAsync(
                                                                    request.ConversationId,
                                                                    question,
                                                                    context);

            ////return Ok(new
            ////{
            ////    Question = question,
            ////    RelevantChunk = bestChunk.ChunkText,
            ////    Answer = answer
            ////});

            return Ok(new
            {
                Question = question,
                RetrievedChunks = topChunks.Select(x => new
                {
                    x.Chunk.ChunkText,
                    x.Score
                }),
                Answer = answer
            });
        }
    }
}
