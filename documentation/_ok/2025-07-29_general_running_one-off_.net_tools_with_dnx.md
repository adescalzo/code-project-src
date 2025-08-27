```yaml
---
title: Running one-off .NET tools with dnx
source: https://andrewlock.net/exploring-dotnet-10-preview-features-5-running-one-off-dotnet-tools-with-dnx/
date_published: 2025-07-29T13:00:00.000Z
date_captured: 2025-08-11T14:18:00.159Z
domain: andrewlock.net
author: Unknown
category: general
technologies: [.NET 10, NuGet, .NET SDK, Node.js, npm, npx, Go, MSBuild, ASP.NET Core]
programming_languages: [C#, Bash, CMD, Shell Script]
tags: [.net, .net-tools, cli, nuget, sdk, command-line, developer-experience, preview-features, tooling, package-management]
key_concepts: [.NET tools, package management, command-line interface, global tools, local tools, NuGet package cache, executable shims, SDK commands]
code_examples: true
difficulty_level: intermediate
summary: |
  This article introduces the new `dnx` command in .NET 10 preview 6, designed for running .NET tools without requiring explicit installation. It explains the functionality of `dnx`, comparing it to Node.js's `npx` and contrasting it with the traditional `dotnet tool install` command. The post details how `dnx` operates by downloading tools to the global NuGet package cache and executing them directly, bypassing the need for persistent tool store installations or executable shims. Furthermore, it provides an in-depth look at the `dnx` command's implementation within the .NET SDK, outlining the `ToolExecuteCommand`'s role in locating, confirming, and running tools.
---
```

# Running one-off .NET tools with dnx

July 29, 2025 ~6 min read

*   .NET 10
*   NuGet

# Running one-off .NET tools with dnx

Exploring the .NET 10 preview - Part 5

This is the fifth post in the series: Exploring the .NET 10 preview.

1.  [Part 1 - Exploring the features of dotnet run app.cs](/exploring-dotnet-10-preview-features-1-exploring-the-dotnet-run-app.cs/)
2.  [Part 2 - Behind the scenes of dotnet run app.cs](/exploring-dotnet-10-preview-features-2-behind-the-scenes-of-dotnet-run-app.cs/)
3.  [Part 3 - C# 14 extension members; AKA extension everything](/exploring-dotnet-10-preview-features-3-csharp-14-extensions-members/)
4.  [Part 4 - Solving the source generator 'marker attribute' problem in .NET 10](/exploring-dotnet-10-preview-features-4-solving-the-source-generator-marker-attribute-problem-in-dotnet-10/)
5.  Part 5 - Running one-off .NET tools with dnx (this post)
6.  [Part 6 - Passkey support for ASP.NET Core identity](/exploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity/)

In this post I briefly show the new `dnx` command for running one-off .NET tools without installing them. I show how to use the command, how the command works in practice, and how the command works behind the scenes in the .NET SDK.

## Running tools without installing them with `dnx`

The Node.js ecosystem has had a tool called `npx` since 2017. Sitting alongside the `npm` package-manager tool, it allows running a Node.js tool from a package without having to installing it globally. .NET Core added support for tools in NuGet packages shortly afterwards in 2018, back in .NET Core 2.1, but this has always required an explicit `dotnet tool install` command before you can run the tool. Until now.

.NET 10 preview 6 added support for running .NET tools without explicitly installing them first with the introduction of the new .NET SDK command `dnx`. What's more, the .NET SDK also ships a standalone `dnx` command so that you can run `dnx <tool>` directly instead of `dotnet dnx <tool>`.

One of the easiest ways to understand what's available with the new `dnx` command is to take a look at the built-in command line help:

```bash
> dnx --help
Description:
  Executes a tool from source without permanently installing it.

Usage:
  dotnet dnx <packageId> [<commandArguments>...] [options]

Arguments:
  <PACKAGE_ID>        Package reference in the form of a package identifier like 'Newtonsoft.Json' or package identifier and version separated by '@' like 'Newtonsoft.Json@13.0.3'.
  <commandArguments>  Arguments forwarded to the tool

Options:
  --version <VERSION>       The version of the tool package to install.
  -y, --yes                 Accept all confirmation prompts using "yes."
  --interactive             Allows the command to stop and wait for user input or action (for example to complete authentication). [default: True]
  --allow-roll-forward      Allow a .NET tool to roll forward to newer versions of the .NET runtime if the runtime it targets isn't installed.
  --prerelease              Include pre-release packages.
  --configfile <FILE>       The NuGet configuration file to use.
  --source <SOURCE>         Replace all NuGet package sources to use during installation with these.
  --add-source <ADDSOURCE>  Add an additional NuGet package source to use during installation.
  --disable-parallel        Prevent restoring multiple projects in parallel.
  --ignore-failed-sources   Treat package source failures as warnings.
  --no-http-cache           Do not cache packages and http requests.
  -v, --verbosity <LEVEL>   Set the MSBuild verbosity level. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].
  -?, -h, --help            Show command line help.
```

As you might expect, there's a lot of overlap with the existing `dotnet tool` commands. One point that differs is the (optional) way you can specify the package version with a `@` separator, for example, `Newtonsoft.Json@13.0.3`. This is a common pattern used in other ecosystems like Node.js and Go, for example, and it's now making it's way to .NET.

> I think we first saw this notation in .NET [with the recent `dotnet run app.cs` feature](/exploring-dotnet-10-preview-features-1-exploring-the-dotnet-run-app.cs/#adding-nuget-package-references). Overall, it points to a willingness to break from some of .NET's tendency towards verbosity in favour of embracing more concise patterns, particularly patterns from other ecosystems.

So now let's try it out. As an example, I'll use the classic demo tool `dotnetsay`. When you run using `dnx dotnetsay`, you have to confirm that the .NET SDK should download the tool. If you answer `n`, the run is cancelled; if you answer `y` then it downloads and runs the tool:

```bash
> dnx dotnetsay
Tool package dotnetsay@2.1.7 will be downloaded from source https://api.nuget.org/v3/index.json.
Proceed? [y/n] (y): y

        Welcome to using a .NET Core global tool!
```

Note that you only get the confirmation the first time you run and download the tool. The next time, the tool runs without confirmation:

```bash
> dnx dotnetsay

        Welcome to using a .NET Core global tool!
```

And there you have it: one-shot .NET tool execution.

## How is `dnx` different to `dotnet tool install`?

Now, you might be thinking that this `dnx` is nice and short, but is it _really_ that different to installing the tool? And the answer is both yes and no.

The typical way you would _install_ the `dotnetsay` tool globally would be using:

```bash
dotnet tool install -g dotnetsay
```

The `-g` means the tool is installed globally, though you can [also install tools locally](/new-in-net-core-3-local-tools/). You can then run the tool by simply running:

```bash
dotnetsay

        Welcome to using a .NET Core global tool!
```

But what does "installed globally" actually _mean_? And how does it differ from what `dnx` does?

Step one in both cases is to download [the dotnetsay NuGet package](https://www.nuget.org/packages/dotnetsay). This goes through layers of caching and is ultimately expanded into the global package location. This is also where all the NuGet packages that are downloaded as part of project builds are stored by default.

> You can see the location of the NuGet-related folders on your machine by running `dotnet nuget locals all --list`

After downloading the package, the SDK also copies the expanded package to the "dotnet tool" store location. By default, these expanded directories can be found at `~/.dotnet/tools/.store`. The .NET SDK _also_ installs an executable shim in `~/.dotnet/tools`, which is also on the machine's `PATH`, so they can be easily invoked.

In contrast, the `dnx` command downloads the package and installs it into the global package cache, but it _doesn't_ install anything in the tool store or add a shim to `~/.dotnet/tools`. Instead it runs the tool directly from the package cache. This is what makes it the "one shot" run instead of the persistent `dotnet tool install` approach.

## How does `dnx` work behind the scenes?

Whenever a big new feature drops in .NET, I like to sniff around to see how it was implemented, and `dnx` was no different. I started by finding the origin of the `dnx` command itself by running `where dnx` from a command line:

```cmd
> where dnx
C:\Program Files\dotnet\dnx.cmd
```

As you can see, the `dnx` command is actually a `cmd` file, installed side-by-side with the `dotnet.exe`. If you crack it open you can see that all it's doing is invoking `dotnet dnx`, and passing the remaining arguments:

```cmd
@echo off
"%~dp0dotnet.exe" dnx %*
```

There's [a similar file](https://github.com/dotnet/sdk/blob/main/src/Layout/redist/dnx) for Linux and MacOS that provides the `dnx` command, and as expected, it does a similar thing:

```bash
#!/bin/sh

# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.

"$(dirname "$0")/dotnet" dnx "$@"
```

So the .NET SDK exposes [a `dnx` command](https://github.com/dotnet/sdk/blob/681138b2d3d7255a17ad6cb4812787a0d5edef99/src/Cli/dotnet/Commands/Dnx/DnxCommandParser.cs#L10) which parses the arguments and options, and creates an instance of [the `ToolExecuteCommand`](https://github.com/dotnet/sdk/blob/main/src/Cli/dotnet/Commands/Tool/Execute/ToolExecuteCommand.cs#L42). This command is responsible for downloading and running the command.

At a high level, this command does the following things:

1.  If a version for the tool is _not_ specified, try to locate the tool [in the local tools manifest](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-local-tool). If it's present in the manifest, run the tool from there.
2.  Otherwise, try to find the package to install from nuget.org.
3.  If the tool is not currently downloaded, ask for permission and download the tool.
4.  Finally, run the tool.

That's pretty much all there is to it, but I'll go into a little more detail below.

The `ToolExecuteCommand` only considers the local tool manifest if you don't specify a version to install, otherwise it skips the step entirely. Next the command looks for a tool manifest, _dotnet-tools.json_. If it finds a manifest, and the tool being run is _listed_ in the manifest, the command downloads and runs the tool without any further interaction.

If you _do_ specify a version, or if there's no manifest, or the manifest doesn't list the tool being run, then it moves onto installing the tool globally. First it searches for the package on nuget.org, or whichever sources are configured in the _nuget.config_ file.

Assuming the requested package exists, the command next checks whether it has permission to download the package. The command prompts for confirmation as we saw earlier (`Proceed? [y/n] (y): y`). Interestingly, this prompt is shown whether or not you're running in an interactive setting; though you can bypass the flag by passing the auto-confirm `--yes` flag.

Once the command has confirmation, it downloads the tool and runs it. Voila!

That's about all there is to the `dnx` command. It's a nice little quality of life bump for the .NET ecosystem, which leverages all the existing features of .NET tools to make things that little bit easier.

## Summary

In this short post I showed the new `dnx` command and how to use it to run .NET tools without explicitly installing them first. Next I discussed the difference between the `dnx` command and the `dotnet tool install` command. Finally I walked through the code behind the feature; the `ToolExecuteCommand` in the .NET SDK.

This Series [Exploring the .NET 10 preview](/series/exploring-the-dotnet-10-preview/)