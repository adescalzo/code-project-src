```yaml
---
title: Service Lifetimes in .NET
source: https://okyrylchuk.dev/blog/service-lifetimes-in-dotnet/
date_published: 2024-03-15T19:51:11.000Z
date_captured: 2025-08-20T18:56:18.392Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET, ASP.NET Core, GitHub]
programming_languages: [C#]
tags: [dependency-injection, service-lifetimes, dotnet, aspnet-core, design-patterns, inversion-of-control, web-api, csharp, object-lifecycle]
key_concepts: [Dependency Injection, Inversion of Control, Singleton lifetime, Scoped lifetime, Transient lifetime, Primary Constructors, Service Provider, Object Lifecycle Management]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a clear explanation of Dependency Injection (DI) and Inversion of Control (IoC) within .NET applications. It thoroughly details the three primary service lifetimes—Singleton, Scoped, and Transient—using practical C# code examples in an ASP.NET Core Web API context. The post illustrates how each lifetime impacts object instantiation and provides console output to demonstrate their distinct behaviors. Additionally, it addresses a common technical interview question regarding the creation of scoped services within a singleton, offering a solution while advising against its general use.
---
```

# Service Lifetimes in .NET

# Service Lifetimes in .NET

**Dependency Injection (DI)** is a design pattern helping to achieve **Inversion of Control (IoC)** between classes and their dependencies.

In simpler terms, instead of manually creating dependencies, they are “injected” into the class from “outside.” The DI container creates the dependency for you.

To “inject” dependency by container, we need to register it first. When the container creates the object, it should know how long it should live. Different objects may have different lifetimes based on application usage patterns and requirements.

Let’s learn how to manage object lifetimes in a DI container in .NET.

## Getting Started

I created the following example as simple as possible to understand the difference between service lifetimes. It’s a simple ASP.NET Core Web API application with only one /test endpoint. The link to the source is at the end of the post.

Let’s create a simple Guid provider. The idea is to create a Guid on the object creation and only return the value.

```csharp
public interface IGuid
{
    Guid Get();
}

public class MyGuid : IGuid
{
    private readonly Guid _guid = Guid.NewGuid();

    public Guid Get() => _guid;
}
```

Let’s create two services with the Guid provider as a dependency. The services are identical; they just return a string message with the Guide provider’s value.

```csharp
public interface IService1
{
    string GetMessage();
}

public interface IService2
{
    string GetMessage();
}

public class Service1(IGuid myGuid) : IService1
{
    private readonly IGuid _guid = myGuid;

    public string GetMessage() =>
        $"Service1 has guid = {_guid.Get()}";
}

public class Service2(IGuid myGuid) : IService2
{
    private readonly IGuid _guid = myGuid;

    public string GetMessage() =>
        $"Service2 has guid = {_guid.Get()}";
}
```

I used a new C# 12 feature in the implementation classes, **Primary Constructors**.

I intentionally created two identical services to show the difference between service lifetimes. Of course, it is usually one service.
Now, let’s set up a testing endpoint with injected services.

```csharp
app.MapGet("/test", (IService1 service1, IService2 service2) =>
{
    Console.WriteLine($"Request number: {RequestNumber.Get()}");
    Console.WriteLine(service1.GetMessage());
    Console.WriteLine(service2.GetMessage());
    Console.WriteLine();
});
```

Every request displays the messages from services alongside the number of requests.

Finally, we need to register our services in the DI container.

```csharp
builder.Services.AddScoped<IService1, Service1>();
builder.Services.AddScoped<IService2, Service2>();
```

We’re ready! Almost.

## Singleton

Almost, because we also need to register a Guid provider. Let’s register it as **Singleton**.

```csharp
builder.Services.AddSingleton<IGuid, MyGuid>();
```

**Singleton** lifetime service is created **once** the first time it’s requested. All dependents will get the same instance of the service.

Let’s see it in action. I requested /tested the endpoint several times.

![Console output showing consistent GUIDs for Singleton lifetime across multiple requests.](https://example.com/singleton-output.png)

You can see that both services in all requests have the same Guid. It means that there is only one instance of MyGuid service. And it’s injected into all dependents.

## Scoped

**Scoped** lifetime service is created once **within scope**. The scope might be different for different application types.

```csharp
builder.Services.AddScoped<IGuid, MyGuid>();
```

For web applications, the scope is created for each request by default. This means that a new service instance will be created for each request. In our example, Service1, Service2, and Guid provider are always fresh instances for each request.

![Console output showing different GUIDs per request but consistent within a request for Scoped lifetime.](https://example.com/scoped-output.png)

Each request displays different Guids, but they have the same value per request.

## Transient

**Transient** lifetime service is created **every time** it’s requested.

```csharp
builder.Services.AddTransient<IGuid, MyGuid>();
```

The easiest way to understand it is to look at console output.

![Console output showing different GUIDs for each service and request for Transient lifetime.](https://example.com/transient-output.png)

You can see that all Guid values are different for each Service and request. Service1 gets one instance of Guid provider, and Service2 gets the second new instance.

## Bonus: Interview Question

At the technical interview, you might be asked about Service Lifetimes in .NET. After answering the difference, the follow-up questions might be about the **Scoped** lifetime.

1.  Can we manage the scope?
2.  Can we create a **Scoped** service in the **Singleton**?

For both questions, the answer is **yes**.

Because we can define the scope, we can create a scoped service in the **Singleton**.

**But it’s not recommended to do that!**

The questions aim to verify how deeply you understand Service Lifetimes.

The following implementation shows how to create a new scope.

```csharp
public interface ISingletonService
{
    string GetMessage();
}

public class SingletonService(IServiceProvider serviceProvider)
    : ISingletonService
{
    private readonly IServiceProvider _serviceProvider
        = serviceProvider;

    public string GetMessage()
    {
        using var scope = _serviceProvider.CreateScope();
        var _guid = scope.ServiceProvider.GetRequiredService<IGuid>();

        return $"SingletonService has guid = {_guid.Get()}";
    }
}
```

The SingletonService is registered as Singleton. However, every time the GetMessage method is called, it gets a new instance of Guid provider.

![Console output demonstrating a new GUID instance for a Scoped service resolved within a Singleton.](https://example.com/singleton-scoped-output.png)

I hope these examples helped you understand the difference between Service Lifetimes.

You can find the source code on my [GitHub](https://github.com/okyrylchuk/dotnet-newsletter/tree/main/ServiceLifetimes "GitHub").