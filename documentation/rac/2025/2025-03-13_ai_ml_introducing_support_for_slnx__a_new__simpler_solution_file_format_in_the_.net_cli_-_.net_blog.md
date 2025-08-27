```yaml
---
title: Introducing support for SLNX, a new, simpler solution file format in the .NET CLI - .NET Blog
source: https://devblogs.microsoft.com/dotnet/introducing-slnx-support-dotnet-cli/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2020
date_published: 2025-03-13T17:30:00.000Z
date_captured: 2025-08-12T22:42:56.005Z
domain: devblogs.microsoft.com
author: Chet Husk
category: ai_ml
technologies: [.NET CLI, Visual Studio, C# Dev Kit, JetBrains Rider, .NET SDK, SLNX, SLN, Microsoft.VisualStudio.SolutionPersistence, slngen, MSBuild, NuGet, .NET, .NET Framework, XML, PowerShell]
programming_languages: [C#, PowerShell]
tags: [.net, dotnet-cli, visual-studio, solution-files, slnx, development-tools, ide, build-system, project-management, cli-tooling]
key_concepts: [solution-file-format, command-line-interface, project-management, migration, ide-integration, build-automation, open-source-development, ci-cd]
code_examples: true
difficulty_level: intermediate
summary: |
  This article introduces SLNX, a new, simpler XML-based solution file format for .NET, designed to replace the older `.sln` format. It details the integration of SLNX support into the .NET SDK 9.0.200, showcasing how developers can use `dotnet` CLI commands to migrate, build, and manage projects with the new format. The post also provides an overview of current SLNX support across various .NET IDEs and tools, including Visual Studio, C# Dev Kit, slngen, and JetBrains Rider, highlighting areas of ongoing development. Developers are encouraged to experiment with the new format and provide feedback to contribute to its general availability.
---
```

# Introducing support for SLNX, a new, simpler solution file format in the .NET CLI - .NET Blog

# Introducing support for SLNX, a new, simpler solution file format in the .NET CLI

![Chet Husk](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2021/11/chet-smaller-head-2-96x96.jpg)

[Chet Husk](https://devblogs.microsoft.com/dotnet/author/chethusk)

Program Manager

## Table of contents

*   [Getting started](#getting-started)
*   [Managing projects from the CLI](#managing-projects-from-the-cli)
*   [Support for SLNX across .NET IDEs and Tooling](#support-for-slnx-across-.net-ides-and-tooling)
*   [Visual Studio](#visual-studio)
*   [C# Dev Kit](#c#-dev-kit)
*   [slngen](#slngen)
*   [JetBrains Rider](#jetbrains-rider)
*   [Feedback and the road to GA](#feedback-and-the-road-to-ga)
*   [Summary](#summary)

Solution files have been a part of the .NET and Visual Studio experience for many years now, and they’ve had the same custom format the whole time. Recently, the [Visual Studio solution team has begun previewing a new](https://devblogs.microsoft.com/visualstudio/new-simpler-solution-file-format), XML-based solution file format called SLNX. Starting in .NET SDK 9.0.200, the `dotnet` CLI supports building and interacting with these files in the same way as it does with existing solution files. In the rest of this post we’ll show how users can migrate to the new format, explore the new support across the `dotnet` CLI, and discuss the next steps towards a generally-available release of the format.

## Getting started

Before the 9.0.200 SDK, the only way to create a SLNX file was through the Visual Studio settings. The `Environment > Preview Features > Use Solution File Persistence Model` setting, when checked, would allow users to `Save As` their existing `.sln` files in the new `.slnx` format. Starting in Visual Studio 17.14, this feature is stable and generally available.

The 9.0.200 SDK provides a command to do this same migration: `dotnet sln migrate`.

Let’s start with a very simple solution and project setup to look at what it takes to migrate. First, we’ll create a new solution:

```powershell
PS C:\Users\chethusk\Code\example> dotnet new sln
The template "Solution File" was created successfully.

PS C:\Users\chethusk\Code\example> cat .\example.sln

Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Global
        GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|Any CPU = Debug|Any CPU
                Release|Any CPU = Release|Any CPU
        EndGlobalSection
        GlobalSection(SolutionProperties) = preSolution
                HideSolutionNode = FALSE
        EndGlobalSection
EndGlobal
```

Now, we’ll create a project and add it to the solution:

```powershell
PS C:\Users\chethusk\Code\example> dotnet new console -n my-app
The template "Console App" was created successfully.

Processing post-creation actions...
Restoring C:\Users\chethusk\Code\example\my-app\my-app.csproj:
Restore succeeded.

PS C:\Users\chethusk\Code\example> dotnet sln add .\my-app\
Project `my-app\my-app.csproj` added to the solution.

PS C:\Users\chethusk\Code\example> cat .\example.sln

Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "my-app", "my-app\my-app.csproj", "{845B7716-6F03-4D02-8E86-79F95485B5D7}"
EndProject
Global
        GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|Any CPU = Debug|Any CPU
                Debug|x64 = Debug|x64
                Debug|x86 = Debug|x86
                Release|Any CPU = Release|Any CPU
                Release|x64 = Release|x64
                Release|x86 = Release|x86
        EndGlobalSection
        GlobalSection(ProjectConfigurationPlatforms) = postSolution
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Debug|Any CPU.Build.0 = Debug|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Debug|x64.ActiveCfg = Debug|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Debug|x64.Build.0 = Debug|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Debug|x86.ActiveCfg = Debug|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Debug|x86.Build.0 = Debug|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Release|Any CPU.ActiveCfg = Release|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Release|Any CPU.Build.0 = Release|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Release|x64.ActiveCfg = Release|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Release|x64.Build.0 = Release|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Release|x86.ActiveCfg = Release|Any CPU
                {845B7716-6F03-4D02-8E86-79F95485B5D7}.Release|x86.Build.0 = Release|Any CPU
        EndGlobalSection
        GlobalSection(SolutionProperties) = preSolution
                HideSolutionNode = FALSE
        EndGlobalSection
EndGlobal
```

Now, let’s convert our solution to the new format:

```powershell
PS C:\Users\chethusk\Code\example> dotnet sln migrate
.slnx file C:\Users\chethusk\Code\example\example.slnx generated.
PS C:\Users\chethusk\Code\example> cat .\example.slnx
<Solution>
  <Configurations>
    <Platform Name="Any CPU" />
    <Platform Name="x64" />
    <Platform Name="x86" />
  </Configurations>
  <Project Path="my-app/my-app.csproj" />
</Solution>
```

The new format is XML-based and is much more concise than the old format – but it contains all of the same data! The data that is missing from the new format is part of the defaults of the format, so no functionality is lost.

This migration is made possible because the Visual Studio Solution team has created a new open-source library for parsing and working with both classic and XML-based solution files – the library is called [Microsoft.VisualStudio.SolutionPersistence](https://github.com/microsoft/vs-solutionpersistence).

## Managing projects from the CLI

You can do more than migrating solution files using the CLI, too. As you might expect, you can build the new solutions the same way you would build the old:

```powershell
PS C:\Users\chethusk\Code\example> dotnet build .\example.slnx
Restore complete (0.6s)
  my-app succeeded (4.3s) → my-app\bin\Debug\net9.0\my-app.dll

Build succeeded in 5.3s
```

We specified the `.slnx` file explicitly above because it’s an error to run `dotnet build` or other commands that need to build in a directory with both a `.sln` and a `.slnx` – we don’t know which one to build!

All of the other interactions you expect from the `dotnet` CLI work as well. We can add projects:

```powershell
PS C:\Users\chethusk\Code\example> dotnet new classlib -n my-lib
The template "Class Library" was created successfully.

Processing post-creation actions...
Restoring C:\Users\chethusk\Code\example\my-lib\my-lib.csproj:
Restore succeeded.

PS C:\Users\chethusk\Code\example> dotnet sln .\example.slnx add my-lib
Project `my-lib\my-lib.csproj` added to the solution.

PS C:\Users\chethusk\Code\example> cat .\example.slnx
<Solution>
  <Configurations>
    <Platform Name="Any CPU" />
    <Platform Name="x64" />
    <Platform Name="x86" />
  </Configurations>
  <Project Path="my-app/my-app.csproj" />
  <Project Path="my-lib/my-lib.csproj" />
</Solution>
```

We can list the projects in a solution:

```powershell
PS C:\Users\chethusk\Code\example> dotnet sln .\example.slnx list
Project(s)
----------
my-app\my-app.csproj
my-lib\my-lib.csproj
```

And finally we can remove projects from the solution:

```powershell
PS C:\Users\chethusk\Code\example> dotnet sln .\example.slnx remove .\my-lib\
Project `my-lib\my-lib.csproj` removed from the solution.

PS C:\Users\chethusk\Code\example> cat .\example.slnx
<Solution>
  <Configurations>
    <Platform Name="Any CPU" />
    <Platform Name="x64" />
    <Platform Name="x86" />
  </Configurations>
  <Project Path="my-app/my-app.csproj" />
</Solution>
```

There are two commands that _don’t_ work in 9.0.200, though – `dotnet nuget why` and `dotnet list package` – those will begin working in the 9.0.201 release in March.

## Support for SLNX across .NET IDEs and Tooling

As mentioned above, the `dotnet` CLI has broad support for the new SLNX file format, but there are still many tools in the ecosystem that have partial or no support for the format. You will need to take this varying level of support into account when choosing whether to migrate to SLNX files. Some examples of tools that have varying levels of support for slnx today are:

### Visual Studio

While the IDE will read the SLNX file when loaded, it currently will not load SLNX files unless the setting to enable SLNX persistence has been enabled. This means if you work on a team and users have not toggled this setting, they will not be able to open SLNX files at all. In addition, double-clicking on SLNX files doesn’t currently open Visual Studio instances the way that sln files do.

### C# Dev Kit

C# DevKit can support SLNX files, but in order to do so you must set the `dotnet.defaultSolution` property to the path to your slnx file:

```json
{
  "dotnet.defaultSolution": "example.slnx"
}
```

### slngen

The [slngen](https://microsoft.github.io/slngen/) tool is a command-line utility used to synthesize a solution file for a given project to help make repos that prefer not to use solution files interoperate better with Visual Studio. This tool is not yet updated to support SLNX – the status of this support can be tracked at [microsoft/slngen#643](https://github.com/microsoft/slngen/issues/643).

### JetBrains Rider

Rider has preliminary support for the SLNX format, and details about their support can be tracked at [RIDER-110777](https://youtrack.jetbrains.com/issue/RIDER-110777).

## Feedback and the road to GA

Despite this end-to-end support in Visual Studio and the `dotnet` CLI, the SLNX format itself is in its infancy. While we think it’s a great step forward in usability for many .NET developers, we want to hear from you as you try it in your teams. Try the migration paths in Visual Studio and the `dotnet` CLI, make sure things work as you expect in your CI/CD pipelines and local builds, and make sure to let the teams know about your experiences in the following ways:

*   for CLI experiences, report new issues or discussions at the [dotnet/sdk](https://github.com/dotnet/sdk) repository
*   for Visual Studio experiences, please raise new tickets at the [Visual Studio Developer Community](https://developercommunity.visualstudio.com/)
*   for feature requests for the solution parsing library, report new issues at the [microsoft/vs-solutionpersistence](https://github.com/microsoft/vs-solutionpersistence) repository

As we’re able to respond to your feedback and solidify core user experiences, we move closer towards being able to make this the default for Visual Studio and the `dotnet` CLI.

## Summary

SLNX files are an exciting new change to the solution file format that we think will make it easier for teams to collaborate and understand their projects. The new capabilities in the `dotnet` CLI allow developers to have a full inner-loop and CI experience using the new format, so we’d love for .NET developers to read through the [updated documentation](https://learn.microsoft.com/dotnet/core/tools/dotnet-sln), try the new support, and give us feedback!