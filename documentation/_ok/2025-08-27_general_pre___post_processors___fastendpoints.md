```yaml
---
title: "Pre / Post Processors | FastEndpoints"
source: https://fast-endpoints.com/docs/pre-post-processors#pre-processors
date_published: unknown
date_captured: 2025-08-27T15:56:13.002Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, ASP.NET Core, .NET, Stopwatch]
programming_languages: [C#]
tags: [pre-processor, post-processor, fastendpoints, aspnet-core, request-lifecycle, logging, error-handling, dependency-injection, state-management, web-api]
key_concepts: [pre-processing, post-processing, request-pipeline, short-circuiting, global-processors, state-sharing, exception-handling, dependency-injection, endpoint-filters]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details the implementation and usage of pre and post processors within the FastEndpoints framework for ASP.NET Core applications. It explains how to execute common logic before or after an endpoint's main handler, covering use cases like request logging, security checks, and response modification. The content also demonstrates advanced features such as short-circuiting requests, handling unhandled exceptions, sharing state between processors and endpoints, and configuring global or open generic processors. It also touches upon dependency injection and offers endpoint filters as an alternative approach.]
---
```

# Pre / Post Processors | FastEndpoints

# Pre / Post Processors

Rather than writing a common piece of logic repeatedly that must be executed either before or after each request to your system, you can write it as a pre or post processor and attach it to endpoints that need them.

There are two types of processors:

*   **Pre Processors**
*   **Post Processors**

## Pre Processors

Let's say for example that you'd like to log every request before being executed by your endpoint handlers. Simply write a pre-processor by implementing the interface **IPreProcessor<TRequest>**:

MyRequestLogger.cs

```csharp
public class MyRequestLogger<TRequest> : IPreProcessor<TRequest>
{
    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        var logger = ctx.HttpContext.Resolve<ILogger<TRequest>>();

        logger.LogInformation(
            $"request:{ctx.Request.GetType().FullName} path: {ctx.HttpContext.Request.Path}");

        return Task.CompletedTask;
    }
}
```

And then attach it to the endpoints you need like so:

CreateOrderEndpoint.cs

```csharp
public class CreateOrderEndpoint : Endpoint<CreateOrderRequest>
{
    public override void Configure()
    {
        Post("/sales/orders/create");
        PreProcessor<MyRequestLogger<CreateOrderRequest>>();
    }
}
```

You can even write a request DTO specific processor like so:

SalesRequestLogger.cs

```csharp
public class SalesRequestLogger : IPreProcessor<CreateSaleRequest>
{
    public Task PreProcessAsync(IPreProcessorContext<CreateSaleRequest> ctx, CancellationToken ct)
    {
        var logger = ctx.HttpContext.Resolve<ILogger<CreateSaleRequest>>();

        logger.LogInformation($"sale value:{ctx.Request.SaleValue}");

        return Task.CompletedTask;
    }
}
```

## Short-Circuiting Execution

It is possible to end processing the request by returning a response from within a pre-processor like so:

SecurityProcessor.cs

```csharp
public class SecurityProcessor<TRequest> : IPreProcessor<TRequest>
{
    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        if (!ctx.HttpContext.Request.Headers.TryGetValue("x-tenant-id", out var tenantID))
        {
            ctx.ValidationFailures.Add(
                new("MissingHeaders", "The [x-tenant-id] header needs to be set!"));

            //sending response here
            return ctx.HttpContext.Response.SendErrorsAsync(ctx.ValidationFailures);
        }

        if (tenantID != "001")
            return ctx.HttpContext.Response.SendForbiddenAsync(); //sending response here

        return Task.CompletedTask;
    }
}
```

All the [Send.*Async methods](misc-conveniences#send-methods) supported by endpoint handlers are available. The send methods are accessed from the **ctx.HttpContext.Response** property as shown above. When a response is sent from a pre-processor, the handler method is not executed.

NOTE

If there are multiple pre-processors configured, they will be executed. If another pre-processor also wants to send a response, they must check if it's possible to do so by checking the result of **ctx.HttpContext.ResponseStarted()** to see if a previously executed pre-processor has already sent a response to the client. See example [here](#global-processors).

## Post Processors

Post-processors are executed after your endpoint handler has completed its work. They can be created similarly by implementing the interface **IPostProcessor<TRequest, TResponse>**:

```csharp
public class MyResponseLogger<TRequest, TResponse> : IPostProcessor<TRequest, TResponse>
{
    public Task PostProcessAsync(IPostProcessorContext<TRequest, TResponse> ctx, ...)
    {
        var logger = ctx.HttpContext.Resolve<ILogger<TResponse>>();

        if (ctx.Response is CreateSaleResponse response)
            logger.LogWarning($"sale complete: {response.OrderID}");

        return Task.CompletedTask;
    }
}
```

And then attach it to endpoints like so:

```csharp
public class CreateOrderEndpoint : Endpoint<CreateSaleResponse, CreateSaleResponse>
{
    public override void Configure()
    {
        Post("/sales/orders/create");
        PostProcessor<MyResponseLogger<CreateSaleResponse, CreateSaleResponse>>();
    }
}
```

#### Handling unhandled exceptions with Post-Processors

Post processors have access to unhandled exceptions that typically result in an automatic 500 response. This can be used as an alternative to an [exception handing middleware](exception-handler#unhandled-exception-handler). Take the following endpoint for an example:

```csharp
public class WipEndpoint : Endpoint<Request, Response>
{
    public override void Configure()
    {
        ...
        PostProcessor<ExceptionProcessor>();
    }

    public override Task HandleAsync(Request r, CancellationToken c)
        => throw new NotImplementedException();
}
```

For which you can implement a post-processor like this:

```csharp
public class ExceptionProcessor : IPostProcessor<Request, Response>
{
    public async Task PostProcessAsync(IPostProcessorContext<Request, Response> ctx, ...)
    {
        if (!ctx.HasExceptionOccurred)
            return;

        if (ctx.ExceptionDispatchInfo.SourceException.GetType() == typeof(NotImplementedException))
        {
            ctx.MarkExceptionAsHandled(); //only if handling the exception here.
        
            await ctx.HttpContext.Response.SendAsync("This endpoint is not implemented yet!", 501);
            return;
        }

        ctx.ExceptionDispatchInfo.Throw();
    }
}
```

The post-processor context has a property for accessing the captured [ExceptionDispatchInfo](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.exceptionservices.exceptiondispatchinfo) instance with which you get full access to the exception details. Calling the **MarkExceptionAsHandled()** method is only necessary if your post-processor is handling the exception and no further action is necessary to deal with that exception. Not calling that method will result in an automatic throwing of the captured exception once all the post-processors have run.

#### Abstracting response sending logic into a Post-Processor

A post-processor can be made the sole mechanism that decides what kind of response needs to be sent, such as when using the "Results Pattern". I.e. instead of placing response sending logic inside the endpoint handler itself, you can return whatever object you want from the endpoint handler method and let a processor receive that object as input and allow it to decide how/what shape of response is to be sent to the client. [Click here](https://gist.github.com/dj-nitehawk/6e23842dcb7640b165fd80ba57967540) to see a full example of this in action.

#### Intercepting responses before being sent

Post-processors run after the endpoint handler has sent the response to the client. As a result, they cannot modify response headers or write to the response stream. If you need to apply common logic to multiple endpoints right before the response is written to the stream, consider using a [Response Interceptor](https://gist.github.com/dj-nitehawk/7cef738cf5c0d5a26981524df9228349) or the less explicit [Global Response Modifier](https://gist.github.com/dj-nitehawk/be15f1125cafc4ddd1c233eca26c0a8a).

## Multiple Processors

Multiple processors can be attached to an endpoint with both **PreProcessor<>()** and **PostProcessor<>()** methods by calling them repeatedly. The processors are executed in the order they are specified in the endpoint configuration.

## Global Processors

Global pre/post processors that operate on multiple endpoints can be created implementing the **IGlobalPreProcessor** & **IGlobalPostProcessor** interfaces.

TenantIDChecker.cs

```csharp
public class TenantIDChecker : IGlobalPreProcessor
{
    public async Task PreProcessAsync(IPreProcessorContext ctx, CancellationToken ct)
    {
        if (ctx.Request is MyRequest r) //can work on specific dto types if desired
        {
            var tID = ctx.HttpContext.Request.Headers["x-tenant-id"];

            if (tID.Count > 0)
            {
                r.TenantID = tID[0];
            }
            else
            {
                ctx.ValidationFailures.Add(
                    new("TenantID", "Unable to retrieve tenant id from header!"));
                
                if (!ctx.HttpContext.ResponseStarted())
                    await ctx.HttpContext.Response.SendErrorsAsync(ctx.ValidationFailures);
            }
        }
    }
}
```

To attach the above pre-processor to all endpoints, add it in the global endpoint configurator function like so:

Program.cs

```csharp
app.UseFastEndpoints(c =>
{
    c.Endpoints.Configurator = ep =>
    {
        ep.PreProcessor<TenantIDChecker>(Order.Before);
    };
});
```

The **Order** enum specifies whether to run the global processors before or after the endpoint level processors if there's also endpoint level processors configured.

#### Open generic global processors

Open generic processors that implement either **IPreProcessor<TRequest>** or  
**IPostProcessor<TRequest, TResponse>** can be registered globally using the endpoint configurator function as follows:

Program.cs

```csharp
app.UseFastEndpoints(c =>
{
    c.Endpoints.Configurator = ep =>
    {
        ep.PreProcessors(Order.Before, typeof(RequestLogger<>));
    };
});
```

RequestLogger.cs

```csharp
sealed class RequestLogger<TRequest> : IPreProcessor<TRequest>
{
    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken c)
    {
        ...
    }
}
```

## Sharing State

In order to share state among pre/post processors and the endpoint, you have to first create a class to act as the state holder such as the following:

```csharp
public class MyStateBag
{
    private readonly Stopwatch _sw = new();

    public bool IsValidAge { get; set; }
    public string Status { get; set; }
    public long DurationMillis => _sw.ElapsedMilliseconds;

    public MyStateBag() => _sw.Start();
}
```

Then create processors implementing the following abstract classes instead of the interfaces mentioned above. They will have the state bag passed in to the process method like so:

```csharp
public class AgeChecker : PreProcessor<MyRequest, MyStateBag>
{
    public override Task PreProcessAsync(IPreProcessorContext<MyRequest> ctx, MyStateBag state)
    {
        if (ctx.Request.Age >= 18)
            state.IsValidAge = true;

        state.Status = $"age checked by pre-processor at {state.DurationMillis} ms.";

        return Task.CompletedTask;
    }
}
```

```csharp
public class DurationLogger : PostProcessor<MyRequest, MyStateBag, object>
{
    public override Task PostProcessAsync(IPostProcessorContext<MyRequest, object> ctx, 
                                          MyStateBag state, 
                                          CancellationToken ct)
    {
        ctx.HttpContext.Resolve<ILogger<DurationLogger>>()
           .LogInformation("request took {@duration} ms.", state.DurationMillis);

        return Task.CompletedTask;
    }
}
```

The endpoint is able to access the same shared/common state by calling the **ProcessorState<MyStateBag>()** method like so:

```csharp
public class MyEndpoint : Endpoint<MyRequest>
{
    public override void Configure()
    {
        ...
        PreProcessor<AgeChecker>();
        PostProcessor<DurationLogger>();
    }

    public override async Task HandleAsync(MyRequest r, CancellationToken c)
    {
        var state = ProcessorState<MyStateBag>();
        Logger.LogInformation("endpoint executed at {@duration} ms.", state.DurationMillis);
        await Task.Delay(100);
        await Send.OkAsync(
            new
            {
                r.Age,
                state.Status
            });
    }
}
```

It's also possible to access the common state when using the processor interfaces, but it has to be done via the http context like so:

```csharp
public class MyPreProcessor : IPreProcessor<Request>
{
    public Task PreProcessAsync(IPreProcessorContext<Request> ctx, CancellationToken ct)
    {
        var state = ctx.HttpContext.ProcessorState<MyStateBag>();
    }
}
```

For global processors, you can implement the **GlobalPreProcessor<TState>** and **GlobalPostProcessor<TState>** abstract classes instead.

## Dependency Injection

Processors are singletons for [performance reasons](/benchmarks). I.e. there will only ever be one instance of a processor. You should not maintain state in them. Dependencies can be resolved as shown [here](dependency-injection#pre-post-processor-dependencies).

---

TIP

As an alternative to pre/post processors, you have the option of using Minimal APIs [endpoint filters](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters?view=aspnetcore-7.0) with FastEndpoints as [shown here](https://gist.github.com/dj-nitehawk/3edcd59ce03230b98369e2f2259bc5d3).

---

Previous [<- Swagger Support](/docs/swagger-support)

Next [Event Bus ->](/docs/event-bus)

© FastEndpoints 2025