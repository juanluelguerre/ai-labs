﻿using Qdrant.Client;
using Qdrant.Client.Grpc;
using RAGEcommerce.Services;

namespace RAGEcommerce.Infrastructure;

public class QdrantIndexer(
    QdrantClient client,
    ProductCatalogService productService,
    IEmbeddingService embeddingService)
{
    public async Task IndexProductsAsync()
    {
        try
        {
            var collections = await client.ListCollectionsAsync();
            if (!collections.Contains("products"))
            {
                //await client.CreateCollectionAsync(
                //    "products",
                //    new VectorParams { Size = 1536, Distance = Distance.Cosine });
                await client.CreateCollectionAsync(
                    "products",
                    new VectorParams { Size = 384, Distance = Distance.Cosine });
            }

            var embeddings = await Task.WhenAll(
                productService.GetAllProducts().Select(async p =>
                    new PointStruct
                    {
                        Id = new PointId { Num = (ulong)p.Id },
                        Vectors = new Vectors
                        {
                            Vector = new Vector
                            {
                                Data =
                                {
                                    (await embeddingService.GenerateEmbedding(p.Description))
                                    .ToArray()
                                }
                            }
                        },
                        Payload =
                        {
                            { "id", new Value { IntegerValue = p.Id } },
                            { "name", new Value { StringValue = p.Name } },
                            { "description", new Value { StringValue = p.Description } }
                        }
                    }));

            // Use a batch approach to avoid large payload issues
            const int batchSize = 100;
            for (var i = 0; i < embeddings.Length; i += batchSize)
            {
                var batch = embeddings.Skip(i).Take(batchSize).ToList();
                await client.UpsertAsync("products", batch);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error indexing products: {ex.Message}", ex);
        }
    }
}
