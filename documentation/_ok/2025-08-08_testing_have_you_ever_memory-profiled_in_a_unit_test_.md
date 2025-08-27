```yaml
---
title: Have you ever memory-profiled in a unit test?
source: https://dateo-software.de/blog/unit-test-memory?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=using-windows-error-reporting-in-net&_bhlid=4e7bed50dfc95432b2074e82288e6b8abb6dc9b1
date_published: unknown
date_captured: 2025-08-08T15:48:37.752Z
domain: dateo-software.de
author: Unknown
category: testing
technologies: [.NET, JetBrains Rider, JetBrains dotMemory Unit, xUnit, Visual Studio, Statamic]
programming_languages: [C#, JavaScript]
tags: [memory-profiling, unit-testing, .net, rider, performance, debugging, memory-management, csharp, development-tools, test-automation]
key_concepts: [memory-management, garbage-collection, memory-leaks, unit-testing, memory-profiling, memory-assertions, continuous-analysis, test-automation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores how to memory profile code within unit tests, a task often overlooked by .NET developers due to automatic garbage collection. It highlights that standard IDE profiling tools typically don't offer direct memory analysis for unit tests. The author demonstrates a solution using the `JetBrains.DotMemoryUnit` NuGet package in JetBrains Rider, providing C# code examples for integration with xUnit. This approach enables developers to write memory assertions, compare snapshots, and analyze memory traffic directly within their tests, helping to identify and prevent memory leaks in an isolated and automated manner.
---
```

# Have you ever memory-profiled in a unit test?

# dateo. Coding Blog

Coding, Tech and Developers Blog

.NET

Rider

unit test

memory

## Have you ever memory-profiled in a unit test?

_Dennis Frühauff_ on October 8th, 2024

Today's article will be a short but hopefully useful one: Ever had to memory profile code during unit test execution?
Thought so. Let's take a look at how that works.

### Introduction

I am sure we've all come across major or minor memory leaks in our application(s). If you are a .NET developer memory management is not something that
we tend to think of all the time. Most of the time, we can just trust the garbage collector to do its work. And, also most of the time, it does this pretty nicely.
This, i.e., living in the ecosystem of a memory-managed framework, does not mean that you cannot write memory-leaking code.
Quite the opposite: .NET developers forgetting to think about it can lead to pretty ugly bugs.

And if one of those pops up, our sharpest tool in the workshop is our favorite IDE's memory profiling tool, which usually let's us run the application with a profiler attached and take dedicated snapshots that we can compare against each other. If you know what to look for, chances are you'll find the leak very soon.

But have you ever memory-profiled a unit test? Do you think it is possible?

Until a few weeks ago, my answer would have been "Of course that's possible. Just right-click and run with a profiler".
Unfortunately, that is not the case.

![Screenshot 1: A screenshot of JetBrains Rider's context menu for running a unit test. The options displayed are "Run", "Debug", "Cover", and "Profile", but there is no specific option for memory profiling.](https://dateo-software.de/assets/articles/memorytest1.png)

But first of all: Why would you want to do that?

*   You have a suspicion of the memory leak in your code and want to prove that in an isolated manner.
*   You want to guard your application against unintentional memory leaks in advance.
*   The test code itself is leaking memory, potentially blowing your CI pipeline when the test number increases.

### Memory-profiling unit tests in JetBrains Rider

By default, Jetbrains Rider offers to profile unit tests on a time-based manner. That means, that you can analyze timings and execution times in your code.
But you cannot just analyze the memory profile of a test.

What you need to do is download a dedicated (and already pretty old) [NuGet package](https://www.jetbrains.com/dotmemory/unit/) `JetBrains.DotMemoryUnit` into your test project.
After doing this, you will see a new test run option in your IDE.

![Screenshot 2: A screenshot of JetBrains Rider's context menu for running a unit test after the `JetBrains.DotMemoryUnit` NuGet package has been installed. A new option, "Run under dotMemory Unit", is now visible in the menu.](https://dateo-software.de/assets/articles/memorytest2.png)

Once that is complete, you can start writing memory assertions directly in your tests:

```csharp
[Fact]
public void Foo()
{
    dotMemory.Check(memory =>
    {
        memory
            .GetObjects(q => q.Namespace.Like("Tests"))
            .ObjectsCount
            .Should().Be(0);
    });
}
```

If you are running your tests with [xUnit](https://xunit.net/) you will face a runtime exception:

```shell
DotMemoryUnitException
xUnit does not capture the standard 
output stream which is used by dotMemory 
Unit to report issues and save workspaces.
```

Fortunately, the package provides you with an API to work around this, so that a full example test class would look like this:

```csharp
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using Xunit;
using Xunit.Abstractions;

namespace MyTests;

public class FirstTestClass
{
    public FirstTestClass(ITestOutputHelper output)
    {
        DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
    }
    
    [Fact]
    public void Foo()
    {
        dotMemory.Check(memory =>
        {
            memory
                .GetObjects(q => q.Namespace.Like("MyTests"))
                .ObjectsCount
                .Should().Be(0);
        });
    }
}
```

This package or, rather, extension to your test runner, lets you:

*   Assert and filter for objects within snapshots,
*   Assert comparisons between different memory snapshots within your test,
*   Analyze memory traffic allocated in your code,
*   Perform continuous analysis by writing automated dumps for failed tests.

### Conclusion

I told you this was a short one. Maybe there are a few readers of this post who can put this to use like I had to recently.
And maybe I could save you some time by pointing you in the right direction.

P.S.: I am not aware about the capabilities of Visual Studio when it comes to this topic. From what I know at this moment, I would assume that there is also no out-of-the-box way to do it, but I did not explicitly check this.