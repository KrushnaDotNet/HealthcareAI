using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.RegularExpressions;

namespace HealthcareAI.RAG
{
    //Just for reference, we can use this class to split text into chunks based on word count and overlap.

    //public class TextChunker
    //{
    //    public List<string> SplitText(
    //        string text,
    //        int chunkSize = 100,
    //        int overlap = 20)
    //    {
    //        var chunks = new List<string>();

    //        if (string.IsNullOrWhiteSpace(text))
    //            return chunks;

    //        var words = text
    //            .Split(
    //                new[] { ' ', '\r', '\n', '\t' },
    //                StringSplitOptions.RemoveEmptyEntries);

    //        int start = 0;

    //        while (start < words.Length)
    //        {
    //            int end = Math.Min(
    //                start + chunkSize,
    //                words.Length);

    //            var chunk = string.Join(
    //                " ",
    //                words[start..end]);

    //            chunks.Add(chunk);

    //            if (end == words.Length)
    //                break;

    //            start = end - overlap;
    //        }

    //        return chunks;
    //    }
    //}

    /// <summary>
    /// This class is responsible for breaking a large document into smaller chunks.
    /// text → complete document
    /// sentencesPerChunk = 2 → put 2 sentences into each chunk
    /// Returns List<string> → multiple text chunks
    /// </summary>

    public class TextChunker
    {
        public List<string> SplitText(string text, int sentencesPerChunk = 2)
        {
            var chunks = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
                return chunks;

            var sentences = Regex.Split(
                text,
                @"(?<=[.!?])\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

             /// <summary>
            /// This is doing three things.
            /// Skip(i) Start from the current sentence.
            /// Take(2) Take 2 sentences.string.Join(" ", ...) Combine them into one string.
            /// </summary>


            for (int i = 0; i < sentences.Count; i += sentencesPerChunk)
            {
                var chunk = string.Join(
                    " ",
                    sentences
                        .Skip(i)
                        .Take(sentencesPerChunk));

                chunks.Add(chunk);
            }

            return chunks;
        }
    }
}