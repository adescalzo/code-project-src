```yaml
---
title: "Why Use HttpClientFactory. In ASP.NET Core, using… | by Engr. Md. Hasan Monsur | ASP DOTNET | Medium"
source: https://medium.com/asp-dotnet/why-use-httpclientfactory-1fa857db78de
date_published: 2024-09-13T16:25:06.196Z
date_captured: 2025-08-27T10:53:38.261Z
domain: medium.com
author: Engr. Md. Hasan Monsur
category: general
technologies: [ASP.NET Core, HttpClientFactory, HttpClient, Polly, .NET, Microsoft.Extensions.Http.Polly, Moq, WireMock]
programming_languages: [C#]
tags: [httpclientfactory, aspnet-core, http-client, network, resilience, polly, dependency-injection, dotnet, socket-exhaustion, web-development]
key_concepts: [HttpClientFactory, socket exhaustion, DNS staleness, connection pooling, dependency injection, resilience policies, typed clients, circuit breaker]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains why HttpClientFactory is the recommended approach for managing HttpClient instances in ASP.NET Core applications. It addresses common issues like socket exhaustion and DNS staleness that arise from manually creating and disposing HttpClient. The post details how to set up HttpClientFactory, including using named and typed clients for better organization and testability. Furthermore, it demonstrates integrating HttpClientFactory with Polly for implementing resilience policies such as retries and circuit breakers. Adopting HttpClientFactory improves application performance, reliability, and maintainability for making HTTP calls.]
---
```

# Why Use HttpClientFactory. In ASP.NET Core, using… | by Engr. Md. Hasan Monsur | ASP DOTNET | Medium

# Why Use HttpClientFactory

Engr. Md. Hasan Monsur

_In ASP.NET Core, using_ **_HttpClientFactory_** _is the recommended approach for creating and managing_ `_HttpClient_` _instances. It avoids common issues like_ **_socket exhaustion_** _and improves the overall performance and reliability of HTTP calls._

![Diagram illustrating HttpClientFactory's role in handling requests and responses, with the author's profile and text "FAVOR COMPOSITION OVER INHERITANCE".](https://miro.medium.com/v2/resize:fit:700/1*_Eip0KT0RcjEJiK2aR4WOw.png)

**Let's explore why** `**HttpClientFactory**` **is beneficial, how to use it, and some best practices.**

### Why Avoid Creating `HttpClient` Manually?

In .NET, creating an instance of `HttpClient` manually for every request, like below, can cause problems:

```csharp
public async Task<string> GetDataAsync()  
{  
    using var client = new HttpClient();  
    var response = await client.GetAsync("https://api.example.com/data");  
    return await response.Content.ReadAsStringAsync();  
}
```

### Problems with the Above Approach:

1.  **Socket Exhaustion**: `HttpClient` instances use a connection pool under the hood. When you create a new `HttpClient` for each request and immediately dispose of it, the underlying socket connections don’t close immediately, leading to socket exhaustion under high load.
2.  **DNS Staleness**: `HttpClient` instances cache DNS entries, and if you reuse the same instance for a long period, DNS records can become stale, potentially causing requests to fail if the endpoint’s IP address changes.
3.  **Performance**: Frequent instantiation and disposal of `HttpClient` creates unnecessary overhead.

### Why Use `HttpClientFactory`?

`HttpClientFactory`, introduced in ASP.NET Core 2.1, provides a centralized way to configure and manage `HttpClient` instances. It offers:

*   **Pooling and Reuse**: Managed reuse of `HttpClientMessageHandler` objects to avoid socket exhaustion.
*   **Configuration and Customization**: Centralized configuration of `HttpClient` for different endpoints.
*   **Resilience Policies**: Integration with Polly for retries, circuit breakers, and more.

### Setting Up `HttpClientFactory` in ASP.NET Core

1.  **Register** `**HttpClientFactory**` **in the** `**Startup.cs**` **or** `**Program.cs**` **file:**

In an ASP.NET Core application, you register `HttpClientFactory` in the `ConfigureServices` method.

```csharp
public void ConfigureServices(IServiceCollection services)  
{  
    // Register the HttpClientFactory  
    services.AddHttpClient();  
  
    // Optionally, configure specific HttpClient instances for different APIs  
    services.AddHttpClient("MyApi", client =>  
    {  
        client.BaseAddress = new Uri("https://api.example.com");  
        client.DefaultRequestHeaders.Add("Accept", "application/json");  
    });  
}
```

This registers `HttpClientFactory` and allows for creating named `HttpClient` instances like `"MyApi"`.

1.  **Use** `**HttpClientFactory**` **in Controllers or Services:**

Inject `IHttpClientFactory` into your controllers, services, or other classes and use it to create `HttpClient` instances.

```csharp
public class MyApiService  
{  
    private readonly IHttpClientFactory _httpClientFactory;  
  
    public MyApiService(IHttpClientFactory httpClientFactory)  
    {  
        _httpClientFactory = httpClientFactory;  
    }  
  
    public async Task<string> GetDataAsync()  
    {  
        // Create a new HttpClient instance from the factory  
        var client = _httpClientFactory.CreateClient("MyApi");  
  
        var response = await client.GetAsync("/data");  
        response.EnsureSuccessStatusCode();  
  
        return await response.Content.ReadAsStringAsync();  
    }  
}
```

By calling `_httpClientFactory.CreateClient("MyApi")`, you get a properly configured `HttpClient` instance that will avoid socket exhaustion and benefit from pooled handlers.

## Typed Clients

For better organization and separation of concerns, you can use **Typed Clients**, which encapsulate `HttpClient` logic into strongly-typed classes.

Step 1: Define a Typed Client:

```csharp
public class MyApiClient  
{  
    private readonly HttpClient _httpClient;  
  
    public MyApiClient(HttpClient httpClient)  
    {  
        _httpClient = httpClient;  
    }  
  
    public async Task<string> GetDataAsync()  
    {  
        var response = await _httpClient.GetAsync("/data");  
        response.EnsureSuccessStatusCode();  
  
        return await response.Content.ReadAsStringAsync();  
    }  
}
```

Step 2: Register the Typed Client:

```csharp
public void ConfigureServices(IServiceCollection services)  
{  
    services.AddHttpClient<