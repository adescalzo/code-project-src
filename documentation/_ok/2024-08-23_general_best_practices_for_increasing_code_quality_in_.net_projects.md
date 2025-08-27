```yaml
---
title: Best Practices for Increasing Code Quality in .NET Projects
source: https://antondevtips.com/blog/best-practices-for-increasing-code-quality-in-dotnet-projects
date_published: 2024-08-23T08:55:21.852Z
date_captured: 2025-08-06T17:28:05.777Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, Visual Studio, JetBrains Rider, Meziantou.Analyzer, SonarAnalyzer.CSharp, Roslynator.Analyzers, NuGet, Qodana, SonarQube, Codacy, GitHub]
programming_languages: [C#, XML, F#, VB]
tags: [code-quality, .net, static-analysis, code-review, development-practices, ide, build-process, analyzers, developer-tools, best-practices]
key_concepts: [static-code-analysis, code-review, nullable-reference-types, implicit-usings, warnings-as-errors, code-style-enforcement, editorconfig, directory.build.props, code-metrics]
code_examples: true
difficulty_level: intermediate
summary: |
  This article provides best practices for improving code quality in .NET projects, emphasizing its impact on maintainability, scalability, and reliability. It discusses various tools and practices, including IDE features, static code analysis, external code analysis software, and code reviews. The post offers practical guidance on configuring `Directory.Build.props` for consistent project settings like nullable reference types and treating warnings as errors, and using `.editorconfig` for enforcing coding styles. It also recommends specific NuGet analyzer packages such as Meziantou.Analyzer, SonarAnalyzer.CSharp, and Roslynator.Analyzers, along with IDE plugins for Visual Studio and JetBrains Rider to enhance code inspection and identify complexity. The author concludes by stressing that investing in code quality leads to more robust and efficient software.
---
```

# Best Practices for Increasing Code Quality in .NET Projects

![Cover image for the article titled "BEST PRACTICES FOR INCREASING CODE QUALITY IN .NET PROJECTS" with a "dev tips" logo.](https://antondevtips.com/media/covers/architecture/cover_architecture_code_quality.png)

# Best Practices for Increasing Code Quality in .NET Projects

Aug 23, 2024

[Download source code](/source-code/best-practices-for-increasing-code-quality-in-dotnet-projects)

5 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Code quality is a crucial aspect of software development that directly impacts the maintainability, scalability, and reliability of a project. High-quality code ensures that the software is easier to understand, modify, and extend, which leads to reduced development time and cost. It also minimizes the possibility of bugs and security vulnerabilities, thereby increasing the overall robustness of the application.

Investing in code quality early saves significant resources in the long run and improves developer productivity and satisfaction. In this blog post I want to share with you some tips for improving your code quality in .NET projects.

## Options to Improve Code Quality

There are several tools and practices available to improve code quality in .NET projects. Each has its own advantages and disadvantages, and a combination of these methods often yields the best results.

Here is a list of available options:

*   Code Editor (IDE) — offers real-time feedback and suggestions to improve code quality
*   Static Code Analysis — helps to apply coding standards, identify code smells, potential bugs and vulnerabilities when building the project in the IDE
*   Code Analysis Software — external software that provides a higher level of code analysis than static analyzers (often these are paid instruments)
*   Code Review — helps to catch issues that automated tools might miss

In the ideal scenario you should have all four options working together. Many teams use the first and the last option for improving the quality of their code and projects.

Code Review is a must-have; it helps to catch issues that automated tools might miss, such as architectural flaws, business or logic errors. It encourages knowledge sharing and collective code ownership among team members. But this option is the most expensive, it's a time-consuming process that requires coordination among team members. The quality of the review depends on the reviewers' expertise and diligence.

Using static code analysis tools can significantly reduce the time and cost of manual code review by reducing possible issues that can survive until the code review. In this blog post I will share with you how to use free static code analysis tools.

## Enforcing Code Analysis with Directory.Build.props

You can use centralized `Directory.Build.props` file to configure project-wide settings ensures consistency across all projects within a solution. You should put this file near your "\*.sln" file.

In my projects I use the following settings in `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AnalysisLevel>latest</AnalysisLevel>
    <AnalysisMode>All</AnalysisMode>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Meziantou.Analyzer" Version="2.0.155">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.26.0.92422">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Roslynator.Analyzers" Version="4.12.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

Let's break down each option:

**Nullable:** The Nullable option enables nullable reference types, which allow developers to explicitly specify whether a variable can be null. This feature helps catch potential null reference exceptions at compile time.

**ImplicitUsings:** The ImplicitUsings option, when enabled, automatically includes a set of common using directives in each file, reducing the need to manually add them.

**AnalysisLevel:** The AnalysisLevel option specifies the version of code analysis rules to apply. Setting it to the latest ensures the use of the most recent rules and improvements.

**AnalysisMode:** The AnalysisMode option defines the scope and strictness of the code analysis. Setting it to All enables a comprehensive set of rules.

**TreatWarningsAsErrors:** The TreatWarningsAsErrors option treats all compiler warnings as errors, enforcing the resolution of warnings before the code can compile successfully.

**CodeAnalysisTreatWarningsAsErrors:** Similar to TreatWarningsAsErrors, the CodeAnalysisTreatWarningsAsErrors option specifically targets code analysis warnings, treating them as errors.

**EnforceCodeStyleInBuild:** The EnforceCodeStyleInBuild option enforces code style rules during the build process, ensuring that the code adheres to the defined style guidelines.

These settings are crucial for maintaining code quality in your .NET projects. You should always strive to have 0 warnings in your project, but many developers are lazy fixing these warnings. `TreatWarningsAsErrors` ensures that you have no warnings in your project, otherwise your project won't compile.

Another option `CodeAnalysisTreatWarningsAsErrors` is also very useful as it raises compile errors for the issues found by static code analyzers.

I recommend turning `Nullable` on in all your new projects, this will save you from the most of NullReferenceExceptions. Imagine you need to mark your field as not required in your API, you change your `string` type to `string?` and the compiler will show you all places where you need to add null checks. Thanks that `TreatWarningsAsErrors` is turned on.

In the `Directory.Build.props` file I also specify `Meziantou.Analyzer`, `SonarAnalyzer.CSharp`, `Roslynator.Analyzers` Nuget packages that will be added to every csproj in the solution. These packages are static code analyzers that analyze the code for potential errors, code smells, security vulnerabilities, and adherence to coding standards. They perform inspection of the codebase that helps in finding issues during the development and build phase of the project.

Static code analyzers work by parsing the source code and applying a set of predefined rules and patterns. These packages are only analyzers, they aren't copied to the output directory. This means that they have no impact on the size of the published application.

You can check this [GitHub repository](https://github.com/cybermaxs/awesome-analyzers) to find even more analyzers and see all available options for the analyzers.

## Enforcing Code Analysis with .editorconfig

`.editorconfig` is a file used to maintain consistent coding styles and conventions across different editors and IDEs. It defines a set of rules for various coding aspects, such as indentation, line endings, and spacing, ensuring that all contributors to a project follow the same guidelines. By specifying these rules in a `.editorconfig` file, you can enforce a uniform coding style, making the codebase more readable and maintainable.

In this file you can specify the severity level for each analyzer rule, either standard provided by .NET itself or the from the Nuget packages. You should put this file near your "\*.sln" file.

![Screenshot of a file explorer showing .editorconfig and Directory.Build.props files highlighted, indicating their common placement near the solution file.](https://antondevtips.com/media/code_screenshots/architecture/code-quality/code_quality_1.png)

Here is what this file looks like:

```
root= true

[*.cs]
trim_trailing_whitespace= true
insert_final_newline= true
end_of_line = lf

[*]
indent_style = tab
indent_size = 4

[*.cshtml]
indent_style = tab
indent_size = 4
end_of_line = lf

[*.{fs,fsx,yml}]
indent_style = space
indent_size = 4
end_of_line = lf

[*.{md,markdown,json,js,csproj,fsproj,targets,targets,props}]
indent_style = space
indent_size = 2
end_of_line = lf

# Dotnet code style settings:
[*.{cs,vb}]

# langugage conventions https://docs.microsoft.com/en-us/visualstudio/ide/editorconfig-code-style-settings-reference#language-conventions

# Organize usings
dotnet_separate_import_directive_groups = false
dotnet_sort_system_directives_first = true

# this. and Me. preferences
dotnet_style_qualification_for_event = false:error
dotnet_style_qualification_for_field = false:error
dotnet_style_qualification_for_method = false:error
dotnet_style_qualification_for_property = false:error

# Language keywords vs BCL types preferences
dotnet_style_predefined_type_for_locals_parameters_members = true:error
dotnet_style_predefined_type_for_member_access = true:error
...
```

You can find an example of `.editorconfig` and `Directory.Build.props` files at the end of the blog post.

You can enforce many rules in `.editorconfig`, for example, you can add the following setting:

```
csharp_prefer_braces = true:error
```

That will make sure that all your statements have braces, otherwise a compilation error is raised:

![Screenshot of C# code snippet showing a compiler error due to missing braces for an 'if' statement, illustrating the csharp_prefer_braces rule enforcement.](https://antondevtips.com/media/code_screenshots/architecture/code-quality/code_quality_2.png)

Make sure to review my `.editorconfig`, this is one I use in my projects. You can turn on or off some of the analyzer rules, depending on your code styles and needs.

## Using IDE Plugins To Improve Code Quality

You can use 3rd party plugins for your IDE to improve code quality.

I suggest using the following plugins: **Visual Studio:**

*   Code Metrics
*   Spell Checker

**Jetbrains Rider:**

*   Cyclomatic Complexity
*   Cognitive Complexity
*   Code Metrics
*   Grazie and Grazie Pro for spell and grammar checking

I personally use Rider and here's how the Complexity plugins work:

![Screenshot from JetBrains Rider showing code complexity metrics for two C# methods, with "Refresh" method highlighted as "very complex (180%)", indicating a need for refactoring.](https://antondevtips.com/media/code_screenshots/architecture/code-quality/code_quality_3.png)

In this code `Refresh` method definitely needs refactoring as it is too complex.

## Summary

By using the tools and practices from this blog post, you can significantly improve the quality of your .NET projects, leading to more maintainable, reliable, and efficient software. Investing in code quality is not just about making your codebase look nice and pretty; it's about building software that stands the test of time.

If you need to further improve the quality of your code, I suggest using 3rd party software for code analysis, for example:

*   [Qodana](https://www.jetbrains.com/qodana/)
*   [SonarQube](https://www.sonarsource.com/products/sonarqube/)
*   [Codacy](https://www.codacy.com/)

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/best-practices-for-increasing-code-quality-in-dotnet-projects)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbest-practices-for-increasing-code-quality-in-dotnet-projects&title=Best%20Practices%20for%20Increasing%20Code%20Quality%20in%20.NET%20Projects)[X](https://twitter.com/intent/tweet?text=Best%20Practices%20for%20Increasing%20Code%20Quality%20in%20.NET%20Projects&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbest-practices-for-increasing-code-quality-in-dotnet-projects)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbest-practices-for-increasing-code-quality-in-dotnet-projects)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.