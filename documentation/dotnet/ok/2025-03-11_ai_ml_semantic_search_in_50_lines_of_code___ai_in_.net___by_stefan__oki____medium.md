```yaml
---
title: "Semantic Search in 50 Lines of Code — AI in .NET | by Stefan Đokić | Medium"
source: https://medium.com/@thecodeman/semantic-search-in-50-lines-of-code-ai-in-net-f6bd98b6e92b
date_published: 2025-03-11T23:39:06.075Z
date_captured: 2025-08-06T17:50:33.960Z
domain: medium.com
author: Stefan Đokić
category: ai_ml
technologies: [Microsoft.Extensions.AI, .NET, ASP.NET Core, Ollama, System.Numerics.Tensors, Azure Feature Management, Azure Key Vault, MediatR, Refit, Ocelot, GraphQL, Polly, Automapper, Aspire, Neon]
programming_languages: [C#, SQL]
tags: [semantic-search, ai, .net, embeddings, machine-learning, csharp, natural-language-processing, data-access, search, software-development]
key_concepts: [semantic-search, embeddings, cosine-similarity, deep-learning, AI-integration, natural-language-processing, IEmbeddingGenerator, query-processing]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article demonstrates how to implement semantic search in .NET applications using the Microsoft.Extensions.AI library. It explains the core concepts of semantic search, including embeddings and cosine similarity, which allow for understanding the meaning behind text rather than just keyword matching. The author provides a practical C# example that leverages Ollama and the `IEmbeddingGenerator` interface to generate and compare text embeddings for blog post titles. The implementation showcases how to find the most semantically similar articles to a user's query, offering a more relevant search experience. The post concludes by highlighting the simplicity of integrating AI capabilities into .NET with this approach.]
---
```

# Semantic Search in 50 Lines of Code — AI in .NET | by Stefan Đokić | Medium

![Featured Image: A scenic landscape featuring a medieval castle tower on the right, a modern wind turbine on a distant hill to the left, under a clear blue sky. Overlaid are yellow rectangular boxes with black text: "THECODEMAN.NET" at the top, and partially visible "SEMANTIC SEARCH IN .NET" at the bottom.](https://miro.medium.com/v2/1*7vDBbOJTX7GzqEooeujkAA.png)

# Semantic Search in 50 Lines of Code — AI in .NET

## What Is Semantic Search in AI?

[![Stefan Đokić's profile picture: A circular image showing a man with short dark hair and a light-colored shirt, looking directly at the camera.](https://miro.medium.com/v2/resize:fill:64:64/1*aqsw_Rn3znWu_frj2UNDRw.png)](/@thecodeman?source=post_page---byline--f6bd98b6e92b---------------------------------------)

[Stefan Đokić](/@thecodeman?source=post_page---byline--f6bd98b6e92b---------------------------------------)

Follow

6 min read

·

Mar 11, 2025

15

Listen

Share

More

Semantic search goes beyond traditional keyword matching.

Instead of merely looking for literal text, it uses deep learning models to understand the meaning behind a user’s query and the content of documents. Here’s how it works:

*   **Embeddings:** Text is converted into high-dimensional numerical vectors (embeddings) that capture the semantic essence of words, sentences, or documents.
*   **Similarity Metrics:** Using measures like cosine similarity, these vectors are compared. The closer two vectors are, the more semantically similar the texts are.
*   **Improved Relevance:** This method allows for finding results that match the intent and context of the query, even if the exact keywords aren’t present.

In AI, semantic search has been widely used for tasks like document retrieval, recommendation systems, and conversational search applications.

## Semantic Search in .NET with Microsoft.Extensions.AI

In the .NET ecosystem, developers can harness semantic search techniques by using libraries that simplify the integration of AI capabilities.

One such library is **Microsoft.Extensions.AI**.

What Is Microsoft.Extensions.AI?

It’s a set of extensions designed to integrate AI services into .NET applications. The library provides abstractions and interfaces (like IEmbeddingGenerator) that let you easily connect to AI endpoints or models.

With these abstractions, you can generate embeddings from text, which is a fundamental step in implementing semantic search. This makes building applications that require understanding and comparing text semantically is much easier.

By using Microsoft.Extensions.AI, developers can avoid writing low-level code to interact with AI models, thereby accelerating development and focusing on business logic.

Let’s see how to implement it.

## Semantic Search Implementation — Real Example

You know I have a blog on my website. Let’s say I want to implement Search functionality via .NET.

The requirement is that I can enter anything in the search field and that they extract the 3 best-ranked articles from the mass of blog posts that match the query I’m looking for.

For the simplicity of the example, I will not use the complete content of each article, but only the titles — it is identical.

First, it is necessary to extract the data, ie, blog post titles. We’ll put that in a list of strings.

```csharp
var blogPostTitles = new[]  
{  
    "Debug and Test Multi-Environment Postgres Db in .NET with Aspire + Neon",  
    "Simplifying Integration with the Adapter Pattern",  
    "Getting Started with OpenTelemetry in .NET",  
    "Saga Orchestration Pattern",  
    ".NET 9 - New LINQ Methods",  
    "HybridCache in ASP.NET Core - .NET 9",  
    "Chain Responsibility Pattern",  
    "Exploring C# 13",  
    "Feature Flags in .NET 8 with Azure Feature Management",  
    "Securing Secrets in .NET 8 with Azure Key Vault",  
    "LINQ Performance Optimization Tips & Tricks",  
    "Using Singleton in Multithreading in .NET",  
    "How to create .NET Custom Guard Clause",  
    "How to implement CQRS without MediatR",  
    "4 Entity Framework Tips to improve performances",  
    "REPR Pattern - For C# developers",  
    "Refit - The .NET Rest API you should know about",  
    "6 ways to elevate your 'clean' code",  
    "Deep dive into Source Generators",  
    "3 Tips to Elevate your Swagger UI",  
    "Memory Caching in .NET",  
    "Solving HttpClient Authentication with Delegating Handlers",  
    "Strategy Design Pattern will help you refactor code",  
    "How to implement API Key Authentication",  
    "Live loading appsettings.json configuration file",  
    "Retry Failed API calls with Polly",  
    "How and why I create my own mapper (avoid Automapper)?",  
    "The ServiceCollection Extension Pattern",  
    "3 things you should know about Strings",  
    "API Gateways - The secure bridge for exposing your API",  
    "5 cool features in C# 12",  
    "Allow specific users to access your API - Part 2",  
    "Allow specific users to access your API - Part 1",  
    "Response Compression in ASP.NET",  
    "API Gateway with Ocelot",  
    "Health Checks in .NET 8",  
    "MediatR Pipeline Behavior",  
    "Getting Started with PLINQ",  
    "Get Started with GraphQL in .NET",  
    "Better Error Handling with Result object",  
    "Background Tasks in .NET 8",  
    "Pre-Optimized EF Query Techniques 5 Steps to Success",  
    "Improve EF Core Performance with Compiled Queries",  
    "How do I implement a workflow using a .NET workflow engine?",  
    "What is and why do you need API Versioning?",  
    "Compile-time logging source generation for highly performant logging"  
};
```

## Initializing the Embedding Generator

An embedding generator is like a translator that converts words or sentences into numbers. These numbers, arranged in a list (or vector), capture the meaning of the text.

This way, computers can compare texts by looking at their numerical representations and easily figure out which texts are similar or related.

```csharp
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =  
    new OllamaEmbeddingGenerator(new Uri("http://127.0.0.1:11434"), modelId: "all-minilm");
```

*   **IEmbeddingGenerator Interface:** Defines the contract for generating embeddings from a string input.
*   **OllamaEmbeddingGenerator:** A concrete implementation that connects to an AI endpoint (in this case, running locally at http://127.0.0.1:11434) and uses a specific model (all-minilm) to generate embeddings.
*   **Purpose:** This generator transforms any input text into a numerical vector that captures its semantic meaning.

Note: You need to download [Ollama](https://ollama.com/) as well as the [“all-minilm” language model](https://ollama.com/library/all-minilm).

## 4\. Generating Embeddings for Blog Posts

```csharp
Console.WriteLine("Generating embeddings for blog post titles...");  
var candidateEmbeddings = await embeddingGenerator.GenerateAndZipAsync(blogPostTitles);  
Console.WriteLine("Embeddings generated successfully.");
```

**GenerateAndZipAsync**: This asynchronous method processes all candidate texts, generates their embeddings, and bundles (or “zips”) them together for efficient access.

**Outcome**: Each blog post title now has an associated embedding that represents its semantic content.

## Interactive Query Processing — Semantic Search

```csharp
while (true)  
{  
    Console.WriteLine("\nEnter your query (or press Enter to exit):");  
    var userInput = Console.ReadLine();  
  
    if (string.IsNullOrWhiteSpace(userInput))  
    {  
        break;  
    }  
  
    // Generate embedding for the user's input.  
    var userEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(userInput);  
}
```

**User Input:** The program prompts the user to enter a query. If the input is empty, the loop ends.

**Embedding Generation:** For each query, an embedding is generated so that it can be compared with the candidate embeddings.

## Computing Similarity and Retrieving Top Matches

```csharp
// Compute cosine similarities and get the top three matches.  
    var topMatches = candidateEmbeddings  
        .Select(candidate => new  
        {  
            Text = candidate.Value,  
            Similarity = TensorPrimitives.CosineSimilarity(  
                candidate.Embedding.Vector.Span, userEmbedding.Vector.Span)  
        })  
        .OrderByDescending(match => match.Similarity)  
        .Take(3);  
  
    Console.WriteLine("\nTop matching blog post titles:");  
    foreach (var match in topMatches)  
    {  
        Console.WriteLine($"Similarity: {match.Similarity:F4} - {match.Text}");  
    }
```

**Cosine Similarity Calculation:** For every candidate (title), the code computes how similar its embedding is to the user’s query embedding using cosine similarity. A higher score means more semantic similarity.

**Selecting Top Matches:** The candidates are sorted in descending order based on their similarity scores, and the top three are selected.

**Displaying Results:** The program then outputs the most semantically similar blog post titles along with their similarity scores.

`TensorPrimitives` is part of `System.Numerics.Tensors`. It provides types and methods for tensor operations. Tensors are essential for representing embeddings as high-dimensional vectors.

## Wrapping Up

This code snippet is a practical example of how semantic search can be implemented in a .NET application using Microsoft.Extensions.AI.

By converting both the candidate texts and user queries into embeddings and comparing them via cosine similarity, the system can deliver search results that truly match the user’s intent.

Feel free to use and adapt this explanation and code breakdown for your blog post to help readers understand the power of semantic search and how to leverage AI within the .NET ecosystem.

In part 2, we will talk about [implementing RAG in .NET](https://thecodeman.net/posts/how-to-implement-rag-in-dotnet?utm_source=Website).

[GitHub repository](https://github.com/StefanTheCode/SemanticSearch-AI-Example).

That’s all from me today.

P.S. Follow me on [YouTube](https://www.youtube.com/@thecodeman_).

_Originally published at_ [_https://www.thecodeman.net_](https://www.thecodeman.net/) _on February 24, 2025._

**P.S. Whenever you’re ready, there are 4 ways I can help you:**

1.  Join the [#1 .NET Community on the Skool](https://www.skool.com/thecodeman) platform.  
    70 members are already there.
2.  [Design Patterns that Deliver:](https://thecodeman.net/design-patterns-that-deliver-ebook) This isn’t just another design patterns book. Dive into real-world examples and practical solutions to real problems in real applications.
3.  [Design Patterns Simplified](https://thecodeman.net/design-patterns-simplified?utm_source=medium): In this concise and affordable e-book, I’ve distilled the essence of design patterns into an easy-to-digest format. [Join 1,300+ readers here](https://thecodeman.net/design-patterns-simplified?utm_source=medium).
4.  Promote yourself or your business to 15,200+ subscribers by [sponsoring TheCodeMan newsletter](https://thecodeman.net/sponsorship).