```yaml
---
title: Using Testing.Platform with NET 9
source: https://dateo-software.de/blog/testing-platform?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=lesser-known-clr-gc-handles&_bhlid=d0033b9fb4107e820ccba437b23b0df2a0ed018a
date_published: unknown
date_captured: 2025-08-08T18:06:07.127Z
domain: dateo-software.de
author: Unknown
category: testing
technologies: [.NET 9, .NET 8, Testing.Platform, MSTest, XUnit, coverlet.collector, Microsoft.NET.Test.Sdk, xunit.runner.visualstudio, NUnit, TUnit, Visual Studio, Rider]
programming_languages: [C#, XML, Bash]
tags: [testing, .net, xunit, unit-testing, test-automation, performance, migration, dotnet-9, test-framework]
key_concepts: [unit-testing, test-automation, test-platform, NativeAOT, HotReload, backwards-compatibility, project-configuration, performance-optimization]
code_examples: true
difficulty_level: intermediate
summary: |
  This article introduces Microsoft's new Testing.Platform, a replacement for MSTest designed for modern .NET applications. It highlights key goals such as determinism, portability, NativeAOT support, extensibility, performance, and security. The author explains that Testing.Platform delivers an executable console application for running tests, supporting features like HotReload. A practical guide demonstrates how to migrate an existing XUnit .NET 8 project to use Testing.Platform with .NET 9, involving changes to the `.csproj` file and referencing preview packages. The article concludes by mentioning support in Visual Studio and Rider and points to further documentation.
---
```

# Using Testing.Platform with NET 9

# Using Testing.Platform with NET 9

_Dennis Frühauff_ on December 4th, 2024

.NET 9.0 is introducing the new Testing.Platform. Let's take a look at what that is how you will benefit from using it.
In this article I want to give you quick introduction into what it is and how you can already incorporate it into your projects.

### What is Testing.Platform

Testing.Platform is supposed to be the replacement for MSTest as the primary platform to run tests for your applications. With the introduction of .NET Core in 2017, MSTest (introduced 2005) was only adjusted to "make it work" but did not receive a proper redesign from scratch. This has changed now and the teams at Microsoft rebuild the testing framework to meet the following future goals:

*   Tests should be deterministic.
*   Tests should be portable, i.e., tests should run everywhere the same way, be it different computers, CI agents or in the cloud.
*   Test should be future-proof, i.e., they must support NativeAOT compilation.
*   Tests should be extensible, .e.g., there should be simple retry support.
*   Tests should be performant. Microsoft claims to see a 30% performance increase just by migrating to Testing.Platform.
*   Tests should be secure, i.e., especially when it comes to external dependencies, which are supposed to be kept to a minimum.

**How do they (MS) achieve it?**
Well, from the outside, the deliverable of Testing.Platform is a .EXE file, a console application. The basic idea is that whatever you can do with a console application, you can do with the output of Testing.Platform. You can run it (which will run the tests), you can debug it, you can even use HotReload and see the output of your tests while it is running.
That is pretty neat.
At the same time, backwards compatibility is kept so that you can still run `dotnet test` to execute your tests even if you already migrated to Testing.Platform.

### Migrating XUnit tests to Testing.Platform

So let's assume you have a classic XUnit testing project in your application, and let's also assume that you are on .NET8. The typical csproj project file for this will look something like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>

        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="coverlet.collector" Version="6.0.0"/>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0"/>
        <PackageReference Include="xunit" Version="2.5.3"/>
        <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3"/>
    </ItemGroup>

    <ItemGroup>
        <Using Include="Xunit"/>
    </ItemGroup>

</Project>
```

To change this into a project that runs under the new Testing.Platform we will adjust the file like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>

        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
        <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="coverlet.collector" Version="6.0.2"/>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1"/>
        <PackageReference Include="xunit.v3" Version="0.7.0-pre.15"/>
        <PackageReference Include="xunit.runner.visualstudio" Version="3.0.0-pre.49"/>
    </ItemGroup>

    <ItemGroup>
        <Using Include="Xunit"/>
    </ItemGroup>

</Project>
```

By the time your are reading this, specific versions might have changed, but the outcome will still be the same. Here are the main changes:

*   Using .NET9 (which also means installing the [latest SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks).
*   Adding the tag `<UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>` which is necessary for XUnit.
*   Referencing preview versions of XUnit v3.

If you now spin up a terminal in this project and enter `dotnet run` you will be greeted with the following output:

```bash
xUnit.net v3 Microsoft.Testing.Platform Runner v0.7.0-pre.15+450dd5a171 (64-bit .NET 9.0.0)


Test run summary: Passed! - bin\Debug\net9.0\XUnit.Tests.dll (net9.0|x64)
  total: 2
  failed: 0
  succeeded: 2
  skipped: 0
  duration: 276ms
```

You can also still run `dotnet test` and get a similar output (although it does take longer to complete).

This is already it, you have successfully migrated to Testing.Platform with XUnit.
Please note that the project file for NUnit looks slightly different. If you want to know how that works, feel free to watch the introduction video for Testing.Platform on [YouTube](https://www.youtube.com/watch?v=9Jz47Ze9LOI).

In fact, there is also [TUnit](https://github.com/thomhurst/TUnit), which is supposedly constructed on top of Testing.Platform, maybe making it the go-to test framework in the future.

And in case you're wondering: Both Visual Studio and Rider already support running tests that support Testing.Platform in their latest versions, so you will not see any degradation in your productivity there.

### Conclusion

This was a very short introduction into Microsoft.Testing.Platform and how you can already migrate towards it and reap all future benefits.
You can find in-depth documentation of all the features in the [Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-platform-intro?view=vs-2022&tabs=dotnetcli). Hopefully you did find a start here!