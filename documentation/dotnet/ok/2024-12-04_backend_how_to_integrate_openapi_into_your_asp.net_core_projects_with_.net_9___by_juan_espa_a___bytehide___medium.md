```yaml
---
title: "How to Integrate OpenAPI into Your ASP.NET Core Projects with .NET 9 | by Juan España | ByteHide | Medium"
source: https://medium.com/bytehide/how-to-integrate-openapi-into-your-asp-net-core-projects-with-net-9-88cf18395136
date_published: 2024-12-04T09:10:05.425Z
date_captured: 2025-08-08T18:07:39.901Z
domain: medium.com
author: Juan España
category: backend
technologies: [ASP.NET Core, .NET 9, OpenAPI, Swagger UI, Redoc, Kiota, Microsoft.AspNetCore.OpenApi, Microsoft.Extensions.ApiDescription.Server, RESTful APIs, Minimal APIs, Controller-based APIs]
programming_languages: [C#, SQL, XML]
tags: [openapi, aspnet-core, dotnet, api-documentation, rest-api, web-api, development-workflow, ci/cd, minimal-apis, api-customization]
key_concepts: [OpenAPI specification, API documentation, RESTful APIs, Minimal APIs, Controller-based APIs, Ahead-of-Time (AoT) Compilation, Continuous Integration (CI), API customization, build-time generation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the native OpenAPI integration in ASP.NET Core with .NET 9, highlighting how it simplifies API documentation and management. It details the benefits of using OpenAPI, such as standardized documentation, improved collaboration, and a rich ecosystem of tools like Swagger UI. The guide provides practical steps for setting up OpenAPI in a .NET 9 project, adding metadata to endpoints, customizing documents with transformers, and generating documents at build time. The integration significantly enhances developer workflow, enabling faster testing and better compatibility with client-generation tools, making it a crucial feature for building scalable and well-documented APIs.]
---
```

# How to Integrate OpenAPI into Your ASP.NET Core Projects with .NET 9 | by Juan España | ByteHide | Medium

# How to Integrate OpenAPI into Your ASP.NET Core Projects with .NET 9

![How to Integrate OpenAPI in ASP.NET Core with .NET 9](https://miro.medium.com/v2/resize:fit:700/1*C-Bu5tqAtgYDduNDMqtktw.png "A dark blue background with the title 'How to Integrate OpenAPI in ASP.NET Core WITH .NET 9' in light blue and white text, along with the 'ByteHide' logo.")

In modern development, APIs are the backbone of scalable, connected, and functional applications. With .NET 9, Microsoft has taken a step forward by simplifying the creation and management of documentation for RESTful APIs through native OpenAPI integration in ASP.NET Core. This not only accelerates the development workflow but also enhances team collaboration and client interaction.

In this article, we will explore how developers can leverage this new capability in .NET 9 to optimize their API documentation and customization.

# What is OpenAPI and Why Should You Use It?

OpenAPI is a widely adopted standard that allows you to clearly and structurally define your APIs’ endpoints, parameters, responses, and security schemes. Among its key advantages are:

*   **Standardized Documentation:** Makes it easier for internal and external teams to understand and use your APIs.
*   **Improved Collaboration:** By offering a common specification, OpenAPI reduces misunderstandings between backend developers, frontend developers, and clients.
*   **Rich Ecosystem:** Tools like Swagger UI, Redoc, and client generators like Kiota streamline development and testing.

With .NET 9, OpenAPI not only documents your APIs but also fully integrates into the development workflow, leveraging automatic generation and customization capabilities.

# What’s New with OpenAPI in .NET 9

1.  **Native OpenAPI Document Generation** You can now generate OpenAPI documents directly from your application at runtime or during the build process.
2.  **Support for Minimal and Controller-Based APIs** Both Minimal APIs and traditional controller-based APIs are compatible with this new functionality.
3.  **Advanced Customization** Includes methods and attributes to add metadata such as descriptions, tags, and examples to your endpoints.
4.  **Transformers for Customization** Allows you to modify OpenAPI documents, individual operations, and schemas with custom classes or methods.
5.  **AoT (Ahead-of-Time Compilation) Compatibility** This functionality is fully compatible with native AoT compilation in Minimal APIs.

# How to Get Started: Integrating OpenAPI in Your ASP.NET Core Project

**1\. Set Up Your Project in .NET 9**

Ensure your project is using .NET 9. If you’re working with an existing project, update your SDK and set the target framework to .NET 9 in your project file.

**2\. Add OpenAPI Support**

For new projects, OpenAPI support is integrated into the `webapi` template. For existing projects, enable it by adding the required package:

```bash
dotnet add package Microsoft.AspNetCore.OpenApi
```

Then, register the services in `Program.cs`:

```csharp
builder.Services.AddOpenApi();  
app.MapOpenApi();
```

**3\. Add Metadata to Your Endpoints**

To enrich the generated documentation, use methods like `WithSummary`, `WithDescription`, and `WithTag` to describe endpoints and parameters:

```csharp
app.MapGet("/hello/{name}", (string name) => $"Hello, {name}!")  
    .WithSummary("Get a personalized greeting")  
    .WithDescription("This endpoint returns a personalized greeting based on the provided name.")  
    .WithTag("Greetings");
```

**4\. Customize Your OpenAPI Documents**

.NET 9 allows you to modify OpenAPI documents with transformers. For example, you can add custom contact information:

```csharp
builder.Services.AddOpenApi(options =>  
{  
    options.AddDocumentTransformer((document, context, cancellationToken) =>  
    {  
        document.Info.Contact = new OpenApiContact  
        {  
            Name = "ByteHide Support",  
            Email = "support@bytehide.com"  
        };  
        return Task.CompletedTask;  
    });  
});
```

**5\. Generate OpenAPI Documents at Build Time**

To include OpenAPI documents in your continuous integration (CI) pipeline, add the `Microsoft.Extensions.ApiDescription.Server` package and configure the project file to specify the document location:

```xml
<PropertyGroup>  
  <OpenApiDocumentsDirectory>./</OpenApiDocumentsDirectory>  
</PropertyGroup>
```

# Benefits of OpenAPI Integration in .NET 9

*   **Time-Saving:** Automates the creation of API documentation, reducing manual effort.
*   **Enhanced Collaboration:** Clear documentation improves communication between developers and other teams.
*   **Faster Testing:** Tools like Swagger UI allow you to test your endpoints directly from the browser.
*   **Compatibility with New Technologies:** Generating documents during the build process simplifies integration with client-generation and automated testing tools.

# Common Use Cases

1.  **Microservices with Minimal APIs** Document multiple endpoints in modular applications and organize your APIs by groups using tags.
2.  **Regulatory Compliance** Generate documents that meet standards and audit security configurations directly from OpenAPI files.
3.  **Automated Development** Use OpenAPI documents to automatically generate clients with tools like Kiota, saving time and avoiding manual errors.

# Conclusion

OpenAPI in .NET 9 is not just an improvement; it’s a key tool for any developer looking to build well-documented, easily integrable, and scalable APIs. With a simple setup and a high level of customization, this functionality will allow you to optimize your workflow and improve the quality of your applications.

Start exploring this integration today and take your projects to the next level!