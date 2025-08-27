```yaml
---
title: "Configuration Settings | FastEndpoints"
source: https://fast-endpoints.com/docs/configuration-settings#customizing-functionality
date_published: unknown
date_captured: 2025-08-27T20:25:47.014Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, System.Text.Json, ASP.NET Core, Newtonsoft.Json, Nuget, .NET Compiler Platform SDK, FluentValidation]
programming_languages: [C#]
tags: [fastendpoints, configuration, json-serialization, error-handling, routing, code-generation, performance, web-api, dotnet, startup]
key_concepts: [configuration-as-code, json-serialization, route-prefixing, endpoint-filtering, global-endpoint-options, problem-details, source-generation, reflection-optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  This document details various configuration settings available in the FastEndpoints library, allowing users to customize its default functionality. It covers how to specify JSON serializer options, unify property naming policies, and set global route prefixes. The article also explains advanced features like filtering endpoint registration, applying global endpoint options, and organizing configurations into groups for a vertical slice architecture. Furthermore, it provides guidance on customizing error responses, enabling RFC7807/RFC9457 compatible Problem Details, and overriding default JSON serialization/deserialization behaviors. Finally, it introduces performance optimizations through source generator-based startup and reflection.
---
```

# Configuration Settings | FastEndpoints

# Configuration Settings

## Customizing Functionality

There are several areas you can customize or override the default functionality of the library. All configuration settings must be specified during app startup with the `UseFastEndpoints()` call.

## Specify JSON Serializer Options

The settings for the default JSON serializer, which is **System.Text.Json**, can be set as follows:

```csharp
app.UseFastEndpoints(c =>
{
    c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});
```

If you're using the [union type returning endpoint handlers](get-started#union-type-returning-handler), the above will not be effective (due to a technical constraint). In this case, you'll have to resort to using the [options pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-7.0) like so:

```csharp
bld.Services.Configure<JsonOptions>(o => 
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
```

When using the options pattern, there's no need to specify it again inside the `UseFastEndpoints(...)` call, and the library will use the same settings from the `JsonOptions`.

## Unified Property Naming Policy

By default, `PropertyNamingPolicy` only applies to JSON serialization (via System.Text.Json). Other binding sources like query parameters, form fields, route parameters, and headers match properties case-insensitively unless explicitly specified using attributes like `[BindFrom(...)]`, `[FromHeader(...)]`, etc.

To unify naming across all binding sources, you can enable the following setting:

```csharp
app.UseFastEndpoints(c => c.Binding.UsePropertyNamingPolicy = true);
```

This removes the need for attributes in most cases but requires the team to follow a consistent naming convention to avoid confusion.

## Global Route Prefix

You can have a specified string automatically prepended to all route names in your app instead of repeating it in each and every route config method by specifying the prefix at app startup.

**Program.cs**

```csharp
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
});
```

For example, the following route config methods would result in the below endpoint routes:

```
Get("client/update"); -> "/api/client/update"
Put("inventory/delete"); -> "/api/inventory/delete"
Post("sales/recent-list"); -> "/api/sales/recent-list"
```

If needed, you can override or disable the global prefix from within individual endpoints like so:

```csharp
public override void Configure()
{
    Post("user/create");
    RoutePrefixOverride("mobile");
}
```

In order to disable the global prefix, simply pass in a `string.Empty` to the `RoutePrefixOverride()` method.

## Filtering Endpoint Registration

If you'd like to prevent some of the endpoints in your project from being registered during startup, you have the option to supply a filtering function which will be run against each discovered endpoint.

If your function returns `true`, that particular endpoint will be registered. If the function returns `false`, that endpoint will be ignored and not registered.

```csharp
app.UseFastEndpoints(c =>
{
    c.Endpoints.Filter = ep =>
    {
        if (ep.Verbs.Contains("GET") && ep.Routes.Contains("/api/mobile/test"))
        {
            return false; // don't register this endpoint
        }
        return true;
    };
});
```

It is also possible to set a `Tag` for an endpoint and use that tag to filter out endpoints during registration as shown below:

```csharp
public override void Configure()
{
    Get("client/update");
    Tags("Deprecated", "ToBeDeleted"); // has no relationship with Swagger tags
}

app.UseFastEndpoints(c =>
{
    c.Endpoints.Filter = ep =>
    {
        if (ep.EndpointTags?.Contains("Deprecated") is true)
        {
            return false; // don't register this endpoint
        }
        return true;
    };
});
```

## Global Endpoint Options

You can have a set of common settings applied to endpoints of your choice by specifying an action for the `Endpoints.Configurator` property.

The action you specify here will be executed for each endpoint during startup. You can inspect the `EndpointDefinition` argument to check what the current endpoint is and call most of the same methods you usually use from within the endpoint `Configure()` method as shown below.

```csharp
app.UseFastEndpoints(c =>
{
    c.Endpoints.Configurator = ep =>
    {
        if (ep.Routes[0].StartsWith("/public") is true)
        {
            ep.AllowAnonymous();
            ep.Options(b => b.RequireHost("www.domain.com"));
            ep.Description(b => b.Produces<ErrorResponse>(400, "application/problem+json"));
        }
    };
});
```

**WARNING**

The following methods will have a compounding effect when called in the configurator:

For example: if you call `Roles("Admin")` at the global level and call `Roles("Manager")` at the endpoint level, the endpoint will allow both roles access to the endpoint.

*   `AuthSchemes()`
*   `Claims()`
*   `ClaimsAll()`
*   `Permissions()`
*   `PermissionsAll()`
*   `Policies()`
*   `PostProcessors()`
*   `PreProcessors()`
*   `Roles()`
*   `Tags()`

The following methods will completely override the endpoint level call:

*   `RoutePrefixOverride()`

## Endpoint Configuration Groups

As an alternative to the `Endpoints.Configurator` function above, you can use **configuration groups**. They can house common configuration in standalone classes away from your `Program.cs`, which can be placed anywhere you like. These groups can be made into a tree structure of infinite depth, making it highly attractive for doing vertical slice architecture.

Start off with a root level group by subclassing the `Group` abstract class like below and call the `Configure()` method in the constructor with a route prefix for the group:

```csharp
public class Administration : Group
{
    public Administration()
    {
        Configure("admin", ep => //admin is the route prefix for the top level group
        {
            ep.Description(x => x
              .Produces(401)
              .WithTags("administration"));
        });
    }
}
```

Endpoints can then specify which group they belong to like so:

```csharp
public override void Configure()
{
    Post("/login");
    AllowAnonymous();
    Group<Administration>();
}
```

A sub-group is created by subclassing the `SubGroup<TParentGroup>` class like so:

```csharp
public class Sales : SubGroup<Administration>
{
    public Sales()
    {
        Configure("sales", ep =>
        {
            ep.Description(x => x
              .Produces(402)
              .WithTags("sales"));
        });
    }
}
```

An endpoint in a nested/sub group only needs to specify the immediate group it belongs to like so:

```csharp
public override void Configure()
{
    Get("/invoice/{id}");
    Group<Sales>();
}
```

The above group config would result in the following routes:

```
/admin/login
/admin/sales/invoice/{id}
```

[See here](https://gist.github.com/dj-nitehawk/5b3e73818f630c2fe90d9f4674847452) for a full program with the above in action.

## Customizing Error Responses

If the [default error response](https://api-ref.fast-endpoints.com/api/FastEndpoints.ErrorResponse.html) is not to your liking, you can specify a function to produce the exact error response you need. Whatever object you return from that function will be serialized to JSON and sent to the client whenever a **400** error response needs to be sent downstream. The function will be supplied a list of validation failures, HTTP context, as well as a status code you can use to construct your own error response object like so:

**Program.cs**

```csharp
app.UseFastEndpoints(c =>
{
    c.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
    {
        return new ValidationProblemDetails(
            failures.GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        keySelector: e => e.Key,
                        elementSelector: e => e.Select(m => m.ErrorMessage).ToArray()))
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = statusCode,
            Instance = ctx.Request.Path,
            Extensions = { { "traceId", ctx.TraceIdentifier } }
        };
    };
});
```

### RFC7807 & RFC9457 Compatible Problem Details

You can enable RFC compatible error responses like the following:

```json
{
  "type": "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "instance": "/api/test/666",
  "traceId": "0HMPNHL0JHL76:00000001",
  "errors": [
    {
      "name": "clientIP",
      "reason": "IP address is blocked!"
    },
    {
      "name": "clientID",
      "reason": "Invalid client ID!"
    }
  ]
}
```

By simply doing the following at startup:

```csharp
app.UseFastEndpoints(x => x.Errors.UseProblemDetails());
```

The following customization options are available for modifying the behavior of **ProblemDetails** responses:

```csharp
app.UseFastEndpoints(
   c => c.Errors.UseProblemDetails(
       x =>
       {
           x.AllowDuplicateErrors = true;  //allows duplicate errors for the same error name
           x.IndicateErrorCode = true;     //serializes the fluentvalidation error code
           x.IndicateErrorSeverity = true; //serializes the fluentvalidation error severity
           x.TypeValue = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1";
           x.TitleValue = "One or more validation errors occurred.";
           x.TitleTransformer = pd => pd.Status switch
           {
               400 => "Validation Error",
               404 => "Not Found",
               _ => "One or more errors occurred!"
           };
       }));
```

### Produces Metadata For Custom Error DTOs

As mentioned [here](swagger-support#describe-endpoints), by default a **400 - Bad Request** **Produces Metadata** is added automatically for endpoints that have validators associated with them. When you are overriding the default error response by specifying your own error response builder function, you should also set the type of the error response DTO which this automatic behavior uses like so:

```csharp
app.UseFastEndpoints(c => c.Errors.ProducesMetadataType = typeof(ProblemDetails));
```

The automatic behavior can be disabled by setting `null` on the above property as well.

## Custom De-Serialization Of JSON

If you'd like to take control of how request bodies are deserialized, simply provide a function like the following. Deserialize the object however you want and return it from the function. This function will be used to deserialize all incoming requests with a JSON body where applicable.

Input parameters:

*   `HttpRequest`: the HTTP request object
*   `Type`: the type of the request DTO
*   `JsonSerializerContext?`: nullable JSON serializer context
*   `CancellationToken`: a cancellation token

```csharp
app.UseFastEndpoints(c =>
{
    c.Serializer.RequestDeserializer = async (req, tDto, jCtx, ct) =>
    {
        using var reader = new StreamReader(req.Body);
        return Newtonsoft.Json.JsonConvert.DeserializeObject(await reader.ReadToEndAsync(), tDto);
    };
});
```

## Custom Response DTO Serialization

The response serialization process can be overridden by specifying a function that returns a `Task` object. You should set the content-type on the HTTP response object and write directly to the response body stream. This function will be used to serialize all outgoing responses where a JSON body is required.

The parameters supplied to the function are as follows:

*   `HttpResponse`: the HTTP response object
*   `object`: the response DTO to be serialized
*   `string`: the response content-type
*   `JsonserializerContext?`: nullable JSON serializer context
*   `CancellationToken`: a cancellation token

```csharp
app.UseFastEndpoints(c =>
{
    c.Serializer.ResponseSerializer = (rsp, dto, cType, jCtx, ct) =>
    {
        rsp.ContentType = cType;
        return rsp.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(dto), ct);
    };
});
```

**NOTE**

It is currently not possible to specify a serialization function per endpoint, nor is it possible to use with [TypedResults/IResult](get-started#union-type-returning-handler) types. The serialization function is only called by the `Send.*Async()` methods.

## Source Generator Based Startup

Reflection based assembly scanning is used by default to discover endpoints, validators, summaries, event handlers & command handlers.

If your application has many hundreds of these types and it's running in a serverless environment, you may be able to get somewhat of a startup speed boost by utilizing our **Type Discovery** source generator.

To enable the source generator, simply install the `FastEndpoints.Generator` package from Nuget.

```bash
dotnet add package FastEndpoints.Generator
```

Then register the generated type array with FastEndpoints at startup:

**Program.cs**

```csharp
bld.Services.AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All);
```

If your types are in different referenced projects/assemblies, add the source generator package to each project and call `SourceGeneratorDiscoveredTypes.AddRange()` multiple times:

**Program.cs**

```csharp
bld.Services.AddFastEndpoints(o =>
{
    o.SourceGeneratorDiscoveredTypes.AddRange(MyApp.DiscoveredTypes.All);
    o.SourceGeneratorDiscoveredTypes.AddRange(SomeAssembly.DiscoveredTypes.All);
});
```

If source generation is not working, make sure you have **.NET Compiler Platform SDK** installed in your environment. [See here](https://github.com/dj-nitehawk/FastEndpoints/issues/117#issuecomment-1136891324) for more info.

## Source Generated Reflection

In order to avoid the cost of runtime expression compilation & reflection based methods, you can use the **Reflection Source Generator** simply by wiring it up at startup like so:

```csharp
app.UseFastEndpoints(c => c.Binding.ReflectionCache.AddFromMyApp());
```

A separate `AddFrom*()` extension method will be generated per assembly/project in your solution. Make sure to install the generator package on each of those projects and chain the generated methods on the `ReflectionCache` property as follows.

```csharp
app.UseFastEndpoints(c => c.Binding.ReflectionCache
                                   .AddFromMyApp()
                                   .AddFromContracts());
```

The source generator package can be installed like so:

```bash
dotnet add package FastEndpoints.Generator
```

The library will automatically fall back to runtime compilation & reflection in scenarios where source generation is not possible such as the following:

*   All properties of `Record` classes
*   Init only properties of any class

---

Previous [<- Integration & Unit Testing](/docs/integration-unit-testing)

Next [Misc Conveniences ->](/docs/misc-conveniences)

© FastEndpoints 2025