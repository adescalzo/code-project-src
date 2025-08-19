using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelProcessor.Models;

namespace ModelProcessor.Services;

public interface IRagService
{
    Task<string> AskQuestionAsync(string question, CancellationToken cancellationToken = default);
    Task<RagResponse> GetAugmentedResponseAsync(string question, SearchRequest? searchRequest = null, CancellationToken cancellationToken = default);
}

public record RagResponse
{
    public required string Answer { get; init; }
    public required ImmutableArray<SearchResult> Sources { get; init; }
    public required TimeSpan ResponseTime { get; init; }
    public string? Model { get; init; }
}

public class RagService(
    IEmbeddingService embeddingService,
    IChatCompletionService chatService,
    ILogger<RagService> logger) : IRagService
{
    private const int MaxContextLength = 8000; // Max tokens for context
    private const string SystemPrompt = """
        You are an AI assistant helping developers with technical questions.
        Use the provided context from technical documentation to answer questions accurately.
        If the context doesn't contain relevant information, say so clearly.
        Always cite the sources when providing information.
        Be concise but thorough in your responses.
        """;
    
    public async Task<string> AskQuestionAsync(string question, CancellationToken cancellationToken = default)
    {
        var response = await GetAugmentedResponseAsync(question, null, cancellationToken);
        return response.Answer;
    }
    
    public async Task<RagResponse> GetAugmentedResponseAsync(
        string question,
        SearchRequest? searchRequest = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // 1. Search for relevant documents
            logger.LogInformation($"Searching for relevant documents for: {question}");
            
            searchRequest ??= new SearchRequest(question.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                MaxResults = 5,
                MinSimilarity = 0.75f
            };
            
            var searchResults = await embeddingService.SearchAsync(question, searchRequest, cancellationToken);
            
            if (!searchResults.Any())
            {
                logger.LogWarning("No relevant documents found");
                return new RagResponse
                {
                    Answer = "I couldn't find any relevant information in the documentation to answer your question.",
                    Sources = [],
                    ResponseTime = DateTime.UtcNow - startTime,
                    Model = chatService.GetModelId()
                };
            }
            
            // 2. Build context from search results
            var context = BuildContext(searchResults);
            
            // 3. Create augmented prompt
            var augmentedPrompt = CreateAugmentedPrompt(question, context, searchResults);
            
            // 4. Get response from LLM
            logger.LogInformation("Generating response with LLM");
            
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(SystemPrompt);
            chatHistory.AddUserMessage(augmentedPrompt);
            
            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                cancellationToken: cancellationToken);
            
            var answer = response?.Content ?? "Unable to generate response";
            
            logger.LogInformation($"Generated response in {(DateTime.UtcNow - startTime).TotalSeconds:F2} seconds");
            
            return new RagResponse
            {
                Answer = answer,
                Sources = searchResults,
                ResponseTime = DateTime.UtcNow - startTime,
                Model = chatService.GetModelId()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate RAG response");
            throw;
        }
    }
    
    private static string BuildContext(ImmutableArray<SearchResult> searchResults)
    {
        var contextBuilder = new StringBuilder();
        var currentLength = 0;
        
        foreach (var result in searchResults.OrderByDescending(r => r.Score))
        {
            var metadata = result.Document.MetadataJson.Deserialize<DocumentMetadata>();
            var content = result.Document.Content;
            
            // Estimate token count (rough approximation: 1 token ≈ 4 characters)
            var estimatedTokens = content.Length / 4;
            
            if (currentLength + estimatedTokens > MaxContextLength)
            {
                // Truncate if necessary
                var remainingChars = (MaxContextLength - currentLength) * 4;
                if (remainingChars > 100)
                {
                    content = content[..Math.Min(content.Length, remainingChars)] + "...";
                }
                else
                {
                    break;
                }
            }
            
            contextBuilder.AppendLine($"### Source: {metadata?.Title ?? "Unknown"}");
            contextBuilder.AppendLine($"Category: {metadata?.Category ?? "general"}");
            
            if (metadata?.Tags?.Any() == true)
            {
                contextBuilder.AppendLine($"Tags: {string.Join(", ", metadata.Tags)}");
            }
            
            contextBuilder.AppendLine($"Relevance Score: {result.Score:P0}");
            contextBuilder.AppendLine();
            contextBuilder.AppendLine(content);
            contextBuilder.AppendLine();
            contextBuilder.AppendLine("---");
            contextBuilder.AppendLine();
            
            currentLength += estimatedTokens;
        }
        
        return contextBuilder.ToString();
    }
    
    private static string CreateAugmentedPrompt(
        string question,
        string context,
        ImmutableArray<SearchResult> searchResults)
    {
        var promptBuilder = new StringBuilder();
        
        promptBuilder.AppendLine("## Context from Technical Documentation:");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine(context);
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("## Question:");
        promptBuilder.AppendLine(question);
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("## Instructions:");
        promptBuilder.AppendLine("Based on the provided context, answer the question thoroughly.");
        promptBuilder.AppendLine("If the context contains code examples relevant to the question, include them in your response.");
        promptBuilder.AppendLine("Cite the source documents when providing information.");
        
        // Add specific technologies if mentioned in results
        var technologies = searchResults
            .Select(r => r.Document.MetadataJson.Deserialize<DocumentMetadata>())
            .Where(m => m != null)
            .SelectMany(m => m!.Technologies)
            .Distinct()
            .ToList();
        
        if (technologies.Any())
        {
            promptBuilder.AppendLine($"Focus on these technologies if relevant: {string.Join(", ", technologies)}");
        }
        
        return promptBuilder.ToString();
    }
}

// Extension for dependency injection
public static class RagServiceExtensions
{
    public static IServiceCollection AddRagServices(
        this IServiceCollection services,
        string openAiApiKey,
        string? chatModel = null)
    {
        // Add chat completion service to existing kernel
        services.AddSingleton<IChatCompletionService>(sp =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            
            // Add OpenAI chat service if not already added
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(
                modelId: chatModel ?? "gpt-4o-mini",
                apiKey: openAiApiKey);
            
            var chatKernel = builder.Build();
            return chatKernel.GetRequiredService<IChatCompletionService>();
        });
        
        // Register RAG service
        services.AddScoped<IRagService, RagService>();
        
        return services;
    }
}