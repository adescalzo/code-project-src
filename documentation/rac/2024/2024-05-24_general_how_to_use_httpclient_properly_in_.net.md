```yaml
---
title: How to Use HttpClient Properly in .NET
source: https://okyrylchuk.dev/blog/how-to-use-httpclient-properly-in-dotnet/
date_published: 2024-05-24T09:40:48.000Z
date_captured: 2025-08-20T21:13:13.348Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [HttpClient, .NET, ASP.NET Core, Polly, Refit]
programming_languages: [C#]
tags: [httpclient, .net, networking, web-requests, dependency-injection, performance, best-practices, socket-management, dns-resolution, ihttpclientfactory]
key_concepts: [HttpClient Management, Socket Exhaustion, DNS Resolution, IHttpClientFactory, Dependency Injection, Named Clients, Typed Clients, Connection Pooling]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the proper way to use `HttpClient` in .NET applications, addressing common pitfalls like socket exhaustion and DNS resolution issues. It highlights that simply disposing `HttpClient` instances per request can lead to resource depletion. The author introduces `IHttpClientFactory`, a .NET Core 2.1 feature, as the recommended solution for managing `HttpClient` instances. The article demonstrates how to register and use `IHttpClientFactory` for basic, named, and typed clients, emphasizing its benefits for connection lifetime management and integration with libraries like Polly and Refit.]
---
```

# How to Use HttpClient Properly in .NET

# How to Use HttpClient Properly in .NET

The **HttpClient** class lets you send HTTP requests and receive HTTP responses easily.

It’s essential to note that the HttpClient class implements the IDisposable interface. This means that when you create an **HttpClient** instance, you must either use the ‘using ‘keyword for automatic disposal or manually call the Dispose method to prevent resource leaks.

```csharp
using HttpClient client = new HttpClient()
{
    BaseAddress = new Uri("http://localhost:5016")
};
```

But what if I tell you that this code is sometimes bad?

Table Of Contents

1.  [Socket Exhaustion Problem](#socket-exhaustion-problem)
2.  [DNS Problem](#dns-problem)
3.  [IHttpClientFactory](#ihttpclientfactory)
4.  [Summary](#summary)

## Socket Exhaustion Problem

As you know, for classes that implement an **IDisposable** interface, you must call the **Dispose** method. It disposes of unmanaged resources.

However, the **HttpClient** class is different. The Dispose method doesn’t release the underlying socket immediately. So, if you initiate many HttpClients, you can get a socket exhaustion problem. You can read more about it in this [blog post](https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/).

In summary, if you instantiate a new **HttpClient** for every request in your application, be aware that this can rapidly consume all available sockets, especially under high loads.

The suggestion is to create one instance of **HttpClient** for the whole application lifetime. However, it can lead to another problem.

## DNS Problem

From documentation:

HttpClient only resolves DNS entries when a connection is created. It does not track any time to live (TTL) durations specified by the DNS server. If DNS entries change regularly, which can happen in some scenarios, **the client won’t respect those updates**.

One way to solve this issue is to limit the connection’s lifetime by setting the PooledConnectionLifetime property to repeat the DNS lookup when the connection is replaced. You can follow [HttpClient Guidelines](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines) for that.

There is also another way to solve this issue.

## IHttpClientFactory

.NET Core 2.1 introduced the **IHttpClientFactory** to create **HttpClient** instances for your application.

The IHttpClientFactory solves the issues mentioned above by managing the lifetime of **HttpMessageHandler**.

Using IHttpClientFactory, you can easily pre-configure the **HttpClients** for specific services and add Polly’s policies for resiliency.

The basic usage is super simple. You need to register **IHttpClientFactory**.

```csharp
builder.Services.AddHttpClient();
```

Then, inject **IHttpClientFactory** and create a client.

```csharp
app.MapGet("/test", async (IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();

    var response = await httpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
});
```

The factory supports the following clients:

*   Named
*   Typed
*   Generated

You can register different pre-configured **HttpClients** by name. It’s helpful when you have many other services you want to communicate.

```csharp
builder.Services.AddHttpClient("MyApi", httpClient =>
    {
        httpClient.BaseAddress = new Uri("http://localhost:5016");
    });
```

When you create an **HttpClient**, you need to pass its name.

```csharp
var httpClient = httpClientFactory.CreateClient("MyApi");
```

The Typed clients are similar to Named, but you don’t need to use strings as names; you create types.

A **TypedClient** must accept **HttpClient** in the constructor.

```csharp
public class MyApiService
{
    private readonly HttpClient _httpClient;

    public MyApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5016");
    }

    public async Task<WeatherForecast[]> GetWeatherForecasts() =>
        await _httpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
}
```

You use this type to register the **HttpClient**.

```csharp
builder.Services.AddHttpClient<MyApiService>();
```

Then, you need to inject your service and use it.

```csharp
app.MapGet("/test", async (MyApiService myApiService) =>
{
    var response = await myApiService.GetWeatherForecasts();
});
```

The **IHttpClientFactory** can also be used with third-party libraries, such as **Refit**.

You can read how to [Boost Your .NET Development with Refit](/blog/boost-your-dotnet-development-with-refit/).

## Summary

The **IHttpClientFactory** is an excellent feature for managing **HttpClients** in .NET.

It solves sockets and DNS issues.

It’s a centralized place for configuring many HTTP clients.

It has many options to pre-configure your **HttpClients**, such as retry policies. You can register clients by name and type and use third-party libraries.

And it’s easy to use.