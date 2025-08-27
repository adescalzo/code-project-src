```yaml
---
title: "Exception Handler | FastEndpoints"
source: https://fast-endpoints.com/docs/exception-handler#unhandled-exception-handler
date_published: unknown
date_captured: 2025-08-27T18:47:00.409Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, ASP.NET Core, System.Text.Json, GitHub]
programming_languages: [C#, JSON]
tags: [exception-handling, middleware, error-management, dotnet, web-api, logging, configuration, validation, fastendpoints]
key_concepts: [exception-handling, middleware, logging, http-status-codes, dependency-injection, validation, post-processor, configuration]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details the use and customization of the default exception handler middleware provided by the FastEndpoints library. It explains how to enable the middleware during application startup and configure ASP.NET Core logging to prevent duplicate entries. Examples of the user-friendly JSON response and server log entries are provided to illustrate its functionality. The content also covers disabling internal validation error catching, handling `ValidationFailureException`, and offers alternatives like custom middleware or Post-Processors for advanced scenarios. This guide helps developers manage unhandled exceptions and validation errors effectively in FastEndpoints applications.]
---
```

# Exception Handler | FastEndpoints

# Exception Handler

## Unhandled Exception Handler

The library ships with a default exception handler middleware you can use to log the exception details on the server and return a user-friendly HTTP 500 response to the requesting client.

## Example JSON Response:

```json
{
  "Status": "Internal Server Error!",
  "Code": 500,
  "Reason": "'x' is an invalid start of a value. Path: $.ValMin | LineNumber: 4...",
  "Note": "See application log for stack trace."
}
```

## Example Server Log Entry:

```
fail: FastEndpoints.ExceptionHandler[0]
      =================================
      HTTP: POST /inventory/adjust-stock
      TYPE: JsonException
      REASON: 'x' is an invalid start of a value. Path: $.ValMin | LineNumber: 4...
      ---------------------------------
         at System.Text.Json.ThrowHelper.ReThrowWithPath(ReadStack& state,...
         at System.Text.Json.Serialization.JsonConverter`1.ReadCore(Utf8JsonReader& reader,...
         at System.Text.Json.JsonSerializer.ReadCore[TValue](JsonConverter jsonConverter,...
         ...
```

## Enabling The Exception Handler

Enable the middleware as shown below during app startup.

Program.cs

```csharp
var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();

var app = bld.Build();
app.UseDefaultExceptionHandler()
   .UseFastEndpoints();
app.Run();
```

Disable the ASP.NET Core Diagnostic logging for unhandled exceptions in order to avoid duplicate log entries.

appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware": "None"
      //add this
    }
  }
}
```

## Customizing The Exception Handler

If you'd like to modify the behavior of the above middleware, you can simply copy the [source code](https://github.com/FastEndpoints/Library/blob/main/Src/Library/Extensions/ExceptionHandlerExtensions.cs) and register your version instead.

## Disable Validation Error Catching

By default, all validation errors thrown by the endpoints are caught internally and not thrown out to the middleware pipeline. If you need those thrown, you can instruct the endpoint to do so by doing the following:

MyEndpoint.cs

```csharp
public override void Configure()
{
    Get("throw-error");
    DontCatchExceptions();
}
```

Do note however that by doing so, you are effectively disabling the automatic error responses sent by the library. It would then be the responsibility of your custom exception handling middleware to handle the exceptions and send appropriate responses to the requesting client.

The thrown exception type is **ValidationFailureException**, which has a property called **Failures** where you can get access to a collection of **ValidationFailure** objects to check what sort of validation failures occurred.

Alternatively, instead of using a custom exception handling middleware, you could register a [Post-Processor](/docs/pre-post-processors#handling-unhandled-exceptions-with-post-processors) that takes care of exceptions.