using System.Net.Http.Json;
using static System.Net.Mime.MediaTypeNames;

namespace HealthcareAI.RAG
{
    /// <summary>
    /// Create Embedding Service
    /// Send text to Ollama's nomic-embed-text model and get back a numerical vector.
    /// </summary>

    public class EmbeddingService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// HttpClient allows our .NET application to call Ollama's HTTP API.
        /// </summary>
        /// <param name="httpClient"></param>
        
        public EmbeddingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Generate embedding
        /// The float[] contains the numerical embedding vector.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            /// <summary>
            /// Create a request object with the model and prompt.  
            /// Create Ollama request
            /// Model = nomic-embed-text
            /// Text = user's/document text
            /// </summary>

            var request = new
            {
                model = "nomic-embed-text",
                prompt = text
            };

            /// <summary>
            /// Call Ollama
            /// Send the request to Ollama's API and get the response.
            /// This calls your local Ollama server
            /// localhost:11434
            /// nomic-embed-text
            /// </summary>

            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:11434/api/embeddings",
                request);

            /// <summary>
            /// Ensure the response is successful. If not, throw an exception.
            /// Check response
            /// If Ollama returns an HTTP error such as 404 or 500, an exception is thrown.
            /// </summary>
            
            response.EnsureSuccessStatusCode();

            /// <summary>
            /// Read Ollama response
            /// Ollama returns JSON containing the embedding.
            /// "embedding": [0.123, -0.456, 0.789, ...]
            /// </summary>

            /// <summary>
            /// Read response
            /// Read the response content as JSON and deserialize it into an EmbeddingResponse object.
            /// Return vector
            /// Return the embedding vector as a float array. If the response is null, return an empty array.
            /// </summary>
            
            var result = await response.Content
                .ReadFromJsonAsync<EmbeddingResponse>();

            /// Return the embedding vector as a float array. If the response is null, return an empty array.
            /// This vector will eventually be stored in our vector database.

            return result?.Embedding
                   ?? Array.Empty<float>();
        }

        private class EmbeddingResponse
        {
            public float[] Embedding { get; set; } = Array.Empty<float>();
        }
    }
}
