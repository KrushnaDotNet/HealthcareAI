using HealthcareAI.Models;

namespace HealthcareAI.RAG
{
    public class VectorSimilarityService
    {
        /// <summary>
        /// vector similarity search
        /// vectorA → user question embedding
        /// vectorB → stored document chunk embedding
        /// Example vectorA → [0.25, 0.71, -0.32, 0.45] → Question: "When should I cancel my appointment?"
        /// Example vectorB → [0.28, 0.68, -0.30, 0.42] → Stored chunk: "Appointments should be cancelled at least 24 hours before..."
        /// vectorB → already stored in DB (chunk embedding).
        /// vectorA → generated when the user asks a question, then compared with stored vectors.
        /// </summary>
        /// <param name="vectorA"></param>
        /// <param name="vectorB"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>

        public double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            /// <summary>
            /// Check dimensions
            /// Both vectors must have the same dimensions.
            /// We need these three values to calculate Cosine Similarity(dotProduct, magnitudeA, magnitudeB).
            /// Loop through all 768 values
            /// This compares the vectors dimension by dimension.
            /// </summary>

            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vector dimensions must match.");

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            /// <summary>
            /// Calculate dot product
            /// It multiplies corresponding values and adds them together.
            /// This calculates the "length" of each vector.
            /// </summary>
            /// <returns></returns>

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];

                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            if (magnitudeA == 0 || magnitudeB == 0)
                return 0;

            /// <summary>
            /// Final Cosine Similarity
            /// Higher score → more similar.
            /// This is the actual retrieval part of RAG.
            /// </summary>
            /// <returns></returns>

            return dotProduct /
                   (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }

        /// <summary>
        /// This method performs the actual retrieval from our stored embeddings.
        /// Tow input parameters: questionEmbedding and stored document chunks.
        /// questionEmbedding → vectorA → embedding generated from the user's question.
        /// chunks → all document chunks loaded from SQL Server, including their stored embeddings (vectorB).
        /// </summary>
        /// <param name="questionEmbedding"></param>
        /// <param name="chunks"></param>
        /// <returns></returns>

        public async Task<DocumentChunk?> FindMostSimilarChunkAsync(float[] questionEmbedding, List<DocumentChunk> chunks)
        {
            /// <summary>
            /// Keep track of the best result
            /// As we compare chunks, we'll keep the one with the highest similarity score.
            /// bestChunk = nothing
            /// highestScore = very small value
            /// </summary>

            DocumentChunk? bestChunk = null;
            double highestScore = double.MinValue;

            /// <summary>
            /// Check every stored chunk
            /// Convert stored JSON back to vector
            /// Remember, we stored the embedding as JSON:
            /// This converts it back to:
            /// Now we can perform mathematical comparison.
            /// Calculate similarity
            /// Keep the highest score . Example (Chunk 1 → 0.62,  Chunk 2 → 0.91 , Chunk 3 → 0.55)
            /// highest -> 0.91
            /// bestChunk = Chunk 2
            /// Return the relevant chunk
            /// </summary>

            foreach (var chunk in chunks)
            {
                var chunkEmbedding =
                    System.Text.Json.JsonSerializer.Deserialize<float[]>(
                        chunk.Embedding);

                if (chunkEmbedding == null)
                    continue;

                var score = CosineSimilarity(
                    questionEmbedding,
                    chunkEmbedding);

                if (score > highestScore)
                {
                    highestScore = score;
                    bestChunk = chunk;
                }
            }

            return bestChunk;
        }

        /// <summary>
        /// This method changes our retrieval from 1 chunk → Top-K chunks.
        /// questionEmbedding → user's question vector (vectorA)
        /// chunks → all chunks retrieved from SQL Server
        /// topK = 3 → return the 3 highest-scoring chunks
        /// Return value contains: Chunk → actual human-readable chunk and Score → similarity score
        /// </summary>
        /// <param name="questionEmbedding"></param>
        /// <param name="chunks"></param>
        /// <param name="topK"></param>
        /// <returns></returns>

        public List<(DocumentChunk Chunk, double Score)> FindTopKSimilarChunks(float[] questionEmbedding, List<DocumentChunk> chunks, int topK = 3, double minScore = 0.70)
        {
            var results = new List<(DocumentChunk Chunk, double Score)>();

            //We compare the question against every stored chunk.
            //Convert stored embedding back to float[]

            foreach (var chunk in chunks)
            {
                var chunkEmbedding =
                    System.Text.Json.JsonSerializer.Deserialize<float[]>(
                        chunk.Embedding);

                if (chunkEmbedding == null)
                    continue;
                //Calculate similarity

                var score = CosineSimilarity(questionEmbedding, chunkEmbedding);

                //Store chunk + score
                // Only return top 3 chunks.

                ////results.Add((chunk, score));

                //// Only return chunks whose similarity score is at least 0.70.

                if (score >= minScore)
                {
                    results.Add((chunk, score));
                }
            }

            /// <summary>
            /// Important: Top - K does not mean the first 3 chunks.It means the 3 most similar chunks.
            /// Sort highest first
            /// we take the first 3. If there were 10 chunks:            
            /// </summary>
            /// <param name="results"></param>
            /// <param name="topK"></param>
            /// <returns></returns>

            return results
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();
        }
    }
}
