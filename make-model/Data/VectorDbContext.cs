using Microsoft.EntityFrameworkCore;
using ModelProcessor.Models;
using Npgsql;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace ModelProcessor.Data;

public class VectorDbContext : DbContext
{
    public VectorDbContext(DbContextOptions<VectorDbContext> options) : base(options)
    {
    }
    
    public DbSet<DocumentEmbedding> DocumentEmbeddings => Set<DocumentEmbedding>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Install pgvector extension
        modelBuilder.HasPostgresExtension("vector");
        
        // Configure DocumentEmbedding entity
        modelBuilder.Entity<DocumentEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Index for document lookups
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.ChunkType);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.CreatedAt);
            
            // Configure vector column with dimensions (1536 for OpenAI embeddings)
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(1536)");
            
            // Create HNSW index for fast similarity search
            entity.HasIndex(e => e.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");
            
            // Store metadata as JSONB for flexible querying
            entity.Property(e => e.MetadataJson)
                .HasColumnType("jsonb");
            
            // GIN index on JSONB for fast JSON queries
            entity.HasIndex(e => e.MetadataJson)
                .HasMethod("gin");
        });
    }
    
    // Custom method for similarity search using pgvector
    public async Task<List<SearchResult>> SimilaritySearchAsync(
        Vector queryEmbedding, 
        int limit = 10,
        float minSimilarity = 0.7f,
        CancellationToken cancellationToken = default)
    {
        // Using raw SQL for pgvector similarity search
        var sql = @"
            SELECT 
                d.*,
                1 - (d.""Embedding"" <=> @queryEmbedding) as similarity
            FROM ""DocumentEmbeddings"" d
            WHERE 1 - (d.""Embedding"" <=> @queryEmbedding) >= @minSimilarity
            ORDER BY d.""Embedding"" <=> @queryEmbedding
            LIMIT @limit";
        
        var results = await DocumentEmbeddings
            .FromSqlRaw(sql, 
                new NpgsqlParameter("@queryEmbedding", queryEmbedding),
                new NpgsqlParameter("@minSimilarity", minSimilarity),
                new NpgsqlParameter("@limit", limit))
            .Select(d => new SearchResult
            {
                Document = d,
                Score = d.SimilarityScore ?? 0,
                HighlightedContent = d.Content.Length > 200 
                    ? d.Content.Substring(0, 200) + "..." 
                    : d.Content
            })
            .ToListAsync(cancellationToken);
        
        return results;
    }
    
    // Hybrid search combining vector similarity and metadata filters
    public async Task<List<SearchResult>> HybridSearchAsync(
        Vector queryEmbedding,
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = DocumentEmbeddings.AsQueryable();
        
        // Apply metadata filters using JSONB queries
        if (request.Categories?.Any() == true)
        {
            query = query.Where(d => EF.Functions.JsonContains(
                d.MetadataJson, 
                $"{{\"category\": \"{request.Categories.First()}\"}}"));
        }
        
        if (request.Tags?.Any() == true)
        {
            var tagJson = System.Text.Json.JsonSerializer.Serialize(
                new { tags = request.Tags });
            query = query.Where(d => EF.Functions.JsonContains(d.MetadataJson, tagJson));
        }
        
        // Combine with vector similarity
        var sql = @"
            SELECT 
                d.*,
                1 - (d.""Embedding"" <=> @queryEmbedding) as similarity
            FROM ""DocumentEmbeddings"" d
            WHERE d.""Id"" = ANY(@ids)
            ORDER BY d.""Embedding"" <=> @queryEmbedding
            LIMIT @limit";
        
        var filteredIds = await query.Select(d => d.Id).ToArrayAsync(cancellationToken);
        
        if (!filteredIds.Any())
            return [];
        
        var results = await DocumentEmbeddings
            .FromSqlRaw(sql,
                new NpgsqlParameter("@queryEmbedding", queryEmbedding),
                new NpgsqlParameter("@ids", filteredIds),
                new NpgsqlParameter("@limit", request.MaxResults))
            .Select(d => new SearchResult
            {
                Document = d,
                Score = d.SimilarityScore ?? 0,
                HighlightedContent = HighlightContent(d.Content, request.Terms)
            })
            .ToListAsync(cancellationToken);
        
        return results;
    }
    
    // Helper method to highlight search terms in content
    private static string HighlightContent(string content, ImmutableArray<string> terms)
    {
        if (terms.IsEmpty || string.IsNullOrEmpty(content))
            return content.Length > 200 ? content[..200] + "..." : content;
        
        var highlighted = content;
        foreach (var term in terms.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            highlighted = highlighted.Replace(
                term, 
                $"**{term}**", 
                StringComparison.OrdinalIgnoreCase);
        }
        
        return highlighted.Length > 200 ? highlighted[..200] + "..." : highlighted;
    }
}

// Extension for dependency injection
public static class VectorDbExtensions
{
    public static IServiceCollection AddVectorDatabase(
        this IServiceCollection services, 
        string connectionString)
    {
        // Configure Npgsql with pgvector support
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();
        
        services.AddDbContext<VectorDbContext>(options =>
            options.UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
                npgsqlOptions.EnableRetryOnFailure(3);
            }));
        
        return services;
    }
    
    public static async Task InitializeDatabaseAsync(this VectorDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Ensure pgvector extension is installed
        await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector");
    }
}