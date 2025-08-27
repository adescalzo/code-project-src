```yaml
---
title: "Scaffolding | FastEndpoints"
source: https://fast-endpoints.com/docs/scaffolding#feature-scaffolding
date_published: unknown
date_captured: 2025-08-27T23:49:04.150Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, Visual Studio Extension, VSCode Extension, JetBrains Rider, .NET, xUnit, OpenAPI Generator, MongoDB, ASP.NET Core]
programming_languages: [C#, YAML]
tags: [scaffolding, code-generation, dotnet, fastendpoints, cli, templates, web-api, testing, vertical-slice, openapi]
key_concepts: [feature-scaffolding, code-snippets, vertical-slice-architecture, dotnet-new-templates, project-scaffolding, integration-testing, openapi-generation, command-query-separation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details various scaffolding options available for FastEndpoints projects, aiming to accelerate development. It covers feature scaffolding through IDE extensions (Visual Studio, VSCode, JetBrains Rider) providing code snippets for common FastEndpoints components like endpoints, validators, mappers, and handlers. Additionally, it explains how to use `dotnet new` item templates for generating full vertical slice feature sets and entire FastEndpoints projects. The content also touches upon project scaffolding using `dotnet new` templates, including options for xUnit integration testing, and leveraging OpenAPI Generator to create FastEndpoints server projects from OpenAPI documents. Community project templates are also highlighted as additional resources.]
---
```

# Scaffolding | FastEndpoints

## Feature Scaffolding

### Code Snippets

Our [VS Extension](https://marketplace.visualstudio.com/items?itemName=dj-nitehawk.FastEndpoints) & [VSCode Extension](https://marketplace.visualstudio.com/items?itemName=drilko.fastendpoints) adds handy code snippet shortcuts to make it faster to scaffold/expand new classes in your project. JetBrains Rider users can [import](https://gist.github.com/dj-nitehawk/6493cb85bf3bb20aad5d2fd7814bad15) the snippets as LiveTemplates.

![Animated GIF showing a VS Code snippet being used to scaffold an endpoint with a request DTO.](/vs-snippet.gif)

#### Available Shortcuts:

#### **epreq**

```csharp
// Scaffolds an endpoint with only a request dto
sealed class Endpoint : Endpoint<Request>
```

#### **epreqres**

```csharp
// Scaffolds an endpoint with request and response dtos
sealed class Endpoint : Endpoint<Request, Response>
```

#### **epnoreq**

```csharp
// Scaffolds an endpoint without a request nor response dto
sealed class Endpoint : EndpointWithoutRequest
```

#### **epres**

```csharp
// Scaffolds an endpoint without a request dto but with a response dto
sealed class Endpoint : EndpointWithoutRequest<Response>
```

#### **epdto**

```csharp
// Scaffolds the request & response dtos for an endpoint
sealed class Request {}
sealed class Response {}
```

#### **epval**

```csharp
// Scaffolds an endpoint validator for a given request dto
sealed class Validator : Validator<Request>
```

#### **epmap**

```csharp
// Scaffolds an endpoint mapper class for the given request, response and entity dtos
sealed class Mapper : Mapper<Request, Response, Entity>
```

#### **epsum**

```csharp
// Scaffolds a summary class for a given endpoint and request dto
sealed class Summary : Summary<Endpoint, Request>
```

#### **epdat**

```csharp
// Scaffolds a static data class for an endpoint
static class Data
```

#### **epfull**

```csharp
Scaffolds the complete set of classes for a full vertical slice
```

#### **cmd**

```csharp
// Scaffolds a command handler for a given command model that does not return a result
sealed class CommandHandler : ICommandHandler<Command>
```

#### **cmdres**

```csharp
// Scaffolds a command handler for a given command model that returns a result
sealed class CommandHandler : ICommandHandler<Command, Result>
```

#### **evnt**

```csharp
// Scaffolds an event handler for a given event model
sealed class EventHandler : IEventHandler<Event>
```

#### **preproc**

```csharp
// Scaffolds a pre-processor for a given request dto
sealed class Processor : IPreProcessor<Request>
```

#### **postproc**

```csharp
// Scaffolds a post-processor for a given request & response dto
sealed class Processor : IPostProcessor<Request, Response>
```

#### Integration Test Scaffolds

*   **tstfixture** - scaffolds a test class fixture
*   **tstclass** - scaffolds a test class
*   **tstmethod** - scaffolds a test method with a [Fact] attribute

### VS New Item Template

If you're doing vertical slice architecture and placing each individual feature in their own namespace, you can take advantage of the [VS Extension](https://marketplace.visualstudio.com/items?itemName=dj-nitehawk.FastEndpoints) that will add a new item to the "add new file" dialog of visual studio to make it convenient to add feature file sets to your project.

Once installed, your visual studio add new item dialog will have **FastEndpoints Feature File Set** listed under **Installed > Visual C#** node. Then, instead of entering a file name, simply enter the namespace you want your new feature to be added to followed by **.cs**

A new feature file set will be created in the folder you selected.

There will be 4 new files created under the namespace you chose.

*   **Data.cs** - Use this class to place all of your data access logic.
    
*   **Models.cs** - Place your request, response DTOs and the validator in this file.
    
*   **Mapper.cs** - Domain entity mapping logic will live here.
    
*   **Endpoint.cs** - This will be your new endpoint definition.
    

[Click here](https://github.com/dj-nitehawk/MiniDevTo/tree/main/Features/Author/Articles/SaveArticle) for an example feature file set.

![Animated GIF showing the Visual Studio "Add New Item" dialog with "FastEndpoints Feature File Set" selected, demonstrating how to create a new vertical slice feature.](/vslice.gif)

### Dotnet New Item Template

If you prefer working with the cli, you can use our **dotnet new** template to create a new feature file set.

installation

Copied!

```bash
dotnet new install FastEndpoints.TemplatePack
```

**Usage:**

The example feature below will use the following input parameters:

*   Namespace: **MyProject.Comments.Create**
*   Method **POST**
*   Route: **api/comments**

Files will be created in folder **Features/Comments/Create**:

```bash
dotnet new feat -n MyProject.Comments.Create -m post -r api/comments -o Features/Comments/Create
```

#### Available Options

```
> dotnet new feat --help

FastEndpoints Feature Fileset (C#)
Options:
  -t|--attributes  Whether to use attributes for endpoint configuration
                   bool - Optional
                   Default: false

  -p|--mapper      Whether to use a mapper
                   bool - Optional
                   Default: true

  -v|--validator   Whether to use a validator
                   bool - Optional
                   Default: true

  -m|--method      Endpoint HTTP method
                       GET
                       POST
                       PUT
                       DELETE
                       PATCH
                   Default: GET

  -r|--route       Endpoint path
                   string - Optional
                   Default: api/route/here
```

---

## Project Scaffolding

In order to scaffold new projects, install the Template Pack if not already installed:

terminal

Copied!

```bash
dotnet new install FastEndpoints.TemplatePack
```

### Bare-Bones Starter Project Template

A new FastEndpoints starter [project](https://github.com/FastEndpoints/Template-Pack/tree/main/templates/project) including traditional integration testing setup with xUnit:

terminal

Copied!

```bash
dotnet new feproj -n MyAwesomeProject
```

### Integrated Testing Project Template

An alternate starter [project](https://github.com/FastEndpoints/Template-Pack/tree/main/templates/integrated) that places xUnit tests alongside the endpoints that are being tested:

terminal

Copied!

```bash
dotnet new feintproj -n MyAwesomeProject
```

### xUnit Test Project

A xUnit integration testing [project](https://github.com/FastEndpoints/Template-Pack/tree/main/templates/test/Tests):

terminal

Copied!

```bash
dotnet new fetest
```

### VS New Project Dialog

After the template pack is installed with **dotnet new**, the project templates will show up in Visual Studio as well.

![Screenshot of the Visual Studio "Create a new project" dialog, filtered for FastEndpoints templates, showing options for starter and xUnit test projects.](/vs-new-proj.png)

### OpenAPI Generator

[OpenAPI Generator](https://openapi-generator.tech/) is a CLI tool that can generate a FastEndpoints server project from a given OpenAPI document. To install the tool, please follow the instructions on the [Getting Started](https://openapi-generator.tech/docs/installation) page. Once installed, a project can be generated using an OpenAPI document as follows:

terminal

Copied!

```bash
openapi-generator-cli generate \
  --generator-name aspnet-fastendpoints \
  --input-spec d:/my_open_api_file.yaml \
  --output d:/my_fastendpoints_project
```

A complete list of parameters can be found in the generator's [documentation](https://openapi-generator.tech/docs/generators/aspnet-fastendpoints). This is a community contribution. please visit the author's [github repository](https://github.com/OpenAPITools/openapi-generator) for feature requests and bug reports.

### Community Project Templates

*   [.Net Backend SaaS Starter](https://www.breakneck.dev)
*   [Rss Feed Aggregator](https://github.com/Maskoe/RssFeedAggregator)
*   [BrosSquad FastEndpoints Template](https://github.com/BrosSquad/FastEndpoints.Template)
*   [MongoDB Web Api Starter](https://github.com/dj-nitehawk/MongoWebApiStarter)

---

Previous [<- Idempotency](/docs/idempotency)

Next [The Cookbook ->](/docs/the-cookbook)

© FastEndpoints 2025

[](/)