```yaml
---
title: Understanding the Circuit Breaker Pattern
source: https://okyrylchuk.dev/blog/understanding-the-circuit-breaker-pattern/
date_published: 2024-08-16T07:31:40.000Z
date_captured: 2025-08-20T21:15:41.117Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: frontend
technologies: [.NET, Polly, HttpClientFactory, Microsoft.Extensions.Http.Polly, GitHub]
programming_languages: [C#]
tags: [circuit-breaker, resilience, distributed-systems, microservices, fault-tolerance, dotnet, design-patterns, polly, csharp, system-stability]
key_concepts: [Circuit Breaker pattern, Distributed Systems, Microservices Architecture, Fault Tolerance, System Resilience, Cascading Failures, Transient Fault Handling, Design Patterns]
code_examples: true
difficulty_level: intermediate
summary: |
  This article provides a comprehensive overview of the Circuit Breaker pattern, highlighting its importance in distributed systems, microservices, and cloud-native applications for enhancing resilience and fault tolerance. It explains the pattern's three states—Closed, Open, and Half-Open—and illustrates how it prevents cascading failures. The post includes a simple, step-by-step implementation of the Circuit Breaker in .NET using C# to demonstrate its core mechanics. Furthermore, it introduces Polly, a popular .NET library for transient-fault handling, showing how to integrate it with HttpClientFactory for practical use. The author emphasizes the pattern's crucial role in safeguarding applications from inevitable failures in complex, interconnected environments.
---
```

# Understanding the Circuit Breaker Pattern

# Understanding the Circuit Breaker Pattern

![Oleg Kyrylchuk's avatar](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)By [Oleg Kyrylchuk](https://okyrylchuk.dev/blog/author/kyrylchuk-oleggmail-com/ "View all posts by Oleg Kyrylchuk") / August 16, 2024

The Circuit Breaker pattern is important in distributed systems, microservices architecture, and cloud-native applications. It helps improve system resilience, stability, and fault tolerance.

Embark on a learning journey in this post, where you’ll understand the Circuit Breaker pattern in a simple implementation and learn how and when to use it. 

Table Of Contents

1.  [What is the Circuit Breaker?](#what-is-the-circuit-breaker)
2.  [Why is Circuit Breaker Pattern Important?](#why-is-circuit-breaker-pattern-important)
3.  [Simple implementation in .NET](#simple-implementation-in-net)
4.  [Polly](#polly)
5.  [Summary](#summary)

## What is the Circuit Breaker?

The Circuit Breaker pattern is a design pattern used in software development to detect and handle system failures. It works similarly to an electrical circuit breaker, which automatically stops the flow of electricity in the event of an overload or short circuit. In software, the Circuit Breaker pattern stops requests to a failing service, preventing further failures and allowing the system to remain stable.

The Circuit Breaker pattern has three states:

*   **Closed State**: The circuit operates normally, and all requests pass through to the service.
*   **Open State**: After several failures, the circuit “opens,” and all requests are immediately rejected.
*   **Half-Open State****:** After a timeout, the circuit allows a limited number of requests to see if the service has recovered.

The Circuit Breaker acts as a proxy between the client and the microservice. 

![Sequence diagram illustrating the interaction flow between a Client, a Circuit Breaker, and a Microservice.](https://ci3.googleusercontent.com/meips/ADKq_NZ2jJhOcdN3kidu5z0jUsLBsy7m8T8xVfCHrAJueMeJRm16YGfmRd6Y7DHitaAEmiD7yuQ834kI6oYdztyrtiDN-JFPfMLi8Ysq0KvUtyWwPnqeD_GiJTxoIp7nW4thkPetKlpPWVTQstUW3Vyqpgvd9ZHYAPZahser_crv87l41cQCn9S-1apcrFiqhswf6gwPqX_iTb6=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723823374128-cb%20serquence%20diagram.png)

## Why is Circuit Breaker Pattern Important?

In distributed systems, you must expect communication between services to sometimes fail. The services can have timeouts or are temporarily unavailable. Network issues happen, especially in the clouds.

You can retry your request for transient fault errors. In many cases, it’s enough.

However, some failures can require longer fixing, or the remote service could be overloaded. Then, request retries will not help. Moreover, such retries can cause cascading failures. A cascading failure in a system refers to a situation where the failure of one component or service triggers a chain reaction, leading to the failure of other components or services in the system.

The Circuit Breaker pattern prevents an application from sending requests likely to fail.

*   Here are some key reasons why this pattern is crucial:
    *   **Preventing Cascading Failures**: Stops failures from spreading throughout the system.
    *   **Improving System Stability**: Conserves resources by avoiding repeated requests to a failing service.
    *   **Providing Time for Recovery**: Gives the failing service time to recover before allowing more requests.
    *   **Faster Failure Detection**: Quickly detects failures and prevents long wait times for users.
    *   **Graceful Degradation:** Allows fallback mechanisms to keep the system running even when some services are down.

## Simple implementation in .NET

Let’s implement the simple Circuit Breaker to understand how it works better. The link to the code source is at the end of the post. 

First, we create an enum for Circuit Breaker states. 

```csharp
public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}
```

The class Circuit Breaker looks as follows: 

```csharp
public class CircuitBreaker(int failureThreshold, TimeSpan openTimeout)
{
    private readonly object _lockObj = new();

    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitBreakerState _state = CircuitBreakerState.Closed;

    public void Execute(Action operation)

    private bool HalfOpenPermitted() 
    private void TryExecute(Action operation)
    private void Reset()
    private void HandleFailure()
}
```

There is only one public method – Execute. It takes Action to execute as a parameter.

Its implementation is to check if the current state is Open. If true, the status will change to HalfOpen after the timeout expires. Before the timeout expires, it does nothing. If the state is Closed or HalfOpen, it tries to execute the operation. 

```csharp
public void Execute(Action operation)
{
    if (_state == CircuitBreakerState.Open && HalfOpenPermitted())
    {
        lock (_lockObj)
            _state = CircuitBreakerState.HalfOpen;
        Console.WriteLine("Circuit breaker is half open.");
    }
    else if (_state == CircuitBreakerState.Open && !HalfOpenPermitted())
    {
        Console.WriteLine("Circuit breaker is open.");
        return;
    }

    if (_state == CircuitBreakerState.HalfOpen)
    {
        TryExecute(operation);
        return;
    }

    TryExecute(operation);
}
```

The HalfOpenPermitted method checks if the Open timeout expired. 

```csharp
private bool HalfOpenPermitted() => DateTime.UtcNow - _lastFailureTime > openTimeout;
```

The TryExecute method executes the operation.

```csharp
private void TryExecute(Action operation)
{
   try
   {
       operation();
       Reset();
   }
   catch (Exception)
    {
       HandleFailure();
       throw;
   }
 }
```

If the operation fails, it increments the failure counter. If the threshold is reached, the state is changed to Open.

```csharp
private void HandleFailure()
{
    lock (_lockObj)
    {
        _failureCount++;
        if (_failureCount < failureThreshold)
            return;

        _state = CircuitBreakerState.Open;
        _lastFailureTime = DateTime.UtcNow;
    }
}
```

If the operation succeeds, the failure counter is reset, and the state is changed to Closed. 

```csharp
private void Reset()
{
    lock (_lockObj)
    {
        _failureCount = 0;
        _state = CircuitBreakerState.Closed;
    }
}
```

That’s it. Now, we can see how we can use our implementation. 

```csharp
var circuitBreaker = new CircuitBreaker(
            failureThreshold: 3,
            openTimeout: TimeSpan.FromSeconds(3)
        );

for (int i = 0; i < 10; i++)
{
    try
    {
        circuitBreaker.Execute(PerformOperation);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

    Thread.Sleep(1000);
}

static void PerformOperation()
{
    throw new Exception("Simulated operation failure");
}
```

The execution result:

![Console output showing the execution flow of the Circuit Breaker, including state changes (open, half-open) and simulated operation failures.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI0MzQiIGhlaWdodD0iMzE4IiB2aWV3Qm94PSIwIDAgNDM0IDMxOCI+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgc3R5bGU9ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)

Of course, this implementation is naive and not production-ready. My goal was just to show how Circuit Breaker should work by the example.

You must consider many issues while implementing a Circuit Breaker, such as logging and catching specific exceptions. Not every exception, such as a Bad Request, should be considered a transient error.

When testing a failed operation in the HalfOpen state, it’s nice to ping the health check endpoint instead of sending the original request.

That’s an incomplete list of considerations. The good news is that you don’t have to worry about it. 

## Polly

[Polly](https://github.com/App-vNext/Polly "Polly") is a .NET resilience and transient-fault-handling library that allows developers to express resilience strategies such as Retry,  **Circuit Breaker**, Hedging, Timeout, Rate Limiter, and Fallback fluently and thread-safely.

Polly is well integrated with HttpClientFactory, so you can easily add Circuit Breaker to your clients. There is a Microsoft Extensions.Http.Polly package for that. 

```csharp
var circuitBreakerPolicy = GetCircuitBreakerPolicy();

builder.Services.AddHttpClient<MyApiService>()
    .AddPolicyHandler(circuitBreakerPolicy);

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{