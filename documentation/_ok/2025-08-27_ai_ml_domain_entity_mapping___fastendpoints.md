```yaml
---
title: "Domain Entity Mapping | FastEndpoints"
source: https://fast-endpoints.com/docs/domain-entity-mapping#mapping-logic-in-the-endpoint
date_published: unknown
date_captured: 2025-08-27T14:59:23.729Z
domain: fast-endpoints.com
author: Unknown
category: ai_ml
technologies: [FastEndpoints, .NET, ASP.NET Core]
programming_languages: [C#]
tags: [fastendpoints, data-mapping, dto, domain-entity, web-api, csharp, dotnet, code-organization, api-development]
key_concepts: [domain-entity-mapping, data-transfer-objects, separation-of-concerns, dependency-injection, singleton-pattern, api-endpoints, code-organization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details FastEndpoints' approach to domain entity mapping, offering a flexible alternative to auto-mapping libraries. It presents two primary methods: defining mapping logic in a dedicated `Mapper<TRequest, TResponse, TEntity>` class or directly within the endpoint using `EndpointWithMapping<TRequest, TResponse, TEntity>`. The framework provides specialized base classes and `SendMapped` methods to streamline the process. A crucial point highlighted is that mapper classes are singletons for performance, emphasizing the need to avoid maintaining state within them, though dependency injection is supported for resolving scoped services.]
---
```

# Domain Entity Mapping | FastEndpoints

# Domain Entity Mapping

For those of us who are not fans of auto-mapping libraries, FastEndpoints offers a simple way to do manual mapping for request DTO to domain entity and back from an entity to a response DTO.

Consider the following request, response and entity classes:

```csharp
public class Request
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string BirthDay { get; set; }
}

public class Response
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public int Age { get; set; }
}

public class Person
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public DateOnly DateOfBirth { get; set; }
}
```

## Mapping Logic In a Separate Class

The recommended approach is to keep the mapping logic in a class of its own by inheriting from **Mapper<TRequest, TResponse, TEntity>** like so:

```csharp
public class PersonMapper : Mapper<Request, Response, Person>
{
    public override Person ToEntity(Request r) => new()
    {
        Id = r.Id,
        DateOfBirth = DateOnly.Parse(r.BirthDay),
        FullName = $"{r.FirstName} {r.LastName}"
    };

    public override Response FromEntity(Person e) => new()
    {
        Id = e.Id,
        FullName = e.FullName,
        UserName = $"USR{e.Id:0000000000}",
        Age = (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - e.DateOfBirth.DayNumber) / 365,
    };
}
```

To use the above mapper you need to inherit your endpoint from **Endpoint<TRequest, TResponse, TMapper>** generic overload like so:

```csharp
public class SavePerson : Endpoint<Request, Response, PersonMapper>
{
    public override void Configure()
    {
        Put("/api/person");
    }

    public override Task HandleAsync(Request r, CancellationToken c)
    {
        Person entity = Map.ToEntity(r);
        Response = Map.FromEntity(entity);
        return Send.OkAsync(Response);
    }
}
```

The mapping logic can be accessed from the **Map** property of the endpoint class. that's all there's to it.

WARNING

Mapper classes are used as **singletons** for performance reasons. You should not maintain state in your mappers.

**Mapper Base Class Variants**

In cases where your endpoint has either just a request DTO or just a response DTO, you can inherit from one of the following mapper base class variants.

*   **RequestMapper<TRequest, TEntity>** for **EndpointWithMapper<TRequest, TMapper>**
    
*   **ResponseMapper<TResponse, TEntity>** for **EndpointWithoutRequest<TResponse, TMapper>**
    

**Mapped Response Sending Methods**

When using any endpoint base class that has Mapper support, you can take advantage of the **SendMapped** response sending methods by supplying the entity directly. It takes care of calling the response mapping method internally.

```csharp
// automatically calls FromEntity method of mapper
await SendMapped(entity, ...);

// automatically calls FromEntityAsync method of mapper
await SendMappedAsync(entity, ...);
```

## Mapping Logic In The Endpoint

If you prefer to place your mapping logic in the endpoint definition itself, you can simply use the **EndpointWithMapping<TRequest,TResponse,TEntity>** generic overload to implement your endpoint and override the **MapToEntity()** and **MapFromEntity()** methods like so:

```csharp
public class SavePerson : EndpointWithMapping<Request, Response, Person>
{
    public override void Configure() => Put("/api/person");

    public override Task HandleAsync(Request r, CancellationToken c)
    {
        Person entity = MapToEntity(r);
        Response = MapFromEntity(entity);
        return Send.OkAsync(Response);
    }

    public override Person MapToEntity(Request r) => new()
    {
        Id = r.Id,
        DateOfBirth = DateOnly.Parse(r.BirthDay),
        FullName = $"{r.FirstName} {r.LastName}"
    };

    public override Response MapFromEntity(Person e) => new()
    {
        Id = e.Id,
        FullName = e.FullName,
        UserName = $"USR{e.Id:0000000000}",
        Age = (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - e.DateOfBirth.DayNumber) / 365,
    };
}
```

## Dependency Injection

Mappers are used as singletons for [performance reasons](/benchmarks). I.e. there will only ever be one instance of a mapper type. You should not maintain state in mappers. If you need to resolve scoped dependencies in your mappers, you may do so as shown [here](dependency-injection#entity-mapper-dependencies).