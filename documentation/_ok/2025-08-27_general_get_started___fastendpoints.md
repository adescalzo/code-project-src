```yaml
---
title: "Get Started | FastEndpoints"
source: https://fast-endpoints.com/docs/get-started#create-project-install-package
date_published: unknown
date_captured: 2025-08-27T13:28:43.892Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, .NET, ASP.NET Core, Minimal APIs, HTTP, JSON, Insomnia, CancellationToken]
programming_languages: [C#]
tags: [web-api, fastendpoints, dotnet, csharp, http, api-development, dto, async, minimal-apis, rest]
key_concepts: [endpoint-definition, request-response-dtos, asynchronous-programming, http-routing, api-configuration, cancellation-tokens, union-types, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  [This guide provides a comprehensive "Get Started" tutorial for building an HTTP POST endpoint using the FastEndpoints library in .NET. It walks through project creation, package installation, and setting up `Program.cs` for FastEndpoints. The core of the tutorial demonstrates defining request and response DTOs, then implementing an endpoint class with `Configure()` for routing and `HandleAsync()` for business logic. Furthermore, it explores various endpoint base types, advanced response handling with union types (`Results<T>`), and alternative configuration using attributes. The guide also touches upon the proper use of `CancellationToken` for asynchronous operations.]
---
```

# Get Started | FastEndpoints

# Get Started

Follow the steps below to create your first endpoint that will handle an HTTP POST request and send a response back to the client.

## Create Project & Install Package

```terminal
dotnet new web -n MyWebApp
cd MyWebApp
dotnet add package FastEndpoints
```

## Prepare Startup

Replace the contents of **Program.cs** file with the following:

```csharp
using FastEndpoints;

var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();

var app = bld.Build();
app.UseFastEndpoints();
app.Run();
```

## Add A Request DTO

Create a file called **MyRequest.cs** and add the following:

```csharp
public class MyRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}
```

## Add A Response DTO

Create a file called **MyResponse.cs** and add the following:

```csharp
public class MyResponse
{
    public string FullName { get; set; }
    public bool IsOver18 { get; set; }
}
```

## Add An Endpoint Class

Create a file called **MyEndpoint.cs** and add the following:

```csharp
public class MyEndpoint : Endpoint<MyRequest, MyResponse>
{
    public override void Configure()
    {
        Post("/api/user/create");
        AllowAnonymous();
    }

    public override async Task HandleAsync(MyRequest req, CancellationToken ct)
    {
        await Send.OkAsync(new()
        {
            FullName = req.FirstName + " " + req.LastName,
            IsOver18 = req.Age > 18
        });
    }
}
```

Now run your app and send a **POST** request to the **/api/user/create** endpoint using a REST client such as [Insomnia](https://insomnia.rest/) with the following request body:

```json
{
  "FirstName": "Marlon",
  "LastName": "Brando",
  "Age": 40
}
```

You should then get a response back such as this:

```json
{
  "FullName": "Marlon Brando",
  "IsOver18": true
}
```

That's all there's to it.

You simply configure how the endpoint should be listening to incoming requests from clients in the **Configure()** section calling methods such as **Get()**, **Post()**, **AllowAnonymous()**, etc. Then you override the **HandleAsync()** method in order to specify your handling logic.

In this example, the request DTO is automatically populated from the JSON body of your HTTP request and passed in to the handler. After processing, the **Send.OkAsync()** method is called with a new response DTO instance to be sent to the requesting client.

---

TIP

There's a bunch of [assemblies](https://github.com/FastEndpoints/FastEndpoints/blob/main/Src/Library/Main/EndpointData.cs#L37-L57) the library excludes by default when scanning for endpoints for auto registration. If your endpoints happen to be located in one of those assembly names, you'll be greeted with an exception about FastEndpoints being unable to discover any endpoints. Quickest way to remedy that would be to rename your project to something that's not in the exclusion list, or manually specify an additional assembly to be scanned [like so](https://gist.github.com/dj-nitehawk/b3615fad393beabefe929e3f81af6822).

---

## Endpoint Types

There are 4 different endpoint base types you can inherit from.

1.  **Endpoint<TRequest>** - Use this type if there's only a request DTO. You can however send any object to the client that can be serialized as a response with this generic overload.

2.  **Endpoint<TRequest,TResponse>** - Use this type if you have both request and response DTOs. The benefit of this generic overload is that you get strongly-typed access to properties of the DTO when doing integration testing and validations.

3.  **EndpointWithoutRequest** - Use this type if there's no request nor response DTO. You can send any serializable object as a response here also.

4.  **EndpointWithoutRequest<TResponse>** - Use this type if there's no request DTO but there is a response DTO.

It is also possible to define endpoints with **EmptyRequest** and **EmptyResponse** if needed like so:

```csharp
public class MyEndpoint : Endpoint<EmptyRequest,EmptyResponse> { }
```

### Fluent Generics

Alternatively, endpoint base classes can be selected for derivation using a fluent generic builder. Start with the **Ep** static entrypoint and choose request & response DTO types as needed.

```csharp
// equivalent of Endpoint<TRequest>
public class MyEndpoint : Ep.Req<MyRequest>.NoRes { }

// equivalent of Endpoint<TRequest,TResponse>
public class MyEndpoint : Ep.Req<MyRequest>.Res<MyResponse> { }

// equivalent of EndpointWithoutRequest
public class MyEndpoint : Ep.NoReq.NoRes { }

// equivalent of EndpointWithoutRequest<TResponse>
public class MyEndpoint : Ep.NoReq.Res<MyResponse> { }
```

---

## Sending Responses

There are multiple [response sending methods](misc-conveniences#send-methods) you can use. It is also possible to simply populate the **Response** [property of the endpoint](misc-conveniences#tres) and get a 200 OK response with the value of the Response property serialized in the body automatically. For ex:

Response DTO

```csharp
public class MyResponse
{
    public string FullName { get; set; }
    public int Age { get; set; }
}
```

Endpoint

```csharp
public class MyEndpoint : EndpointWithoutRequest<MyResponse>
{
    public override void Configure()
    {
        Get("/api/person");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var person = await dbContext.GetFirstPersonAsync();

        Response.FullName = person.FullName;
        Response.Age = person.Age;
    }
}
```

Assigning a new instance to the Response property has the same effect:

```csharp
public override Task HandleAsync(CancellationToken ct)
{
    Response = new()
    {
        FullName = "john doe",
        Age = 124
    };
    return Task.CompletedTask;
}
```

---

## Union-Type Returning Handler

Minimal APIs has the ability for endpoints to conditionally return one of multiple results/outcomes via a union type called [Results<T1,T2,...>](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-7.0#resultstresult1-tresultn).

The result objects are instantiated by calling static methods on the [TypedResults](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-7.0#typedresults-vs-results) class depending on what kind of outcome is needed.

FastEndpoints also supports this strategy simply by setting the response DTO type of the endpoint's generic parameter to the desired union type and overriding the **ExecuteAsync(...)** handler method instead of the usual **HandleAsync(...)** method.

```csharp
public class MyEndpoint : Endpoint<MyRequest,
                                   Results<Ok<MyResponse>,
                                           NotFound,
                                           ProblemDetails>>
{
    public override void Configure() { /* ... */ }

    public override async Task<Results<Ok<MyResponse>, NotFound, ProblemDetails>> ExecuteAsync(
        MyRequest req, CancellationToken ct)
    {
        await Task.CompletedTask; //simulate async work

        if (req.Id == 0) //condition for a not found response
        {
            return TypedResults.NotFound();
        }

        if (req.Id == 1) //condition for a problem details response
        {
            AddError(r => r.Id, "value has to be greater than 1");
            return new FastEndpoints.ProblemDetails(ValidationFailures);
        }

        // 200 ok response with a DTO
        return TypedResults.Ok(new MyResponse
        {
            RequestedId = req.Id
        });
    }
}
```

If there's only one type of outcome, set the **TResponse** generic parameter of the endpoint to the desired [IResult](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-7.0#iresult-return-values) type like so:

```csharp
public class MyEndpoint : EndpointWithoutRequest<NotFound>
{
    public override void Configure() { /* ... */ }

    public override async Task<NotFound> ExecuteAsync(CancellationToken ct)
    {
        await Task.CompletedTask;
        return TypedResults.NotFound();
    }
}
```

It is also possible to send **TypedResults** when using **HandleAsync()** as shown below with the use of **Send.ResultAsync()** method.

```csharp
public class MyEndpoint : Endpoint<MyRequest, Results<Ok<MyResponse>, NotFound>>
{
    public override void Configure() { /* ... */ }

    public override async Task HandleAsync(MyRequest r, CancellationToken c)
    {
        if (true)
            await Send.ResultAsync(TypedResults.Ok<MyResponse>(new(){ /* ... */ }));
        else
            await Send.ResultAsync(TypedResults.NotFound());
    }
}
```

---

## Configuring With Attributes

Instead of overriding the **Configure()** method, endpoint classes can be annotated with the following limited set of attributes:

*   **[Http{VERB}("/route")]** - sets up the verb and route
*   **[AllowAnonymous]** - allows un-authenticated access
*   **[AllowFileUploads]** - allows file uploads with multipart/form-data
*   **[Authorize(...)]** - specifies authorization requirements with roles and policies
*   **[Group<TGroup>]** - associates an endpoint with a [configuration group](configuration-settings#endpoint-configuration-groups)
*   **[PreProcessor<TProcessor>]** - adds a [pre-processor](pre-post-processors#pre-processors) to the pipeline
*   **[PostProcessor<TProcessor>]** - adds a [post-processor](pre-post-processors#post-processors) to the pipeline

```csharp
[HttpPost("/my-endpoint")]
[Authorize(Roles = "Admin,Manager")]
[PreProcessor<MyProcessor>]
public class MyEndpoint : Endpoint<MyRequest, MyResponse>
{
    /* ... */
}
```

Any other attributes you place on the endpoint class will be automatically added to the endpoint metadata collection which would be the equivalent of the following when [configuring endpoints](misc-conveniences#endpoint-options) inside the **Configure()** method:

```csharp
Options(b => b.WithMetadata(new MyCustomAttribute()));
```

INFO

Advanced usage however does require overriding **Configure()**. You can only use one of these strategies. An exception will be thrown if you use both or none at all. When using **Configure()**, any custom attributes on the endpoints are ignored. Use **WithMetadata** as shown above instead.

---

## Cancellation Token

The **HandleAsync** method of the endpoint is supplied a CancellationToken which you can pass down to your own async methods within the handler that requires a token.

The **Send.*Async** methods of the endpoint also optionally accepts a CancellationToken. I.e. you can either pass down the same token supplied to the HandleAsync method, or you may create/use a different token with these response sending methods depending on your requirement.

However, do note that it is not required to supply a CancellationToken to the **Send.*Async** methods, and there's no real need to muddy your code like the following:

```csharp
await Send.OkAsync(response, cancellation: ct);
```

Because if you do not supply the token to the **Send.*Async** methods, the library automatically supplies the same token that is supplied to the HandleAsync method internally, and your code can remain cleaner.

The analyzer hint/warning can be turned off by adding the following to your csproj file:

```xml
<NoWarn>CA2016</NoWarn>
```