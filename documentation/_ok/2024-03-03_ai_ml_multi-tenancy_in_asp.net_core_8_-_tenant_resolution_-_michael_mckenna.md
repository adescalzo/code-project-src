```yaml
---
title: Multi-tenancy in ASP.NET Core 8 - Tenant Resolution - Michael McKenna
source: https://michael-mckenna.com/multi-tenant-asp-dot-net-8-tenant-resolution/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2052
date_published: 2024-03-03T11:29:12.000Z
date_captured: 2025-08-19T11:24:24.444Z
domain: michael-mckenna.com
author: Michael McKenna
category: ai_ml
technologies: [ASP.NET Core 8, .NET 8, NuGet, GitHub]
programming_languages: [C#, SQL]
tags: [multi-tenancy, aspnet-core, saas, tenant-resolution, dependency-injection, web-api, application-architecture]
key_concepts: [multi-tenancy, tenant-resolution, dependency-injection, builder-pattern, ambient-context, IStartupFilter, saas-architecture]
code_examples: false
difficulty_level: intermediate
summary: |
  This article, the first in a series, revisits multi-tenancy implementation in ASP.NET Core 8. It defines multi-tenancy, focusing on shared application instances with database-per-tenant or sharded multi-tenant models. The core problem of tenant resolution from HTTP requests is addressed using `ITenantResolutionStrategy` and `ITenantLookupService` interfaces. The post details how to integrate these services into the ASP.NET Core pipeline using a builder pattern and `IStartupFilter` to make tenant information available early via ambient context. Code examples illustrate the implementation of these components and their usage in a controller.
---
```

# Multi-tenancy in ASP.NET Core 8 - Tenant Resolution - Michael McKenna

# Multi-tenancy in ASP.NET Core 8 - Tenant Resolution

## Introduction

It's been 4 and a half years since the initial series on multi-tenancy, which was based on ASP.NET Core 2.2 and later ASP.NET Core 3.1. With ASP.NET Core 8 now released, it's a good time to revisit the topic and take a fresh look at how to implement multi-tenancy today.

In this first installment, we’ll look at what exactly multi-tenancy is and how to resolve the tenant from the request.

## What is multi-tenancy?

Multi-tenancy can be achieved in a number of ways, the most common are:

*   **Standalone app:** Redeploy the application on new infrastructure for each tenant
*   **Database per tenant:** Each tenant has their own database, but the application is shared
*   **Sharded multi-tenant:** All tenants share the same database, but tenant data is partitioned

In this series, we will be looking at the last two options where a single deployed instance of your application has the ability to host multiple tenants.

Each tenant can share the same infrastructure (including the application and database) to reduce hosting costs. Tenant isolation is enforced at the code level. This is a common requirement for SaaS applications where you want to host multiple customers on a single instance of your application.

## Core requirements

To achieve this, we need to solve a few key problems:

### Tenant resolution

We need a way to identify which tenant is making the current request. This could be a domain, a path, or a header in the request.

### Tenant specific settings & services

The application might be configured differently depending on which tenant context is loaded, e.g., the tenant’s name, connection string, and other such things.

### Tenant data isolation

We need to ensure that tenant data is isolated from other tenants. This could be at the database level, or at the code level; regardless, we need to ensure that a tenant can’t access another tenant’s data.

### The source code

You can see all the code in action on [GitHub](https://github.com/myquay/MultiTenant.AspNetCore) and there’s a [NuGet package](https://www.nuget.org/packages/MultiTenant.AspNetCore/) which you can use to implement multi-tenancy in your application.

## Tenant Resolution

We need a way to identify which tenant is making the current request. To do this, we need to be able to extract something from the HTTP request that identifies which tenant we need to load.

To do this, we use an `ITenantResolutionStrategy` to extract the tenant identifier from the request. For example, this could be the domain, path, or a header in the request.

In the library, we define a `HostResolutionStrategy` which uses the host to resolve the tenant. This is a common approach for SaaS applications where each tenant has their own subdomain.

```csharp
/// <summary>
/// Resolve the host to a tenant identifier
/// </summary>
internal class HostResolutionStrategy(IHttpContextAccessor httpContextAccessor) : ITenantResolutionStrategy
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Get the tenant identifier
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<string> GetTenantIdentifierAsync()
    {
        if (_httpContextAccessor.HttpContext == null)
            throw new InvalidOperationException("HttpContext is not available");

        return await Task.FromResult(_httpContextAccessor.HttpContext.Request.Host.Host);
    }
}
```

Once we have the identifier, we need a way to exchange this for a tenant. This is where the `ITenantLookupService` comes into play. We use it to fetch the tenant information for that identifier. This could be from a database, a configuration file, or any other datasource that’s suitable for your application.

In the library, we define an `InMemoryTenantLookupService` which is a simple implementation that stores the tenant information in memory. This is useful for testing and development.

```csharp
internal class InMemoryLookupService<T>(IEnumerable<T> Tenants) : ITenantLookupService<T> where T : ITenantInfo
{
    public Task<T> GetTenantAsync(string identifier)
    {
        return Task.FromResult(Tenants.Single(t => t.Identifier == identifier));
    }
}
```

Because we don’t know what kind of information an application requires about a tenant, we define a simple interface `ITenantInfo` which contains the minimum amount of information we need to know to resolve a tenant.

```csharp
/// <summary>
/// Tenant information
/// </summary>
public interface ITenantInfo
{
    /// <summary>
    /// The tenant Id
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// The tenant identifier
    /// </summary>
    string Identifier { get; set; }

}
```

The implementing application can implement `ITenantLookupService`, `ITenantInfo` and `ITenantLookupService` to resolve the tenant in a way that makes sense for their specific application.

Other tenant resolution strategies and lookup services can be implemented to resolve tenants in different ways, for example, a `PathResolutionStrategy` could be used to resolve tenants based on the request path.

### Integration with ASP.NET Core pipeline

There are two main aspects to integrating the tenant resolution strategy and lookup service with the ASP.NET Core pipeline:

1.  Registering the tenant resolution strategy and lookup service with the dependency injection container
2.  Setting the current tenant on the `IMultiTenantContextAccessor` for each request, making the tenant available to the rest of the application through ambient context.

#### Registering the services

To provide a familiar developer experience to other ASP.NET Core services, we will use the builder pattern to register the tenant resolution strategy and lookup services.

First, an extension method to support the `.AddMultiTenancy<...>()` pattern.

```csharp
/// <summary>
/// Nice method to create the tenant builder
/// </summary>
public static class WebBuilderExtensions
{
    /// <summary>
    /// Add the services
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static TenantBuilder<T> AddMultiTenancy<T>(this IServiceCollection Services) where T : ITenantInfo
    {
        //Provide ambient tenant context
        Services.AddScoped<IMultiTenantContextAccessor<T>, AsyncLocalMultiTenantContextAccessor<T>>();

        //Register middleware to populate the ambient tenant context early in the pipeline
        Services.Insert(0, ServiceDescriptor.Transient<IStartupFilter>(provider => new MultiTenantContextAccessorStartupFilter<T>()));

        return new TenantBuilder<T>(Services);
    }
}
```

It does a few things: it registers the `IMultiTenantContextAccessor` and an `IStartupFilter` to set the current tenant on the `IMultiTenantContextAccessor` early on in the pipeline so the ambient tenant context is available for all downstream processing.

Then it returns a `TenantBuilder` which is used to provide the “fluent” extensions to register the application-specific tenant resolution strategy and lookup service.

```csharp
/// <summary>
 /// Tenant builder
 /// </summary>
 /// <param name="services"></param>
 public class TenantBuilder<T>(IServiceCollection Services) where T : ITenantInfo
 {
     /// <summary>
     /// Register the tenant resolver implementation
     /// </summary>
     /// <typeparam name="V"></typeparam>
     /// <param name="lifetime"></param>
     /// <returns></returns>
     public TenantBuilder<T> WithResolutionStrategy<V>() where V : class, ITenantResolutionStrategy
     {
         Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
         Services.TryAddSingleton(typeof(ITenantResolutionStrategy), typeof(V));
         return this;
     }

     /// <summary>
     /// Register the tenant lookup service implementation
     /// </summary>
     /// <typeparam name="V"></typeparam>
     /// <param name="lifetime"></param>
     /// <returns></returns>
     public TenantBuilder<T> WithTenantLookupService<V>() where V : class, ITenantLookupService<T>
     {
         Services.TryAddSingleton<ITenantLookupService<T>, V>();
         return this;
     }

 }
```

#### Setting the current tenant

We touched on this earlier with the following piece of code from the builder extensions:

```csharp
//Register middleware to populate the ambient tenant context early in the pipeline
Services.Insert(0, ServiceDescriptor.Transient<IStartupFilter>(provider => new MultiTenantContextAccessorStartupFilter<T>()));
```

This is an `IStartupFilter` which is used to register middleware that sets the current tenant on the `IMultiTenantContextAccessor` for each request. This is important because we want the tenant to be available as early as possible in the request pipeline.

The middleware itself is very simple; it just uses the tenant resolution strategy and lookup service to set the current tenant on the `IMultiTenantContextAccessor` for each request.

```csharp
/// <summary>
/// This middleware is responsible for setting up the scope for the tenant specific request services
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="tenantServicesConfiguration"></param>
internal class MultiTenantContextAccessorMiddleware<T>(RequestDelegate next, IHttpContextAccessor httpContextAccessor, IMultiTenantContextAccessor<T> TenantAccessor, ITenantLookupService<T> TenantResolver, ITenantResolutionStrategy TenantResolutionStrategy) where T : ITenantInfo
{

    /// <summary>
    /// Set the services for the tenant to be our specific tenant services
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        //Set context if missing so it can be used by the tenant services to resolve the tenant
        httpContextAccessor.HttpContext ??= context;
        TenantAccessor.TenantInfo ??= await TenantResolver.GetTenantAsync(await TenantResolutionStrategy.GetTenantIdentifierAsync());
        await next.Invoke(context);
    }
}
```

### The result

Now you can inject `IMultiTenantContextAccessor` into your controllers and services to access the current tenant.

```csharp
/// <summary>
/// A controller that returns a value
/// </summary>
[Route("api/values")]
[ApiController]
public class Values : Controller
{

    private readonly IMultiTenantContextAccessor<Tenant> _tenantService; 

    /// <summary>
    /// Constructor with required services
    /// </summary>
    /// <param name="tenantService"></param>
    public Values(IMultiTenantContextAccessor<Tenant> tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Get the value
    /// </summary>
    /// <param name="definitionId"></param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<string> GetValue(Guid definitionId)
    {
        return (await _tenantService.TenantInfo?.Identifier);
    }
}
```

## Summary

In this post, we looked at how to resolve the tenant from the request. We looked at how to use an `ITenantResolutionStrategy` to extract the tenant identifier from the request and an `ITenantLookupService` to fetch the tenant information for that identifier.

We also looked at how to integrate the tenant resolution strategy and lookup service with the ASP.NET Core pipeline to make the current tenant available to access through ambient context.