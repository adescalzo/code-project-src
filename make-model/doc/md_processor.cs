using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarkdownProcessor
{
    public class DocumentMetadata
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<string> Tags { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public string ExtraInfo { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
    }

    public class ProcessedChunk
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty; // "summary", "content", "code", "full"
        public string Content { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty; // for code chunks
        public DocumentMetadata Metadata { get; set; } = new();
        public int Priority { get; set; } // 1=highest (summaries), 2=medium (content), 3=low (full article)
    }

    public class MarkdownProcessor
    {
        private readonly IDeserializer _yamlDeserializer;
        private readonly Regex _yamlFrontmatterRegex;
        private readonly Regex _codeBlockRegex;
        private readonly Regex _headerRegex;
        private readonly Regex _imageRegex;

        public MarkdownProcessor()
        {
            _yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _yamlFrontmatterRegex = new Regex(@"^---\s*\n(.*?)\n---\s*\n", 
                RegexOptions.Singleline | RegexOptions.Compiled);
            
            _codeBlockRegex = new Regex(@"```(\w+)?\s*\n(.*?)\n```", 
                RegexOptions.Singleline | RegexOptions.Compiled);
            
            _headerRegex = new Regex(@"^(#{1,6})\s+(.+)$", 
                RegexOptions.Multiline | RegexOptions.Compiled);
            
            _imageRegex = new Regex(@"!\[.*?\]\((.*?)\)", 
                RegexOptions.Compiled);
        }

        public async Task<List<ProcessedChunk>> ProcessDirectoryAsync(string rootPath)
        {
            var allChunks = new List<ProcessedChunk>();
            
            var mdFiles = Directory.GetFiles(rootPath, "*.md", SearchOption.AllDirectories);
            
            Console.WriteLine($"Found {mdFiles.Length} markdown files to process...");

            foreach (var filePath in mdFiles)
            {
                try
                {
                    var chunks = await ProcessFileAsync(filePath);
                    allChunks.AddRange(chunks);
                    Console.WriteLine($"Processed: {Path.GetFileName(filePath)} -> {chunks.Count} chunks");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                }
            }

            Console.WriteLine($"Total chunks created: {allChunks.Count}");
            return allChunks;
        }

        public async Task<List<ProcessedChunk>> ProcessFileAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var chunks = new List<ProcessedChunk>();
            
            // Extract year from path
            var year = ExtractYearFromPath(filePath);
            
            // Parse frontmatter
            var metadata = ParseFrontmatter(content, filePath, year);
            
            // Remove frontmatter from content
            var bodyContent = RemoveFrontmatter(content);
            
            // 1. Create summary chunk (highest priority)
            if (!string.IsNullOrEmpty(metadata.Summary))
            {
                chunks.Add(new ProcessedChunk
                {
                    Type = "summary",
                    Content = metadata.Summary,
                    Metadata = metadata,
                    Priority = 1
                });
            }

            // 2. Create extra info chunk if exists
            if (!string.IsNullOrEmpty(metadata.ExtraInfo))
            {
                chunks.Add(new ProcessedChunk
                {
                    Type = "extra_info",
                    Content = metadata.ExtraInfo,
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
            chunks.Add(new ProcessedChunk
            {
                Type = "full_article",
                Content = CleanContentForEmbedding(bodyContent),
                Metadata = metadata,
                Priority = 3
            });

            return chunks;
        }

        private DocumentMetadata ParseFrontmatter(string content, string filePath, string year)
        {
            var match = _yamlFrontmatterRegex.Match(content);
            var metadata = new DocumentMetadata
            {
                SourceFile = filePath,
                Year = year
            };

            if (match.Success)
            {
                try
                {
                    var yamlContent = match.Groups[1].Value;
                    var yamlData = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yamlContent);
                    
                    if (yamlData.ContainsKey("title"))
                        metadata.Title = yamlData["title"].ToString() ?? string.Empty;
                    
                    if (yamlData.ContainsKey("date") && DateTime.TryParse(yamlData["date"].ToString(), out var date))
                        metadata.Date = date;
                    
                    if (yamlData.ContainsKey("tags") && yamlData["tags"] is List<object> tagList)
                        metadata.Tags = tagList.Select(t => t.ToString() ?? string.Empty).ToList();
                    
                    if (yamlData.ContainsKey("summary"))
                        metadata.Summary = yamlData["summary"].ToString() ?? string.Empty;
                    
                    if (yamlData.ContainsKey("extra_info"))
                        metadata.ExtraInfo = yamlData["extra_info"].ToString() ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not parse YAML frontmatter in {filePath}: {ex.Message}");
                }
            }

            return metadata;
        }

        private string RemoveFrontmatter(string content)
        {
            var match = _yamlFrontmatterRegex.Match(content);
            return match.Success ? content.Substring(match.Length) : content;
        }

        private string ExtractYearFromPath(string filePath)
        {
            var pathParts = filePath.Split(Path.DirectorySeparatorChar);
            var yearPattern = new Regex(@"^\d{4}$");
            return pathParts.FirstOrDefault(part => yearPattern.IsMatch(part)) ?? "unknown";
        }

        private List<ProcessedChunk> ExtractCodeBlocks(string content, DocumentMetadata metadata)
        {
            var codeChunks = new List<ProcessedChunk>();
            var matches = _codeBlockRegex.Matches(content);

            foreach (Match match in matches)
            {
                var language = match.Groups[1].Value.ToLowerInvariant();
                var codeContent = match.Groups[2].Value.Trim();
                
                if (!string.IsNullOrEmpty(codeContent))
                {
                    codeChunks.Add(new ProcessedChunk
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
            var contentWithoutCode = _codeBlockRegex.Replace(content, "[CODE_BLOCK_REMOVED]");
            
            // Split by headers
            var sections = SplitByHeaders(contentWithoutCode);
            
            foreach (var section in sections)
            {
                var cleanedSection = CleanContentForEmbedding(section);
                
                if (!string.IsNullOrWhiteSpace(cleanedSection) && cleanedSection.Length > 100) // Only meaningful sections
                {
                    contentChunks.Add(new ProcessedChunk
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
            var matches = _headerRegex.Matches(content);
            
            if (matches.Count == 0)
            {
                // No headers found, return the whole content as one section
                sections.Add(content);
                return sections;
            }

            var lastIndex = 0;
            
            foreach (Match match in matches)
            {
                if (lastIndex < match.Index)
                {
                    var sectionContent = content.Substring(lastIndex, match.Index - lastIndex).Trim();
                    if (!string.IsNullOrEmpty(sectionContent))
                        sections.Add(sectionContent);
                }
                lastIndex = match.Index;
            }
            
            // Add the last section
            if (lastIndex < content.Length)
            {
                var lastSection = content.Substring(lastIndex).Trim();
                if (!string.IsNullOrEmpty(lastSection))
                    sections.Add(lastSection);
            }

            return sections;
        }

        private string CleanContentForEmbedding(string content)
        {
            // Remove excessive whitespace
            content = Regex.Replace(content, @"\s+", " ");
            
            // Remove base64 images (they're too long and not useful for search)
            content = Regex.Replace(content, @"data:image/[^;]+;base64,[A-Za-z0-9+/=]+", "[IMAGE_REMOVED]");
            
            // Keep image links but clean them
            content = _imageRegex.Replace(content, "[IMAGE: $1]");
            
            // Remove multiple consecutive newlines
            content = Regex.Replace(content, @"\n{3,}", "\n\n");
            
            return content.Trim();
        }

        public async Task SaveChunksToJsonAsync(List<ProcessedChunk> chunks, string outputPath)
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var json = JsonSerializer.Serialize(chunks, options);
            await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8);
            
            Console.WriteLine($"Saved {chunks.Count} chunks to {outputPath}");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Markdown Document Processor");
            Console.WriteLine("===========================");

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: MarkdownProcessor.exe <path_to_markdown_directory> [output_file.json]");
                return;
            }

            var inputPath = args[0];
            var outputPath = args.Length > 1 ? args[1] : "processed_chunks.json";

            if (!Directory.Exists(inputPath))
            {
                Console.WriteLine($"Directory not found: {inputPath}");
                return;
            }

            try
            {
                var processor = new MarkdownProcessor();
                var chunks = await processor.ProcessDirectoryAsync(inputPath);
                await processor.SaveChunksToJsonAsync(chunks, outputPath);

                // Print statistics
                Console.WriteLine("\nProcessing Statistics:");
                Console.WriteLine("=====================");
                Console.WriteLine($"Total chunks: {chunks.Count}");
                Console.WriteLine($"Summary chunks: {chunks.Count(c => c.Type == "summary")}");
                Console.WriteLine($"Content chunks: {chunks.Count(c => c.Type == "content")}");
                Console.WriteLine($"Code chunks: {chunks.Count(c => c.Type == "code")}");
                Console.WriteLine($"Full article chunks: {chunks.Count(c => c.Type == "full_article")}");
                
                var languages = chunks.Where(c => c.Type == "code" && !string.IsNullOrEmpty(c.Language))
                                    .GroupBy(c => c.Language)
                                    .OrderByDescending(g => g.Count());
                
                Console.WriteLine("\nCode languages found:");
                foreach (var lang in languages)
                {
                    Console.WriteLine($"  {lang.Key}: {lang.Count()} blocks");
                }

                Console.WriteLine($"\nOutput saved to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}