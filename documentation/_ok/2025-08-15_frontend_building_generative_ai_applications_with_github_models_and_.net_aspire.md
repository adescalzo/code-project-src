```yaml
---
title: Building Generative AI Applications With GitHub Models and .NET Aspire
source: https://www.milanjovanovic.tech/blog/building-generative-ai-applications-with-github-models-and-dotnet-aspire
date_published: 2025-08-16T00:00:00.000Z
date_captured: 2025-08-20T19:01:23.148Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: frontend
technologies: [.NET Aspire 9.4, GitHub Models, OpenAI, Microsoft, Meta, Aspire.Hosting.GitHub.Models, Azure AI Client, Aspire.Azure.AI.Inference, Microsoft.Extensions.AI, HtmlAgilityPack, ASP.NET Core, .NET]
programming_languages: [C#]
tags: [generative-ai, dotnet, .net-aspire, github-models, ai-integration, application-development, api, csharp, orchestration, llm]
key_concepts: [generative-ai, ai-model-integration, service-orchestration, service-discovery, health-checks, telemetry, prompt-engineering, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  This article demonstrates how to build a simple AI-powered blog analyzer using .NET Aspire 9.4 and its new GitHub Models integration. It highlights how .NET Aspire significantly simplifies the complexity of integrating AI services by handling API key management, service discovery, health checks, and telemetry automatically. The author provides practical C# code examples for configuring AI models in the AppHost, consuming them in services, and using prompt engineering to categorize blog content. The post emphasizes the ease of adding generative AI capabilities to .NET applications, offering a foundation for developing more sophisticated AI features.
---
```

# Building Generative AI Applications With GitHub Models and .NET Aspire

![Building Generative AI Applications With GitHub Models and .NET Aspire](/blog-covers/mnw_155.png?imwidth=3840)

# Building Generative AI Applications With GitHub Models and .NET Aspire

I wanted to see what the simplest practical AI app I could build was, and this is what I came up with.

Every week, I publish blog posts covering different topics - architecture patterns, cloud services, programming techniques, business insights. Sometimes I write about DevOps, other times about security. After years of writing, I realized I had no systematic way to categorize my content. Sure, I could manually tag each post, but where's the fun in that?

So I built a simple AI-powered blog analyzer. It fetches any blog post, extracts the content, and uses AI to automatically categorize it. The entire thing took less than an hour to build thanks to **.NET Aspire 9.4** and it's new **GitHub Models** integration.

What surprised me wasn't just how easy it was to build, but how the integration completely removes the typical AI service complexity. No juggling API keys in configuration files, no manual HTTP client setup, no wrestling with different SDK patterns for different AI providers. You declare an AI model in your AppHost just like you would a database, and Aspire handles the rest.

Here's what I learned building this simple app, and how you can use the same patterns to add AI to your applications.

## What are GitHub Models?

[GitHub Models](https://docs.github.com/en/github-models) is a service that provides access to AI models from OpenAI, Microsoft, Meta, and others through a single API. You get free tier access for prototyping (with rate limits), an interactive playground for testing prompts, and pay-per-use billing when you're ready for production.

The models range from cost-effective options like **GPT-4o-mini** to more advanced models. Each model has different strengths - some excel at reasoning, others at code generation or creative writing.

When you combine GitHub Models with .NET Aspire's orchestration, you get:

*   Automatic API key management via [external parameters](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/external-parameters)
*   [**Service discovery**](how-dotnet-aspire-simplifies-service-discovery) between your components
*   Built-in [**health checks**](health-checks-in-asp-net-core) and [**telemetry**](introduction-to-distributed-tracing-with-opentelemetry-in-dotnet)
*   Consistent configuration patterns

## Setting Up the Integration

The GitHub Models integration splits into two parts: configuring models in your `AppHost` and consuming them in your services.

### AppHost Configuration

In your AppHost project, you define AI models as resources alongside your other services:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var blogService = builder.AddExternalService("dotnet-blog", "https://www.milanjovanovic.tech/blog");
var aiModel = builder.AddGitHubModel("ai-model", "openai/gpt-4o-mini");

builder.AddProject<Projects.GitHub_Models_Demo>("github-models-demo")
    .WithReference(blogService)
    .WithReference(aiModel);
```

Notice how the AI model sits alongside the external blog service. Both are resources that your main application depends on. Aspire handles the connection details - you just declare what you need.

Make sure to install the [Aspire.Hosting.GitHub.Models](https://www.nuget.org/packages/Aspire.Hosting.GitHub.Models) NuGet package to enable this integration.

To call the GitHub Models inference API you need a personal access token with the `models:read` permission. When you call `AddGitHubModel`, Aspire automatically creates a parameter named `{resourceName}-gh-apikey` (for example, `ai-model-gh-apikey`)

You can populate the parameter through user secrets for local development:

```json
{
  "Parameters": {
    "ai-model-gh-apikey": "github_pat_YOUR_PERSONAL_ACCESS_TOKEN"
  }
}
```

If you don't provide this value from configuration, Aspire will prompt you to enter it when you run the application. You'll have an option to store the access token securely in user secrets.

### Client Setup

In your consuming service, add the Azure AI client (which works with GitHub Models):

```csharp
builder
    .AddAzureChatCompletionsClient("ai-model")
    .AddChatClient();
```

That's it. No manual HTTP client configuration, no hardcoded endpoints. The `ai-model` name matches what you defined in the AppHost, and Aspire wires everything together.

You'll need to install the [Aspire.Azure.AI.Inference](https://www.nuget.org/packages/Aspire.Azure.AI.Inference) NuGet package to enable this integration. It also exposes an integration with [**MEAI**](working-with-llms-in-dotnet-using-microsoft-extensions-ai) using the `AddChatClient` method. This simplifies the process of interacting with LLMs in your applications.

## Building the Blog Analyzer

Now let's build something useful. We'll create a service that fetches blog posts and uses AI to categorize them.

### Fetching Blog Content

First, we need to extract readable content from blog posts:

```csharp
public async Task<string> GetBlogContentAsync(string slug)
{
    var response = await httpClient.GetAsync(slug);
    response.EnsureSuccessStatusCode();
    var htmlContent = await response.Content.ReadAsStringAsync();

    return ExtractArticleContent(htmlContent);
}
```

The `ExtractArticleContent` method (not shown) uses [HtmlAgilityPack](https://www.nuget.org/packages/htmlagilitypack/) to pull out the main article text, stripping away navigation, ads, and other page elements.

### AI-Powered Categorization

Here's where it gets interesting. We'll use the AI model to analyze the content and assign a category:

```csharp
public async Task<string> SummarizeBlogAsync(string blogContent)
{
    var prompt =
        @"""
        You are a blog content assistant. Summarize the following blog post
        as one of the following categories: Technology, Business, Programming,
        Architecture, DevOps, Cloud, Security, General.
        Only those eight values are allowed. Be as concise as possible.
        I want a 1-word response with one of these options: Technology, Business,
        Programming, Architecture, DevOps, Cloud, Security, General.

        The blog content is: {blogContent}
        """;

    var response = await chatClient.GetResponseAsync(prompt);

    if (!response.Messages.Any())
    {
        return "General";
    }

    var category = response.Messages.First().Text switch
    {
        var s when s.Contains("Technology", StringComparison.OrdinalIgnoreCase) => "Technology",
        var s when s.Contains("Business", StringComparison.OrdinalIgnoreCase) => "Business",
        var s when s.Contains("Programming", StringComparison.OrdinalIgnoreCase) => "Programming",
        var s when s.Contains("Architecture", StringComparison.OrdinalIgnoreCase) => "Architecture",
        var s when s.Contains("DevOps", StringComparison.OrdinalIgnoreCase) => "DevOps",
        var s when s.Contains("Cloud", StringComparison.OrdinalIgnoreCase) => "Cloud",
        var s when s.Contains("Security", StringComparison.OrdinalIgnoreCase) => "Security",
        var s when s.Contains("General", StringComparison.OrdinalIgnoreCase) => "General",
        _ => "General"
    };

    return category;
}
```

A few things make this work reliably:

*   The prompt is explicit about allowed categories
*   We request a single-word response to avoid parsing complex output
*   The switch expression handles variations in the AI's response
*   There's always a fallback to "General"

### Exposing the API

Finally, we wrap everything in a simple API endpoint:

```csharp
app.MapPost("/summarize-blog", async (
    string slug,
    BlogService blogService,
    BlogSummarizer blogSummarizer) =>
{
    var content = await blogService.GetBlogContentAsync(slug);
    var category = await blogSummarizer.SummarizeBlogAsync(content);

    return Results.Ok(new
    {
        slug,
        category,
        content = $"{content.Substring(0, 50)}..."
    });
});
```

Call this endpoint with a blog post URL slug, and you get back the category and a preview of the content. The dependency injection handles service resolution, and Aspire manages the AI model connection behind the scenes.

Here's an example response:

```json
{
  "slug": "screaming-architecture", // https://www.milanjovanovic.tech/blog/screaming-architecture
  "category": "Architecture",
  "content": "If you were to glance at the folder structure of y..."
}
```

Here's the distributed trace in Aspire showing the request and response flow. You can see the request to the blog service and the AI model.

![Distributed trace in Aspire showing the request and response flow using GitHub Models](/blogs/mnw_155/distributed_trace.png?imwidth=3840)

## Wrapping Up

The GitHub Models integration with .NET Aspire removes much of the complexity from adding AI to your applications. You get:

*   Simple configuration through the AppHost pattern
*   Automatic service discovery and connection management
*   Access to multiple AI models without vendor lock-in
*   The same observability and deployment benefits as other Aspire resources

Whether you're [**adding AI to an existing system**](working-with-llms-in-dotnet-using-microsoft-extensions-ai) or building something new, this integration provides the foundation you need. Start with simple categorization or summarization, then expand as you learn what works for your use case.

Thanks for reading.

And stay awesome!

---