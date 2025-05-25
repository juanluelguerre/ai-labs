using Microsoft.Extensions.AI;

namespace RAGEcommerce.Services;

public class EmbeddingService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    : IEmbeddingService
{
    // Fist simple demo just using a dummy vector.
    //public float[] GenerateEmbedding(string text)
    //{
    //    // Dummy vector, replace with real embeddings
    //    return new float[1536];
    //}

    public async Task<ReadOnlyMemory<float>> GenerateEmbedding(string text)
    {
        return await embeddingGenerator.GenerateVectorAsync(text);
    }
}
