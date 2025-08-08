```yaml
---
title: "📧 Understanding the Circuit Breaker Pattern - andresdescalzo@gmail.com - Gmail"
source: https://mail.google.com/mail/u/0/#section_query/(in%3Ainbox+OR+label%3A%5Eiim)+is%3Aunread/FMfcgzQVzFTSXbDmbhtQLsJJNkNrhnNs
date_published: unknown
date_captured: 2025-08-06T18:23:57.626Z
domain: mail.google.com
author: Unknown
category: frontend
technologies: [.NET, Polly, HttpClientFactory, Microsoft.Extensions.Http.Polly]
programming_languages: [C#]
tags: [circuit-breaker, distributed-systems, microservices, resilience, fault-tolerance, dotnet, design-patterns, error-handling, polly, cloud-native]
key_concepts: [circuit-breaker-pattern, distributed-systems, microservices-architecture, fault-tolerance, cascading-failures, transient-fault-errors, graceful-degradation, polly-library, httpclientfactory-integration]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive understanding of the Circuit Breaker pattern, a vital design pattern for enhancing resilience and stability in distributed systems, microservices, and cloud-native applications. It explains the pattern's three core states—Closed, Open, and Half-Open—and illustrates its functionality with a simple .NET implementation. The post emphasizes the pattern's role in preventing cascading failures and allowing services time to recover. Additionally, it introduces Polly, a robust .NET library, as a production-ready solution for implementing circuit breakers and other resilience strategies, including its integration with HttpClientFactory.
---
```

# 📧 Understanding the Circuit Breaker Pattern - andresdescalzo@gmail.com - Gmail

## Understanding the Circuit Breaker Pattern

Read time: 5 minutes

The Circuit Breaker pattern is important in distributed systems, microservices architecture, and cloud-native applications. It helps improve system resilience, stability, and fault tolerance.

Embark on a learning journey in this post, where you'll understand the Circuit Breaker pattern in a simple implementation and learn how and when to use it.

Content:

*   What is the Circuit Breaker Pattern?
*   Why is the Circuit Breaker Pattern Important?
*   Simple implementation in .NET
*   Polly
*   Summary

**What is the Circuit Breaker Pattern?**

The Circuit Breaker pattern is a design pattern used in software development to detect and handle system failures. It works similarly to an electrical circuit breaker, which automatically stops the flow of electricity in the event of an overload or short circuit. In software, the Circuit Breaker pattern stops requests to a failing service, preventing further failures and allowing the system to remain stable.

The Circuit Breaker pattern has three states:

*   **Closed State**: The circuit operates normally, and all requests pass through to the service.
*   **Open State**: After several failures, the circuit "opens," and all requests are immediately rejected.
*   **Half-Open State**: After a timeout, the circuit allows a limited number of requests to see if the service has recovered.

The Circuit Breaker acts as a proxy between the client and the microservice.

![Sequence diagram illustrating the interaction between Client, Circuit Breaker, and Microservice. The client sends a request to the Circuit Breaker, which then forwards it to the Microservice. The Microservice sends a response back to the Circuit Breaker, which then relays it to the Client.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723823374128-cb%20serquence%20diagram.png)

**Why is the Circuit Breaker Pattern Important?**

In distributed systems, you must expect communication between services to sometimes fail. The services can have timeouts or are temporarily unavailable. Network issues happen, especially in the clouds.

You can retry your request for transient fault errors. In many cases, it's enough.

However, some failures can require a longer time to fix, or the remote service could be overloaded. Then, request retries will not help. Moreover, such retries can cause cascading failures. A cascading failure in a system refers to a situation where the failure of one component or service triggers a chain reaction, leading to the failure of other components or services in the system.

The Circuit Breaker pattern prevents an application from sending requests likely to fail.

Here are some key reasons why this pattern is crucial:

1.  **Preventing Cascading Failures**: Stops failures from spreading throughout the system.
2.  **Improving System Stability**: Conserves resources by avoiding repeated requests to a failing service.
3.  **Providing Time for Recovery**: Gives the failing service time to recover before allowing more requests.
4.  **Faster Failure Detection**: Quickly detects failures and prevents long wait times for users.
5.  **Graceful Degradation**: Allows fallback mechanisms to keep the system running even when some services are down.

**Simple implementation in .NET**

Let's implement the simple Circuit Breaker to understand how it works better. The link to the code source is at the end of the post.

First, we create an enum for Circuit Breaker states.

![C# code defining the CircuitBreakerState enum with values Closed, Open, and HalfOpen.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723822707346-enum%20and%20exception.png)

The class Circuit Breaker looks as follows:

![C# code defining the CircuitBreaker class constructor and private fields for _lockObj, _failureCount, _lastFailureTime, and _state. It also declares public and private methods: Execute, HalfOpenPermitted, TryExecute, Reset, and HandleFailure.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723822721261-class.png)

There is only one public method - Execute. It takes Action to execute as a parameter.

Its implementation is to check if the current state is Open. If true, the status will change to HalfOpen after the timeout expires. Before the timeout expires, it does nothing. If the state is Closed or HalfOpen, it tries to execute the operation.

![C# code for the Execute method of the CircuitBreaker class, showing conditional logic based on the current state (Open, HalfOpen) and calling TryExecute or returning.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723822770539-execute.png)

The HalfOpenPermitted method checks if the Open timeout expired.

![C# code for the HalfOpenPermitted method, which checks if the current time is past the _lastFailureTime plus the _openTimeout, indicating if the circuit is allowed to transition to HalfOpen.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723823055892-halfopenpermitted.png)

The TryExecute method executes the operation.

![C# code for the TryExecute method, which attempts to execute the provided operation. If successful, it calls Reset(); if it catches an Exception, it calls HandleFailure() and rethrows the exception.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723823434958-try_execute.png)

If the operation fails, it increments the failure counter. If the threshold is reached, the state is changed to Open.

![C# code for the HandleFailure method, which increments the _failureCount. If _failureCount exceeds _failureThreshold, it sets _state to CircuitBreakerState.Open and records the _lastFailureTime.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723823520625-handle_failure.png)

If the operation succeeds, the failure counter is reset, and the state is changed to Closed.

![C# code for the Reset method, which sets _failureCount to 0 and _state to CircuitBreakerState.Closed.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723823669433-reset.png)

That's it. Now, we can see how we can use our implementation.

![C# code demonstrating the usage of the custom CircuitBreaker class, including creating an instance and calling its Execute method within a loop to simulate successful and failing operations.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723823842587-usage.png)

![Console output showing the behavior of the custom CircuitBreaker implementation, transitioning between Closed, Open, and Half-Open states based on simulated failures and successes.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723823869129-output.png)

Of course, this implementation is naive and not production-ready. My goal was just to show how Circuit Breaker should work by the example.

There are many issues you have to consider while implementing Circuit Breaker, such as logging and catching specific exceptions. Not every exception, such as a Bad Request, should be considered a transient error.

When testing a failed operation in the HalfOpen state, it's nice to ping the health check endpoint instead of sending the original request.

That's an incomplete list of considerations. However, the good news is that you don't have to worry about that.

**Polly**

[Polly](https://github.com/App-vNext/Polly?tab=readme-ov-file#circuit-breaker) is a .NET resilience and transient-fault-handling library that allows developers to express resilience strategies such as Retry, **Circuit Breaker**, Hedging, Timeout, Rate Limiter, and Fallback fluently and thread-safely.

Polly is well integrated with HttpClientFactory, so you can easily add Circuit Breaker to your clients. There is Microsoft.Extensions.Http.Polly package for that.

![C# code snippet showing how to add a Circuit Breaker policy using Polly with HttpClientFactory in a .NET application's service configuration.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723825824099-add%20polly.png)

![C# code snippet showing how to retrieve a Polly policy for use with HttpClient.](https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1723825837119-get%20polly.png)

**Summary**

The Circuit Breaker pattern is a critical tool for anyone building distributed systems. It helps prevent cascading failures, improves system stability, and allows for graceful degradation when services fail.

If you're building a distributed system or working with microservices, consider using the Circuit Breaker pattern to safeguard your application from the inevitable failures in a complex, interconnected environment.

The source code you can find on my [GitHub](https://github.com/okyrylchuk/dotnet-newsletter/tree/main/SimpleCircuitBreaker/SimpleCircuitBreaker).

Thank you for reading!