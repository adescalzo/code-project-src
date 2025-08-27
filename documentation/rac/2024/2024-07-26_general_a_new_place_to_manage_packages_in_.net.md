```yaml
---
title: A New Place To Manage Packages in .NET
source: https://okyrylchuk.dev/blog/new-place-to-manage-packages-in-dotnet/
date_published: 2024-07-26T19:30:50.000Z
date_captured: 2025-08-20T21:13:40.582Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [NuGet, .NET, MSBuild, Newtonsoft.Json, MediatR, Serilog]
programming_languages: [C#, XML]
tags: [nuget, package-management, dotnet, dependencies, msbuild, configuration, versioning, solution-management]
key_concepts: [central-package-management, directory.packages.props, packagereference, transitive-pinning, version-overriding, dependency-resolution]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces Central Package Management (CPM) in NuGet, a feature available since NuGet 6.2, designed to simplify dependency management in multi-project .NET solutions. It explains how to use the `Directory.Packages.props` file to centralize package version definitions, eliminating the need to specify versions in individual project files. The article also covers advanced scenarios such as managing multiple `Directory.Packages.props` files, overriding package versions for specific projects, and enabling transitive pinning to explicitly control transitive dependencies. Adopting CPM helps ensure consistency, reduce conflicts, and streamline updates across large solutions, enhancing overall project maintainability.]
---
```

# A New Place To Manage Packages in .NET

# A New Place To Manage Packages in .NET

NuGet is an open-source package management system for .NET developed by Microsoft. It allows developers to create, share, and consume reusable code in packages. These packages can include compiled code (DLLs), source code, configuration files, scripts, and other content.

NuGet supports versioning, allowing developers to specify which versions of a package they need and to easily manage updates and rollbacks.

Managing dependencies for a single project is easy. It becomes complicated when there are many projects in the solution.

Let’s see how we can manage NuGet packages from one central place.

## History

For old project types, you could use **packages.config** file having an XML format. It’s deprecated.

Then, there was the **packages.json** file in JSON format. It’s also deprecated 🙂

For new-type projects, you should use PackageReference in project files.

![Example of a .NET project file showing PackageReference items with explicit versions for Newtonsoft.Json, MediatR, and Serilog.](packagereference_v.png)

As said, managing versions for a single project is easy. You have one place to change. But it takes work when you have many projects, each with many dependencies.

## Central Package Management

Since NuGet 6.2, you can manage your dependencies in all your projects in one place. You have to add **Directory.Packages.props** file to your solution.

![Content of a Directory.Packages.props file, enabling central package management and defining PackageVersion elements for various dependencies.](directory_packages_props_initial.png)

It’s an XML file, as you can see. You must add the MSBuild property `ManagePackageVersionsCentrally` and set the `true` value.

Then, you list all your dependencies with `<PackageVersion />` elements defining their versions.

When you build the solution, you’ll get the error:

`NU1008 Projects that use central package version management should not define the version on the PackageReference items but on the PackageVersion items: Serilog;Newtonsoft.Json;MediatR.`

That’s because from now the **Directory.Packages.props** file is the only source of truth. So, let’s get rid of the versions in the project.

![A .NET project file showing PackageReference items without explicit versions, relying on central package management.](packagereference_no_version.png)

That’s it. Now, all versions of dependencies can be managed from one place.

## Many Directory.Packages.props Files

You can have multiple **Directory.Packages.props** files in your repository when you have several solutions.

The closest to the project **Directory.Packages.props** file will be used.

![A diagram illustrating a repository structure with multiple Directory.Packages.props files at different levels, showing how projects resolve their package versions from the closest file.](multiple_directory_packages_props.png)

Project1 will use the **Directory.Packages.props** file in the Repository\Solution1\ directory.

Project2 will use the **Directory.Packages.props** file in the Repository\ directory.

## Overriding Versions

The `VersionOverride` property allows you to override a specific package version in the project. This can be helpful if you want to test different package versions.

![A .NET project file demonstrating how to use the VersionOverride property within a PackageReference to specify a different version for a package.](version_override.png)

You can disable overriding versions by setting the MSBuild property **CentralPackageVersionOverrideEnabled** to false.

![A Directory.Packages.props file showing the CentralPackageVersionOverrideEnabled property set to false, disabling version overrides.](disable_version_override.png)

## Transitive Pinning

When you add a NuGet package to your project, that package may have its own dependencies, which in turn may have its own dependencies, and so on. These are known as transitive dependencies. By default, NuGet will resolve these transitive dependencies based on version constraints specified by the packages and the rules for version resolution.

Transitive pinning in NuGet explicitly specifies the versions of transitive dependencies.

To enable this feature, you can do it with the MSBuild property **CentralPackageTransitivePinningEnabled** set to true.

![A Directory.Packages.props file showing the CentralPackageTransitivePinningEnabled property set to true, enabling transitive pinning.](transitive_pinning.png)

## Summary

**Central Package Management** in NuGet is a feature that allows you to manage package versions for all projects in a solution from a single location. This centralized approach simplifies dependency management and ensures consistency across multiple projects.

Use the **Directory.Packages.props** file to define package versions. This file is placed at the root of your solution and applies to all projects within the solution.

It ensures all projects use the same package version, reducing the risk of version conflicts and simplifying updates.

Managing package versions from one file makes updating and maintaining dependencies easier across large solutions.

By adopting central package management, you can streamline workflow, enhance project maintainability, and ensure a more robust and consistent development environment.