```yaml
---
title: Setting Up OpenAI Chat in a .NET API - by Muhammad Waseem
source: https://mwaseemzakir.substack.com/p/setting-up-openai-chat-in-a-net-api?utm_source=post-email-title&publication_id=1232416&post_id=151058559&utm_campaign=email-post-title&isFreemail=true&r=a97lu&triedRedirect=true
date_published: 2024-11-02T06:14:04.000Z
date_captured: 2025-08-06T18:25:59.875Z
domain: mwaseemzakir.substack.com
author: Muhammad Waseem
category: backend
technologies: [OpenAI, .NET, NuGet]
programming_languages: [C#]
tags: [openai, dotnet, api, ai, chat, nuget]
key_concepts: [api-integration, api-key-management, error-handling, best-practices, asynchronous-programming]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a concise guide on integrating OpenAI's chat capabilities into a .NET API. It outlines three simple steps: obtaining an API key, installing the necessary OpenAI NuGet package, and configuring the C# code to interact with the OpenAI API. The post also details available methods and properties of the `ChatClient` and `ChatCompletion` objects. Furthermore, it offers best practices for improving the initial code, such as using enums for models, securing API keys, implementing dedicated services, and robust error handling.
---
```

# Setting Up OpenAI Chat in a .NET API - by Muhammad Waseem

# Setting Up OpenAI Chat in a .NET API

### Read Time : 2 Mins

AI is everywhere!

Let’s walk through three simple steps to add it to your Web App or Web API :

### Get your API Key

Retrieve your API key from the [OpenAI website](https://platform.openai.com/). Make sure you have already created the account.

### **Install the Required NuGet Package**

Add the OpenAI NuGet package to your .NET API project :

```bash
dotnet add package OpenAI
```

### **Configure Your Code**

Set up your code to initialize and use the OpenAI API, this is the sample code

```csharp
public async Task<string> Chat(string query)
{
    ChatClient client = new(model: "gpt-4o", apiKey: API_KEY);
    ChatCompletion completion = await client.CompleteChatAsync(query);
    string response = completion.Content[0].Text;
    return response;
}
```

This code's purpose is to demonstrate how we can connect the Open AI, we will see in the next sections how we can improve this code.

### Available Methods and Properties

`ChatClient` comes from the `OpenAI.Chat` and the `ChatClient` constructor can take:

1.  Model and ApiKey
2.  Model and ApiKeyCredential
3.  Model, ApiKeyCredential and OpenAIClientOptions

It has the following methods:

```csharp
Task<ClientResult<ChatCompletion>>CompleteChatAsync(
   IEnumerable<ChatMessage> messages,
   ChatCompletionOptions options = null,
   CancellationToken cancellationToken = default);
```
```csharp
ClientResult<ChatCompletion> CompleteChat(
   IEnumerable<ChatMessage> messages,
   ChatCompletionOptions options = null,
   CancellationToken cancellationToken = default);
```
```csharp
Task<ClientResult<ChatCompletion>> CompleteChatAsync(
params ChatMessage[] messages);

ClientResult<ChatCompletion> CompleteChat(params ChatMessage[] messages);
```

Each method has its Async version as well we can use the one according to our need.

There is an additional asynchronous method called `CompleteChatStreaming`. Unlike `CompleteChatAsync`, this method streams the completion back token by token as it is generated.

`ChatCompletion` contains `Id`, `Model`, `SystemFingerprint`, `Role`, `Content` , `ToolCalls` and `Usage` details.

Listing every detail isn’t feasible, but our response is found within the `Content` object. This object includes `Text`, `Refusal`, and additional properties related to images if the response contains one.

### Best Practices

Let’s improve this code :

```csharp
public async Task<string> Chat(string query)
{
    ChatClient client = new(model: "gpt-4o", apiKey: API_KEY);
    ChatCompletion completion = await client.CompleteChatAsync(query);
    string response = completion.Content[0].Text;
    return response;
}
```

1.  Create an `Enum` to define the available models rather than passing the model as a string.
2.  Retrieve the `API_Key` from app settings or a secure vault.
3.  Implement a dedicated service for OpenAI features to ensure reusability.
4.  Provide separate methods for asynchronous and synchronous calls.
5.  Perform `NULL` checks to validate the query.
6.  Use `try-catch` blocks to handle exceptions.
7.  Refer to OpenAI's documentation to display detailed error messages for each error code :

### Common Errors in Package Configuration

While setting up the library you can get this error :

> The model `gpt-4o` does not exist or you do not have access to it.

To resolve this you must have a five USS balance in your Open API account.