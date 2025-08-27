```yaml
---
title: The state machine in C# with async/await
source: https://steven-giesel.com/blogPost/720a48fd-0abe-4c32-83ac-26926d501895?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=the-memento-design-pattern-in-c
date_published: 2024-07-22T07:57:00.000Z
date_captured: 2025-08-08T13:25:10.308Z
domain: steven-giesel.com
author: Unknown
category: ai_ml
technologies: [.NET, HttpClient, JsonSerializer, TaskScheduler, ASP.NET Core]
programming_languages: [C#]
tags: [async, await, state-machine, csharp, dotnet, concurrency, exception-handling, compiler-internals, asynchronous-programming, task-parallel-library]
key_concepts: [async/await, state machine, TaskScheduler, exception handling, asynchronous programming, compiler transformation, Task, SynchronizationContext, ThreadPool]
code_examples: false
difficulty_level: intermediate
summary: |
  This article delves into the internal mechanisms of C# `async`/`await` keywords, explaining how the compiler transforms asynchronous methods into a state machine. It illustrates how methods are "cut" at `await` points, allowing control to return to the caller and resume execution via the `TaskScheduler` once the awaited operation completes. The post also clarifies the behavior of exception handling within asynchronous code, detailing how exceptions are stored in the `Task` object rather than immediately bubbling up. It highlights the dangers of `async void` and unawaited `async Task` calls, providing simplified code examples to demystify these complex compiler behaviors.
---
```

# The state machine in C# with async/await

# The state machine in C# with async/await

## Table of Contents

*   [The state machine](#the-state-machine)
*   [Cutting the method along the await border](#cutting-the-method-along-the-await-border)
*   [TaskScheduler](#taskscheduler)
*   [Where is the state machine stored?](#where-is-the-state-machine-stored)
*   [Exceptions with async / await](#exceptions-with-async--await)
*   [But why does it bubble then?](#but-why-does-it-bubble-then)

You often hear that the `async`/`await` keywords leads to a state machine. But what does that mean? Let's discuss this with a simple example.

## The state machine

Let's consider the following code:

```csharp
async Task<Dto> GetAllAsync()
{
    using var client = new HttpClient();
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/pokemon/");
    var response = await client.GetAsync("");
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<Dto>(content);
}

public class Dto;
```

We have multiple `await` calls here - the first one to get the response and the second one to read the content. So what happens here?

### Cutting the method along the `await` border

Let's start with what `async` / `await` is all about: We want to utilize resources more efficiently by not blocking the caller while waiting for a result. So every time we call `await`, we know (given that it is I/O bound at least) that we have no work to do but wait for the result. So wouldn't it be nice to basically go out of the method in that moment and return once the result is there? And that is exactly the idea.

The compiler will cut the method along the `await` border and create a state machine. So you can imagine it like the following (I will oversimplify here):

```csharp
public class GetAllAsync_StateMachine
{
    public ContinuationMachine _builder = ContinuationMachineBuilder.Create();

    private int _state = 0;
    private HttpClient _client;
    private HttpResponseMessage _response;
    private string _content;

    private void MoveNext()
    {
        switch (_state)
        {
            case 0:
                _client = new HttpClient();
                _client.BaseAddress = new Uri("https://pokeapi.co/api/v2/pokemon/");
                _client.GetAsync("");
                _builder.Continue(ref this);
                break;
            case 1:
                _response.EnsureSuccessStatusCode();
                _response.Content.ReadAsStringAsync();
                _state = 2;
                _builder.Continue(ref this);
                break;
            case 2:
                return JsonSerializer.Deserialize<Dto>(_content);
        }
    }
}
```

This is a very simplified version of what the compiler does and leaves out vital parts like - how does the content be assigned from the `HttpClient`. The important part is that we synchronously call the bits between the `await` until the `await` and then use a mechanism via callbacks (that is why we use `ref this`) to continue the method. So once the HTTP call is done, we go back into the method and continue where we left off (state 1, and if that finishes then state 2 and so on).

### `TaskScheduler`

The thing you put your continuation on is the `TaskScheduler`. The `TaskScheduler` takes your state machine and schedules it to be executed once the awaited task is done. So the `TaskScheduler` is responsible for the continuation. There are more nifty bits in here like `SynchronizationContext` and `ThreadPools` but they are details.

So small recap: When using `async` / `await` the compiler will cut the method along the `await` border and create a state machine. This state machine will be scheduled by the `TaskScheduler` to continue once the awaited task is done.

## Where is the state machine stored?

That is very simple to answer: In general: `Task` or `Task<T>`. Yes that is where your stuff is stored. The current state (including the continuation) as well as the result of the awaited task. So the `Task` is the state machine.

But furthermore your `Task` object also stores exceptions. Exceptions flow a bit different with `async` / `await` than with synchronous code.

## Exceptions with `async` / `await`

Imagine the following code:

```csharp
static async Task ThrowExceptionAsync()
{
    await Task.Yield();
    throw new Exception("Hey");
}
```

The compiler does the following code out of it:

```csharp
try
{
    YieldAwaitable.YieldAwaiter awaiter;
    // Here is some other stuff
    awaiter.GetResult();
    throw new Exception("Hey");
}
catch (Exception exception)
{
    <>1__state = -2;
    <>t__builder.SetException(exception);
}
```

Important is to note that we have zero `throw`s inside the catch block. That means nothing will bubble up if we have an exception in the asynchronous part of the method. The exception will be stored in the `Task` object itself.

And maybe now you can see why `async void` is a bad idea in general: Your exceptions will be lost. They are there but you cannot catch them or handle them in any way. The same applies to `async Task` if you don't await the `Task` itself. So something like `_ = MyThrowingAsyncTask();`.

```csharp
static async Task ThrowExceptionAsync()
{
    throw new Exception("Hey");
    await SomethingAsync();
}

_ = ThrowExceptionAsync(); // Will cause an exception (but doesn't bubble)
```

So will:

```csharp
static async void ThrowExceptionAsync()
{
    throw new Exception("Hey");
    await SomethingAsync();
}

ThrowExceptionAsync(); // Will cause an exception (but doesn't bubble)
```

### But why does it bubble then?

We saw that an exception is caught and stored on the `Task` object without bubbling it up in any fashion. So why does it bubble up then or better: When does it throw then? The simple answer: When you `await` the `Task` object. The `await` call will be translated to something like `GetAwaiter().GetResult()` and that is where the exception is thrown from the `Task` object to you.

![C# code snippet showing an async Task method for signing in, using HttpContext.ChallengeAsync with authentication properties.](image.png)