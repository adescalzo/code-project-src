```yaml
---
title: "The new Dependabot NuGet updater: 65% faster with native .NET - .NET Blog"
source: https://devblogs.microsoft.com/dotnet/the-new-dependabot-nuget-updater/?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-259
date_published: 2025-08-04T15:00:00.000Z
date_captured: 2025-08-11T13:09:41.220Z
domain: devblogs.microsoft.com
author: Jamie Magee
category: ai_ml
technologies: [Dependabot, NuGet, .NET, MSBuild, .NET CLI, GitHub, Azure Artifacts, Central Package Management, Ruby]
programming_languages: [C#, Ruby, XML, YAML]
tags: [dependabot, nuget, .net, package-management, security, automation, performance, dependency-management, msbuild, ci-cd]
key_concepts: [dependency-management, transitive-dependencies, central-package-management, msbuild-api, nuget-client-libraries, vulnerability-resolution, automated-updates, ci-cd-integration]
code_examples: false
difficulty_level: intermediate
summary: |
  This article announces a significant refactor of the Dependabot NuGet updater, transitioning from a hybrid Ruby-based solution to a native .NET implementation. This rewrite dramatically improves performance, reducing update times by 65% and increasing success rates from 82% to 94%. The new updater leverages native .NET tooling like NuGet client libraries, MSBuild APIs, and the .NET CLI for accurate dependency detection and sophisticated resolution. Key enhancements include proper handling of conditional package references, transitive dependency updates, related package updates, `global.json` support, and full Central Package Management integration. These improvements provide faster, more reliable, and accurate dependency management for .NET projects on GitHub.
---
```

# The new Dependabot NuGet updater: 65% faster with native .NET - .NET Blog

# The new Dependabot NuGet updater: 65% faster with native .NET

If you’ve ever waited impatiently for Dependabot to update your .NET dependencies, or worse, watched it fail with cryptic errors, we have some great news. Over the past year, the Dependabot team has worked on a refactor of the NuGet updater, and the results are impressive.

## From hybrid to native

The previous NuGet updater used a hybrid solution that relied heavily on manual XML parsing and string replacement operations written in Ruby. While this approach worked for basic scenarios, it struggled with the complexity and nuances of modern .NET projects. The new updater takes a completely different approach by using .NET’s native tooling directly.

Instead of trying to reverse-engineer what NuGet and MSBuild do, the new updater leverages actual .NET tooling:

*   [NuGet client libraries](https://learn.microsoft.com/nuget/reference/nuget-client-sdk) for package operations
*   [MSBuild APIs](https://learn.microsoft.com/visualstudio/msbuild/msbuild-api) for project evaluation and dependency resolution
*   [.NET CLI](https://learn.microsoft.com/dotnet/core/tools/) for restore operations

This shift from manual XML manipulation to using the actual .NET toolchain means the updater now behaves exactly like the tools developers use every day.

## Performance and reliability improvements

The improvements in the new updater are dramatic. The test suite that previously took 26 minutes now completes in just 9 minutes—a 65% reduction in runtime. But speed is only part of the story. The success rate for updates has jumped from 82% to 94%, meaning significantly fewer failed updates that require manual intervention.

These improvements work together to deliver a faster, more reliable experience. When Dependabot runs on your repository, it spends less time processing updates and succeeds more often—reducing both the wait time and the manual intervention needed to keep your dependencies current.

## Real dependency detection with MSBuild

One of the most significant improvements is how the updater discovers and analyzes dependencies. Previously, the Ruby-based parser would attempt to parse project files as XML and guess what the final dependency graph would look like. This approach was fragile and missed complex scenarios.

The new updater uses MSBuild’s project evaluation engine to properly understand your project’s true dependency structure. This means it can now handle complex scenarios that previously caused problems.

For example, the old parser missed conditional package references like this:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
</ItemGroup>
```

With the new MSBuild-based approach, the updater can handle

*   Conditional package references based on target framework or build configuration
*   [`Directory.Build.props` and `Directory.Build.targets`](https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory) that modify dependencies
*   MSBuild variables and property evaluation throughout the project hierarchy
*   Complex package reference patterns that weren’t reliably detected before

## Dependency resolution solving

One of the most impressive features of the new updater is its sophisticated dependency resolution engine. Instead of updating packages in isolation, it now performs comprehensive conflict resolution. This includes two key capabilities:

### Transitive dependency updates

When you have a vulnerable transitive dependency that can’t be directly updated, the updater will now automatically find the best way to resolve the vulnerability. Let’s look at a real scenario where your app depends on a package that has a vulnerable transitive dependency:

```plaintext
YourApp
└── PackageA v1.0.0
    └── TransitivePackage v2.0.0 (CVE-2024-12345)
```

The new updater follows a smart resolution strategy:

1.  First, it checks if `PackageA` has a newer version available that depends on a non-vulnerable version of `TransitivePackage`. If `PackageA` v2.0.0 depends on `TransitivePackage` v3.0.0 (which fixes the vulnerability), Dependabot will update `PackageA` to v2.0.0.

2.  If no updated version of `PackageA` is available, Dependabot will add a direct dependency on a non-vulnerable version of `TransitivePackage` to your project. This leverages NuGet’s [‘direct dependency wins’ rule](https://learn.microsoft.com/nuget/concepts/dependency-resolution#direct-dependency-wins), where direct dependencies take precedence over transitive ones:

```xml
<PackageReference Include="PackageA" Version="1.0.0" />
<PackageReference Include="TransitivePackage" Version="3.0.0" />
```

With this approach, even though `PackageA` v1.0.0 still references `TransitivePackage` v2.0.0, NuGet will use v3.0.0 because it’s a direct dependency of your project. This ensures your application uses the secure version without waiting for `PackageA` to be updated.

### Related package updates

The updater also identifies and updates related packages to avoid version conflicts. If updating one package in a family (like `Microsoft.Extensions.*` packages) would create version mismatches with related packages, the updater automatically updates the entire family to compatible versions.

This intelligent conflict resolution dramatically reduces the number of failed updates and eliminates the manual work of resolving package conflicts.

## Honoring global.json

The new updater now properly respects [`global.json`](https://learn.microsoft.com/dotnet/core/tools/global-json) files, a feature that was inconsistently supported in the previous version. If your project specifies a particular .NET SDK version, the updater will install the exact SDK version specified in your `global.json`. This ensures that the updater evaluates dependency updates using the same .NET SDK version that your development team and CI/CD pipelines use, eliminating a common source of inconsistencies.

This improvement complements Dependabot’s recently added capability to [update .NET SDK versions in global.json files](https://devblogs.microsoft.com/dotnet/using-dependabot-to-manage-dotnet-sdk-updates/). While the SDK updater keeps your .NET SDK version current with security patches and improvements, the NuGet updater respects whatever SDK version you’ve chosen—whether manually specified or automatically updated by Dependabot. This seamless integration means you get the best of both worlds: automated SDK updates when you want them, and consistent package dependency resolution that honors your SDK choices.

## Full Central Package Management support

[Central Package Management (CPM)](https://learn.microsoft.com/nuget/consume-packages/central-package-management) has become increasingly popular in .NET projects for managing package versions across multiple projects. The previous updater had limited support for CPM scenarios, often requiring manual intervention.

The new updater provides comprehensive CPM support. It automatically detects `Directory.Packages.props` files, properly updates versions in centralized version files, supports package overrides in individual projects, and handles transitive dependencies managed through CPM. Whether you’re using CPM for version management, security vulnerability management, or both, the new updater handles these scenarios seamlessly.

## Support for all compliant NuGet feeds

The previous updater struggled with private NuGet feeds, especially those with non-standard authentication or API implementations. The new updater uses NuGet’s official client libraries. This means it automatically supports all [NuGet v2 and v3 feeds](https://learn.microsoft.com/nuget/api/overview), including nuget.org, Azure Artifacts, and GitHub Packages. It also:

*   Works with standard authentication mechanisms like API keys or personal access tokens
*   Handles feed-specific behaviors and quirks that the NuGet client manages
*   Supports [package source mapping](https://learn.microsoft.com/nuget/consume-packages/package-source-mapping) configurations for enterprise scenarios

If your .NET tools can access a feed, Dependabot can too.

## What this means for you

If you’re using Dependabot for .NET projects, you should notice these improvements immediately. Faster updates mean dependency scans and update generation happen more quickly. More successful updates result in fewer failed updates that require manual intervention. Better accuracy ensures updates that properly respect your project’s configuration and constraints. And when updates do fail, you’ll get clearer errors with actionable error messages.

You don’t need to change anything in your [`dependabot.yml`](https://docs.github.com/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file) configuration—you automatically get these improvements for all .NET projects.

## Looking forward

This rewrite represents more than just performance improvements—it’s a foundation for future enhancements. By building on .NET’s native tooling, the Dependabot team will be able to add support for new .NET features as they’re released, improve integration with .NET developer workflows, extend capabilities to handle more complex enterprise scenarios, and provide better diagnostics and debugging information.

The new architecture also makes it easier for the community to contribute improvements and fixes, as we rewrote the codebase in C# and leverage the same tools and libraries that .NET developers use every day. This means that developers can make contributions using familiar .NET development practices, making it easier for the community to help shape the future of Dependabot’s NuGet support.

## Try it out

The new NuGet updater is already live and processing updates for .NET repositories across GitHub. If you haven’t enabled Dependabot for your .NET projects yet, now is a great time to start. Here’s a minimal configuration to get you started:

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
```

And if you’re already using Dependabot, you should already be seeing the improvements. Faster updates, fewer failures, and clearer error messages—all without changing a single line of configuration.

The rewrite demonstrates how modern dependency management should work: fast, accurate, and transparent. By leveraging the same tools that developers use every day, Dependabot can now provide an experience that feels native to the .NET ecosystem while delivering the automation and security benefits that make dependency management less of a chore.