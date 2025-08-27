```yaml
---
title: "How to implement RAG system — AI in .NET | by Stefan Đokić | Medium"
source: https://medium.com/@thecodeman/how-to-implement-rag-system-ai-in-net-002bfe02a9d0
date_published: 2025-03-11T23:45:00.552Z
date_captured: 2025-08-06T17:49:18.377Z
domain: medium.com
author: Stefan Đokić
category: ai_ml
technologies: [.NET, Ollama, PostgreSQL, Neon, HttpClient, Npgsql, .NET Minimal API, Confluence, Notion]
programming_languages: [C#, SQL]
tags: [rag, ai, dotnet, llm, vector-database, postgresql, semantic-search, web-api, data-access, ollama]
key_concepts: [retrieval-augmented-generation, large-language-models, vector-embeddings, semantic-search, vector-database, dependency-injection, minimal-api, ai-hallucination]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to implementing a Retrieval-Augmented Generation (RAG) system using .NET. It explains RAG's core principles, demonstrating how it enhances LLM responses by integrating external data sources like databases. The implementation details cover creating an embedding generator with Ollama, setting up a vector database using Neon (PostgreSQL with pgvector), and building a RAG service to combine semantic search with AI-powered generation. The post includes practical C# code examples for each component, culminating in a functional .NET Minimal API.
---
```

# How to implement RAG system — AI in .NET | by Stefan Đokić | Medium

![Featured Image: A scenic view of a forested mountain landscape with a lake, overlaid with text "THECODEMAN.NET HOW TO IMPLEMENT RAG - AI IN .NET"](https://miro.medium.com/v2/1*YaXSRnwC5GFaa22volwpaQ.png)

# How to implement RAG system — AI in .NET

## What is RAG?

**RAG (Retrieval-Augmented Generation)** is an AI framework that enhances generative large language models (LLMs) by integrating traditional information retrieval methods, such as search engines and databases.

This approach allows LLMs to generate responses that are more accurate, current, and contextually relevant by leveraging both your data and broader world knowledge.

![Diagram illustrating the RAG system workflow. A client sends a query to an application. The application performs a retrieval query to a Postgres vector database. The database returns a vector response (context) to the application. The application then sends the query, response, and instructions to an LLM. The LLM processes and sends an answer back to the application, which then summarizes and sends the final answer to the client.](https://miro.medium.com/v2/resize:fit:700/1*E-mQ8CtztYb2CghVBaGqaQ.png)

## How does Retrieval-Augmented Generation work?

RAG systems work in a couple of key steps to make AI-generated responses more accurate and useful:

**Finding and Preparing Information:** Instead of relying only on what an AI model already knows, RAG searches internal sources — like web pages, databases, company knowledge bases, or anything that we give it — to find relevant information. Once it pulls the data, it cleans it up by breaking it into tokens, removing unnecessary words, and making it easier for the AI to process.

**Generating Smarter Responses:** The AI then blends this fresh information with what it already knows, creating responses that are not just more accurate but also more relevant and engaging. This way, instead of guessing or relying on outdated knowledge, the AI can deliver answers that are both well-informed and up-to-date.

## Example: Customer Support Chatbot with RAG

Imagine a company that wants to provide instant and accurate answers to customer queries about its products and services. Instead of relying solely on a pre-trained LLM, which may not have the latest company-specific information, the chatbot is designed as an RAG system.

**How It Works:**

1.  **User Query:** A customer asks, _“What is the return policy for my latest order?”_
2.  **2\. Retrieval Phase:** The system first searches the company’s internal documentation, knowledge base, or database (e.g., FAQs, policy documents, order history).
3.  **3\. Augmented Generation:** The retrieved information is then fed into the LLM, which generates a well-structured, context-aware response.
4.  **4\. Response to User:** The chatbot replies, _“Our return policy allows you to return items within 30 days of purchase. Since your order was placed 15 days ago, you can still request a return. Would you like to start the process?”_

By using **RAG**, the chatbot ensures its responses are:

*   Accurate (based on the latest return policy)
*   • Up-to-date (fetches real-time order details) • Relevant (answers specific to the customer’s situation)

## How to implement it in .NET?

What I’m going to create: I will use my brand TheCodeMan for these purposes, and I will create a mini-database that tells a little more about my brand, what I do, where TheCodeMan can be seen, a little about sponsorships and partnerships, and the like.

When I fill my database, I will test the system through a couple of questions to find out if the system knows some information about the brand itself based on the data of who has it.

Here ,I expect to get an answer like _“I don’t have such information”_ if I ask a question for which the answer is not actually in the database.

Let’s implement it.

If you want to start with AI Basics in .NET, you can read [How to implement Semantic Search in .NET 9](https://thecodeman.net/posts/semantic-search-ai-in-dotnet9?utm_source=Website).

## Create Embedding Generator

The **OllamaEmbeddingGenerator** generates vector embeddings for textual data by interacting with the **Ollama API**.

It converts input text into a numerical representation (float\[\]), making it useful for **semantic search, Retrieval-Augmented Generation (RAG) systems, and AI-driven applications**.

The class sends an HTTP request to **Ollama’s /api/embeddings endpoint**, retrieves the embedding, and processes the response. If the API request fails or returns invalid data, it throws an exception to ensure reliability.

This embedding generator is essential for applications that require **text similarity comparison, knowledge retrieval, and intelligent search**. Using embeddings instead of traditional keyword matching enables **context-aware search and AI-powered content recommendations**.

When integrated with **pgvector (PostgreSQL)** or other vector databases, it allows for efficient **semantic retrieval of relevant data** based on meaning rather than exact words.

```csharp
public class OllamaEmbeddingGenerator(Uri ollamaUrl, string modelId = "mistral") : IEmbeddingGenerator  
{  
    private readonly HttpClient _httpClient = new();  
    private readonly Uri _ollamaUrl = ollamaUrl;  
    private readonly string _modelId = modelId;  
  
    public async Task<float[]> GenerateEmbeddingAsync(string text)  
    {  
        var requestBody = new { model = _modelId, prompt = text };  
  
        var response = await _httpClient.PostAsync(  
            new Uri(_ollamaUrl, "/api/embeddings"),  
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));  
  
        if (!response.IsSuccessStatusCode)  
        {  
            throw new Exception($"Ollama API error: {await response.Content.ReadAsStringAsync()}");  
        }  
  
        var responseJson = await response.Content.ReadAsStringAsync();  
        Console.WriteLine("Ollama Response: " + responseJson);  
  
        var serializationOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };  
  
        var embeddingResponse = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(responseJson, serializationOptions);  
  
        if (embeddingResponse?.Embedding == null || embeddingResponse.Embedding.Length == 0)  
        {  
            throw new Exception("Failed to generate embedding.");  
        }  
  
        return embeddingResponse.Embedding;  
    }  
}
```

**IEmbeddingGenerator interface:**

```csharp
public interface IEmbeddingGenerator  
{  
    Task<float[]> GenerateEmbeddingAsync(string text);  
}
```

**OllamaEmbeddingResponse:**

```csharp
public class OllamaEmbeddingResponse  
{  
    [JsonPropertyName("embedding")]  
    public float[] Embedding { get; set; } = [];  
}
```

## Create Vector Database with Neon

The **TextRepository class** is a data access layer that manages text storage and retrieval using [Neon, a serverless PostgreSQL database](https://neon.tech/?refcode=DNZ1AUO3). I created this database in less than 30 seconds with a couple of clicks.

It works by storing text with vector embeddings and later retrieving the most relevant texts based on similarity. The repository relies on pgvector, a PostgreSQL extension for vector-based search, enabling efficient semantic retrieval instead of traditional keyword matching.

When storing text, it first generates an embedding using IEmbeddingGenerator (e.g., Ollama’s embedding API) and then saves both the text and its embedding in the database.

When retrieving data, it converts the query into an embedding and finds the top 5 most relevant matches using a vector similarity search.

This setup allows fast and scalable AI-powered search, leveraging Neon’s serverless PostgreSQL, which was set up in less than 30 seconds, ensuring high availability and automatic scaling without database management overhead.

```csharp
public class TextRepository(string connectionString, IEmbeddingGenerator embeddingGenerator)  
{  
    private readonly string _connectionString = connectionString;  
    private readonly IEmbeddingGenerator _embeddingGenerator = embeddingGenerator;  
  
    public async Task StoreTextAsync(string content)  
    {  
        var embedding = await _embeddingGenerator.GenerateEmbeddingAsync(content);  
  
        using var conn = new NpgsqlConnection(_connectionString);  
        await conn.OpenAsync();  
  
        string query = "INSERT INTO text_contexts (content, embedding) VALUES (@content, @embedding)";  
        using var cmd = new NpgsqlCommand(query, conn);  
        cmd.Parameters.AddWithValue("content", content);  
        cmd.Parameters.AddWithValue("embedding", embedding);  
  
        await cmd.ExecuteNonQueryAsync();  
    }  
  
    public async Task<List<string>> RetrieveRelevantText(string query)  
    {  
        var queryEmbedding = await _embeddingGenerator.GenerateEmbeddingAsync(query);  
  
        using var conn = new NpgsqlConnection(_connectionString);  
        await conn.OpenAsync();  
  
        string querySql = @" SELECT content FROM text_contexts WHERE embedding <-> CAST(@queryEmbedding AS vector) > 0.7 ORDER BY embedding <-> CAST(@queryEmbedding AS vector) LIMIT 5";  
  
        using var cmd = new NpgsqlCommand(querySql, conn);  
  
        string embeddingString = $"[{string.Join(",", queryEmbedding.Select(v => v.ToString("G", CultureInfo.InvariantCulture)))}]";  
        cmd.Parameters.AddWithValue("queryEmbedding", embeddingString);  
        return results.Any() ? results : new List<string> { "No relevant context found." };  
    }  
}
```

**TextContext class:**

```csharp
public class TextContext  
{  
    public int Id { get; set; }  
    public string Content { get; set; } = string.Empty;  
    public float[] Embedding { get; set; } = [];  
}
```

## Implement a RAG Service

The RagService class is the core of the Retrieval-Augmented Generation (RAG) system, combining Neon’s serverless PostgreSQL for semantic search and Ollama’s AI model for response generation.

It retrieves relevant stored knowledge using **vector similarity search**, then uses an AI model (default: **“mistral”**) to generate an answer strictly based on the retrieved context.

When a user asks a question, RagService:

1.  Queries the database via TextRepository, retrieving the top 5 most relevant records based on vector similarity.
2.  Combines the retrieved texts into a single context block.
3.  Ensures AI does not hallucinate by enforcing a strict prompt — if the answer isn’t in the provided context, it must respond with: _“I don’t know. No relevant data found.”_
4.  Sends the structured request to the Ollama API, instructing it to generate a response using only the given context.
5.  Returns a structured response, including both the retrieved context and the AI-generated answer.

```csharp
public class RagService(TextRepository retriever, Uri ollamaUrl, string modelId = "mistral")  
{  
    private readonly TextRepository _textRepository = retriever;  
    private readonly HttpClient _httpClient = new();  
    private readonly Uri _ollamaUrl = ollamaUrl;  
    private readonly string _modelId = modelId;  
  
    public async Task<object> GetAnswerAsync(string query)  
    {  
        // Retrieve multiple relevant texts  
        List<string> contexts = await _textRepository.RetrieveRelevantText(query);  
  
        // Combine multiple contexts into one string  
        string combinedContext = string.Join("\n\n---\n\n", contexts);  
  
        // If no relevant context is found, return a strict message  
        if (contexts.Count == 1 && contexts[0] == "No relevant context found.")  
        {  
            return new  
            {  
                Context = "No relevant data found in the database.",  
                Response = "I don't know."  
            };  
        }  
  
        var requestBody = new  
        {  
            model = _modelId,  
            prompt = $"""  
        You are a strict AI assistant. You MUST answer ONLY using the provided context.   
        If the answer is not in the context, respond with "I don't know. No relevant data found."  
  
        Context:  
        {combinedContext}  
  
        Question: {query}  
        """,  
            stream = false  
        };  
  
        var response = await _httpClient.PostAsync(  
            new Uri(_ollamaUrl, "/api/generate"),  
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));  
  
        if (!response.IsSuccessStatusCode)  
        {  
            return new  
            {  
                Context = combinedContext,  
                Response = "Error: Unable to generate response."  
            };  
        }  
  
        var responseJson = await response.Content.ReadAsStringAsync();  
        var serializationOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };  
        var completionResponse = JsonSerializer.Deserialize<OllamaCompletionResponse>(responseJson, serializationOptions);  
  
        return new  
        {  
            Context = combinedContext,  
            Response = completionResponse?.Response ?? "I don't know. No relevant data found."  
        };  
    }  
}
```

## Setup and Endpoints

The **Program** class initializes and runs a **.NET Minimal API** for a **RAG system**, combining Neon’s serverless PostgreSQL with **Ollama’s AI model (mistral)**.

It sets up **dependency injection**, configuring an IEmbeddingGenerator to generate vector embeddings, a TextRepository to store and retrieve embeddings using **pgvector**, and a RagService to process AI-powered queries while ensuring responses are strictly based on stored knowledge.

The API provides two endpoints: **POST /add-text**, which generates embeddings and stores text for retrieval, and **GET /ask**, which retrieves the most relevant stored contexts, sends them to Ollama, and returns an AI-generated response only if relevant data is found.

```csharp
public class Program  
{  
    public static void Main(string[] args)  
    {  
        var builder = WebApplication.CreateBuilder(args);  
        var configuration = builder.Configuration;  
  
        // Load settings from appsettings.json  
        var connectionString = configuration.GetConnectionString("PostgreSQL");  
  
        if (string.IsNullOrEmpty(connectionString))  
        {  
            throw new InvalidOperationException("Required configuration settings are missing.");  
        }  
  
        // Register services  
        builder.Services.AddSingleton(sp =>  
            new OllamaEmbeddingGenerator(new Uri("http://127.0.0.1:11434"), "mistral"));  
  
        builder.Services.AddSingleton(sp =>  
            new TextRepository(connectionString, sp.GetRequiredService<IEmbeddingGenerator>()));  
  
        builder.Services.AddSingleton(sp =>  
            new RagService(sp.GetRequiredService<TextRepository>(), new Uri("http://127.0.0.1:11434"), "mistral"));  
  
        var app = builder.Build();  
  
        // Minimal API endpoints  
        app.MapPost("/add-text", async (TextRepository textRepository, HttpContext context) =>  
        {  
            var request = await context.Request.ReadFromJsonAsync<AddTextRequest>();  
            if (string.IsNullOrWhiteSpace(request?.Content))  
            {  
                return Results.BadRequest("Content is required.");  
            }  
        });  
  
        app.Run();  
    }  
}
```

## Wrapping Up

In this post, we built a Retrieval-Augmented Generation (RAG) system in .NET, combining Neon’s serverless PostgreSQL for efficient vector storage and retrieval with Ollama’s AI model (mistral) for generating responses based on stored knowledge.

We structured the system to store text embeddings, perform semantic search using pgvector, and ensure that AI responses are strictly context-aware — eliminating hallucinations and improving reliability.

By leveraging [Neon’s instant setup](https://neon.tech/?refcode=DNZ1AUO3) and automatic scaling, we eliminated database management overhead, while Ollama’s local inference allowed AI-powered responses without relying on external APIs.

This architecture enables fast, scalable, and intelligent knowledge retrieval, making it ideal for AI-powered chatbots, documentation assistants, and enterprise search solutions.

## Next Steps?

You can extend this system by automatically syncing Confluence documentation, Notion database, or something else, adding user feedback loops, or optimizing vector search with hybrid retrieval techniques.

Whether you’re building a developer assistant, a smart knowledge base, or an AI-powered search engine, this foundation sets you up for scalable and efficient AI-driven retrieval!

**Check out the** [**source code here**](https://thecodeman.net/rag-system-dotnet)**.**

That’s all from me today.

P.S. Follow me on [YouTube](https://www.youtube.com/@thecodeman_).

_Originally published at_ [_https://www.thecodeman.net_](https://www.thecodeman.net/) _on March 10, 2025._

**P.S. Whenever you’re ready, there are 4 ways I can help you:**

1.  Join the [#1 .NET Community on the Skool](https://www.skool.com/thecodeman) platform.  
    70 members are already there.
2.  [Design Patterns that Deliver:](https://thecodeman.net/design-patterns-that-deliver-ebook) This isn’t just another design patterns book. Dive into real-world examples and practical solutions to real problems in real applications.
3.  [Design Patterns Simplified](https://thecodeman.net/design-patterns-simplified?utm_source=medium): In this concise and affordable e-book, I’ve distilled the essence of design patterns into an easy-to-digest format. [Join 1,300+ readers here](https://thecodeman.net/design-patterns-simplified?utm_source=medium).
4.  Promote yourself or your business to 15,200+ subscribers by [sponsoring TheCodeMan newsletter](https://thecodeman.net/sponsorship).