```yaml
---
title: Manage API Versioning Like A Pro in ASP.NET Core
source: https://okyrylchuk.dev/blog/manage-api-versioning-like-a-pro-in-asp-net-core/
date_published: 2025-04-11T16:04:22.000Z
date_captured: 2025-08-11T16:15:01.058Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: backend
technologies: [ASP.NET Core, ASP.NET API Versioning, .NET, Minimal API, ASP.NET Web API, ASP.NET Core MVC, OData v4.0, OpenAPI, Kestrel]
programming_languages: [C#]
tags: [api-versioning, aspnet-core, web-api, dotnet, minimal-api, rest-api, backend, development, api-design, compatibility]
key_concepts: [API versioning, backward-compatibility, version-readers, query-string-versioning, url-segment-versioning, header-versioning, media-type-versioning, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the critical role of API versioning in ASP.NET Core applications, emphasizing its importance for maintaining backward compatibility as APIs evolve. It introduces the official ASP.NET API Versioning library, detailing its support for various ASP.NET frameworks including Minimal API, Web API, and MVC. The post provides practical C# code examples to demonstrate initial setup, defining API versions, and associating them with endpoints. Furthermore, it explains different versioning strategies such as query parameters, URL segments, HTTP headers, and media types, along with advanced configuration options for default and deprecated versions. The author concludes by advocating for explicit API versioning to ensure robust and maintainable public APIs and microservices.]
---
```

# Manage API Versioning Like A Pro in ASP.NET Core

# Manage API Versioning Like A Pro in ASP.NET Core

By Oleg Kyrylchuk / April 11, 2025

Tired of the endless build-restart cycle just to tweak UI, or battling the gap between static mockups and your actual application data? **[Enter Hot Design](https://go.okyrylchuk.dev/unotplatform2)** – a Visual Designer that uses your live data. Modify layouts, see how components react to real data scenarios, and adjust bindings instantly — all without breaking your focus. Stay productive, stay in your IDE, and ship beautiful, data-robust cross-platform .NET apps fast.

## **Why API Versioning Matters and When to Use It**

As your API evolves, you’ll likely introduce changes that could break existing clients — like renaming fields or changing response formats. API versioning helps you avoid this by allowing you to support multiple versions of your API side by side. It ensures backward compatibility and will enable clients to upgrade at their own pace.

Versioning is especially important when external apps, front-ends, or third-party integrations use your API. It also makes your API easier to maintain, debug, and document over time.

When should you implement API versioning? The short answer is that as soon as your API is consumed by more than just yourself.

Also, explicit API versioning is a must, according to [Microsoft REST API Guidelines](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning).

## **ASP.NET API Versioning**

[ASP.NET API Versioning](https://github.com/dotnet/aspnet-api-versioning) is an excellent project that offers packages for various ASP.NET frameworks, including:

*   ASP.NET Core Minimal API
*   ASP.NET Web API
*   ASP.NET Core MVC

Additionally, it supports OData v4.0 and provides API Explorers for all project types, allowing you to document your REST APIs using OpenAPI.

## **Getting Started**

Let’s see how easy and fast we can configure API versioning. I’ll show you how to do that for Minimal API, but it’s very similar for other project types.

First, install the **Asp.Versioning.Http** package:

```
dotnet add package Asp.Versioning.Http
```

The initial configuration looks as follows:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiVersioning();

var app = builder.Build();

ApiVersionSet apiVersionsSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .HasApiVersion(new ApiVersion(2))
    .Build();

app.MapGet("/todo", () =>
{
    return "Version 1";
})
.WithApiVersionSet(apiVersionsSet)
.HasApiVersion(1);

app.MapGet("/todo", () =>
{
    return "Version 2";
})
.WithApiVersionSet(apiVersionsSet)
.HasApiVersion(2);

app.Run();
```

Let’s break down what this code does:

*   _builder.Services.AddApiVersioning()_ registers the **API versioning system** in the DI container.
*   _app.NewApiVersionSet()_ creates a reusable **version set**, which groups multiple API versions under a shared identity.
*   _.HasApiVersion(new ApiVersion(1))_ and _.HasApiVersion(new ApiVersion(2))_ declare that this set supports **v1** and **v2**.
*   Each endpoint is associated with the previously defined _apiVersionsSet_. It declares that this endpoint belongs to **version 1** _using .HasApiVersion(1)_.

And that’s it. Let’s send the request:

![Screenshot showing a request to `/todo?api-version=1` returning "Version 1" in the response body. The query parameter `api-version=1` is highlighted.](https://okyrylchuk.dev/wp-content/uploads/2025/04/request1-png.avif "request1")

## **Where to Specify a Version?**

As you can see in the example above, the version is passed through the query parameter. This is the default behavior, but you can change it.

The **Asp.Versioning.Http** supports a few version readers:

*   **UrlSegmentApiVersionReader** – the version in the endpoint route like /v1/todo.
*   **QueryStringApiVersionReader** – the default version in the query params like /todo?api-version=1
*   **HeaderApiVersionReader** – the version in the HTTP headers like /todo -H ‘Api-Version: 1’
*   **MediaTypeApiVersionReader** – the version in the media type like /todo – H ‘ContentType: application/json;v=1’

You can set the version reader using the **ApiVersionReader** option:

```csharp
builder.Services.AddApiVersioning(options => 
{
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
```

When you want to use the **QueryStringApiVersionReader**, you must also put a version in the endpoint routes:

```csharp
app.MapGet("v{version:apiVersion}/todo", () =>
{
    return "Version 1";
})
.WithApiVersionSet(apiVersionsSet)
.HasApiVersion(1);
```

Then, we can use the version in the endpoint route:

![Screenshot showing a request to `/v2/todo` returning "Version 2" in the response body. The URL segment `/v2` is highlighted.](https://okyrylchuk.dev/wp-content/uploads/2025/04/routeversion-png.avif "routeversion")

All version readers implement the **IApiVersionReader** interface, so you can implement it if you need a custom version reader.

## **Other Options**

The API Versioning libraries are very flexible. Let’s see some useful options:

```csharp
builder.Services.AddApiVersioning(options => 
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
```

You can specify a default version with a **DefaultApiVersion** option.

Having the default version, you use it when no version is sent with an **AssumeDefaultVersionWhenUnspecified** option.

Also, the **ReportApiVersions** option is helpful. When it’s enabled, the response headers contain information about supported and deprecated versions:

![Screenshot of HTTP response headers, showing `api-supported-versions: 1, 2` and `api-deprecated-versions: 1` highlighted.](https://okyrylchuk.dev/wp-content/uploads/2025/04/headers-png.avif "headers")

You can mark the version as deprecated with the **HasDeprecatedApiVersion** method for ApiVersionSet or a single endpoint:

```csharp
ApiVersionSet apiVersionsSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .HasApiVersion(new ApiVersion(2))
    .HasDeprecatedApiVersion(new ApiVersion(1))
    .Build();
```

## **Conclusion**

If you’re building public APIs or evolving microservices, versioning is necessary.

ASP.NET API Versioning is a great project that allows you to manage API versioning easily. It’s open-sourced but was absorbed into the .NET organization.

With just a few configuration lines, you can support multiple versions side by side, confidently introduce changes, and give consumers the flexibility to upgrade on their timeline.