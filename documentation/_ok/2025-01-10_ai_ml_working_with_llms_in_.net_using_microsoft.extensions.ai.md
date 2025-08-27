```yaml
---
title: Working with LLMs in .NET using Microsoft.Extensions.AI
source: https://www.milanjovanovic.tech/blog/working-with-llms-in-dotnet-using-microsoft-extensions-ai?utm_source=newsletter&utm_medium=email&utm_campaign=tnw124
date_published: 2025-01-11T00:00:00.000Z
date_captured: 2025-08-08T21:42:21.796Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [Microsoft.Extensions.AI, .NET 9, Ollama, Semantic Kernel, Docker, NuGet, OpenAI, Azure OpenAI, RavenDB, Postman, GPT models, Llama3]
programming_languages: [C#, Bash]
tags: [llm, ai, dotnet, ollama, semantic-kernel, microsoft-extensions-ai, local-llm, chat-completion, text-summarization, dependency-injection]
key_concepts: [Large Language Models, Local LLM Deployment, Chat Completion, Streaming Responses, Conversation History, Structured Output, Dependency Injection, LLM Provider Abstraction]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores integrating Large Language Models (LLMs) into .NET applications using the `Microsoft.Extensions.AI` library. It highlights the benefits of running LLMs locally with Ollama and demonstrates how `Microsoft.Extensions.AI` provides a unified abstraction over various LLM providers. The author provides practical C# code examples for simple chat completion, maintaining conversation history, article summarization, and extracting strongly typed data from LLM responses. The piece emphasizes the flexibility of switching between local and cloud-based LLM providers, showcasing the library's utility for diverse AI-powered features.
---
```

# Working with LLMs in .NET using Microsoft.Extensions.AI

![Working with LLMs in .NET using Microsoft.Extensions.AI](/blog-covers/mnw_124.png?imwidth=3840)
*Blog post cover image with the title 'Working with LLMs in .NET using Microsoft.Extensions.AI' and the 'MJ Tech' logo.*

# Working with LLMs in .NET using Microsoft.Extensions.AI

6 min read · January 11, 2025

Transform your database performance with [**RavenDB**](https://ravendb.net/ravendb-vs-mongodb?utm_source=influencer_mj&utm_medium=newsletter&utm_campaign=slot1): Struggling with database bottlenecks and slow queries? RavenDB is a lightning-fast document database with a distributed architecture that scales effortlessly to meet your needs. [**Try RavenDB today**](https://ravendb.net/ravendb-vs-mongodb?utm_source=influencer_mj&utm_medium=newsletter&utm_campaign=slot1).

[**AI Agents are the future of AI innovation**](https://fnf.dev/298o5bG). And APIs will be the key to their success. Postman CEO Abhinav Asthana predicts that AI agents could increase API utility by 10X-100X and offers some advice for how to optimize your APIs for agentic integration in [**this insightful article**](https://fnf.dev/298o5bG).

[Sponsor this newsletter](/sponsor-the-newsletter)

I've been experimenting with different approaches to integrating LLMs into .NET apps, and I want to share what I've learned about using `Microsoft.Extensions.AI`.

Large Language Models (LLMs) have revolutionized how we approach AI-powered applications. While many developers are familiar with cloud-based solutions like OpenAI's GPT models, running LLMs locally has become increasingly accessible thanks to projects like [Ollama](https://ollama.com/).

In this article, we'll explore how to use LLMs in .NET applications using `Microsoft.Extensions.AI`, a powerful abstraction that extends the [Semantic Kernel](https://github.com/microsoft/semantic-kernel) SDK.

## Understanding the Building Blocks

### Large Language Models (LLMs)

LLMs are deep learning models trained on vast amounts of data, capable of understanding and generating human-like text. These models can perform various tasks such as text completion, summarization, classification, and engaging in conversation. While traditionally accessed through cloud APIs, recent advances have made it possible to run them locally on standard hardware.

![Timeline of large language models.](/blogs/mnw_124/large_language_models.png?imwidth=3840)
*A timeline chart illustrating the evolution and release of various large language models from 2019 to 2023, including models like GPT-3, Llama, T5, and others, with open-source models highlighted.*

Source: [Weights & Biases](https://wandb.ai/vincenttu/blog_posts/reports/A-Survey-of-Large-Language-Models--VmlldzozOTY2MDM1)

### Ollama

Ollama is an open-source project that simplifies running LLMs locally. It provides a Docker container that can run various open-source models like Llama, making it easy to experiment with AI without depending on cloud services. Ollama handles model management and optimization and provides a simple API for interactions.

### Microsoft.Extensions.AI

[Microsoft.Extensions.AI](https://www.nuget.org/packages/Microsoft.Extensions.AI) is a library that provides a unified interface for working with LLMs in .NET applications. Built on top of Microsoft's Semantic Kernel, it abstracts away the complexity of different LLM implementations, allowing developers to switch between providers (like Ollama, Azure, or OpenAI) without changing application code.

## Getting Started

Before diving into the examples, here's what you need to run LLMs locally:

1.  Docker running on your machine
2.  Ollama container running with the `llama3` model:

```bash
# Pull the Ollama container
docker run --gpus all -d -v ollama_data:/root/.ollama -p 11434:11434 --name ollama ollama/ollama

# Pull the llama3 model
docker exec -it ollama ollama pull llama3
```

3.  A few NuGet packages (I built this using a .NET 9 console application):

```powershell
Install-Package Microsoft.Extensions.AI # The base AI library
Install-Package Microsoft.Extensions.AI.Ollama # Ollama provider implementation
Install-Package Microsoft.Extensions.Hosting # For building the DI container
```

## Simple Chat Completion

Let's start with a basic example of chat completion. Here's the minimal setup:

```csharp
var builder = Host.CreateApplicationBuilder();

builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434"), "llama3"));

var app = builder.Build();

var chatClient = app.Services.GetRequiredService<IChatClient>();

var response = await chatClient.GetResponseAsync("What is .NET? Reply in 50 words max.");

Console.WriteLine(response.Message.Text);
```

Nothing fancy here - we're just setting up dependency injection and asking a simple question. If you're used to using raw API calls, you'll notice how clean this feels.

The `AddChatClient` extension method registers the chat client with the DI container. This allows you to inject `IChatClient` into your services and interact with LLMs using a simple API. The implementation uses the `OllamaChatClient` to communicate with the Ollama container running locally.

## Implementing Chat with History

Building on the previous example, we can create an interactive chat that maintains conversation history. This is useful for context-aware interactions and real-time chat applications. All we need is a `List<ChatMessage` to store the chat history:

```csharp
var chatHistory = new List<ChatMessage>();

while (true)
{
   Console.WriteLine("Enter your prompt:");
   var userPrompt = Console.ReadLine();
   chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

   Console.WriteLine("Response from AI:");
   var chatResponse = "";
   await foreach (var item in chatClient.GetStreamingResponseAsync(chatHistory))
   {
       // We're streaming the response, so we get each message as it arrives
       Console.Write(item.Text);
       chatResponse += item.Text;
   }
   chatHistory.Add(new ChatMessage(ChatRole.Assistant, chatResponse));
   Console.WriteLine();
}
```

The cool part here is the streaming response - you get that nice, gradual text appearance like in ChatGPT. We're also maintaining chat history, which lets the model understand context from previous messages, making conversations feel more natural.

## Getting Practical: Article Summarization

Let's try something more useful - automatically summarizing articles. I've been using this to process blog posts:

```csharp
var posts = Directory.GetFiles("posts").Take(5).ToArray();
foreach (var post in posts)
{
   string prompt = $$"""
         You will receive an input text and the desired output format.
         You need to analyze the text and produce the desired output format.
         You not allow to change code, text, or other references.

         # Desired response

         Only provide a RFC8259 compliant JSON response following this format without deviation.

         {
            "title": "Title pulled from the front matter section",
            "summary": "Summarize the article in no more than 100 words"
         }

         # Article content:

         {{File.ReadAllText(post)}}
         """;

   var response = await chatClient.GetResponseAsync(prompt);
   Console.WriteLine(response.Message.Text);
   Console.WriteLine(Environment.NewLine);
}
```

Pro tip: Being specific about the output format (like requesting [RFC8259](https://datatracker.ietf.org/doc/html/rfc8259) compliant JSON) helps get consistent results. I learned this the hard way after dealing with occasionally malformed responses!

## Taking It Further: Smart Categorization

Here's where it gets really interesting - we can get strongly typed responses directly from our LLM:

```csharp
class PostCategory
{
    public string Title { get; set; } = string.Empty;
    public string[] Tags { get; set; } = [];
}

var posts = Directory.GetFiles("posts").Take(5).ToArray();
foreach (var post in posts)
{
    string prompt = $$"""
          You will receive an input text and the desired output format.
          You need to analyze the text and produce the desired output format.
          You not allow to change code, text, or other references.

          # Desired response

          Only provide a RFC8259 compliant JSON response following this format without deviation.

          {
             "title": "Title pulled from the front matter section",
             "tags": "Array of tags based on analyzing the article content. Tags should be lowercase."
          }

          # Article content:

          {{File.ReadAllText(post)}}
          """;

    var response = await chatClient.GetResponseAsync<PostCategory>(prompt);

    Console.WriteLine(
      $"{response.Result.Title}. Tags: {string.Join(",",response.Result.Tags)}");
}
```

The strongly typed approach provides compile-time safety and better IDE support, making it easier to maintain and refactor code that interacts with LLM responses.

## Flexibility with Different LLM Providers

One of the key advantages of `Microsoft.Extensions.AI` is support for different providers. While our examples use Ollama, you can easily switch to other providers:

```csharp
// Using Azure OpenAI
builder.Services.AddChatClient(new AzureOpenAIClient(
        new Uri("AZURE_OPENAI_ENDPOINT"),
        new DefaultAzureCredential())
            .AsChatClient());

// Using OpenAI
builder.Services.AddChatClient(new OpenAIClient("OPENAI_API_KEY").AsChatClient());
```

This flexibility allows you to:

*   Start development with local models
*   Move to production with cloud providers
*   Switch between providers without changing application code
*   Mix different providers for different use cases (categorization, image recognition, etc.)

## Takeaway

`Microsoft.Extensions.AI` makes it very simple to integrate LLMs into .NET applications. Whether you're building a chat interface, processing documents, or adding AI-powered features to your application, the library provides a clean, consistent API that works across different LLM providers.

I've only scratched the surface here. Since integrating this into my projects, I've found countless uses:

*   Automated content moderation for user submissions
*   Automated support ticket categorization
*   Content summarization for newsletters

I'm also planning a small side project that will use LLMs to process images from a camera feed. The idea is to detect anything unusual and trigger alerts in real-time.

What are you planning to build with this? I'd love to hear about your projects and experiences. The AI space is moving fast, but with tools like `Microsoft.Extensions.AI`, we can focus on building features rather than wrestling with infrastructure.

Good luck out there, and see you next week.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,100+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Accelerate Your .NET Skills 🚀

![PCA Cover](/_next/static/media/cover.27333f2f.png?imwidth=384)
*Cover image for the 'Pragmatic Clean Architecture' course.*

[Pragmatic Clean Architecture](/pragmatic-clean-architecture?utm_source=article_page)

![MMA Cover](/_next/static/media/cover.31e11f05.png?imwidth=384)
*Cover image for the 'Modular Monolith Architecture' course.*

[Modular Monolith Architecture](/modular-monolith-architecture?utm_source=article_page)

![PRA Cover](/_next/static/media/cover_1.fc0deb78.png?imwidth=384)
*Cover image for the 'Pragmatic REST APIs' course.*

[Pragmatic REST APIs](/pragmatic-rest-apis?utm_source=article_page)