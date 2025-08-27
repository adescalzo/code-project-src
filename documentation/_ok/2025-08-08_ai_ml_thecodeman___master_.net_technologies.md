```yaml
---
title: "TheCodeMan | Master .NET Technologies"
source: https://thecodeman.net/posts/semantic-search-ai-in-dotnet9?utm_source=Newsletter&utm_medium=email&utm_campaign=TheCodeMan%20Newsletter%20-%20Semantic%20Search%20in%2050%20Lines%20of%20Code%20-%20%20AI%20in%20.NET%20-%20Part%201
date_published: unknown
date_captured: 2025-08-08T21:42:59.623Z
domain: thecodeman.net
author: Unknown
category: ai_ml
technologies: [.NET, Microsoft.Extensions.AI, Ollama, System.Numerics.Tensors]
programming_languages: [C#]
tags: [semantic-search, ai, .net, embeddings, machine-learning, cosine-similarity, ollama, text-processing]
key_concepts: [semantic-search, embeddings, deep-learning-models, cosine-similarity, AI-integration, IEmbeddingGenerator, OllamaEmbeddingGenerator, vector-space-model]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a practical guide to implementing semantic search in a .NET application using the `Microsoft.Extensions.AI` library. It explains how semantic search leverages deep learning models to convert text into high-dimensional numerical vectors, known as embeddings, which capture the meaning of the content. The core of the implementation involves generating these embeddings and then using cosine similarity to compare them, enabling the retrieval of semantically relevant results. The author demonstrates a real-world example by applying semantic search to blog post titles, utilizing the Ollama tool for local AI model execution.
---
```

# TheCodeMan | Master .NET Technologies

## Semantic Search in 50 Lines of Code - AI in .NET

Feb 24 2025

##### **Many thanks to the sponsors who make it possible for this newsletter to be free for readers.**

##### • The best [Pragmatic RESTful APIs in .NET course](https://www.courses.milanjovanovic.tech/a/aff_9044l6t3/external?affcode=1486372_ocagegla) is finally live! It's created by Milan Jovanovic. This is not a paid ad - it's just my recommendation. I didn't watch the better material than this. You have a discount through my affiliate link. [Check it out now](https://www.courses.milanjovanovic.tech/a/aff_9044l6t3/external?affcode=1486372_ocagegla).

##### • I'm preapring Enforcing Code Style course in my [TheCodeMan Community](https://www.skool.com/thecodeman). For 3 consecutive subscriptions ($12) or annual ($40) you get this course, plus everything else in the group.🚀 [Join now](https://www.skool.com/thecodeman) and grab my first ebook for free.

### [Watch YouTube video here](https://www.youtube.com/watch?v=Y9qJSIF0ZFs&ab_channel=TheCodeMan)

![Image showing a man on the left and a white robot on the right, with "SEMANTIC SEARCH" in large blue and white text in the middle. The robot has ".NET AI" written on its arm.](images/blog/posts/semantic-search-ai-in-dotnet9/youtube.png)

### What Is Semantic Search in AI?

##### Semantic search goes beyond traditional keyword matching.

##### Instead of merely looking for literal text, it uses deep learning models to understand the meaning behind a user's query and the content of documents. Here’s how it works:

##### **• Embeddings:** Text is converted into high-dimensional numerical vectors (embeddings) that capture the semantic essence of words, sentences, or documents.

##### **• Similarity Metrics:** Using measures like cosine similarity, these vectors are compared. The closer two vectors are, the more semantically similar the texts are.

##### **• Improved Relevance:** This method allows for finding results that match the intent and context of the query, even if the exact keywords aren’t present.

##### In the context of AI, semantic search has been widely used for tasks like document retrieval, recommendation systems, and conversational search applications.

### Semantic Search in .NET with Microsoft.Extensions.AI

##### In the .NET ecosystem, developers can harness semantic search techniques by using libraries that simplify the integration of AI capabilities.

##### One such library is **Microsoft.Extensions.AI**.

##### What Is Microsoft.Extensions.AI?

##### It’s a set of extensions designed to integrate AI services into .NET applications. The library provides abstractions and interfaces (like IEmbeddingGenerator) that let you easily connect to AI endpoints or models.

##### With these abstractions, you can generate embeddings from text, which is a fundamental step in implementing semantic search. This makes it much easier to build applications that require understanding and comparing text semantically.

##### By using Microsoft.Extensions.AI, developers can avoid writing low-level code to interact with AI models, thereby accelerating development and focusing on business logic.

##### Let's see how to implement it.

### Semantic Search Implmentation - Real Example

##### You know I have a blog on my website. Let's say I want to implement Search functionality via .NET.

##### The requirement is that I can enter anything in the search field and that they extract the 3 best-ranked articles from the mass of blog posts that match the query I'm looking for.

##### For the simplicity of the example, I will not use the complete content of each article, but only the titles - it is absolutely identical.

##### First, it is necessary to extract the data, ie. blog post titles. We'll put that in a list of strings.

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

### Initializing the Embedding Generator

##### An embedding generator is like a translator that converts words or sentences into numbers. These numbers, arranged in a list (or vector), capture the meaning of the text.

##### This way, computers can compare texts by looking at their numerical representations and easily figure out which texts are similar or related.

```csharp
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    new OllamaEmbeddingGenerator(new Uri("http://127.0.0.1:11434"), modelId: "all-minilm");
```

##### • IEmbeddingGenerator Interface: Defines the contract for generating embeddings from a string input.

##### • OllamaEmbeddingGenerator: A concrete implementation that connects to an AI endpoint (in this case, running locally at http://127.0.0.1:11434) and uses a specific model (all-minilm) to generate embeddings.

##### • Purpose: This generator transforms any input text into a numerical vector that captures its semantic meaning.

##### Note: You need to download [Ollama](https://ollama.com/) as well as ["all-minilm" language model](https://ollama.com/library/all-minilm).

### 4. Generating Embeddings for Blog Posts

```csharp
Console.WriteLine("Generating embeddings for blog post titles...");
var candidateEmbeddings = await embeddingGenerator.GenerateAndZipAsync(blogPostTitles);
Console.WriteLine("Embeddings generated successfully.");
```

##### **GenerateAndZipAsync**: This asynchronous method processes all candidate texts, generates their embeddings, and bundles (or "zips") them together for efficient access.

##### **Outcome**: Each blog post title now has an associated embedding that represents its semantic content.

### Interactive Query Processing - Semantic Search

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

##### **User Input:** The program prompts the user to enter a query. If the input is empty, the loop ends.

##### **Embedding Generation:** For each query, an embedding is generated so that it can be compared with the candidate embeddings.

### Computing Similarity and Retrieving Top Matches

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

##### **Cosine Similarity Calculation:** For every candidate (title), the code computes how similar its embedding is to the user’s query embedding using cosine similarity. A higher score means more semantic similarity.

##### **Selecting Top Matches:** The candidates are sorted in descending order based on their similarity scores, and the top three are selected.

##### **Displaying Results:** The program then outputs the most semantically similar blog post titles along with their similarity scores.

##### TensorPrimitives is part of System.Numerics.Tensors. It provides types and methods for tensor operations. Tensors are essential for representing embeddings as high-dimensional vectors.

### Wrapping Up

##### This code snippet is a practical example of how semantic search can be implemented in a .NET application using Microsoft.Extensions.AI.

##### By converting both the candidate texts and user queries into embeddings and comparing them via cosine similarity, the system can deliver search results that truly match the user’s intent.

##### Feel free to use and adapt this explanation and code breakdown for your blog post to help readers understand the power of semantic search and how to leverage AI within the .NET ecosystem.

##### In part 2 we will talk about [implementing RAG in .NET](https://thecodeman.net/posts/how-to-implement-rag-in-dotnet?utm_source=Website).

##### [GitHub repository](https://github.com/StefanTheCode/SemanticSearch-AI-Example).

##### That's all from me today.

##### P.S. Follow me on [YouTube](https://www.youtube.com/@thecodeman_).