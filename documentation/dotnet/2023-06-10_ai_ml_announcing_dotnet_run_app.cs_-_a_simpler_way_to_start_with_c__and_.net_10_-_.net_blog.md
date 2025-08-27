## Summary
This article announces `dotnet run app.cs`, a new feature in .NET 10 Preview 4 that simplifies C# development by allowing direct execution of single `.cs` files without requiring a project file. It introduces file-level directives like `#:package`, `#:sdk`, and `#:property` for configuration, and supports shebang lines for scripting. The feature aims to lower the entry barrier for C# and offers a seamless conversion path to full project-based applications.

---

---
**Source:** https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-210&hide_banner=true
**Date Captured:** 2025-07-28T17:30:20.240Z
**Domain:** devblogs.microsoft.com
**Author:** Damian Edwards
**Category:** ai_ml
---

# Announcing dotnet run app.cs – A simpler way to start with C# and .NET 10

By [Damian Edwards](https://devblogs.microsoft.com/dotnet/author/dedward), Principal Architect
![Damian Edwards](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2023/06/Damian-Edwards-Profile-Pic-2020-square-96x96.jpg)

## Table of contents

*   [What is dotnet run app.cs?](#what-is-dotnet-run-app.cs)
*   [New file-level directives for file-based C# apps](#new-file-level-directives-for-file-based-c-apps)
*   [Referencing NuGet packages with #:package](#referencing-nuget-packages-with-package)
*   [Specifying an SDK with #:sdk](#specifying-an-sdk-with-sdk)
*   [Setting MSBuild properties with #:property](#setting-msbuild-properties-with-property)
*   [Using shebang lines for shell scripts](#using-shebang-lines-for-shell-scripts)
*   [Converting to a project-based app](#converting-to-a-project-based-app)
*   [Example](#example)
*   [Existing ways to run C# without projects](#existing-ways-to-run-c-without-projects)
*   [Getting Started](#getting-started)
*   [Learn more](#learn-more)
*   [The road ahead](#the-road-ahead)

We are super excited to introduce a new feature that was released as part of .NET 10 Preview 4 that makes getting started with C# easier than ever. You can now run a C# file directly using `dotnet run app.cs`. This means you no longer need to create a project file or scaffold a whole application to run a quick script, test a snippet, or experiment with an idea. It’s simple, intuitive, and designed to streamline the C# development experience, especially for those just getting started.

## What is `dotnet run app.cs`?

Until now, executing C# code using the `dotnet` CLI required a project structure that included a `.csproj` file. With this new capability, which we call _file-based apps_, you can run a standalone `.cs` file directly, much like you would with scripting languages such as Python or JavaScript.

This lowers the entry barrier to trying out C# and makes the language a much more attractive choice for learning, prototyping, or automation scenarios.

*   **Quick Start, No Project File Required** – Great for learning, experimentation, and small scripts.
*   **First-Class CLI Integration** – No extra tools, no dependencies, just `dotnet` and your `.cs` file.
*   **Scales to Real Applications** – This isn’t a separate dialect or runtime. When your script grows up, it can evolve into a full-fledged project using the same language, syntax, and tooling.

## New file-level directives for file-based C# apps

With .NET 10 Preview 4, file-based apps also support a set of powerful **file-level directives** that allow to declare a small number of important things that are stored in project files for project-based apps, all without leaving your single `.cs` file. These directives make file-based apps more flexible and expressive while maintaining compatibility with MSBuild concepts.

### Referencing NuGet packages with `#:package`

You can add NuGet package references directly in your `.cs` file using the `#:package` directive:

```csharp
#:package Humanizer@2.14.1

using Humanizer;

var dotNet9Released = DateTimeOffset.Parse("2024-12-03");
var since = DateTimeOffset.Now - dotNet9Released;

Console.WriteLine($"It has been {since.Humanize()} since .NET 9 was released.");
```

### Specifying an SDK with `#:sdk`

By default, file-based apps use the `Microsoft.NET.Sdk` SDK. If you’re building something like a web API, you can change the SDK using the `#:sdk` directive:

```csharp
#:sdk Microsoft.NET.Sdk.Web
```

This tells the tooling to treat the file as if it were part of a web project, enabling features of ASP.NET Core like Minimal APIs and MVC.

### Setting MSBuild properties with `#:property`

You can configure additional build properties using `#:property`. For example:

```csharp
#:property LangVersion preview
```

This allows your file-based app to opt into advanced language features and platform targeting, without needing a full project file.

### Using shebang lines for shell scripts

File-based apps also support [shebang](https://en.wikipedia.org/wiki/Shebang_%28Unix%29) lines (`#!`), allowing you to write cross-platform C# shell scripts that are executable directly on Unix-like systems. For example:

```csharp
#!/usr/bin/dotnet run
Console.WriteLine("Hello from a C# script!");
```

You can make the file executable and run it directly:

```bash
chmod +x app.cs
./app.cs
```

This makes C# a convenient option for CLI utilities, automation scripts, and tooling, no project setup required.

## Converting to a project-based app

When your file-based app grows in complexity, or you simply want the extra capabilities afforded in project-based apps, you can convert it to a standard project with:

```bash
dotnet project convert app.cs
```

This command creates a new directory named for your file, scaffolds a `.csproj` file, moves your code into a `Program.cs` file, and translates any `#:` directives into MSBuild properties and references.

### Example

Given this file:

```csharp
#:sdk Microsoft.NET.Sdk.Web
#:package Microsoft.AspNetCore.OpenApi@10.*-*

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/", () => "Hello, world!");
app.Run();
```

The generated `.csproj` would be:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.*-*" />
  </ItemGroup>

</Project>
```

This makes the transition seamless, from a single file to a fully functional, buildable, and extensible project.

## Existing ways to run C# without projects

This is far from the first time developers have wanted to run C# without a project. Community projects like [CS-Script](https://github.com/oleg-shilo/cs-script), [dotnet-script](https://github.com/dotnet-script/dotnet-script), [Cake](https://cakebuild.net/), and others have long filled this role, enabling scripting workflows, REPL experiences, and other experiences with C#. Here’s a [blog post by Scott Hanselman from 2018 detailing the `dotnet-script` global tool](https://www.hanselman.com/blog/c-and-net-core-scripting-with-the-dotnetscript-global-tool).

These tools remain valuable and are worth checking out, especially for more advanced scripting scenarios. However, with this new built-in support, developers can get started immediately: no additional installation, configuration, or discovery steps required.

Equally important: this isn’t a separate dialect or mode of C#. We’re being intentional about making this feature a natural earlier “click-stop” from a regular C# project-based app. You’re writing the same C#, using the same compiler, and when your code grows up, it transitions naturally into a project-based app, if and when you want.

## Getting Started

1.  **Install .NET 10 Preview 4** Download and install it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0).
2.  **Install Visual Studio Code (recommended)** If you’re using Visual Studio Code, install the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) and then follow these instructions to update the C# extension for file-based apps support:

    > To enable support for file-based apps and directives, you’ll need the latest **pre-release version** of the C# extension:
    >
    > *   Open the Extensions sidebar (`Ctrl+Shift+X`)
    > *   Search for “C#”
    > *   In the extension page, click the **Switch to Pre-Release Version** button
    > *   Ensure the version installed is at least `2.79.8`

3.  **Write your code** Create a file called `hello.cs`:

    ```csharp
    Console.WriteLine("Hello, world!");
    ```

4.  **Run it!** Open a terminal in the same folder and run:

    ```bash
    dotnet run hello.cs
    ```

5.  **Convert to a project** To convert the file to a project, run:

    ```bash
    dotnet project convert hello.cs
    ```

## Learn more

Watch this feature in action in this [demo session from Microsoft Build](https://build.microsoft.com/sessions/DEM518?source=sessions): [No projects, just C# with `dotnet run app.cs`](https://www.youtube.com/watch?v=98MizuB7i-w)

You’ll see how easy it is to get started, explore directives, and convert to a full project when ready.

## The road ahead

With `dotnet run app.cs`, we’re making C# more approachable, while preserving the full power and depth of the .NET ecosystem. Whether you’re prototyping, teaching, or building production systems, this new capability helps you move faster from idea to execution.

In upcoming .NET 10 previews we’re aiming to improve the experience of working with file-based apps in VS Code, with enhanced IntelliSense for the new file-based directives, improved performance, and support for debugging. At the command line we’re exploring support for file-based apps with [multiple files](https://github.com/dotnet/sdk/blob/main/documentation/general/dotnet-run-file.md#multiple-c-files), and ways to make running file-based apps faster.

Try it out today and send your [feedback to GitHub](https://github.com/dotnet/sdk/issues/new) as we continue to shape this experience during .NET 10 and beyond.