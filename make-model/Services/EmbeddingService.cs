using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using ModelProcessor.Data;
using ModelProcessor.Models;
using Pgvector;

namespace ModelProcessor.Services;

public interface IEmbeddingService
{
    Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<int> ProcessAndStoreChunksAsync(IEnumerable<ProcessedChunk> chunks, CancellationToken cancellationToken = default);
    Task<ImmutableArray<SearchResult>> SearchAsync(string query, SearchRequest? request = null, CancellationToken cancellationToken = default);
}

public class EmbeddingService(
    ITextEmbeddingGenerationService embeddingService,
    VectorDbContext dbContext,
    ILogger<EmbeddingService> logger) : IEmbeddingService
{
    private const int EmbeddingDimensions = 1536; // OpenAI embeddings dimension
    private const int BatchSize = 100; // Process chunks in batches
    
    public async Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate embedding using Semantic Kernel
            var embedding = await embeddingService.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
            
            // Convert to pgvector Vector type
            var vectorArray = embedding.ToArray();
            return new Vector(vectorArray);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate embedding for text");
            throw;
        }
    }
    
    public async Task<int> ProcessAndStoreChunksAsync(
        IEnumerable<ProcessedChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        var chunkList = chunks.ToList();
        var processedCount = 0;
        
        // Process in batches to avoid overwhelming the API
        for (int i = 0; i < chunkList.Count; i += BatchSize)
        {
            var batch = chunkList.Skip(i).Take(BatchSize).ToList();
            
            logger.LogInformation($"Processing batch {i / BatchSize + 1} ({batch.Count} chunks)");
            
            // Generate embeddings for batch
            var tasks = batch.Select(async chunk =>
            {
                try
                {
                    var embedding = await GenerateEmbeddingAsync(chunk.Content, cancellationToken);
                    
                    var documentEmbedding = new DocumentEmbedding
                    {
                        DocumentId = chunk.Id,
                        ChunkType = chunk.Type,
                        Content = chunk.Content,
                        Embedding = embedding,
                        MetadataJson = chunk.Metadata.Serialize(),
                        Priority = chunk.Priority,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    return documentEmbedding;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Failed to process chunk {chunk.Id}");
                    return null;
                }
            });
            
            var embeddings = await Task.WhenAll(tasks);
            var validEmbeddings = embeddings.Where(e => e != null).Cast<DocumentEmbedding>().ToList();
            
            if (validEmbeddings.Any())
            {
                // Store in database
                await dbContext.DocumentEmbeddings.AddRangeAsync(validEmbeddings, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                processedCount += validEmbeddings.Count;
                
                logger.LogInformation($"Stored {validEmbeddings.Count} embeddings in database");
            }
            
            // Rate limiting delay
            if (i + BatchSize < chunkList.Count)
            {
                await Task.Delay(1000, cancellationToken); // 1 second delay between batches
            }
        }
        
        logger.LogInformation($"Successfully processed {processedCount} out of {chunkList.Count} chunks");
        return processedCount;
    }
    
    public async Task<ImmutableArray<SearchResult>> SearchAsync(
        string query,
        SearchRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate embedding for query
            var queryEmbedding = await GenerateEmbeddingAsync(query, cancellationToken);
            
            // Perform search based on request type
            List<SearchResult> results;
            
            if (request != null && (request.Categories?.Any() == true || request.Tags?.Any() == true))
            {
                // Hybrid search with filters
                results = await dbContext.HybridSearchAsync(queryEmbedding, request, cancellationToken);
            }
            else
            {
                // Simple similarity search
                var limit = request?.MaxResults ?? 10;
                var minSimilarity = request?.MinSimilarity ?? 0.7f;
                results = await dbContext.SimilaritySearchAsync(queryEmbedding, limit, minSimilarity, cancellationToken);
            }
            
            logger.LogInformation($"Found {results.Count} search results for query: {query}");
            
            return [.. results];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Search failed for query: {query}");
            throw;
        }
    }
}

// Extension methods for dependency injection
public static class EmbeddingServiceExtensions
{
    public static IServiceCollection AddEmbeddingServices(
        this IServiceCollection services,
        string openAiApiKey,
        string? model = null)
    {
        // Configure Semantic Kernel
        services.AddSingleton<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();
            
            // Add OpenAI embedding service
            builder.AddOpenAITextEmbeddingGeneration(
                modelId: model ?? "text-embedding-3-small",
                apiKey: openAiApiKey);
            
            // Add logging
            builder.Services.AddLogging(logging => logging.AddConsole());
            
            return builder.Build();
        });
        
        // Register embedding service from kernel
        services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        });
        
        // Register our embedding service
        services.AddScoped<IEmbeddingService, EmbeddingService>();
        
        return services;
    }
}