```yaml
---
title: "Stop using .sln files — switch to .slnx in .NET (yes, now!) 🚀 | by Florian Métoz | Aug, 2025 | Medium"
source: https://medium.com/@metoz.florian/stop-using-sln-files-switch-to-slnx-in-net-now-ae736f18f72a
date_published: 2025-08-08T11:58:57.557Z
date_captured: 2025-08-11T13:08:02.822Z
domain: medium.com
author: Florian Métoz
category: general
technologies: [.NET, Visual Studio, .NET SDK, MSBuild, JetBrains Rider, Git]
programming_languages: []
tags: [.net, solution-file, slnx, sln, visual-studio, cli, msbuild, version-control, developer-experience, tooling]
key_concepts: [solution-file-format, merge-conflicts, xml-format, command-line-interface, developer-workflow, tooling-support, migration]
code_examples: false
difficulty_level: intermediate
summary: |
  This article advocates for migrating from the legacy `.sln` solution file format to the new XML-based `.slnx` format in .NET projects. It highlights the long-standing issues with `.sln`, such as cryptic GUIDs and frequent merge conflicts, which hinder team collaboration and CI/CD pipelines. The `.slnx` format offers improved human readability, easier diffing, and better integration with CLI tools and modern IDEs like Visual Studio and JetBrains Rider. The author provides practical steps for creating new `.slnx` files or migrating existing `.sln` files using .NET SDK commands, emphasizing the benefits for developer experience and team velocity.
---
```

# Stop using .sln files — switch to .slnx in .NET (yes, now!) 🚀 | by Florian Métoz | Aug, 2025 | Medium

# Stop using `.sln` files — switch to `.slnx` in .NET (yes, now!) 🚀

The old `.sln` served us well, but the new XML-based `.slnx` fixes decades of pain — fewer merge fights, better tooling, and CLI-first workflows. Here’s why you should consider migrating today (and how to do it painlessly). ✨

> If you are not a Medium member yet, you can read the full article [here](/@metoz.florian/ae736f18f72a?sk=890d2ff57b8c2d33fb1eb3782554f752) — please feel free to give it some applause 😊

![A graphic with an orange background. At the top, it says "STOP USING" above a white document icon labeled ".SLN" with a red circle-slash "no" symbol over it. Below, it says "SWITCH TO" above another white document icon labeled ".SLNX" with a white rocket ship taking off next to it. At the bottom, it reads "IN .NET NOW". This image visually conveys the article's main message: stop using .SLN and switch to .SLNX in .NET.](https://miro.medium.com/v2/resize:fit:700/1*-gVl9DSMk7eESznNCMp_Vw.png)

# TL;DR

`.slnx` is an XML-based solution file format that modernizes the legacy `.sln`. It’s now supported by Visual Studio, the .NET SDK/CLI, MSBuild and third-party IDEs — which means fewer GUID headaches, easier diffs, and smoother CI. Migrate using `dotnet sln migrate` or `dotnet new sln --format slnx`. 👇

# Why `.sln` has been annoying for years 🥴

The classic `.sln` format is essentially a bespoke blob: lots of GUIDs referencing GUIDs, cryptic layout sections, and frequent, nasty merge conflicts when multiple people touch the solution file. That made `.sln` hard to read, edit, and maintain outside Visual Studio — and it often became a bottleneck in team workflows.

```
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

# Meet `.slnx`: what it is and why it matters 🌱

`.slnx` is a new, **XML-based** solution format designed to replace the legacy `.sln`. It was created to be readable, tool-friendly, and easier to diff and merge. Tooling support is coming together: **Microsoft Visual Studio, JetBrains Rider** and the .NET SDK/CLI can create, read, and build `.slnx` files, and third-party IDEs (like JetBrains Rider) are already adding support. **In short:** it’s the modern, maintainable version of the solution file.

```xml
<Solution>
  <Configurations>
    <Platform Name="Any CPU" />
    <Platform Name="x64" />
    <Platform Name="x86" />
  </Configurations>
  <Project Path="my-app/my-app.csproj" />
</Solution>
```

# The practical benefits (real-world wins) ✅

*   **Fewer merge conflicts:** XML structure and clearer references make diffs meaningful and conflicts simpler to resolve.
*   **Human-readable:** You can open and understand a solution file without wrestling with GUID lists.
*   **CLI + build friendliness:** `dotnet` tooling can operate on `.slnx` the same way it does `.sln` — generate, migrate, build. That makes automation and CI pipelines cleaner.
*   **Easier cross-tool support:** With a standard, documented XML schema, editors and tooling can interoperate without hacks.

# When not to rush a migration ⚠️

*   If you’re pinned to an older Visual Studio or SDK that lacks support, hold off until your toolchain is updated. (But check your SDK — recent .NET SDKs and Visual Studio previews already support `.slnx`.)
*   Big mono-repo with specialized tooling? Test the migration in a branch and run CI thoroughly — don’t flip the whole repo overnight.

# How to try it today — step-by-step 🚶‍♀️🚶‍♂️

[https://devblogs.microsoft.com/dotnet/introducing-slnx-support-dotnet-cli/](https://devblogs.microsoft.com/dotnet/introducing-slnx-support-dotnet-cli/)

**1\. Create a fresh** `**.slnx**`

```bash
# new empty solution in slnx format
dotnet new sln --format slnx -n MySolution
```

**2\. Migrate an existing** `**.sln**`

With .NET SDK 9.0.200 or greater, use:

```bash
# creates a .slnx version from your existing .sln
dotnet sln migrate
```

Or use `saveAs` on your favorite IDE after enabling the `**.slnx**` feature.

**3\. Build from** `**.slnx**`

```bash
dotnet build ./MySolution.slnx   # Explicitly target the .slnx file
# or
dotnet build                     # Implicitly builds the only solution file in the folder
```

(These commands are supported in recent .NET SDK releases and integrate with `dotnet` CLI workflows.)

# A tiny migration checklist for teams 🧰

*   Create a **migration branch** and run `dotnet sln migrate` on your main solution.
*   Run **all tests in CI** with the `.slnx` file.
*   Ensure developers **update to a supported VS/SDK version** (document the minimum).
*   Add a short note to **PR templates** reminding reviewers that `.slnx` diffs are expected.
*   Keep a **rollback plan** — you can still use `.sln` if you need to revert.

# What about editing `.slnx` by hand? ✍️

Yes — `.slnx` is XML, so it’s easier to edit in a text editor or script programmatically. But don’t treat it like a replacement for proper tooling: prefer `dotnet sln` commands or your IDE for adding/removing projects to avoid mistakes.

# Final pitch: less pain, more velocity 🎯

If you value **readable repos**, **fewer merge conflicts**, and tooling that plays nicely with CI and editors — `.slnx` is not a gimmick. It’s a practical upgrade for the .NET ecosystem.

Try it on a small solution, prove it in CI, then roll it out team-wide.
Your **future self** (and your teammates) will thank you. 🙌

**_💬 What’s the one thing that would convince you to try_** `**_.slnx_**` **_on your next project?_**