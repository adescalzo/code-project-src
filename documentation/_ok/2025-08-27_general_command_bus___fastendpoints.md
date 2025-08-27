```yaml
---
title: "Command Bus | FastEndpoints"
source: https://fast-endpoints.com/docs/command-bus#_1-define-a-command
date_published: unknown
date_captured: 2025-08-27T16:34:36.727Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, .NET]
programming_languages: [C#, JSON]
tags: [command-bus, design-pattern, decoupling, csharp, fastendpoints, middleware, dependency-injection, error-handling, validation, generics]
key_concepts: [Command Bus Pattern, Command Handler, Event Bus, Chain of Responsibility Pattern, Dependency Injection, Generic Programming, Error Handling, Validation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the In-Process Command Bus pattern, a decoupled, command-driven approach where each command has a single handler. It details how to define commands using `ICommand` interfaces and implement their corresponding handlers with `ICommandHandler`, demonstrating both synchronous and asynchronous execution. The content also covers handling generic commands and handlers, and manipulating endpoint error states directly from command handlers. Furthermore, it explains the implementation of a command middleware pipeline using the Chain of Responsibility pattern for cross-cutting concerns like logging and validation. The article provides practical C# code examples for each concept, illustrating how to build robust and maintainable command-based systems, particularly within the FastEndpoints framework.]
---
```

# Command Bus | FastEndpoints

# In-Process Command Bus Pattern

Similarly to the [Event Bus](/docs/event-bus), you can take a decoupled, command driven approach with the distinction that a command can only have a single handler which may or may not return a result. Whereas an event can have many handlers and they cannot return results back to the publisher.

## 1. Define A Command

This is the data contract that will be handed to the command handler. Mark the class with either the **ICommand** or **ICommand<TResult>** interface in order to make any class a command. Use the former if no result is expected and the latter if a result is expected back from the handler.

```csharp
public class GetFullName : ICommand<string>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

## 2. Define A Command Handler

This is the code that will be executed when a command of the above type is executed. Implement either the **ICommandHandler<TCommand, TResult>** or **ICommandHandler<TCommand>** interface depending on whether a result needs to be returned or not.

```csharp
public class FullNameHandler : ICommandHandler<GetFullName, string>
{
    public Task<string> ExecuteAsync(GetFullName command, CancellationToken ct)
    {
        var result = command.FirstName + " " + command.LastName;
        return Task.FromResult(result);
    }
}
```

## 3. Execute The Command

Simply call the **ExecuteAsync()** extension method on the command object.

```csharp
var fullName = await new GetFullName()
{
    FirstName = "john",
    LastName = "snow"
}
.ExecuteAsync();
```

## Generic Commands & Handlers

Generic commands & handlers require a bit of special handling. Say for example, you have a generic command type and a generic handler that's supposed to handle that generic command such as the following:

```csharp
//command
public class MyCommand<T> : ICommand<IEnumerable<T>> { ... }

//handler
public class MyCommandHandler<T> : ICommandHandler<MyCommand<T>, IEnumerable<T>> { ... }
```

In order to make this work, you need to register the association between the two with open generic types like so:

```csharp
app.Services.RegisterGenericCommand(typeof(MyCommand<>), typeof(MyCommandHandler<>));
```

Once registered, it's business as usual and you can execute generic commands such as this:

```csharp
var results = await new MyCommand<SomeType>().ExecuteAsync();
var results = await new MyCommand<AnotherType>().ExecuteAsync();
```

## Manipulating Endpoint Error State

By implementing command handlers using the **CommandHandler<>** abstract types instead of the interfaces mentioned above, you are able to manipulate the validation/error state of the endpoint that issued the command like so:

GetFullNameEndpoint.cs

```csharp
public class GetFullNameEndpoint : EndpointWithoutRequest<string>
{
    ...

    public override async Task HandleAsync(CancellationToken c)
    {
        AddError("an error added by the endpoint!");

        //command handler will be adding/throwing it's own validation errors
        Response = await new GetFullName
        {
            FirstName = "yoda",
            LastName = "minch"
        }.ExecuteAsync();
    }
}
```

FullNameHandler.cs

```csharp
public class FullNameHandler : CommandHandler<GetFullName, string>
{
    public override Task<string> ExecuteAsync(GetFullName cmd, CancellationToken ct = default)
    {
        if (cmd.FirstName.Length < 5)
            AddError(c => c.FirstName, "first name is too short!");

        if (cmd.FirstName == "yoda")
            ThrowError("no jedi allowed here!");

        ThrowIfAnyErrors();

        return Task.FromResult(cmd.FirstName + " " + cmd.LastName);
    }
}
```

In this particular case, the client will receive the following error response:

```json
{
  "statusCode": 400,
  "message": "One or more errors occured!",
  "errors": {
    "generalErrors": [
      "an error added by the endpoint!",
      "no jedi allowed here!"
    ],
    "firstName": [
      "first name is too short!"
    ]
  }
}
```

## Command Middleware Pipeline

If you'd like to make use of the [Chain Of Responsibility Pattern](https://deviq.com/design-patterns/chain-of-responsibility-pattern), middleware components can be made to wrap around the command handlers in a layered fashion. Rather than calling a command handler directly, the execution of a command is passed through a pipeline of middleware components. Each middleware piece can execute common logic such as logging, validation, error handling, etc. **_before_** and **_after_** invoking the next piece of middleware in the chain.

Create open-generic middleware components implementing the **ICommandMiddleware<TCommand, TResult>** interface:

```csharp
sealed class CommandLogger<TCommand, TResult>(ILogger<TCommand> logger)
    : ICommandMiddleware<TCommand, TResult> where TCommand : ICommand<TResult>
{
    public async Task<TResult> ExecuteAsync(TCommand command, 
                                            CommandDelegate<TResult> next, 
                                            CancellationToken ct)
    {
        logger.LogInformation("Executing command: {name}", command.GetType().Name);

        var result = await next();

        logger.LogInformation("Got result: {value}", result);

        return result;
    }
}
```

Then register each component in the exact order you need them executed:

```csharp
bld.Services.AddCommandMiddleware(
    c =>
    {
        c.Register(typeof(CommandLogger<,>), 
                   typeof(CommandValidator<,>),
                   typeof(ResultLogger<,>));        
    });
```

Closed generic middleware can be written like so:

```csharp
sealed class ClosedGenericMiddleware : ICommandMiddleware<MyCommand, string>
{
    // ... implementation
}
```

Closed generic middleware needs to be registered with the generic overload of the **Register()** method:

```csharp
bld.Services.AddCommandMiddleware(c => c.Register<MyCommand, string, ClosedGenericMiddleware>());
```

## Dependency Injection

Dependencies in command handlers can be resolved as described [here](/docs/dependency-injection#command-handler-dependencies).

---

Previous [<- Event Bus](/docs/event-bus)

Next [Job Queues ->](/docs/job-queues)

© FastEndpoints 2025