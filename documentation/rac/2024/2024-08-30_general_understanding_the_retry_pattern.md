```yaml
---
title: Understanding the Retry Pattern
source: https://okyrylchuk.dev/blog/understanding-the-retry-pattern/
date_published: 2024-08-30T15:38:18.000Z
date_captured: 2025-08-20T21:16:06.830Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET, Polly, HttpClientFactory, Microsoft.Extensions.Http.Polly, Entity Framework Core, SQL Server, Azure SQL, Distributed Systems, Microservices, Cloud Services]
programming_languages: [C#, SQL]
tags: [retry-pattern, resilience, fault-tolerance, distributed-systems, microservices, cloud-computing, csharp, dotnet, design-patterns, error-handling]
key_concepts: [Retry Pattern, Transient Failures, Exponential Backoff, Circuit Breaker Pattern, Idempotency, Optimistic Concurrency, Database Connection Resiliency, HttpClientFactory integration]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores the Retry pattern, a crucial design pattern for building resilient applications in distributed systems by handling transient failures. It explains when to effectively apply the pattern, such as for network issues, external API calls, and database connection failures, while also highlighting scenarios to avoid. The post provides a practical C# implementation of a RetryHelper class, demonstrating exponential backoff. Additionally, it discusses the synergy between the Retry and Circuit Breaker patterns and showcases how popular .NET libraries like Polly and Entity Framework Core integrate retry capabilities for HTTP requests and database operations, respectively.
---
```

# Understanding the Retry Pattern

# Understanding the Retry Pattern

In today’s world of distributed systems, microservices, and cloud services, failures are inevitable. But not all failures are created equal — some are transient and will resolve themselves if given a second (or third) chance. The Retry pattern is a simple yet powerful approach to handling these kinds of transient failures.

This blog post will explore the Retry pattern, when to use it, and how to implement it effectively in C#.

## What is the Retry Pattern?

The Retry pattern is a design pattern that automatically retries a failed operation several times before giving up. It’s beneficial for handling transient failures — temporary issues that will likely be resolved if the operation is attempted again after a short delay.

The key idea behind this pattern is to provide resilience in your applications by automatically recovering from temporary failures.

## **When to Use the Retry Pattern**

The Retry pattern isn’t a silver bullet; it’s important to know when it makes sense to use it.

Here are some scenarios where the Retry pattern is particularly beneficial:

1.  **Network Issues**: Temporary network outages or slowdowns can cause failures. Retrying the operation can help in recovering from these temporary issues.
2.  **External API Calls**: Third-party APIs might be temporarily unavailable or impose rate limits. Retrying the request after a delay might allow the request to succeed once the API becomes available again or the rate limit resets.
3.  **Database Connection Failures**: Sometimes, a database might be temporarily overloaded or unreachable. Retrying the connection can succeed once the issue resolves.
4.  **Optimistic Concurrency**: In systems using optimistic concurrency, conflicts might occur when multiple processes try to update the same resource simultaneously. Retrying the operation after a conflict can help in achieving eventual consistency.

However, it’s important to avoid using the Retry pattern in situations where failures are not transient, such as:

*   **Permanent failures**: Incorrect input, authentication failures, or configuration issues.
*   **Non-idempotent operations**: Retrying non-idempotent operations can lead to undesired side effects, such as duplicate transactions.
*   **Latency-sensitive applications**: In scenarios where low latency is critical, retries can introduce unacceptable delays.

## **Simple Implementation in C#**

Let’s create a simple implementation to understand better how the Retry pattern works.

We’ll create a RetryHelper class with one method, Execute, taking Action as a parameter. The constructor takes three parameters: maxRetries, initialDelay, and maxDelay. 

```csharp
public class RetryHelper(int maxRetries, int initialDelay, int maxDelay)
{
    public void Execute(Action action)
    {
        int attempts = 0;
        int delay = initialDelay;

        while (attempts < maxRetries)
        {
            try
            {
                attempts++;
                Console.WriteLine($"Attempt # {attempts}");
                action();

                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                if (attempts >= maxRetries)
                {
                    Console.WriteLine("Retry helper stopped.");
                    throw;
                }

                Console.WriteLine($"Wait for {delay} ms");
                Thread.Sleep(delay);


                delay = Math.Min(delay * 2, maxDelay);
            }
        }
    }
}
```

The maxRetries is the maximum number of retries we can make. If this fails, we must stop trying the operation.

The intialDelay is the initial delay between retries. When the operation fails, we wait some time before retrying the operation. We can do the same delay between retries.

Or, the delay can be increased incrementally or exponentially, depending on the type of failure and the probability that it will be corrected during this time.

The delay increases after every operation failure, so we have a maximum delay parameter we cannot overcome. 

If the operation fails, we repeat it in the loop until we reach maximum retries. After reaching the maximum number of retries, the RetryHelper must stop retries.

After each operation fails, the delay doubles. However, we cannot increase the delay more than the max delay parameter. 

Let’s see the RetryHelper usage. 

```csharp
var retryHelper = new RetryHelper(
            maxRetries: 5,
            initialDelay: 500,
            maxDelay: 5000
        );

retryHelper.Execute(PerformOperation);

static void PerformOperation()
{
    throw new Exception("Simulated operation failure");
}
```

After running the application, you’ll see the following output.

![Output of the RetryHelper showing multiple attempts and delays.](placeholder-output-screenshot.png)

## **Compare to Circuit Breaker**

The Circuit Breaker pattern serves a different purpose than the Retry pattern. While the Retry pattern allows an application to attempt an operation multiple times, hoping it will eventually succeed, the Circuit Breaker pattern stops an application from attempting an operation that is likely to fail. 

These two patterns can be combined effectively by using the Retry pattern to call an operation through a circuit breaker, ensuring retries are only attempted when it’s likely that the operation can succeed.

Read more about the [Circuit Breaker pattern in my post](/blog/understanding-the-circuit-breaker-pattern/). 

## **Polly for HTTP**

[Polly](https://github.com/App-vNext/Polly "Polly") is a .NET resilience and transient-fault-handling library that allows developers to fluently and thread-safely express resilience strategies such as Retry, Circuit Breaker, Hedging, Timeout, Rate Limiter, and Fallback.

Polly is well integrated with HttpClientFactory, so you can easily add Circuit Breaker to your clients. There is a Microsoft Extensions.Http.Polly package for that. 

```csharp
builder.Services.AddHttpClient<MyApiService>()
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .RetryAsync(5));
```

## **Entity Framework Core**

Entity Framework Core can also do retries for database connection resiliency. 

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSqlServer("[ConnectionString]", 
                options => options.EnableRetryOnFailure());
```

This default strategy is specifically tailored to SQL Server (including Azure SQL). It is pre-configured with error numbers for transient errors that can be retried.

Default values of 6 for the maximum retry count and 30 seconds for the maximum default delay are used.

You can implement your custom retry strategy if you need to. 

## Summary

The Retry pattern is a powerful tool for dealing with transient failures in distributed systems, cloud services, and microservices. It allows you to build more resilient, fault-tolerant applications that gracefully handle temporary issues.

However, as with any pattern, it’s important to use it judiciously and understand its limitations. Always ensure that the operation you’re retrying is idempotent and that you’re not introducing unnecessary latency or resource consumption into your application.

Combine the Retry pattern with the Circuit Breaker pattern to prevent excessive retries when an operation consistently fails.

You can find the source code on my [GitHub](https://github.com/okyrylchuk/dotnet-newsletter/tree/main/RetryPattern).