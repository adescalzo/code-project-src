```yaml
---
title: "C# 13 and .NET 9: Personal Highlights | by Dmytro Misik | Medium"
source: https://medium.com/@dmytro.misik/c-13-and-net-9-personal-highlights-8ee96d32f2de
date_published: 2024-11-13T09:53:13.753Z
date_captured: 2025-08-08T15:51:00.977Z
domain: medium.com
author: Dmytro Misik
category: programming
technologies: [.NET 9, C# 13, ASP.NET Core, Blazor, System.Text.Json, Azure, OpenAI, LlamaIndex, Microsoft.Extensions.AI, Microsoft.Extensions.VectorData, "System.Span<T>", "System.ReadOnlySpan<T>", "System.Collections.Generic.IEnumerable<T>", System.Threading.Lock, LINQ, JIT, GC, .NET 8, .NET 7, .NET 10, .NET 11]
programming_languages: [C#]
tags: [dotnet, csharp, .net-9, csharp-13, performance, web-development, ai, language-features, updates, programming]
key_concepts: [standard-term-support, long-term-support, performance-optimization, artificial-intelligence-integration, web-development, thread-synchronization, params-collections, implicit-index-access]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a personal overview of key features introduced in C# 13 and .NET 9, released on November 12, 2024. It discusses .NET 9's status as a standard-term support release and advises on its adoption for various project types. The author highlights improvements in .NET 9, including performance enhancements, AI integration, and web development updates in ASP.NET Core and Blazor. For C# 13, specific new language features like `params` collections, the new `System.Threading.Lock` type, and implicit index access in object initializers are explained with code examples. The post concludes by emphasizing how these updates contribute to faster, easier, and more powerful .NET development.
---
```

# C# 13 and .NET 9: Personal Highlights | by Dmytro Misik | Medium

# C# 13 and .NET 9: Personal Highlights

## Breaking Down the Updates That Matter

[

![Dmytro Misik](https://miro.medium.com/v2/resize:fill:64:64/1*Z_IAq3sxXra0NNtxck2sFQ.jpeg)

](/@dmytro.misik?source=post_page---byline--8ee96d32f2de---------------------------------------)

[Dmytro Misik](/@dmytro.misik?source=post_page---byline--8ee96d32f2de---------------------------------------)

Follow

6 min read

·

Nov 13, 2024

134

1

Listen

Share

More

Press enter or click to view image in full size

![A purple hexagon with the C# logo in white, representing the C# programming language.](https://miro.medium.com/v2/resize:fit:700/1*SZFopbBIpPiQT7rCOL2YlQ.png)

On November 12, 2024, Microsoft released .NET 9, alongside with C# 13.

[

## Announcing .NET 9 - .NET Blog

### Announcing the release of .NET 9, the most productive, modern, secure, intelligent, and performant release of .NET yet…

devblogs.microsoft.com

](https://devblogs.microsoft.com/dotnet/announcing-dotnet-9/?source=post_page-----8ee96d32f2de---------------------------------------)

With each new version, C# keeps improving, adding features that boost productivity, performance, and adapt to modern coding needs. In this post, I’ll share my favorite features of C# 13 and .NET 9, highlighting why they might be valuable for you and how they can enhance your development experience.

But first, if you’ve missed my insights on the previous versions of C#, feel free to check out my posts on C# 12 and C# 11 for a quick trip down memory lane:

[

## C# 12 My Personal Top Features

### New features I like the most

medium.com

](/@dmytro.misik/c-12-my-personal-top-features-24f414cae94d?source=post_page-----8ee96d32f2de---------------------------------------)

[

## C# 11 My Personal Top Features

### New features I like the most

medium.com

](/@dmytro.misik/c-11-my-personal-top-features-5a83afa90a50?source=post_page-----8ee96d32f2de---------------------------------------)

# Introduction

.NET 9 is a standard-term support release and will be succeeded by the long-term support (LTS) version, .NET 10, in November 2025.

Press enter or click to view image in full size

![A timeline showing .NET release cycles from .NET 7 to .NET 11, indicating Standard Term Support (purple) and Long Term Support (grey) durations. It highlights .NET 9 as the "Latest release" with standard term support.](https://miro.medium.com/v2/resize:fit:700/1*tX0v77mRfhhGc09Z1tN3xA.png)

So if you’re working in a large enterprise company, your primary stack is .NET, and you’re planning to migrate from .NET 8 to .NET 9, it’s better to wait until next year when .NET 10 is released, as it will offer long-term support, providing greater stability and a longer lifecycle for enterprise applications

But still, I encourage you to consider trying .NET 9 or migrating to it if:

*   **You’re Working on Short-Term Projects**: For applications with a limited lifespan or ones that don’t require long-term maintenance, .NET 9 can provide the latest features without locking you into a long support cycle.
*   **You’re Prototyping or Experimenting**: If your focus is on building prototypes, proof-of-concepts, or conducting research, .NET 9 offers cutting-edge capabilities that can accelerate innovation and testing.
*   **Your Project Isn’t Bound by Enterprise Policies**: For non-enterprise applications or those with flexible requirements, .NET 9 can be a great choice to leverage the newest tools and improvements.
*   **You Want to Stay Ahead with the Latest Features**: Teams or developers eager to explore the newest advancements in .NET can benefit from adopting .NET 9, gaining experience with its features ahead of the .NET 10 release.
*   **You’re Preparing for a Future .NET 10 Migration**: Adopting .NET 9 can serve as a preparatory step, allowing you to test compatibility, identify potential migration challenges, and be better equipped for a smooth transition to .NET 10.
*   **Your Organization Allows a Faster Update Cycle**: If your company or team is agile enough to adopt new technologies quickly and doesn’t rely on extended support lifecycles, .NET 9 can offer an opportunity to innovate and modernize.

Before diving into the new features of C# 13, let’s first take a look at the key improvements in .NET 9!

# .NET 9 Improvements

## Better Performance, Less Memory

Year after year, one of the biggest achievements of the .NET platform is delivering faster applications that consume less memory, and .NET 9 is no exception. This release is the most performant yet, with over 1,000 performance-related changes across the runtime, workloads, and languages.

Highlights include:

*   **Adaptive Server GC.**
*   **Runtime and JIT Enhancements.**
*   **LINQ Optimizations.**
*   **System.Text.Json Improvements.**

## Artificial Intelligence

.NET 9 continues to expand its capabilities for integrating AI into applications, with new learning resources, streamlined tools, and partnerships to grow the AI ecosystem. With collaborations involving Azure, OpenAI, LlamaIndex, and others, .NET now offers an extensive ecosystem of libraries and components for building AI-infused applications.

While these advancements are exciting, I’m not an AI expert — I specialize in web development — so I can’t comment deeply on these changes. However, the introduction of unified abstractions like `Microsoft.Extensions.AI` and `Microsoft.Extensions.VectorData` simplifies interactions with AI services, making it easier for developers to leverage tools like language models and vector stores.

## Web Development

ASP.NET Core in .NET 9 brings significant improvements for building modern web apps and scalable backend services. It delivers better performance, security, and developer experience across the board. Notable updates include optimized handling of static files. The framework also enhances monitoring, tracing, and AOT compilation, ensuring apps are secure by default and more efficient.

Blazor continues to evolve with runtime render mode detection, improved reconnection experiences for Blazor Server, and a new hybrid/web app template.

# C# 13 Improvements

## `params` collections

If you’ve ever developed libraries, you’ve likely encountered situations where you need to create a unified interface for methods that can handle varying inputs — such as no arguments, a single argument, or a collection of arguments.

With C# you can declare the next method:

```csharp
static void LogMessage(string message, params string[] contexts)
{
    if (contexts.Length == 0)
    {
        Console.WriteLine($"[General] {message}");
    }
    else if (contexts.Length == 1)
    {
        Console.WriteLine($"[{contexts[0]}] {message}");
    }
    else
    {
        string joinedContexts = string.Join(", ", contexts);
        Console.WriteLine($"[{joinedContexts}] {message}");
    }
}
```

And now you can call it like this:

```csharp
LogMessage("Hello, world!"); // [General] Hello, world!
LogMessage("Hello, world!", "Debug"); // [Debug] Hello, world!
LogMessage("Hello, world!", "Debug", "Info"); // [Debug, Info] Hello, world!
LogMessage("Hello, world!", ["Debug", "Info", "Warning"]); // [Debug, Info, Warning] Hello, world!
```

Before C# 13, this function could handle the following types of inputs:

*   No arguments
*   A single argument
*   Multiple arguments
*   An array of arguments

Each of these was supported by leveraging the `params` keyword. However, if you wanted to pass a different type of collection, you first needed to convert it into an array, adding an extra step to your code:

```csharp
var list = new List<string> { "Debug", "Info", "Warning" };
LogMessage("Hello, world!", list.ToArray()); // [Debug, Info, Warning] Hello, world!
```

With C# 13, this limitation has been removed: you can now use any collection type alongside the `params` keyword. This includes `System.Span<T>`, `System.ReadOnlySpan<T>`, and types that implement `System.Collections.Generic.IEnumerable<T>` with an `Add` method:

```csharp
static void LogMessage(string message, params IList<string> contexts)
{
    if (contexts.Count == 0)
    {
        Console.WriteLine($"[General] {message}");
    }
    else if (contexts.Count == 1)
    {
        Console.WriteLine($"[{contexts[0]}] {message}");
    }
    else
    {
        string joinedContexts = string.Join(", ", contexts);
        Console.WriteLine($"[{joinedContexts}] {message}");
    }
}

var list = new List<string> { "Debug", "Info", "Warning" };
LogMessage("Hello, world!", list); // [Debug, Info, Warning] Hello, world!
```

## New lock object

The `lock` keyword in C# is used to ensure that a block of code is executed by only one thread at a time. It helps prevent race conditions when multiple threads access shared resources simultaneously.

Imagine you’re building a logging system where multiple threads write log entries to a shared file. Without synchronization, threads might overwrite each other’s data, leading to corrupted or incomplete log entries. The `lock` keyword can ensure that only one thread writes to the log file at a time:

```csharp
public class Logger
{
    private static readonly object _lock = new object();
    private static readonly string _logFilePath = "log.txt";

    public void LogMessage(string message)
    {
        lock (_lock)
        {
            // Open the file, write the message, and close the file
            using var writer = new StreamWriter(_logFilePath, append: true);
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
```

But here comes the problem — if you want to execute `async` code here, the code won’t compile.

C# 13 introduces a new `System.Threading.Lock` type, offering improved thread synchronization through a more robust and flexible API. Additionally, the `lock` statement in C# now seamlessly integrates with this new type. More details:

[

## Lock Class (System.Threading)

### Provides a mechanism for achieving mutual exclusion in regions of code between different threads.

learn.microsoft.com

](https://learn.microsoft.com/dotnet/api/system.threading.lock?source=post_page-----8ee96d32f2de---------------------------------------)

You still cannot use `lock` statements to execute asynchronous code. However, you can achieve similar functionality using a `using` block:

```csharp
public async Task LogMessageAsync(string message)
{
    using (_lock.EnterScope())
    {
        // Open the file, write the message, and close the file
        using var writer = new StreamWriter(_logFilePath, append: true);
        await writer.WriteLineAsync($"{DateTime.Now}: {message}");
    }
}
```

## Implicit index access

C# 13 introduces a powerful enhancement: the ability to use the `^` index operator ("from the end") directly in object initializers. This simplifies scenarios where you want to initialize arrays or collections by referencing elements from the end, which wasn’t possible in earlier versions of the language.

Imagine you’re building a scoreboard that displays rankings in reverse order. You can now use the `^` operator to initialize the array in a concise and readable way:

```csharp
var scoreboard = new Scoreboard()
{
    Rankings =
    {
        [^1] = "Gold",
        [^2] = "Silver",
        [^3] = "Bronze",
        [^4] = "Participant"
    }
};
```

# Conclusion

C# 13 and .NET 9 bring exciting updates that make development faster, easier, and more powerful. With features like improved performance, better tools for handling data, and support for modern AI, this release helps developers build better apps, whether for the web, APIs, or high-performance systems. These updates show how .NET keeps getting better, making it a great time to be a .NET developer!

# Resources

[

## What's new in C# 13

### Get an overview of the new features in C# 13.

learn.microsoft.com

](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13?source=post_page-----8ee96d32f2de---------------------------------------)