```yaml
---
title: "The Hidden Cost of String Parameters | by Andrés Espino | Turbo .NET | Medium"
source: https://medium.com/turbo-net/the-hidden-cost-of-string-parameters-e47519e618ed
date_published: 2025-04-08T10:20:26.220Z
date_captured: 2025-08-17T22:03:11.380Z
domain: medium.com
author: Andrés Espino
category: general
technologies: [Unity, UnityEngine.Debug]
programming_languages: [C#]
tags: [logging, performance, unity, csharp, optimization, garbage-collection, lazy-evaluation, string-interpolation, profiling]
key_concepts: [lazy-evaluation, string-interpolation, performance-optimization, garbage-collection, profiling, log-levels, lambda-functions]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the hidden performance costs associated with string parameters in logging, particularly within Unity applications. It demonstrates how C# string interpolation leads to immediate allocations and processing, even when log levels are set to filter out messages, causing unnecessary pressure on the Garbage Collector. The solution proposed involves implementing lazy evaluation by passing lambda functions (`Func<object>`) to logger methods, deferring message computation until it's actually needed. This approach significantly reduces allocations and CPU overhead, highlighting the importance of profiling and challenging assumptions in performance optimization.]
---
```

# The Hidden Cost of String Parameters | by Andrés Espino | Turbo .NET | Medium

# The Hidden Cost of String Parameters

## How Lazy Evaluation Can Save Performance

It is a well-known fact that the number one tool for debugging in Unity is logging (just kidding). However, while logging is undeniably useful, it is also an expensive operation that, if abused, can severely impact application responsiveness.

To mitigate this, we have standardized the concept of log levels, which allows us to remove log calls at compilation time based on the build configuration. Standards help establish a common understanding, but they can also create a false sense of security — we assume that simply following a standard means we automatically get all of its benefits.

Let’s examine a real-world scenario I recently encountered. This application relied heavily on logging for development, assuming there would be no performance impact in release builds.
Let’s challenge that assumption!

## The Naive Logger Implementation

```csharp
public enum LogLevel { Info, Warning, Error }
 
public static class Logger
{
    static LogLevel _level = LogLevel.Error;
 
    public static void LogInfo(object message)
    {
        if (_level > LogLevel.Info) return;
        UnityEngine.Debug.Log(message);
    }
 
    public static void LogError(object message)
    {
        if (_level > LogLevel.Error) return;
        UnityEngine.Debug.LogError(message);
    }
}
```

A simple static logger that avoids calling `UnityEngine.Debug` with a simple if statement.

## A test client for the logger

```csharp
public class LoggerClient : MonoBehaviour
{
    [SerializeField] int _logCountPerFrame = 100;
 
    void Update()
    {
        for (int i = 0; i < _logCountPerFrame; i++)
        {
            Logger.LogInfo($"Test Log from {name} [{i}]");
        }
    }
}
```

I created a test client so we can start profiling the `Logger`. Lets set the LogLevel to `Info` and call `LogInfo` 100 times per `Update`:

![Unity Profiler showing high CPU usage and allocations (around 6.45ms) when 100 log statements are processed with LogLevel set to Info.](https://miro.medium.com/v2/resize:fit:1000/1*z1wN04hkTs4A9ZIaSlQygw.png)

Logging performance cost for 100 log statements

Looking at the Profiler, we see that this update takes, in average, around 6.4–6.5ms per frame. We knew logging was expensive; however, since the log level in the release builds is set to `Error`, we assume that we can reclaim those 6 milliseconds for gameplay. Let’s verify this assumption **with the Profiler,** I set the LogLevel to Error and repeat the same test:

![Unity Profiler showing reduced CPU usage (around 0.20ms) but still significant allocations (red marks) when 100 log statements are called with LogLevel set to Error, due to eager string interpolation.](https://miro.medium.com/v2/resize:fit:1000/1*bFwWJJGXSWLq-PfLyNEV_Q.png)

Filtered logging performance cost for 100 calls

Surprisingly, even when filtering the log statements, the Profiler shows unnecessary allocations. The cost may seem very small; even in this exaggerated client, the update method only takes 0.20 ms, coming from 6.40 ms. This is clearly good enough, isn’t it?
Those red marks in the Profiler indicate an **allocation** that will eventually be freed by the Garbage Collector (GC)! We are putting extra pressure on the GC, and every time it is triggered, our application will shutter.

# What is happening?

Those red marks in the Profiler happen because we are using a string (not a compile-time constant) as a parameter. Let’s take a closer look at the caller end:

```csharp
Logger.LogInfo($"Test Log from {name} [{i}]");
```

The C# syntactic sugar can be misleading here; string interpolation (`$"{var}"`) is under the hood a method call, and it is **evaluated immediately.** This creates allocations and unnecessary processing, even if the string is never used.

Although our log level check seems to prevent unnecessary logging, it does **not** prevent the expensive computation of interpolated strings. This is done **before** the function is even called.

# Solving the Problem with Lazy Evaluation

A simple and effective fix is to delay the evaluation of the log message until it is actually needed. We can achieve this by modifying our logger methods to accept a `Func<object>`:

```csharp
public static void LogInfo(Func<object> message)
{
    if (_level > LogLevel.Info) return;
    UnityEngine.Debug.Log(message());
}
 
public static void LogError(Func<object> message)
{
    if (_level > LogLevel.Error) return;
    UnityEngine.Debug.LogError(message());
}
```

Now, when logging, we pass a **lambda function** instead of an eagerly evaluated string:

```csharp
Logger.LogInfo(() => $"Test Log from {name} [{i}]");
```

With this approach, **if logging is disabled**, the lambda function is never invoked, completely eliminating unnecessary string interpolations and allocations.

![Unity Profiler showing drastically reduced CPU usage (around 0.01ms) and minimal allocations after implementing lazy evaluation for log messages, even when LogLevel is set to Error.](https://miro.medium.com/v2/resize:fit:1000/1*q39JqXx8bhnyxPetxSwLsA.png)

Filtered logging + lazy evaluated message performance cost

It was hard to spot, only 0.002–3 ms, but the important part is that we are reducing the allocations drastically. Even with lazy evaluation, some overhead remains, but the improvement in allocation and CPU usage is significant, ensuring that expensive computations are **performed only when necessary**.

# Final thoughts

I would like to say that this article is not about logging; a proper LogLevel implementation should strip calls at compile time.

This article is about challenging assumptions, using tools and data before jumping to conclusions, and about enjoying the little details.

> What gets us into trouble is not what we don’t know. It’s what we know for sure that just ain’t so — Mark Twain