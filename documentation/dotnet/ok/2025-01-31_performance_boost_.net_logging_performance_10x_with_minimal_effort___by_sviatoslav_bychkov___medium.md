```yaml
---
title: "Boost .NET Logging Performance 10x with Minimal Effort | by Sviatoslav Bychkov | Medium"
source: https://medium.com/@stbychkov/boost-net-logging-performance-10x-with-minimal-effort-a5c741a43f49
date_published: 2025-01-31T11:10:24.854Z
date_captured: 2025-08-06T17:51:55.865Z
domain: medium.com
author: Sviatoslav Bychkov
category: performance
technologies: [.NET, Microsoft.Extensions.Logging, AutoLoggerMessage, NLog, NuGet, Roslyn]
programming_languages: [C#]
tags: [logging, .net, performance, source-generator, optimization, memory-management, csharp, nuget, boilerplate]
key_concepts: [logging-performance, string-allocation, boxing, LoggerMessage.Define, LoggerMessage-source-generator, source-generators, interceptors, memory-optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  This post explores the evolution of logging techniques in .NET, addressing common performance pitfalls like redundant string allocation and boxing in standard `ILogger.Log` calls. It details Microsoft's solutions, `LoggerMessage.Define` and the `LoggerMessage` source generator, while highlighting their drawbacks related to boilerplate and code organization. The article then introduces `AutoLoggerMessage`, a third-party source generator, as an effective solution that automatically optimizes logging calls using .NET 8+ interceptors, achieving significant performance boosts with minimal developer effort. It explains the underlying mechanics of `AutoLoggerMessage`, including its use of generic extension methods and interceptors to avoid boxing and improve efficiency.
---
```

# Boost .NET Logging Performance 10x with Minimal Effort | by Sviatoslav Bychkov | Medium

# Boost .NET Logging Performance 10x with Minimal Effort

![Sviatoslav Bychkov](https://miro.medium.com/v2/resize:fill:32:32/0*MH4G4h3guaiMEPi6.jpg)

[Sviatoslav Bychkov](/@stbychkov?source=post_page---byline--a5c741a43f49---------------------------------------)

Follow

4 min read

·

Jan 28, 2025

58

3

Listen

Share

More

This post will explore the evolution of logging techniques in .NET, from basic implementations to advanced ones. We’ll look at common pitfalls, how to avoid them, and how to significantly boost logging performance with **about** **zero** effort. Let’s go!

# Iteration #0.

We all know how important logging is in our apps and you might find plenty of logging statements in almost every project. Let’s take this logging statement as an example:

```csharp
logger.LogTrace(  
    "Event {EventName} has been processed in {Time} ms",   
    eventName, time  
);
```

Looking closely at this logging statement, we can notice several issues:

1.  Redundant **string allocation** even if the logger has been configured with a higher minimum log level than in the logging statement.
2.  It involves redundant **boxing** message parameters into an object array, causing memory-related problems.
3.  Potential **runtime** errors if a parameter is missing.
4.  There is limited caching of formatters to transform templates into actual log messages

# Iteration #1.

To address these problems, Microsoft introduced the [_LoggerMessage.Define<T>_](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggermessage.define?view=net-8.0-pp&viewFallbackFrom=net-8.0) method, allowing you to specify custom formatting and resolve the issues mentioned earlier.

For that, you have to define the logging statement delegate like this:

```csharp
private static readonly Action<ILogger, string, double, Exception?> _callback =   
    LoggerMessage.Define<string, double>(  
        LogLevel.Trace,  
        new EventId(eventId, nameof(EventProcessed)),  
        "Event {EventName} has been processed in {Time} ms"  
    );
```

While it resolves most issues, this code appears more suited for machines than humans. It’s not easy to maintain and I can hardly imagine anyone enthusiastically replacing standard logger calls with this.

# Iteration #2.

Microsoft released **LoggerMessage** source generator that became a go-to tool for efficient logging. This source generator creates _LoggerMessage.Define_ methods automatically based on specific method declarations. For example:

```csharp
[LoggerMessage(  
    Level = LogLevel.Trace,  
    Message = "Event {EventName} has been processed in {Time} ms"  
)]  
public partial void LogEventProcessesing(string eventName, double time);
```

So basically it generates the same code as in the previous example but in a more human-readable form. It looks okay, right?

I tried using this approach in a mid-sized project with thousands of logging statements but gave up after just four 😣. I found this approach is definitely better than iteration #1, but it still comes with several challenges:

1.  It forces you to organize your project in a specific way. Either you use a separate static class for your log message declarations, making them distant from where they’re actually used, or you end up with a bunch of partial methods in the class where the logging statements are present (too much not business-related code + the class must be changed to partial as well 🫣).
2.  It’s still a lot of boilerplate. You can easily multiply the amount of code you write by 3 for each logging statement…
3.  It can lead to inconsistency of how the logger is used in your project and when you see mixing approaches here and there, it doesn’t look good IMHO.

![Morpheus from The Matrix with the text "WHAT IF I TOLD YOU WE CAN DO BETTER"](https://miro.medium.com/v2/resize:fit:642/0*FwJetNcr4ppet4KN)

# Iteration #3.

I want to revisit iteration #0 as it offers a simpler way to write logger statements while retaining the benefits of the later iterations. How can we achieve that?

This is where **AutoLoggerMessage** comes in. It’s another source generator that does all routine work for you, so by using your regular ILogger.Log+ methods, you can achieve performance boosts of up to 90% according to the benchmarks.

![Performance benchmark table comparing "Default implementation", "Default + LoggerMessage", and "AutoLoggerMessage" by Mean execution time, Ratio, and Allocated memory.](https://miro.medium.com/v2/resize:fit:700/0*5bqCSMXiyT4OlzNo)

Performance gains achieved using AutoLoggerMessage compared to standard ILogger.Log+ method calls.

# Demo:

# How it works?

For those, who are interested in the details, I’ve explained how it works [here](https://github.com/stbychkov/AutoLoggerMessage/tree/main/docs), but let’s briefly go over the challenges I faced.

# Logger Extensions

The current set of logger extension methods accepts only _params object\[\]_ for parameters. Some logger implementations actually support a generic version (for example _NLog_), but the **default abstraction** **doesn’t**, which is quite sad.

To avoid any boxing for these calls, _AutoLoggerMessage_ generator provides a set of generic extension methods that will be picked up due to the higher specificity rule of the method parameter types. This ensures that no matter how you call your logging methods, the generator has a suitable overload ready to go.

# Source Generator

Next, the generator scans your code for any _Logger.Log+_ methods and captures the parameters passed to these methods, which will later be used to generate the corresponding _LoggerMessage_ methods.

Finally, the generator creates a set of **interceptors**. Read more about Interceptors [here](https://github.com/dotnet/roslyn/blob/main/docs/features/interceptors.md), but in short, these act as a bridge between your logging calls and the generated _LoggerMessage_ methods. The interceptors forward logging requests to the correct methods, ensuring everything works smoothly. It’s available only in .NET 8+, so this is the main reason why the minimum target framework is set to 8.

![Diagram illustrating how AutoLoggerMessage works, showing Logger.Log+ calls being intercepted and forwarded to generated LoggerMessage methods.](https://miro.medium.com/v2/resize:fit:600/0*YR9ioOd69Qhf1nGW)

# Limitations:

*   Target framework net8+. It could be lowered to .NET 6, but that would require changes in the [.NET runtime repository](https://github.com/dotnet/runtime), which may take more time. Check out [this](https://github.com/dotnet/runtime/discussions/110364) discussion for more updates.
*   It works only with _Microsoft.Extensions.Logging.ILogger_ abstraction.

# Installation:

To make it work you just need to install a NuGet package:

```bash
dotnet add package stbychkov.AutoLoggerMessage
```

And that’s it! From this point forward, the package handles everything for you.

Why not give it a try and share your feedback? If you like the project, you can give it a [star](https://github.com/stbychkov/AutoLoggerMessage) to make it more visible to others. Enjoy!

![Willy Wonka meme with the text "ARE YOU STILL HERE? GOOD"](https://miro.medium.com/v2/resize:fit:642/0*FwJetNcr4ppet4KN)