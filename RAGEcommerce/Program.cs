#pragma warning disable SKEXP0070

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Qdrant.Client;
using RAGEcommerce.Infrastructure;
using RAGEcommerce.Models;
using RAGEcommerce.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(_ => new QdrantClient("localhost"));

builder.Services.AddSingleton<Kernel>(_ =>
{
    const string modelPath =
        @"C:\ai-models\phi-3\models\Phi-3-mini-4k-instruct-onnx\cpu_and_mobile\cpu-int4-awq-block-128";

    var kernel = Kernel.CreateBuilder()
        .AddOnnxRuntimeGenAIChatCompletion("phi-3", modelPath)
        .AddBertOnnxEmbeddingGenerator(
            onnxModelPath: @"C:\ai-models\bge-micro-v2\onnx\model.onnx",
            vocabPath: @"C:\ai-models\bge-micro-v2\vocab.txt")
        .Build();

    return kernel;
});


builder.Services.AddSingleton<QdrantIndexer>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<ProductCatalogService>();
builder.Services.AddSingleton<IEmbeddingService, EmbeddingService>();


builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    return kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RAGEcommerce API v1");
    c.RoutePrefix = String.Empty;
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet(
    "/health", async (QdrantClient qdrantClient) =>
    {
        var health = await qdrantClient.HealthAsync();
        return Results.Ok($"Status: {health}");
    });

app.MapPost(
        "/api/rag/init", async (ProductCatalogService productService, QdrantIndexer indexer) =>
        {
            try
            {
                await indexer.IndexProductsAsync();
                return Results.Ok(
                    $"Successfully initialized collection with {productService.GetAllProducts().Count} products");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error initializing collection: {ex.Message}");
            }
        })
    .WithName("Initialize")
    .WithOpenApi();

app.MapGet(
        "/api/products", (ProductCatalogService productService) => productService.GetAllProducts())
    .WithName("GetProducts")
    .WithOpenApi();

app.MapPost(
        "/api/rag/query", async (
            UserQuery userQuery, QdrantClient qdrantClient,
            IEmbeddingService embeddingService, ChatService chatService) =>
        {
            var embedding = await embeddingService.GenerateEmbedding(userQuery.Question);
            var results = await qdrantClient.SearchAsync("products", embedding, limit: 3);

            var context = results.Any()
                ? String.Join("\n", results.Select(r => r.Payload["name"].ToString()))
                : "No matching products found.";

            var prompt =
                $"Using the following context:\n{context}\nAnswer the question:\n{userQuery.Question}";

            var answer = await chatService.GetChatResponseAsync(prompt);
            return Results.Ok(answer);
        })
    .WithName("QueryProduct")
    .WithOpenApi();

app.Run();
