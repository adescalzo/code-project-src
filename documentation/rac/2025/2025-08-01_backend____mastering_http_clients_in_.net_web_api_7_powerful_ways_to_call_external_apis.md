```yaml
---
title: 🔗 Mastering HTTP Clients in .NET Web API 7 Powerful Ways to Call External APIs
source: https://newsletter.kanaiyakatarmal.com/p/mastering-http-clients-in-net-web
date_published: 2025-08-01T04:31:22.000Z
date_captured: 2025-09-09T14:55:32.846Z
domain: newsletter.kanaiyakatarmal.com
author: Kanaiya Katarmal
category: backend
technologies: [.NET, ASP.NET Core, HttpClient, IHttpClientFactory, Refit, Polly, RestSharp]
programming_languages: [C#]
tags: [http-client, web-api, dotnet, external-api, microservices, resilience, dependency-injection, rest-client, best-practices, api-integration]
key_concepts: [socket-exhaustion, dependency-injection, named-clients, typed-clients, declarative-http, retry-policy, circuit-breaker, http-resilience]
code_examples: false
difficulty_level: intermediate
summary: |
  The article details seven effective methods for making HTTP calls in .NET Web API, focusing on efficient and scalable API integration. It begins by highlighting the pitfalls of direct `HttpClient` instantiation and then introduces `IHttpClientFactory` as a recommended solution, along with Named and Typed Clients for structured API consumption. The guide further explores advanced libraries like Refit for declarative HTTP calls, Polly for implementing resilience patterns such as retries, and RestSharp for handling complex request payloads. It concludes by recommending a combination of `IHttpClientFactory`, Typed Clients, and Polly for most production applications, while suggesting Refit for speed and simplicity.
---
```

# 🔗 Mastering HTTP Clients in .NET Web API 7 Powerful Ways to Call External APIs

# 🔗 Mastering HTTP Clients in .NET Web API: 7 Powerful Ways to Call External APIs

Calling external APIs is a common need in almost every .NET Web API project. Whether you're integrating with payment gateways, external services, or microservices — you need an efficient, scalable, and testable way to perform HTTP operations.

In this article, we’ll cover **seven powerful ways** to make HTTP calls in .NET using best practices like `IHttpClientFactory`, `Typed Clients`, `Refit`, `Polly`, and more — with complete working examples. 🚀

## 1️⃣ Using `HttpClient` Directly (NOT Recommended for Production)

This is the most basic way to make HTTP requests. However, repeated use of `new HttpClient()` can lead to **socket exhaustion** due to improper connection reuse.

```csharp
public class RawHttpClientService
{
    public async Task<string> GetData()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts");
        return await response.Content.ReadAsStringAsync();
    }
}
```

✅ Good for: quick scripts, console tools  
❌ Avoid in production web apps

## 2️⃣ Using `IHttpClientFactory` (Recommended)

Introduced in ASP.NET Core 2.1, `IHttpClientFactory` solves `HttpClient` lifecycle issues and integrates smoothly with dependency injection.

**Register:**

```csharp
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClientFactoryService>();
```

**Use:**

```csharp
public class HttpClientFactoryService
{
    private readonly HttpClient _client;

    public HttpClientFactoryService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient();
    }

    public async Task<string> GetData()
    {
        var response = await _client.GetAsync("https://jsonplaceholder.typicode.com/posts");
        return await response.Content.ReadAsStringAsync();
    }
}
```

✅ Recommended for most production scenarios

## 3️⃣ Named Clients

Named clients allow you to register and configure multiple `HttpClient` instances with different base URLs and settings.

**Register:**

```csharp
builder.Services.AddHttpClient("JsonClient", client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});
```

**Use:**

```csharp
public class NamedClientService
{
    private readonly HttpClient _client;

    public NamedClientService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("JsonClient");
    }

    public async Task<string> GetPosts()
    {
        return await _client.GetStringAsync("posts");
    }
}
```

✅ Ideal for apps calling multiple external services

## 4️⃣ Typed Clients

Typed clients provide a clean, testable abstraction over `HttpClient`. These are strongly typed and fully integrated with DI.

**Service:**

```csharp
public class TypedClientService
{
    private readonly HttpClient _httpClient;

    public TypedClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetPosts()
    {
        return await _httpClient.GetStringAsync("posts");
    }
}
```

**Register**:

```csharp
builder.Services.AddHttpClient<TypedClientService>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});
```

✅ Recommended for clean architecture and SOLID principles

## 5️⃣ Refit – Declarative HTTP Calls

[Refit](https://github.com/reactiveui/refit) is a REST client for .NET inspired by Retrofit (Java). It allows you to define your API interface and Refit generates the implementation.

**Install:**

```bash
dotnet add package Refit
```

**Define Interface:**

```csharp
public interface IJsonPlaceholderApi
{
    [Get("/posts")]
    Task<List<Post>> GetPostsAsync();
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
}
```

**Register:**

```csharp
builder.Services.AddRefitClient<IJsonPlaceholderApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com"));
```

**Use:**

```csharp
public class RefitService
{
    private readonly IJsonPlaceholderApi _api;

    public RefitService(IJsonPlaceholderApi api)
    {
        _api = api;
    }

    public async Task<List<Post>> GetPosts()
    {
        return await _api.GetPostsAsync();
    }
}
```

✅ Super fast for API consumption  
✅ Great for shared SDKs or microservices

## 6️⃣ Polly — Retry & Resilience for Free

Polly adds resilience features like retry, circuit breaker, and timeout.

**Register:**

```csharp
builder.Services.AddHttpClient("RetryClient")
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .RetryAsync(3));
```

**Use:**

```csharp
public class RetryService
{
    private readonly HttpClient _httpClient;

    public RetryService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("RetryClient");
    }

    public async Task<string> GetWithRetry()
    {
        var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts");
        return await response.Content.ReadAsStringAsync();
    }
}
```

✅ Helps avoid failures from flaky networks  
✅ Combines well with `IHttpClientFactory`

## 7️⃣ RestSharp – For Complex Payloads

RestSharp is another library for making RESTful requests. Offers a fluent API and better multipart/form support.

**Install:**

```bash
dotnet add package RestSharp
```

**Use**:

```csharp
public class RestSharpService
{
    public async Task<string> GetData()
    {
        var client = new RestClient("https://jsonplaceholder.typicode.com");
        var request = new RestRequest("posts", Method.Get);
        var response = await client.ExecuteAsync(request);
        return response.Content!;
    }
}
```

✅ Great for uploading files, working with custom headers  
❌ Extra dependency; avoid unless needed

## 💬 Conclusion

.NET gives you **flexible and powerful options** for making HTTP calls — but not all are created equal. For most apps, using `IHttpClientFactory` with **Typed Clients** and **Polly** is the sweet spot. Use Refit when you want speed and simplicity.

When working with APIs in .NET, `HttpClient` is the go-to utility for making HTTP calls. But what if you want to log requests and responses? Or add custom headers like correlation IDs or tokens? That’s where `DelegatingHandler` comes in — a powerful but often underutilized tool in .NET.

---

I hope you found this guide helpful and informative.