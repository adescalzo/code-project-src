using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using ModelProcessor.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ModelProcessor.Services;

public partial class MarkdownProcessor
{
    private readonly IDeserializer _yamlDeserializer;
    
    // C# 13 - Source-generated regex for better performance
    [GeneratedRegex(@"^---\s*\n(.*?)\n---\s*\n", RegexOptions.Singleline)]
    private static partial Regex YamlFrontmatterRegex();
    
    [GeneratedRegex(@"```(\w+)?\s*\n(.*?)\n```", RegexOptions.Singleline)]
    private static partial Regex CodeBlockRegex();
    
    [GeneratedRegex(@"^(#{1,6})\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex HeaderRegex();
    
    [GeneratedRegex(@"!\[.*?\]\((.*?)\)")]
    private static partial Regex ImageRegex();
    
    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex YearPattern();
    
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
    
    [GeneratedRegex(@"data:image/[^;]+;base64,[A-Za-z0-9+/=]+")]
    private static partial Regex Base64ImageRegex();
    
    public MarkdownProcessor()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }
    
    public async Task<ImmutableArray<ProcessedChunk>> ProcessDirectoryAsync(
        string rootPath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var mdFiles = Directory.GetFiles(rootPath, "*.md", SearchOption.AllDirectories);
        progress?.Report($"Found {mdFiles.Length} markdown files to process...");
        
        // C# 13 - collection expression with async enumerable
        var chunks = new List<ProcessedChunk>();
        
        await Parallel.ForEachAsync(mdFiles, cancellationToken, async (filePath, ct) =>
        {
            try
            {
                var fileChunks = await ProcessFileAsync(filePath, ct);
                lock (chunks)
                {
                    chunks.AddRange(fileChunks);
                }
                progress?.Report($"Processed: {Path.GetFileName(filePath)} -> {fileChunks.Length} chunks");
            }
            catch (Exception ex)
            {
                progress?.Report($"Error processing {filePath}: {ex.Message}");
            }
        });
        
        progress?.Report($"Total chunks created: {chunks.Count}");
        return [.. chunks];
    }
    
    public async Task<ImmutableArray<ProcessedChunk>> ProcessFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
        var chunks = new List<ProcessedChunk>();
        
        // Extract year from path
        var year = ExtractYearFromPath(filePath);
        
        // Parse frontmatter
        var metadata = ParseFrontmatter(content, filePath, year);
        
        // Remove frontmatter from content
        var bodyContent = RemoveFrontmatter(content);
        
        // 1. Create summary chunk (highest priority)
        if (!string.IsNullOrWhiteSpace(metadata.Summary))
        {
            chunks.Add(new ProcessedChunk(Guid.NewGuid().ToString())
            {
                Type = "summary",
                Content = metadata.Summary.Trim(),
                Metadata = metadata,
                Priority = 1
            });
        }
        
        // 2. Create metadata chunk for searchable fields
        var metadataContent = CreateMetadataSearchContent(metadata);
        if (!string.IsNullOrWhiteSpace(metadataContent))
        {
            chunks.Add(new ProcessedChunk(Guid.NewGuid().ToString())
            {
                Type = "metadata",
                Content = metadataContent,
                Metadata = metadata,
                Priority = 1
            });
        }
        
        // 3. Extract and create code chunks
        var codeChunks = ExtractCodeBlocks(bodyContent, metadata);
        chunks.AddRange(codeChunks);
        
        // 4. Create content chunks by sections
        var contentChunks = CreateContentChunks(bodyContent, metadata);
        chunks.AddRange(contentChunks);
        
        // 5. Create full article chunk (lowest priority)
        chunks.Add(new ProcessedChunk(Guid.NewGuid().ToString())
        {
            Type = "full_article",
            Content = CleanContentForEmbedding(bodyContent),
            Metadata = metadata,
            Priority = 3
        });
        
        return [.. chunks];
    }
    
    private DocumentMetadata ParseFrontmatter(string content, string filePath, string year)
    {
        var match = YamlFrontmatterRegex().Match(content);
        
        if (!match.Success)
        {
            return new DocumentMetadata(
                Title: Path.GetFileNameWithoutExtension(filePath),
                Source: string.Empty,
                DatePublished: null,
                DateCaptured: DateTime.UtcNow,
                Domain: string.Empty,
                Author: null,
                Category: "unknown")
            {
                SourceFile = filePath,
                Year = year
            };
        }
        
        try
        {
            var yamlContent = match.Groups[1].Value;
            var yamlData = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yamlContent);
            
            return new DocumentMetadata(
                Title: GetYamlValue(yamlData, "title"),
                Source: GetYamlValue(yamlData, "source"),
                DatePublished: ParseDate(GetYamlValue(yamlData, "date_published")),
                DateCaptured: ParseDate(GetYamlValue(yamlData, "date_captured")) ?? DateTime.UtcNow,
                Domain: GetYamlValue(yamlData, "domain"),
                Author: GetYamlValue(yamlData, "author"),
                Category: GetYamlValue(yamlData, "category", "general"))
            {
                Technologies = ParseList(yamlData, "technologies"),
                ProgrammingLanguages = ParseList(yamlData, "programming_languages"),
                Tags = ParseList(yamlData, "tags"),
                KeyConcepts = ParseList(yamlData, "key_concepts"),
                CodeExamples = bool.TryParse(GetYamlValue(yamlData, "code_examples"), out var hasCode) && hasCode,
                DifficultyLevel = GetYamlValue(yamlData, "difficulty_level", "intermediate"),
                Summary = GetYamlValue(yamlData, "summary").Trim('[', ']', ' '),
                SourceFile = filePath,
                Year = year
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not parse YAML frontmatter in {filePath}: {ex.Message}");
            return new DocumentMetadata(
                Title: Path.GetFileNameWithoutExtension(filePath),
                Source: string.Empty,
                DatePublished: null,
                DateCaptured: DateTime.UtcNow,
                Domain: string.Empty,
                Author: null,
                Category: "unknown")
            {
                SourceFile = filePath,
                Year = year
            };
        }
    }
    
    private static string GetYamlValue(Dictionary<string, object> data, string key, string defaultValue = "")
    {
        return data.TryGetValue(key, out var value) ? value?.ToString() ?? defaultValue : defaultValue;
    }
    
    private static DateTime? ParseDate(string dateStr)
    {
        return DateTime.TryParse(dateStr, out var date) ? date : null;
    }
    
    private static List<string> ParseList(Dictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out var value) && value is List<object> list)
        {
            return list.Select(item => item?.ToString() ?? string.Empty)
                      .Where(s => !string.IsNullOrEmpty(s))
                      .ToList();
        }
        return [];
    }
    
    private string CreateMetadataSearchContent(DocumentMetadata metadata)
    {
        // C# 13 - String interpolation with collection expressions
        var parts = new List<string>
        {
            metadata.Title,
            metadata.Category,
            metadata.DifficultyLevel
        };
        
        if (metadata.Technologies.Any())
            parts.Add($"Technologies: {string.Join(", ", metadata.Technologies)}");
        
        if (metadata.ProgrammingLanguages.Any())
            parts.Add($"Languages: {string.Join(", ", metadata.ProgrammingLanguages)}");
        
        if (metadata.Tags.Any())
            parts.Add($"Tags: {string.Join(", ", metadata.Tags)}");
        
        if (metadata.KeyConcepts.Any())
            parts.Add($"Concepts: {string.Join(", ", metadata.KeyConcepts)}");
        
        return string.Join(" | ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
    
    private string RemoveFrontmatter(string content)
    {
        var match = YamlFrontmatterRegex().Match(content);
        return match.Success ? content[match.Length..] : content;
    }
    
    private string ExtractYearFromPath(string filePath)
    {
        var pathParts = filePath.Split(Path.DirectorySeparatorChar);
        return pathParts.FirstOrDefault(part => YearPattern().IsMatch(part)) 
               ?? DateTime.Now.Year.ToString();
    }
    
    private List<ProcessedChunk> ExtractCodeBlocks(string content, DocumentMetadata metadata)
    {
        var codeChunks = new List<ProcessedChunk>();
        var matches = CodeBlockRegex().Matches(content);
        
        foreach (Match match in matches)
        {
            var language = match.Groups[1].Value.ToLowerInvariant();
            var codeContent = match.Groups[2].Value.Trim();
            
            if (!string.IsNullOrWhiteSpace(codeContent))
            {
                codeChunks.Add(new ProcessedChunk(Guid.NewGuid().ToString())
                {
                    Type = "code",
                    Content = codeContent,
                    Language = language,
                    Metadata = metadata,
                    Priority = 2
                });
            }
        }
        
        return codeChunks;
    }
    
    private List<ProcessedChunk> CreateContentChunks(string content, DocumentMetadata metadata)
    {
        var contentChunks = new List<ProcessedChunk>();
        
        // Remove code blocks from content for processing
        var contentWithoutCode = CodeBlockRegex().Replace(content, "[CODE_BLOCK]");
        
        // Split by headers
        var sections = SplitByHeaders(contentWithoutCode);
        
        foreach (var section in sections)
        {
            var cleanedSection = CleanContentForEmbedding(section);
            
            // Only meaningful sections (more than 100 chars)
            if (!string.IsNullOrWhiteSpace(cleanedSection) && cleanedSection.Length > 100)
            {
                contentChunks.Add(new ProcessedChunk(Guid.NewGuid().ToString())
                {
                    Type = "content",
                    Content = cleanedSection,
                    Metadata = metadata,
                    Priority = 2
                });
            }
        }
        
        return contentChunks;
    }
    
    private List<string> SplitByHeaders(string content)
    {
        var sections = new List<string>();
        var matches = HeaderRegex().Matches(content);
        
        if (matches.Count == 0)
        {
            sections.Add(content);
            return sections;
        }
        
        var lastIndex = 0;
        
        foreach (Match match in matches)
        {
            if (lastIndex < match.Index)
            {
                var sectionContent = content[lastIndex..match.Index].Trim();
                if (!string.IsNullOrEmpty(sectionContent))
                    sections.Add(sectionContent);
            }
            lastIndex = match.Index;
        }
        
        // Add the last section
        if (lastIndex < content.Length)
        {
            var lastSection = content[lastIndex..].Trim();
            if (!string.IsNullOrEmpty(lastSection))
                sections.Add(lastSection);
        }
        
        return sections;
    }
    
    private string CleanContentForEmbedding(string content)
    {
        // Remove excessive whitespace
        content = WhitespaceRegex().Replace(content, " ");
        
        // Remove base64 images
        content = Base64ImageRegex().Replace(content, "[IMAGE_REMOVED]");
        
        // Keep image links but clean them
        content = ImageRegex().Replace(content, "[IMAGE: $1]");
        
        // Remove multiple consecutive newlines
        content = Regex.Replace(content, @"\n{3,}", "\n\n");
        
        return content.Trim();
    }
}