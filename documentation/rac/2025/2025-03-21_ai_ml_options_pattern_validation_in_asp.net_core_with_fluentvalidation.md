```yaml
---
title: Options Pattern Validation in ASP.NET Core With FluentValidation
source: https://www.milanjovanovic.tech/blog/options-pattern-validation-in-aspnetcore-with-fluentvalidation?utm_source=LinkedIn&utm_medium=social&utm_campaign=04.08.2025
date_published: 2025-03-22T00:00:00.000Z
date_captured: 2025-08-11T17:40:28.973Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [ASP.NET Core, FluentValidation, .NET, Data Annotations, Docker, MassTransit, RabbitMQ, Azure Service Bus, KendoReact, React]
programming_languages: [C#, PowerShell]
tags: [aspnet-core, validation, fluentvalidation, options-pattern, configuration, dependency-injection, startup, dotnet, best-practices]
key_concepts: [options-pattern, configuration-validation, dependency-injection, fluentvalidation, data-annotations, extension-methods, application-startup, microservices]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details how to enhance configuration validation in ASP.NET Core by integrating the FluentValidation library with the Options Pattern. It highlights the limitations of built-in Data Annotations for complex scenarios and demonstrates building a custom `IValidateOptions<T>` implementation. The guide shows how to enforce validation at application startup using `ValidateOnStart()`, ensuring configuration integrity early. This approach provides more expressive rules, better separation of concerns, and is highly beneficial for microservice and containerized environments.]
---
```

# Options Pattern Validation in ASP.NET Core With FluentValidation

![Blog post cover image showing "OPTIONS PATTERN VALIDATION WITH FLUENTVALIDATION" on a blue and black background with geometric patterns and the "MJ Tech" logo.](/blog-covers/mnw_134.png?imwidth=3840)

# Options Pattern Validation in ASP.NET Core With FluentValidation

If you've worked with the [**Options Pattern**](how-to-use-the-options-pattern-in-asp-net-core-7) in ASP.NET Core, you're likely familiar with the built-in validation using [Data Annotations](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-9.0#validation-attributes). While functional, Data Annotations can be limiting for complex validation scenarios.

The **Options Pattern** lets you use classes to obtain strongly typed configuration objects at runtime.

The problem? You can't be certain that the configuration is valid until you try to use it.

So why not validate it at application startup?

In this article, we'll explore how to integrate the more powerful [FluentValidation](https://docs.fluentvalidation.net/en/latest/) library with ASP.NET Core's [Options Pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options), to build a robust validation solution that executes at application startup.

## Why FluentValidation Over Data Annotations?

Data Annotations work well for simple validations, but FluentValidation offers several advantages:

*   More expressive and flexible validation rules
*   Better support for complex conditional validations
*   Cleaner separation of concerns (validation logic separate from model)
*   Easier testing of validation rules
*   Better support for custom validation logic
*   Allows for injecting dependencies into validators

## Understanding the Options Pattern Lifecycle

Before diving deep into validation, it's important to understand the lifecycle of options in ASP.NET Core:

*   Options are registered with the DI container
*   Configuration values are bound to options classes
*   Validation occurs (if configured)
*   Options are resolved when requested via `IOptions<T>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>`

The `ValidateOnStart()` method forces validation to occur during application startup rather than when options are first resolved.

## Common Configuration Failures Without Validation

Without validation, configuration issues can manifest in several ways:

*   **Silent failures**: An incorrectly configured option may result in default values being used without warning
*   **Runtime exceptions**: Configuration issues may only surface when the application tries to use invalid values
*   **Cascading failures**: One misconfigured component can cause failures in dependent systems

By validating at startup, you create a fast feedback loop that prevents these issues.

## Setting Up the Foundation

First, let's add the FluentValidation package to our project:

```powershell
Install-Package FluentValidation # base package
Install-Package FluentValidation.DependencyInjectionExtensions # for DI integration
```

For our example, we'll use a `GitHubSettings` class that requires validation:

```csharp
public class GitHubSettings
{
    public const string ConfigurationSection = "GitHubSettings";

    public string BaseUrl { get;init; }
    public string AccessToken { get; init; }
    public string RepositoryName { get; init; }
}
```

## Creating a FluentValidation Validator

Next, we'll create a validator for our settings class:

```csharp
public class GitHubSettingsValidator : AbstractValidator<GitHubSettings>
{
    public GitHubSettingsValidator()
    {
        RuleFor(x => x.BaseUrl).NotEmpty();

        RuleFor(x => x.BaseUrl)
            .Must(baseUrl => Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.baseUrl))
            .WithMessage($"{nameof(GitHubSettings.BaseUrl)} must be a valid URL");

        RuleFor(x => x.AccessToken)
            .NotEmpty();

        RuleFor(x => x.RepositoryName)
            .NotEmpty();
    }
}
```

## Building the FluentValidation Integration

To integrate FluentValidation with the Options Pattern, we need to create a custom `IValidateOptions<T>` implementation:

```csharp
using FluentValidation;
using Microsoft.Extensions.Options;

public class FluentValidateOptions<TOptions>
    : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _name;

    public FluentValidateOptions(IServiceProvider serviceProvider, string? name)
    {
        _serviceProvider = serviceProvider;
        _name = name;
    }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (_name is not null && _name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        ArgumentNullException.ThrowIfNull(options);

        using var scope = _serviceProvider.CreateScope();

        var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

        var result = validator.Validate(options);
        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        var type = options.GetType().Name;
        var errors = new List<string>();

        foreach (var failure in result.Errors)
        {
            errors.Add($"Validation failed for {type}.{failure.PropertyName} " +
                       $"with the error: {failure.ErrorMessage}");
        }

        return ValidateOptionsResult.Fail(errors);
    }
}
```

A few important notes about this implementation:

1.  We create a scoped service provider to properly resolve the validator (since validators are typically registered as scoped services)
2.  We handle named options through the `_name` property
3.  We build informative error messages that include the property name and error message

## How the FluentValidation Integration Works

When adding our custom FluentValidation integration, it's helpful to understand how it connects to ASP.NET Core's options system:

1.  The `IValidateOptions<T>` interface is the hook that ASP.NET Core provides for options validation
2.  Our `FluentValidateOptions<T>` class implements this interface to bridge to FluentValidation
3.  When `ValidateOnStart()` is called, ASP.NET Core resolves all `IValidateOptions<T>` implementations and runs them
4.  If validation fails, an `OptionsValidationException` is thrown, preventing the application from starting

## Creating Extension Methods for Easy Integration

Now, let's create a few extension methods to make our validation easier to use:

```csharp
public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
        this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        builder.Services.AddSingleton<IValidateOptions<TOptions>>(
            serviceProvider => new FluentValidateOptions<TOptions>(
                serviceProvider,
                builder.Name));

        return builder;
    }
}
```

This extension method allows us to call `.ValidateFluentValidation()` when configuring options, similar to the built-in `.ValidateDataAnnotations()` method.

For even more convenience, we can create another extension method to simplify the entire configuration process:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOptionsWithFluentValidation<TOptions>(
        this IServiceCollection services,
        string configurationSection)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .BindConfiguration(configurationSection)
            .ValidateFluentValidation() // Configure FluentValidation validation
            .ValidateOnStart(); // Validate options on application start

        return services;
    }
}
```

## Registering and Using the Validation

There are a few ways to use our FluentValidation integration:

### Option 1: Standard Registration with Manual Validator Registration

```csharp
// Register the validator
builder.Services.AddScoped<IValidator<GitHubSettings>, GitHubSettingsValidator>();

// Configure options with validation
builder.Services.AddOptions<GitHubSettings>()
    .BindConfiguration(GitHubSettings.ConfigurationSection)
    .ValidateFluentValidation() // Configure FluentValidation validation
    .ValidateOnStart();
```

### Option 2: Using the Convenience Extension Method

```csharp
// Register the validator
builder.Services.AddScoped<IValidator<GitHubSettings>, GitHubSettingsValidator>();

// Use the convenience extension
builder.Services.AddOptionsWithFluentValidation<GitHubSettings>(GitHubSettings.ConfigurationSection);
```

### Option 3: Automatic Validator Registration

If you have many validators and want to register them all at once, you can use FluentValidation's assembly scanning:

```csharp
// Register all validators from assembly
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Use the convenience extension
builder.Services.AddOptionsWithFluentValidation<GitHubSettings>(GitHubSettings.ConfigurationSection);
```

## What Happens at Runtime?

With `.ValidateOnStart()`, the application will throw an exception during startup if any validation rules fail. For example, if your `appsettings.json` is missing the required `AccessToken`, you'll see something like:

```
Microsoft.Extensions.Options.OptionsValidationException:
    Validation failed for GitHubSettings.AccessToken with the error: 'Access Token' must not be empty.
```

This prevents your application from even starting with invalid configuration, ensuring issues are caught as early as possible.

## Working with Different Configuration Sources

ASP.NET Core's configuration system supports multiple sources. When using the Options Pattern with FluentValidation, remember that validation works regardless of the source:

*   Environment variables
*   Azure Key Vault
*   User secrets
*   JSON files
*   In-memory configuration

This is particularly useful for containerized applications where configuration comes from environment variables or mounted secrets.

## Testing Your Validators

One benefit of using FluentValidation is that validators are easy to test:

```csharp
// Uses helper methods from FluentValidation.TestHelper
[Fact]
public void GitHubSettings_WithMissingAccessToken_ShouldHaveValidationError()
{
    // Arrange
    var validator = new GitHubSettingsValidator();
    var settings = new GitHubSettings { RepositoryName = "test-repo" };

    // Act
    TestValidationResult<GitHubSettings>? result = await validator.TestValidate(settings);

    // Assert
    result.ShouldNotHaveAnyValidation