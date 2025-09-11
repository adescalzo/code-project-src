```yaml
---
title: "Minimal APIs in .NET 10: Now with OpenAPI 3.1 Support | by Adem KORKMAZ | Sep, 2025 | JavaScript in Plain English"
source: https://javascript.plainenglish.io/minimal-apis-in-net-10-now-with-openapi-3-1-support-6e33f0a79eb9
date_published: 2025-09-06T00:10:19.620Z
date_captured: 2025-09-10T12:47:19.758Z
domain: javascript.plainenglish.io
author: Adem KORKMAZ
category: backend
technologies: [.NET 10, Minimal APIs, ASP.NET Core, OpenAPI 3.1, Swagger UI, Postman, NSwag, JSON Schema, JWT, Microsoft.AspNetCore.OpenApi, Microsoft.OpenApi.Models]
programming_languages: [C#]
tags: [minimal-apis, openapi, dotnet, web-api, api-documentation, json-schema, api-design, api-versioning, jwt, csharp]
key_concepts: [Minimal APIs, OpenAPI Specification, JSON Schema, API Documentation, API Versioning, Security Schemes, Contract-First Development, YAML Output]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores the significant advancements in .NET 10 Minimal APIs, specifically its native support for OpenAPI 3.1. It highlights how this integration simplifies the creation of standards-compliant API documentation, crucial for modern RESTful API development and seamless integration with tools like Swagger UI and Postman. The author details new features such as YAML output, improved metadata, and a cleaner `MapOpenApi()` method. Practical C# code examples demonstrate setup, advanced security schemes with JWT, API versioning, and rich response examples. The piece concludes that .NET 10 empowers developers to build high-quality, well-documented APIs with minimal friction.
---
```

# Minimal APIs in .NET 10: Now with OpenAPI 3.1 Support | by Adem KORKMAZ | Sep, 2025 | JavaScript in Plain English

# Minimal APIs in .NET 10: Now with OpenAPI 3.1 Support

## **.NET 10**, Minimal APIs have taken another leap forward

![A scenic view of a coastline at sunset, with the sun low over the calm sea and cliffs on the right, topped with several wind turbines.](https://miro.medium.com/v2/resize:fit:700/0*sPuZ8SggQ4ydjNov)

Minimal APIs have been one of the most exciting additions to ASP.NET Core since .NET 6. They gave developers a way to build lightweight, high-performance HTTP endpoints without the ceremony of traditional controllers. With just a few lines of code, you could spin up a fully functional API — perfect for microservices, internal tools, or quick prototypes.

Now, with **.NET 10**, Minimal APIs have taken another leap forward. The framework introduces **native OpenAPI 3.1 support**, making it easier than ever to produce clean, standards-compliant API documentation that works seamlessly with modern tooling like Swagger UI, Postman, and NSwag.

## Why This Matters

OpenAPI is the de facto standard for describing RESTful APIs. It allows developers to:

*   Generate client SDKs in multiple languages.
*   Validate requests and responses automatically.
*   Provide interactive documentation for consumers.
*   Enable contract-first development workflows.

Until now, Minimal APIs required extra setup or third-party packages to produce a fully compliant OpenAPI spec. With .NET 10, you can generate **OpenAPI 3.1** specifications out of the box — including **YAML output** — without additional dependencies. This means less boilerplate, fewer compatibility issues, and faster onboarding for teams.

## Why OpenAPI 3.1 Matters

OpenAPI 3.1 isn’t just a version bump — it’s a big leap forward:

*   **JSON Schema Alignment**: Uses the latest JSON Schema standard, making validation more accurate.
*   **Nullable Types**: Better handling of optional fields.
*   **oneOf / anyOf Support**: More expressive API contracts.
*   **Tooling Compatibility**: Works better with Postman, Swagger UI, NSwag, and code generators.

If you’re building APIs that will be consumed by other teams, languages, or platforms, OpenAPI 3.1 makes your life easier.

## What’s New in .NET 10 Minimal APIs

The key improvements include:

*   **OpenAPI 3.1 compliance** with JSON Schema 2020–12 support.
*   **YAML output** alongside JSON for API specifications.
*   **Improved metadata** for routes, including descriptions and response documentation.
*   **Cleaner integration** via the new `MapOpenApi()` method.

These changes make Minimal APIs not just minimal in code, but also minimal in friction when it comes to documentation.

![A table comparing features of .NET 8, .NET 9, and .NET 10 related to OpenAPI, JSON Schema, YAML output, route metadata, and MapOpenApi() integration, showing .NET 10's significant improvements.](https://miro.medium.com/v2/resize:fit:700/1*ZCHRiUaLR2QRyXg7yVEF-Q.png)

## Setting Up Minimal APIs with OpenAPI 3.1

Here’s a simple example of how to enable OpenAPI 3.1 in a .NET 10 Minimal API project:

```csharp
var builder = WebApplication.CreateBuilder(args);  
  
// Enable OpenAPI 3.1  
builder.Services.AddOpenApi(options =>  
{  
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;  
});  
var app = builder.Build();  
// Minimal API endpoint  
app.MapGet("/hello", () => new { Message = "Hello, .NET 10!" })  
   .WithName("HelloEndpoint")  
   .WithDescription("Returns a greeting message")  
   .Produces(200);  
app.MapOpenApi(); // Serve OpenAPI spec  
app.Run();
```

## Viewing the Output

Once the application is running, you can access:

*   JSON: `https://localhost:5001/openapi/v1.json`
*   YAML: `https://localhost:5001/openapi/v1.yaml`

Here’s an example of what the **YAML output** might look like for the `/hello` endpoint:

```yaml
openapi: 3.1.0  
info:  
  title: My Minimal API  
  version: 1.0.0  
paths:  
  /hello:  
    get:  
      operationId: HelloEndpoint  
      description: Returns a greeting message  
      responses:  
        '200':  
          description: OK  
          content:  
            application/json:  
              schema:  
                type: object  
                properties:  
                  message:  
                    type: string  
                    example: Hello, .NET 10!
```

This YAML format is often preferred for human readability and is supported by most API tooling. It’s also easier to version-control because diffs are cleaner compared to JSON.

## Real-World Benefits

1.  **Faster Onboarding** New developers can explore your API via Swagger UI without digging into the codebase.
2.  **Better Client Generation** Tools like NSwag and OpenAPI Generator can now produce more accurate clients thanks to JSON Schema 2020–12 support.
3.  **Contract-First Development** You can define your API spec first, then implement it — ensuring no surprises for consumers.
4.  **Multi-Format Documentation** Offering both JSON and YAML means you can satisfy both machine and human consumers.

## Migration Tips

If you’re upgrading from .NET 8 or 9:

1.  Update your `Microsoft.AspNetCore.OpenApi` package.
2.  Replace any manual Swagger setup with `MapOpenApi()`.
3.  Validate your schema with an OpenAPI 3.1 validator.
4.  Ensure your client generation tools support OpenAPI 3.1.
5.  Review nullable type handling — OpenAPI 3.1 changes how nullability is represented.

## Advanced Usage: Taking Minimal APIs + OpenAPI 3.1 to the Next Level

Minimal APIs in .NET 10 are not just about simplicity — they can also handle advanced API design needs. With OpenAPI 3.1, you can define **security schemes**, **API versioning**, and **rich response examples** directly in your Minimal API configuration.

### 1\. Adding Security Schemes (JWT Bearer Example)

```csharp
builder.Services.AddOpenApi(options =>  
{  
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;  
    options.AddDocumentTransformer((document, context, cancellationToken) =>  
    {  
        document.Components ??= new();  
        document.Components.SecuritySchemes["BearerAuth"] =    
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme  
        {  
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,  
            Scheme = "bearer",  
            BearerFormat = "JWT",  
            Description = "Enter your JWT token"  
        };  
  
        document.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement  
        {  
            {  
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme  
                {  
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference  
                    {  
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,  
                        Id = "BearerAuth"  
                    }  
                },  
                Array.Empty<string>()  
            }  
        });  
        return Task.CompletedTask;  
    });  
});
```

### 2\. API Versioning in Minimal APIs

```csharp
app.MapGet("/v1/hello", () =>  
    new { Message = "Hello from v1" })  
   .WithName("HelloV1")  
   .WithTags("v1");  
  
app.MapGet("/v2/hello", () =>  
    new { Message = "Hello from v2", Timestamp = DateTime.UtcNow })  
   .WithName("HelloV2")  
   .WithTags("v2");
```

### 3\. Rich Response Examples

```csharp
app.MapGet("/weather", () => new  
{  
    TemperatureC = 22,  
    Summary = "Sunny"  
})  
.WithName("GetWeather")  
.WithDescription("Returns the current weather")  
.Produces(200, typeof(object), "application/json")  
.WithOpenApi(op =>  
{  
    op.Responses["200"].Content["application/json"].Examples = new Dictionary<string, Microsoft.OpenApi.Any.IOpenApiAny>  
    {  
        ["Sample"] = new Microsoft.OpenApi.Any.OpenApiObject  
        {  
            ["temperatureC"] = new Microsoft.OpenApi.Any.OpenApiInteger(22),  
            ["summary"] = new Microsoft.OpenApi.Any.OpenApiString("Sunny")  
        }  
    };  
    return op;  
});
```

## YAML Output with Advanced Features

```yaml
openapi: 3.1.0  
info:  
  title: My Minimal API  
  version: 1.0.0  
components:  
  securitySchemes:  
    BearerAuth:  
      type: http  
      scheme: bearer  
      bearerFormat: JWT  
paths:  
  /weather:  
    get:  
      description: Returns the current weather  
      responses:  
        '200':  
          description: OK  
          content:  
            application/json:  
              examples:  
                Sample:  
                  value:  
                    temperatureC: 22  
                    summary: Sunny  
security:  
  - BearerAuth: []
```

## Conclusion

Minimal APIs were already a great way to build fast, clean endpoints in .NET. With **.NET 10’s OpenAPI 3.1 support**, they’re now even more powerful — giving you better documentation, stronger contracts, and smoother integration with modern tooling.

From quick prototypes to production-grade APIs with authentication, versioning, and rich examples, .NET 10 makes it easier than ever to deliver high-quality, well-documented services.

If you haven’t tried it yet, spin up a new .NET 10 project and see how quickly you can go from zero to a fully documented API — in both JSON and YAML.