using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Pgvector;

namespace ModelProcessor.Models;

// C# 13 primary constructor with field keyword
public record DocumentMetadata(
    string Title,
    string Source,
    DateTime? DatePublished,
    DateTime DateCaptured,
    string Domain,
    string? Author,
    string Category)
{
    public List<string> Technologies { get; init; } = [];
    public List<string> ProgrammingLanguages { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<string> KeyConcepts { get; init; } = [];
    public bool CodeExamples { get; init; }
    public string DifficultyLevel { get; init; } = "intermediate";
    public string Summary { get; init; } = string.Empty;
    public string SourceFile { get; init; } = string.Empty;
    public string Year { get; init; } = DateTime.Now.Year.ToString();
}

// C# 13 - Using field keyword and primary constructor
public class ProcessedChunk(string id)
{
    // Field keyword allows access to the backing field
    public string Id { get; } = field = id ?? Guid.NewGuid().ToString();
    
    public required string Type { get; init; } // "summary", "content", "code", "full", "metadata"
    public required string Content { get; init; }
    public string? Language { get; init; } // for code chunks
    public required DocumentMetadata Metadata { get; init; }
    public int Priority { get; init; } // 1=highest (summaries), 2=medium (content), 3=low (full article)
    public Vector? Embedding { get; set; } // pgvector type for storing embeddings
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
    
    // C# 13 collection expression
    public ImmutableArray<string> GetSearchableTokens() => 
        [.. Content.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .Take(100)];
}

// Entity for EF Core with pgvector
public class DocumentEmbedding
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string DocumentId { get; set; }
    public required string ChunkType { get; set; }
    public required string Content { get; set; }
    public required Vector Embedding { get; set; } // pgvector type
    public required string MetadataJson { get; set; } // Store metadata as JSON
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Computed property for similarity search
    [JsonIgnore]
    public float? SimilarityScore { get; set; }
}

// C# 13 - params collections for flexible parameter passing
public class SearchRequest(params string[] searchTerms)
{
    public ImmutableArray<string> Terms { get; } = [.. searchTerms];
    public int MaxResults { get; init; } = 10;
    public float MinSimilarity { get; init; } = 0.7f;
    public List<string>? Categories { get; init; }
    public List<string>? Tags { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}

// Result model with C# 13 features
public record SearchResult
{
    public required DocumentEmbedding Document { get; init; }
    public required float Score { get; init; }
    public required string HighlightedContent { get; init; }
    
    // C# 13 - collection expression with spread
    public ImmutableArray<string> GetRelevantTags(IEnumerable<string> userTags) =>
        [.. Document.MetadataJson
            .Deserialize<DocumentMetadata>()?.Tags
            .Intersect(userTags) ?? []];
}

// Extension methods using C# 13 features
public static class DocumentExtensions
{
    // Pattern matching with property patterns
    public static string GetDocumentCategory(this ProcessedChunk chunk) => chunk switch
    {
        { Type: "code", Language: var lang } when !string.IsNullOrEmpty(lang) 
            => $"code-{lang}",
        { Type: "summary", Priority: 1 } 
            => "high-priority-summary",
        { Type: "content", Metadata.Category: var cat } 
            => $"content-{cat}",
        _ => "general"
    };
    
    // Using SearchValues for performance (new in .NET 9)
    private static readonly System.Buffers.SearchValues<char> _punctuation = 
        System.Buffers.SearchValues.Create(".,;:!?");
    
    public static ReadOnlySpan<char> CleanContent(this string content)
    {
        var span = content.AsSpan();
        var index = span.IndexOfAny(_punctuation);
        return index >= 0 ? span[..index] : span;
    }
}

// Helper for JSON serialization
internal static class JsonHelper
{
    private static readonly System.Text.Json.JsonSerializerOptions _options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public static T? Deserialize<T>(this string json) =>
        System.Text.Json.JsonSerializer.Deserialize<T>(json, _options);
    
    public static string Serialize<T>(this T obj) =>
        System.Text.Json.JsonSerializer.Serialize(obj, _options);
}