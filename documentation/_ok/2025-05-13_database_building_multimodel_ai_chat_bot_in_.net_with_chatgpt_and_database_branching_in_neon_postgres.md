```yaml
---
title: Building Multimodel AI Chat Bot in .NET with ChatGPT and Database Branching in Neon Postgres
source: https://antondevtips.com/blog/building-multimodel-ai-chat-bot-in-dotnet-with-chat-gpt-and-database-branching-in-neon-postgres
date_published: 2025-05-13T07:45:18.368Z
date_captured: 2025-08-06T16:41:46.076Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [.NET, ASP.NET Core, ChatGPT, OpenAI API, Neon Serverless Postgres, Entity Framework Core, Nuget, React, TypeScript, Tailwind CSS, Axios, Vite, JetBrains Rider, AI Assistant, Azure Marketplace]
programming_languages: [C#, SQL, JavaScript, TypeScript]
tags: [ai, chatbot, dotnet, database-branching, postgresql, openai, web-api, frontend, data-isolation, multimodel-ai]
key_concepts: [multimodel-ai, database-branching, serverless-database, api-integration, dependency-injection, parallel-processing, data-isolation, frontend-development]
code_examples: false
difficulty_level: intermediate
summary: |
  This article demonstrates building a multimodel AI chatbot using .NET, OpenAI's ChatGPT, and Neon Serverless Postgres. It highlights how to integrate multiple AI models and compare their responses. A key feature is the use of Neon's database branching to isolate and analyze data from each AI model, similar to Git branching. The post covers backend implementation with C# and EF Core, as well as a basic React frontend for user interaction and response rating. The approach emphasizes data isolation, performance analysis, and scalability for AI-powered applications.
---
```

# Building Multimodel AI Chat Bot in .NET with ChatGPT and Database Branching in Neon Postgres

![Cover image for the article titled 'Building Multimodel AI Chat Bot in .NET with ChatGPT and Database Branching in Neon Postgres'](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fdotnet%2Fcover_neon_ai_branching.png&w=3840&q=100)

# Building Multimodel AI Chat Bot in .NET with ChatGPT and Database Branching in Neon Postgres

May 13, 2025

[Download source code](/source-code/building-multimodel-ai-chat-bot-in-dotnet-with-chat-gpt-and-database-branching-in-neon-postgres)

6 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

AI is evolving very fast in 2025. Almost every new software product is built with AI features.

Today I want to show you how you can build your own AI-powered application. We will build a multimodel AI chatbot using ChatGPT.

Here are a few things you need to consider before building such an application:

*   Using multiple AI models and comparing the prompt results
*   How to analyze the results and improve how each model performs
*   Having a list of pre-defined requirements for each model
*   Isolating data in the database for each model

As you know, I have been intensively using Neon Serverless Postgres for my projects. And I found [Neon's branching](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/neon1722366567200.neon_serverless_postgres_azure_prod?tab=overview&refcode=44WD03UH) feature a perfect match for building this AI chatbot.

I was able to create a separate branch for each AI model and play with data in isolation. It's like creating a separate branch in the GIT for developing a new feature.

The best part? You can get started with Neon and branching for free.

In this post, I will show you:

*   How to get started with [ChatGPT](https://platform.openai.com/) and get responses
*   How to set up database branching in [Neon](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/neon1722366567200.neon_serverless_postgres_azure_prod?tab=overview&refcode=44WD03UH) for multimodel prompts
*   How to implement a chatbot backend that generates answers with multiple models in ChatGPT
*   Explore a chatbot frontend application

Let's dive in.

## Getting Started with ChatGPT

First, to get started with ChatGPT you need to register an account in [OpenAI API](https://platform.openai.com/) website.

Next, you need to create an API Key:

![Screenshot of OpenAI Platform showing 'Create new secret key' dialog, prompting for a key name and permissions.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_1.png)

From April 2025, you need to charge your account by 5$ to be able to call OpenAI models via API.

When using OpenAI models via API, you consume a certain number of tokens. On the [official website](https://platform.openai.com/docs/pricing) you find more information on the pricing details for 1M tokens.

Now we can start building our application.

To get started with ChatGPT you need to install the following Nuget package for [OpenAI](https://github.com/openai/openai-dotnet):

```bash
dotnet add package OpenAI
```

First, let's set up the OpenAI client with your API key from `appsettings.json`:

```json
{
  "AiConfiguration": {
    "ApiKey": "your-api-key-here"
  }
}
```
```csharp
public record AiResponse(string Answer);

public class ChatGptService
{
    private readonly AiConfiguration _configuration;

    public ChatGptService(IOptions<AiConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    public async Task<AiResponse> RespondAsync(
        string prompt,
        string model,
        CancellationToken cancellationToken = default)
    {
        var client = new ChatClient(model: model, apiKey: _configuration.ApiKey);

        var message = new UserChatMessage(prompt);
        var response = await client.CompleteChatAsync([message],
            cancellationToken: cancellationToken);
        
        return new AiResponse(response.Value.Content[0].Text);
    }
}
```

As you can see, you can call OpenAI model with a few lines of code.

Now let's create a webapi endpoint for our chatbot:

```csharp
public static class AiModels
{
    public const string Gpt35Turbo = "gpt-3.5-turbo";
    public const string Gpt4 = "gpt-4o";
}

public record ChatResponse(Guid PromptId, string Response, string Model);

app.MapPost("/api/ai/chat", async (
    [FromBody] ChatRequest request,
    ChatGptService aiService,
    CancellationToken cancellationToken) =>
{
    var aiResponse = await aiService.RespondAsync(request.Prompt, AiModels.Gpt35Turbo,
        cancellationToken);
    
    var respone = new ChatResponse(Guid.NewGuid(), response.Answer, AiModels.Gpt35Turbo);
    return Results.Ok(respone);
})
.WithName("ChatWithAi")
.WithOpenApi();
```

You can use multiple models for OpenAI, in this application, I used `gpt-3.5-turbo` and `gpt-4o`.

For more AI use cases, refer to [official documentation](https://github.com/openai/openai-dotnet).

## Setting up Database Branching in Neon

[Neon Serverless Postgres](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/neon1722366567200.neon_serverless_postgres_azure_prod?tab=overview&refcode=44WD03UH) provides a powerful branching feature (like in the GIT) that lets you create isolated copies of your database. This is perfect for our multimodel AI chatbot as we can store and analyze responses from different models separately.

[Branching](https://neon.tech/flow?refcode=44WD03UH) allows you to:

*   Create a new branch for each model
*   Isolate data for each model
*   Run parallel tests safely
*   Wipe experiments without touching trusted data (main production branch)

Neon's branching is a Git-like fork at the storage engine level. You can create a new branch with a single CLI/API call or via the Neon console.

A new branch is created instantly (in a second), inherits schema and data from its parent branch.

You can get started with Neon for free in the [Azure Marketplace](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/neon1722366567200.neon_serverless_postgres_azure_prod?tab=overview&refcode=44WD03UH).

Select "Get It Now" and select a free tier to get started with:

![Screenshot of Neon Serverless Postgres product page on Azure Marketplace, highlighting the 'Get It Now' button.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_5.png)

![Screenshot of Azure Marketplace showing 'Neon Serverless Postgres' with a dropdown for selecting a plan, highlighting the 'Free Plan' option.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_6.png)

Enter database project details, Postgres version and click "Create":

![Screenshot of the 'Create a Neon Serverless Postgres Resource' form, showing fields for Project name, Postgres version, Database name, and Project region.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_7.png)

After our database is ready, you can navigate to Neon Console:

![Screenshot of the Neon Console dashboard, displaying a list of database projects.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_8.png)

Let's create a project with 2 branches there:

*   `gpt-3.5-turbo`
*   `gpt-4o`

> If you want to get started from zero with Neon, explore one of my [previous posts](https://antondevtips.com/blog/how-to-deploy-dotnet-application-to-azure-using-neon-postgres-and-dotnet-aspire/?utm_source=antondevtips&utm_medium=email&utm_campaign=13-05-2025).

![Screenshot of Neon Console showing the main branch and an option to create a new branch.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_2.png)

![Screenshot of Neon Console displaying two database branches: 'gpt-3.5-turbo' and 'gpt-4o'.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_3.png)

For each branch, copy a connection string from the "Connection Strings" tab. We will use it in our application.

On the free plan you can create up to 10 branches, and you can scale for production as you go.

Let's start building our chatbot.

## Building a Multimodel AI Chatbot

First, we need to create a model for our AI prompt:

```csharp
public class AiPrompt
{
    public required Guid Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Prompt { get; set; }
    public required string Model { get; set; }
    public required string Answer { get; set; }
    public required decimal? Score { get; set; }
}
```

We can use EF Core to connect to our database:

```csharp
public class AiDbContext(
    DbContextOptions<AiDbContext> options)
    : DbContext(options)
{
    public DbSet<AiPrompt> AiPrompts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DatabaseConsts.Schema);
        
        modelBuilder.ApplyConfiguration(new AiPromptConfiguration());
    }
}
```

Add connection strings to both branches in `appsettings.json`:

```json
"ConnectionStrings": {
    "PostgresGpt35": "",
    "PostgresGpt4": ""
}
```

Next, we need to create a factory for our database context to be able to switch between branches:

```csharp
public class AiDbContextFactory
{
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<AiDbContext> _dbContextFactory;

    private static readonly Dictionary<string, string> ConnectionStringMapping = new()
    {
        {AiModels.Gpt35Turbo, "PostgresGpt35"},
        {AiModels.Gpt4, "PostgresGpt4"}
    };

    public AiDbContextFactory(IConfiguration configuration,
        IDbContextFactory<AiDbContext> dbContextFactory)
    {
        _configuration = configuration;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<AiDbContext> CreateDbContextAsync(string model)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var connectionString = _configuration.GetConnectionString(ConnectionStringMapping[model]);
        dbContext.Database.SetConnectionString(connectionString);
        
        return dbContext;
    }
    
    public async Task MigrateDatabaseAsync(string model)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var connectionString = _configuration.GetConnectionString(ConnectionStringMapping[model]);
        dbContext.Database.SetConnectionString(connectionString);
        
        await dbContext.Database.MigrateAsync();
    }
}
```

After creating a `DbContext` you can set the connection string for the branch you want to use:

```csharp
var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
var connectionString = _configuration.GetConnectionString(ConnectionStringMapping[model]);
dbContext.Database.SetConnectionString(connectionString);
```

> Note: first method doesn't dispose the `DbContext` after the method is finished. Because it returns a reference to `DbContext` to the outside world; It needs to be disposed manually by the caller. Second method performs a migration on the database and disposes the `DbContext` after the method is finished.

Now, we can update our API endpoint and call both OpenAI models with the same prompt:

```csharp
app.MapPost("/api/ai/chat", async (
    [FromBody] ChatRequest request,
    AiDbContextFactory dbContextFactory,
    ChatGptService aiService,
    CancellationToken cancellationToken) =>
{
    string[] models = [AiModels.Gpt35Turbo, AiModels.Gpt4 ];
    
    var aiTasks = models.Select(model => aiService.RespondAsync(request.Prompt, model,
        cancellationToken)).ToArray();
    
    var aiResponses = await Task.WhenAll(aiTasks);
    
    var modelResponses = new List<ModelResponse>();
    
    for (var i = 0; i < models.Length; i++)
    {
        var model = models[i];
        var response = aiResponses[i];
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(model);
        
        var aiPrompt = CreateAiPrompt(model, request, response);
        dbContext.AiPrompts.Add(aiPrompt);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        modelResponses.Add(new ModelResponse(aiPrompt.Id, response.Answer, model));
    }
        
    return Results.Ok(new ChatResponse(modelResponses.ToArray()));
})
.WithName("ChatWithAi")
.WithOpenApi();
```

How it works:

*   I am calling `ChatGptService` in parallel for each model
*   Waiting for both models to complete
*   Getting `DbContext` from the factory based on the model
*   Making changes to the appropriate database branch

![Screenshot of a database table in Neon Console, likely showing 'AiPrompts' data with columns such as Id, CreatedAt, Prompt, Model, Answer, and Score.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_4.png)

## Building Chatbot Frontend

Now we came to the most interesting part of this project: building the chatbot frontend that connects everything together.

For this project, I used the following stack:

*   React
*   TypeScript
*   Tailwind CSS (CSS stylying)
*   Axios (sending HTTP request to the backend)
*   Vite (building the frontend)

I am using JetBrains Rider as my IDE and AI Assistant for daily coding.

With the latest update, [AI Assistant](https://www.jetbrains.com/ai/) can edit the code directly in the solution just like Cursor.

In 30 minutes and a few prompts I have created such an amazing UI:

![Screenshot of the chatbot frontend UI, showing an input field for typing messages and a 'Send' button.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_9.png)

![Screenshot of the chatbot frontend UI, displaying a conversation with a user prompt and two distinct responses from 'gpt-3.5-turbo' and 'gpt-4o' models.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_10.png)

Finally, after receiving the prompt result for two models, I can compare the results and decide which model is the best for my use case. I can rate each prompt, and the data will be saved in the appropriate database branch:

![Screenshot of the chatbot frontend UI, showing the ability to rate the responses from different models with a star rating system.](https://antondevtips.com/media/code_screenshots/dotnet/neon-chatgpt/img_11.png)

Of course, this is just a simple example. You can expand this example further, just play around with the code that you can download at the end of this post.

## Summary

In this blog post, we've explored how to build a multimodel AI chatbot using OpenAI's ChatGPT and Neon Serverless Postgres.

The key advantages of our approach include:

1.  **Model Comparison**: We can generate responses from multiple AI models simultaneously and compare their outputs.
2.  **Data Isolation**: Using Neon's database branching feature, we can store and analyze the responses from different models in separate environments.
3.  **Performance Analysis**: Our implementation allows us to analyze how each model performs with various prompts and improve their effectiveness.
4.  **Scalability**: Neon Serverless Postgres scales effortlessly as your application grows, and you only pay for what you use.

Get started with [Neon Serverless Postgres](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/neon1722366567200.neon_serverless_postgres_azure_prod?tab=overview&refcode=44WD03UH) for free and revolutionize your applications!

---

Disclaimer: this newsletter is sponsored by [Neon](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/neon1722366567200.neon_serverless_postgres_azure_prod?tab=overview&refcode=44WD03UH).

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/building-multimodel-ai-chat-bot-in-dotnet-with-chat-gpt-and-database-branching-in-neon-postgres)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbuilding-multimodel-ai-chat-bot-in-dotnet-with-chat-gpt-and-database-branching-in-neon-postgres&title=Building%20Multimodel%20AI%20Chat%20Bot%20in%20.NET%20with%20ChatGPT%20and%20Database%20Branching%20in%20Neon%20Postgres)[X](https://twitter.com/intent/tweet?text=Building%20Multimodel%20AI%20Chat%20Bot%20in%20.NET%20with%20ChatGPT%20and%20Database%20Branching%20in%20Neon%20Postgres&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbuilding-multimodel-ai-chat-bot-in-dotnet-with%20chat-gpt-and-database-branching-in-neon-postgres)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbuilding-multimodel-ai-chat-bot-in-dotnet-with-chat-gpt-and-database-branching-in-neon-postgres)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.