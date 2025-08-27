```yaml
---
title: "Misc Conveniences | FastEndpoints"
source: https://fast-endpoints.com/docs/misc-conveniences#endpoint-options
date_published: unknown
date_captured: 2025-08-27T23:48:33.949Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, ASP.NET Core, System.Text.Json, .NET]
programming_languages: [C#]
tags: [web-api, dotnet, routing, http, endpoint-configuration, response-handling, validation, api-development, fastendpoints, middleware]
key_concepts: [endpoint-configuration, http-verbs, routing, route-parameters, request-response-cycle, dependency-injection, validation, response-headers, hook-methods]
code_examples: false
difficulty_level: intermediate
summary: |
  This document outlines various convenience features and configuration options available within the FastEndpoints framework for building web APIs. It details how to customize endpoint registration, handle multiple HTTP verbs and routes, and utilize shorthand route configurations. The content also covers strongly-typed route parameters, a comprehensive list of available endpoint properties, and built-in methods for sending diverse HTTP responses. Furthermore, it explains how to extend response sending capabilities, implement attribute-driven response headers, and leverage hook methods for pre/post-validation and handler execution, enhancing developer control and streamlining API development.
---
```

# Misc Conveniences | FastEndpoints

# Misc Conveniences

## Endpoint Options

In addition to the convenient methods you can use in the endpoint configuration to setup your endpoints (mentioned in previous pages), you can use the **Options()** method to customize aspects of endpoint registration/setup like so:

MyEndpoint.cs

```csharp
Options(b => b.RequireCors(x => x.AllowAnyOrigin())
              .RequireHost("domain.com")
              .ProducesProblem(404));
```

## Multiple Verbs & Routes

A single endpoint can be made to listen to more than one type of http verb and on multiple routes like below. This is sometimes useful when your endpoint handler code is almost the same with a minor variation across different routes/verbs. Instead of creating multiple endpoint classes, you can use this strategy to handle multiple use-cases with a single piece of handler logic via `HttpContext.Request.Path` and `HttpContext.Request.HttpMethod`.

MyEndpoint.cs

```csharp
public override void Configure()
{
    Verbs(Http.POST, Http.PUT, Http.Patch);
    Routes("/api/user/create", "/api/user/save");
}
```

**WARNING**

The above registers 6 endpoints in the routing system even though they all use the same handler logic.

## Shorthand Route Configuration

Instead of the **Verbs()** and **Routes()** combo, you can use the shorthand versions that combines them when configuring your endpoints like so:

*   **Get()**
*   **Post()**
*   **Put()**
*   **Patch()**
*   **Delete()**

MyEndpoint.cs

```csharp
public override void Configure()
{
    Get("/api/customer/{CustomerID}");
}
```

The above is equivalent to using both **Verbs()** and **Routes()**. Do note that you can't configure multiple verbs with the shorthand version. You can however setup multiple route patterns with the shorthand methods.

## Strongly Typed Route Parameters

In cases where the route parameters are bound to the request DTO, it is possible to tie the parameter names to the properties of the DTO. Simply prefix the parameters in the route template with an **@ sign** and provide a **new expression** with the bound properties in the correct order as shown below:

```csharp
Get("/customer/{@cid}/invoice/{@inv}", x => new { x.CustomerId, x.InvoiceId });
```

The resulting route from the above example would be `/customer/{CustomerId}/invoice/{InvoiceId}`. The route param names can be customized by annotating request DTO properties with a **[BindFrom("Id")]** attribute, which produces the route: `/customer/{Id}/invoice/{InvoiceId}`. Properties without an annotation will simply use the property name.

## Endpoint Properties

The following properties are available to all endpoint classes.

*   **BaseURL** (string) The base URL of the current request in the form of **https://hostname:port/** (includes trailing slash). if your server is behind a proxy/gateway, use the [forwarded headers middleware](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-6.0) to get the correct address.
*   **Config** (IConfiguration) Gives access to current configuration of the web app
*   **Env** (IWebHostEnvironment) Gives access to the current web hosting environment
*   **Files** (IFormFileCollection) Exposes the uploaded file collection in case of **multipart/form-data** uploads.
*   **Form** (IFormCollection) Exposes the form data in case of **application/x-www-form-urlencoded** or **multipart/form-data** uploads.
*   **HttpContext** (HttpContext) Gives access to the current HTTP context of the request.
*   **HttpMethod** (Http enum value) The http method of the current request as an enum value.
*   **Logger** (ILogger) The default logger for the current endpoint type
*   **Response** (TResponse) Exposes a blank response DTO for the current endpoint before the endpoint handler is executed or represents the populated response DTO after a response has been sent to the client.
*   **User** (ClaimsPrincipal) The current claims principal associated with the current request.
*   **ValidationFailed** (bool) Indicates the current validation status
*   **ValidationFailures** (List<ValidationFailure>) The list of validation failures for the current execution context.

## Send Methods

The following built-in methods are available via the **Send** property of endpoints for sending different types of responses to clients:

*   **.ResponseAsync()** Sends a given response DTO or any object that can be serialized as JSON down to the requesting client. Allows customizations of status code.
*   **.CreatedAtAsync()** Sends a **201 created** response with a **Location** header containing where the resource can be retrieved from. [See note](swagger-support#custom-endpoint-names) about using with custom endpoint names.
*   **.AcceptedAtAsync()** Sends a **202 accepted** response with a **Location** header containing where the resource can be retrieved from. [See note](swagger-support#custom-endpoint-names) about using with custom endpoint names.
*   **.StringAsync()** Sends a given string to the client in the response body.
*   **.OkAsync()** Sends a 200 ok response with or without a body.
*   **.NoContentAsync()** Sends a 204 no content response.
*   **.RedirectAsync()** Sends a 30X moved response with a location header containing the URL to redirect to.
*   **.ErrorsAsync()** Sends a 400 error response with the current list of validation errors describing the validation failures.
*   **.NotFoundAsync()** Sends a 404 not found response.
*   **.UnauthorizedAsync()** Sends a 401 unauthorized response.
*   **.ForbiddenAsync()** Sends a 403 forbidden response.
*   **.BytesAsync()** Sends a byte array to the client.
*   **.FileAsync()** Sends a file to the client.
*   **.StreamAsync()** Sends the contents of a stream to the client.
*   **.EventStreamAsync()** Sends a "server-sent-events" data stream to the client.
*   **.ResultAsync()** Sends any [IResult](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-7.0#iresult-return-values) instance produced by the [Results](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.results) or [TypedResults](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.typedresults) static classes in Minimal APIs.

#### Custom Send Methods

If the provided response sending methods are not sufficient, you can easily add your own by creating extension methods targeting the **IResponseSender** interface like so:

```csharp
public static class SendExtensions
{
    public static Task SendStatusCode(this IResponseSender sender, int statusCode)
    {
        sender.HttpContext.MarkResponseStart(); //don't forget to always do this
        sender.HttpContext.Response.StatusCode = statusCode;
        return sender.HttpContext.Response.StartAsync();
    }
}
```

Which you'd then be able to call from your endpoint handler like so:

```csharp
public override async Task HandleAsync(Request r, CancellationToken ct)
{
    await Send.StatusCode(219);
}
```

## Attribute Driven Response Headers

If your JSON responses have accompanying response headers, you can make them part of the response DTO itself while marking them with an attribute. When the DTO is serialized to the response stream, the decorated properties are ignored by STJ and the values of those properties are added to the response as headers.

```csharp
sealed class MyResponse
{
    public int UserId { get; set; }

    [ToHeader("x-session-id")]
    public string SessionId { get; set; }
}

sealed class UserEndpoint : EndpointWithoutRequest<MyResponse>
{
    // ... other code ...

    public override Task HandleAsync(CancellationToken c)
        => Send.OkAsync(
            new()
            {
                UserId = 12345,
                SessionId = "xyzxyzxyzxyzxyzxyzxyz"
            });
}
```

The above results in the following Http response:

```http
HTTP/1.1 200 OK
Content-Type: application/json
x-session-id: xyzxyzxyzxyzxyzxyzxyz

{
  "userId": 12345
}
```

**LIMITATIONS:**

*   Only works with the built-in serializer (STJ).
*   Only works with **Send.*Async()** methods that accept a response DTO.
*   Not compatible with **IResult** (Results/TypedResults) response types.

## Hook Methods

The following 5 hook methods allow you to do something before and after DTO validation as well as handler execution.

*   **OnBeforeValidate()** override this method if you'd like to do something to the request dto before it gets validated.
*   **OnAfterValidate()** override this method if you'd like to do something to the request dto after it gets validated.
*   **OnValidationFailed()** override this method if you'd like to do something when validation fails.
*   **OnBeforeHandle()** override this method if you'd like to do something to the request dto before the handler is executed.
*   **OnAfterHandle()** override this method if you'd like to do something after the handler is executed.

---

Previous [<- Configuration Settings](/docs/configuration-settings)

Next [API Versioning ->](/docs/api-versioning)

© FastEndpoints 2025