```yaml
---
title: GPT-OSS - A C# Guide with Ollama - .NET Blog
source: https://devblogs.microsoft.com/dotnet/gpt-oss-csharp-ollama/
date_published: 2025-08-19T17:05:00.000Z
date_captured: 2025-08-22T22:43:08.979Z
domain: devblogs.microsoft.com
author: Bruno Capuano
category: frontend
technologies: [GPT-OSS, Ollama, .NET 8 SDK, Microsoft.Extensions.AI, OllamaSharp, Foundry Local, Azure AI, OpenAI, Windows]
programming_languages: [C#, Bash, SQL]
tags: [ai, llm, local-ai, dotnet, csharp, ollama, gpt-oss, machine-learning, console-app, data-privacy]
key_concepts: [large-language-models, local-ai-deployment, streaming-responses, chat-history, function-calling, agentic-applications, unified-ai-abstractions, retrieval-augmented-generation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces GPT-OSS, OpenAI's first open-weight model, emphasizing its utility for local AI development, privacy, and cost-effectiveness. It presents a comprehensive C# guide on integrating GPT-OSS with Ollama to create a fast, private, and offline-capable AI chat application. The tutorial details setting up a .NET 8 console app, incorporating the `Microsoft.Extensions.AI` and `OllamaSharp` NuGet packages, and implementing real-time streaming chat functionality. Furthermore, it hints at building advanced "agentic" applications using function calling and previews future integration with Foundry Local for Windows-native GPU acceleration. The guide empowers C# developers to harness powerful AI models directly on their local machines.]
---
```

# GPT-OSS - A C# Guide with Ollama - .NET Blog

# GPT-OSS – A C# Guide with Ollama

## Table of contents

*   [What you’ll need](#what-you’ll-need)
*   [C# toolbox](#c#-toolbox)
*   [Step 1: Create a new console app](#step-1:-create-a-new-console-app)
*   [Step 2: Add the NuGet packages](#step-2:-add-the-nuget-packages)
*   [Step 3: Write your chat code](#step-3:-write-your-chat-code)
*   [Step 4: Run your application](#step-4:-run-your-application)
*   [Build agentic apps next](#build-agentic-apps-next)
*   [Your mission (should you choose to accept it) 🕵️](#your-mission-\(should-you-choose-to-accept-it\)-🕵️)
*   [Up next — Foundry Local](#up-next-—-foundry-local)
*   [Summary](#summary)

GPT-OSS is OpenAI’s first open-weight model since GPT-2, and it’s a game-changer for developers who want powerful AI without the cloud dependency. You get two flavors – gpt-oss-120b and gpt-oss-20b – both delivering solid performance on coding, math, and tool use while keeping your data completely private. The 20B model is especially interesting because it runs on just 16GB of memory, making it perfect for local development and experimentation. Check out the [official OpenAI announcement](https://openai.com/index/introducing-gpt-oss/) to see how these models are putting serious AI power directly in developers’ hands.

Running GPT-OSS locally opens up new possibilities for experimentation, cost efficiency, and privacy. In this guide, you’ll learn how to use the open-source GPT-OSS model with Ollama to build fast, private, and offline-capable AI features using C#.

## What you’ll need

*   A machine with at least 16 GB of RAM and a capable GPU (or an Apple Silicon Mac).
*   The .NET 8 SDK or higher installed.
*   Ollama installed and running.
*   The GPT-OSS:20b model pulled with `ollama pull gpt-oss:20b`.

## C# toolbox

Microsoft has made it easy to work with AI models using the `Microsoft.Extensions.AI` libraries. These libraries provide a unified set of abstractions, letting you write code that can work with different AI providers—like Ollama, Azure AI, or OpenAI—without changing your core logic.

## Step 1: Create a new console app

First, create a new console application. Open your terminal and run:

```bash
dotnet new console -n OllamaGPTOSS
cd OllamaGPTOSS
```

## Step 2: Add the NuGet packages

To connect to Ollama using `Microsoft.Extensions.AI`, you’ll need two main packages. The `Microsoft.Extensions.AI` package provides the core abstractions, while the `OllamaSharp` package acts as the provider that implements these abstractions for Ollama.

```bash
dotnet add package Microsoft.Extensions.AI
dotnet add package OllamaSharp
```

**Note:** The `Microsoft.Extensions.AI.Ollama` package is deprecated. Use `OllamaSharp` as the recommended alternative for connecting to Ollama.

## Step 3: Write your chat code

Open `Program.cs` and replace its contents with the following code. This example keeps a rolling chat history and streams responses in real time.

```csharp
using Microsoft.Extensions.AI;
using OllamaSharp;

// Initialize OllamaApiClient targeting the "gpt-oss:20b" model
IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "gpt-oss:20b");

// Maintain conversation history
List<ChatMessage> chatHistory = new();

Console.WriteLine("GPT-OSS Chat - Type 'exit' to quit");
Console.WriteLine();

// Prompt user for input in a loop
while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();

    if (userInput?.ToLower() == "exit")
        break;

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    // Add user message to chat history
    chatHistory.Add(new ChatMessage(ChatRole.User, userInput));

    // Stream the AI response and display in real time
    Console.Write("Assistant: ");
    var assistantResponse = "";

    await foreach (var update in chatClient.GetStreamingResponseAsync(chatHistory))
    {
        Console.Write(update.Text);
        assistantResponse += update.Text;
    }

    // Append assistant message to chat history
    chatHistory.Add(new ChatMessage(ChatRole.Assistant, assistantResponse));
    Console.WriteLine();
    Console.WriteLine();
}
```

## Step 4: Run your application

Make sure your Ollama service is running. Then run your .NET console app:

```bash
dotnet run
```

Video Player

[https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2025/08/gpt-oss-ollama-demo.webm](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2025/08/gpt-oss-ollama-demo.webm)

Your application will connect to the local Ollama server, and you can start chatting with your own private GPT-OSS model.

## Build agentic apps next

This is just the beginning. The `Microsoft.Extensions.AI` libraries also support function calling, allowing you to give your local LLM access to your C# methods, APIs, and data. This is where you can build truly powerful, “agentic” applications.

### Your mission (should you choose to accept it) 🕵️

1.  Get this sample running and see how easy local LLM development is.
2.  Explore the documentation for `Microsoft.Extensions.AI` and `OllamaSharp`.
3.  Integrate this into a project: document summarizer, code generator, or intelligent assistant that runs on your machine.

The future of AI is decentralized, and as a C# developer, you have the tools to lead the charge. The power is on your machine—now go build something incredible!

## Up next — Foundry Local

In follow-up posts we’ll show how to run the same GPT-OSS model using Foundry Local instead of Ollama. Foundry Local offers Windows-native GPU acceleration and a slightly different runtime, and we’ll provide Foundry-specific configuration, tips for GPU setup, and an example C# wiring that mirrors this guide’s chat + streaming pattern.

Read the announcement for [Foundry Local support on the Windows Developer Blog](https://blogs.windows.com/windowsdeveloper/2025/08/05/available-today-gpt-oss-20b-model-on-windows-with-gpu-acceleration-further-pushing-the-boundaries-on-the-edge/).

## Summary

You learned how to: (1) set up a .NET console app, (2) add `Microsoft.Extensions.AI` plus `OllamaSharp`, (3) stream chat completions from a local GPT-OSS model, and (4) prepare for advanced scenarios like function calling. Try extending this sample with tool invocation or local RAG over your documents to unlock richer agent patterns—all while keeping data local.