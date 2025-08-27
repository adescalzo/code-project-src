```yaml
---
title: Minimal APIs in .NET 8
source: https://okyrylchuk.dev/blog/minimal-apis-in-dotnet-8/
date_published: 2024-01-19T09:32:12.000Z
date_captured: 2025-08-20T18:56:15.220Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: backend
technologies: [.NET 8, ASP.NET Core, Minimal APIs, Razor Slices, .NET 6]
programming_languages: [C#, HTML]
tags: [minimal-apis, dotnet, aspnet-core, web-api, security, forms, object-pooling, performance, web-development]
key_concepts: [Antiforgery tokens, Form binding, File uploads, Object pooling, IResettable interface, Dependency Injection, HTTP services, Cross-site request forgery (CSRF)]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores new features in Minimal APIs introduced in .NET 8. It covers the implementation and importance of antiforgery tokens for mitigating cross-site request forgery attacks when handling forms. The post also demonstrates how .NET 8 enhances form binding, allowing direct binding of form values and complex types, a significant improvement over previous versions. Finally, it introduces the `IResettable` interface and its role in optimizing object reuse within ASP.NET Core's object pooling mechanism. The content includes practical C# and HTML code examples to illustrate these concepts.
---
```

# Minimal APIs in .NET 8

# Minimal APIs in .NET 8

.NET 6 introduced the **Minimal APIs** feature. It simplifies building lightweight HTTP services with minimal code and ceremony. The idea is to reduce the boilerplate code needed to create a basic web API or service, making it more approachable for small applications and microservices. Each subsequent version of .NET improves minimal APIs. Let’s see what .NET 8 brings.

Table Of Contents

1.  [Antiforgery Tokens](#antiforgery-tokens)
2.  [Binding to Forms](#binding-to-forms)
3.  [IResettable Interface](#iresettable-interface)
4.  [Summary](#summary)

## Antiforgery Tokens

.NET 8 introduces a middleware for validating antiforgery tokens. They are used to mitigate cross-site request forgery attacks.

You can register antiforgery services in a dependency injection container by calling the **AddAntiforgery()** method. Then, call the **UseAntiForgery()** method in **IApplicationBuilder** to add the anti-forgery middleware to the pipeline.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseAntiforgery();

app.MapGet("/", () =>
    Results.Extensions.RazorSlice("Slices/Home.cshtml"));

app.Run();
```

This feature is not accidental. You need antiforgery tokens if you want to use Forms. Previously, you could use Forms only for sending files in Minimal APIs.

.NET 8 extends explicit binding to Form values using the `[FromForm]` attribute. Let’s directly jump into it.

## Binding to Forms

As I mentioned, you could use Forms in Minimal APIs to upload files. Let’s refresh our minds on how we can do it.

As Minimal APIs don’t offer any built-in tooling for working with HTML, I use [Razor Slices](https://github.com/DamianEdwards/RazorSlices "Razor Slices"), a great library for returning HTML from Minimal APIs.

You could notice the line in the first sample.

`app.MapGet("/", () => Results.Extensions.RazorSlice("/Slices/Home.cshtml"));`

Like with Razor, you can create **cshtml** pages and return rendered HTML from Minimal APIs.

Let’s create a simple Form for uploading a file.

```html
<form action="/upload-file" method="POST" enctype="multipart/form-data">
    <input type="file" name="file" />
    <input type="submit" />
</form>
```

The Minimal API implementation.

```csharp
app.MapPost("/upload-file", async ([FromForm] IFormFile file) =>
{
    string fileName = Path.GetRandomFileName();
    await using var stream = File.Open(fileName, FileMode.Create);
    await file.CopyToAsync(stream);
});
```

You could add details to the Form with files.

```html
<form action="/upload-file" method="POST" enctype="multipart/form-data">
    <input type="file" name="file" />
    <input type="text" name="description" />
    <input type="submit" />
</form>
```

However, getting values from Form could be more convenient.

```csharp
app.MapPost("/upload-file", async ([FromForm] IFormFile file, HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var description = form["description"];
});
```

Let’s create a standard Form without files.

```html
<form action="/submit-form" method="POST">
    <input type="text" name="name" />
    <input type="text" name="description" />
    <input type="submit" />
</form>
```

And the corresponding endpoint implementation.

```csharp
app.MapPost("/submit-form", async ([FromForm] string name, [FromForm] string description) =>
{
});
```

This implementation doesn’t work in .NET 6 and 7. You get the error.

**NotSupportedException: IFromFormMetadata is only supported for parameters of type IFormFileCollection and IFormFile.**

In .NET 8, binding to Form values is now supported. The code above works. However, you need to add one missing piece. Remember about antiforgery tokens? Without it, you get an error.

**AntiforgeryValidationException: The required antiforgery cookie “.AspNetCore.Antiforgery.QYcNn7wwpwA” is not present.**

Let’s get back to our home endpoint and add it.

```csharp
app.MapGet("/", (HttpContext context, IAntiforgery antiforgery) =>
    Results.Extensions.RazorSlice(
        "Slices/Home.cshtml", 
        antiforgery.GetAndStoreTokens(context)));
```

And set the antiforgery token in our Form.

```html
<form action="/submit-form" method="POST">
    <input name="@Model.ForFieldName" type="hidden" value="@Model.RequestToken" />
    <input type="text" name="name" />
    <input type="text" name="description" />
    <input type="submit" />
</form>
```

Now everything works perfectly. 

You can disable the antiforgery token validation on the endpoint by calling **DisableAntiforgery()**. However, it’s not recommended.

```csharp
app.MapPost("/submit-form", async ([FromForm] string name, [FromForm] string description) =>
{
})
.DisableAntiforgery();
```

Minimal APIs in .NET 8 also support binding complex types.

```csharp
app.MapPost("/submit-form", async ([FromForm] Person person) =>
{
    // do something with person
});

public record Person(string Name, string Surname);
```

## IResettable Interface

First, we need to understand ObjectPool in ASP.NET Core. It’s part of the ASP.NET Core infrastructure that supports keeping objects in memory for reuse rather than allowing them to be garbage collected.

ASP.NET Core has had this feature from the beginning. However, the IResettable interface is new in .NET 8. It allows the object to be automatically reset when returned to an object pool.

Let’s see how we can add an object pool to our application.

```csharp
builder.Services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
builder.Services.TryAddSingleton(serviceProvider =>
{
    var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
    var policy = new DefaultPooledObjectPolicy<ReusableBuffer>();
    return provider.Create(policy);
});
```

That’s how you register ObjectPool in the application. In the first line, you register the **DefaultObjectPoolProvider** as **ObjectPoolProvider**. Then, you register your **ObjectPool**. In my case, it’s for the **ReusableBuffer** type, which implementation you can see below.

```csharp
public class ReusableBuffer : IResettable
{
    public byte[] Data { get; } = new byte[1024];

    public bool TryReset()
    {
        Array.Clear(Data);
        return true;
    }
}
```

Notice that the **ReusableBuffer** implements the **IResettable** interface. It has one method **TryReset()**, which resets the object to a neutral state.

Let’s see how we can use the object pool in our application.

```csharp
app.MapPost("/buffer", (ObjectPool<ReusableBuffer> bufferPool) =>
{
    ReusableBuffer buffer = bufferPool.Get();

    // do something with buffer

    // here TryReset is called to reset the buffer ot its initial state
    // because this type implemens IResettable interface
    bufferPool.Return(buffer);
});
```

## Summary

In this post, I showed you three new features in Minimal APIs in .NET 8. The Antiforgery tokens are essential for security reasons when using POST Forms. And now, you can bind Form values in Minimal APIs. The IResettable interface can make your life easier when you use ObjectPools in your application.