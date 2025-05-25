namespace RAGEcommerce.Services;

public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> GenerateEmbedding(string text);
}
