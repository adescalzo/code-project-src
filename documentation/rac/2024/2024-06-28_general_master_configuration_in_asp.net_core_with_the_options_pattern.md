```yaml
---
title: Master Configuration in ASP.NET Core With The Options Pattern
source: https://antondevtips.com/blog/master-configuration-in-asp-net-core-with-the-options-pattern
date_published: 2024-06-28T11:00:22.044Z
date_captured: 2025-08-06T17:21:17.667Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core, .NET, Entity Framework Core, FluentValidation, Microsoft.Extensions.Options]
programming_languages: [C#, JSON, SQL]
tags: [configuration, aspnet-core, options-pattern, dependency-injection, validation, type-safety, appsettings, background-services, dotnet, best-practices]
key_concepts: [Options Pattern, IOptions, IOptionsSnapshot, IOptionsMonitor, dependency-injection, configuration-management, background-services, data-annotations, custom-validation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to mastering configuration in ASP.NET Core using the Options Pattern. It explains how to manage settings in a type-safe manner, detailing the distinct behaviors and use cases of `IOptions`, `IOptionsSnapshot`, and `IOptionsMonitor`. The post also covers robust validation techniques, including integrating FluentValidation for custom validation rules. Furthermore, it demonstrates how to load configuration from various sources beyond standard `appsettings.json` files, emphasizing the pattern's benefits for maintainability, testability, and separation of concerns.]
---
```

# Master Configuration in ASP.NET Core With The Options Pattern

![A dark-themed banner image with "MASTER CONFIGURATION ASP.NET CORE THE OPTIONS PATTERN" in large white text and a white "</>" icon with "dev tips" below it.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_aspnet_options_pattern.png&w=3840&q=100)

# Master Configuration in ASP.NET Core With The Options Pattern

Jun 28, 2024

[Download source code](/source-code/master-configuration-in-asp-net-core-with-the-options-pattern)

6 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

**Options Pattern** in ASP.NET Core provides a robust way to manage configurations in a type-safe manner. This blog post explores the **Options Pattern**, its benefits, and how to implement it in your ASP.NET Core applications.

[](#how-to-manage-configuration-in-aspnet-core-apps)

## How To Manage Configuration in ASP.NET Core Apps?

Every ASP.NET application needs to manage configuration.

Let's explore how to manage `BlogPostConfiguration` from appsettings.json in ASP.NET Core app:

```json
{
  "BlogPostConfiguration": {
    "ScheduleInterval": 10,
    "PublishCount": 5
  }
}
```

The naive approach for managing configuration is using a custom configuration class registered as Singleton in the DI container:

```csharp
public record BlogPostConfiguration
{
    public int ScheduleInterval { get; init; }

    public int PublishCount { get; init; }
}

var configuration = new BlogPostConfiguration();
builder.Configuration.Bind("BlogPostConfiguration", configuration);

builder.Services.AddSingleton(configuration);
```

Let's implement a `BackgroundService` service that will use this configuration to trigger a blog post publishment job every X seconds based on the configuration. This job should get a configured count of blogs per each iteration. A simplified implementation will be as follows:

```csharp
public class BlogBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BlogPostConfiguration _configuration;
    private readonly ILogger<BlogBackgroundService> _logger;

    public BlogBackgroundService(
        IServiceScopeFactory scopeFactory,
        BlogPostConfiguration configuration,
        ILogger<BlogBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Trigger blog publishment background job");

            using var scope = _scopeFactory.CreateScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var blogs = await dbContext.BlogPosts
                .Take(_configuration.PublishCount)
                .ToListAsync(cancellationToken: stoppingToken);

            _logger.LogInformation("Publish {BlogsCount} blogs: {@Blogs}",
                blogs.Count, blogs.Select(x => x.Title));

            var delay = TimeSpan.FromSeconds(_configuration.ScheduleInterval);
            await Task.Delay(delay, stoppingToken);
        }
    }
}
```

Here, we are injecting `BlogPostConfiguration` configuration class directly into the Job's constructor and using it in the `ExecuteAsync` method.

At first glance, this approach might seem okay, but it has several drawbacks:

1.  Configuration is built manually, it doesn't have any validation
2.  Configuration is registered as singleton, it can't be changed without restarting an application
3.  Configuration is tightly coupled with the service logic. This approach reduces the flexibility and maintainability of the code
4.  Testing can be more cumbersome since the configuration is tightly bound to the services. Mocking the configuration for unit tests requires more setup and can be error-prone.

Another approach will be injecting `IConfiguration` into the Job's constructor and calling `GetSection("").GetValue<T>()` method each time we need to read configuration. This method is much worse as it creates even more coupling of configuration with the service logic.

The much better approach is to use the **Options Pattern**.

[](#the-basics-of-options-pattern-in-aspnet-core)

## The Basics Of Options Pattern in ASP.NET Core

The **Options Pattern** is a convention in ASP.NET Core that allows developers to map configuration settings to strongly-typed classes.

This pattern has the following benefits:

1.  **Type safety:** configuration values are mapped to strongly typed objects, reducing errors due to incorrect configurations
2.  **Validation:** supports validation of configuration values
3.  **Separation of concerns:** configuration logic is separated from application logic, making the codebase cleaner and easier to maintain.
4.  **Ease of Testing:** configuration can be easily mocked during testing, improving testability.

There are three ways to get configuration in ASP.NET core with the **Options Pattern**: `IOptions`, `IOptionsSnapshot` and `IOptionsMonitor`.

[](#ioptions)

### IOptions

`IOptions<T>` is a singleton service that retrieves configuration values once at application startup and does not change during the application's lifetime. It is best used when configuration values do not need to change once the application is running. IOptions is the most performant option of the three.

[](#ioptionssnapshot)

### IOptionsSnapshot

`IOptionsSnapshot<T>` is a scoped service that retrieves configuration values each time they are accessed within the same request. It is useful for handling configuration changes without restarting the application. It has a performance cost as it provides a new instance of the options class for each request.

[](#ioptionsmonitor)

### IOptionsMonitor

`IOptionsMonitor<T>` is a singleton service that provides real-time updates to configuration values. It allows subscribing to change notifications and provides the current value of the options at any point in time. It is ideal for scenarios where configuration values need to change dynamically without restarting the application.

These classes behave differently. Let's have a detailed look at each of these options.

[](#how-to-use-ioptions-in-aspnet-core)

## How to Use IOptions in ASP.NET Core

The registration of configuration in DI for all three option classes is the same.

Let's rewrite `BlogPostConfiguration` using Options Pattern. First, we need to update the configuration registration to use `AddOptions`:

```csharp
builder.Services.AddOptions<BlogPostConfiguration>()
    .Bind(builder.Configuration.GetSection(nameof(BlogPostConfiguration)));
```

Now we can inject this configuration into the Background Service using `IOptions` interface:

```csharp
public BlogBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<BlogPostConfiguration> options,
    ILogger<BlogBackgroundService> logger)
{
    _scopeFactory = scopeFactory;
    _options = options;
    _logger = logger;
}

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        // ...
        var blogs = await dbContext.BlogPosts
            .Take(_options.Value.PublishCount)
            .ToListAsync(cancellationToken: stoppingToken);
    }
}
```

To get a configuration value you need to use `_options.Value`.

[](#how-to-use-ioptionssnapshot-in-aspnet-core)

## How to Use IOptionsSnapshot in ASP.NET Core

To best illustrate the difference between `IOptions` and `IOptionsSnapshot` let's create two minimal API endpoints that return configuration using these classes:

```csharp
app.MapGet("/api/configuration-singleton", (IOptions<BlogPostConfiguration> options) =>
{
    var configuration = options.Value;
    return Results.Ok(configuration);
});

app.MapGet("/api/configuration-snapshot", (IOptionsSnapshot<BlogPostConfiguration> options) =>
{
    var configuration = options.Value;
    return Results.Ok(configuration);
});
```

Each time you call "configuration-singleton" endpoint it will always return the same configuration.

But if you update your appsettings.json file and save it, the next call to "configuration-snapshot" endpoint will render a different result:

![A screenshot of console/log output showing HTTP GET requests to `/api/configuration-snapshot`. The first request shows `scheduleInterval:10, publishCount:5` in the response body, while a subsequent request shows `scheduleInterval:5, publishCount:2`, demonstrating configuration changes being picked up by `IOptionsSnapshot`.](https://antondevtips.com/media/code_screenshots/aspnetcore/options-pattern/img_aspnet_options_pattern_1.png)

[](#how-to-use-ioptionsmonitor-in-aspnet-core)

## How to Use IOptionsMonitor in ASP.NET Core

To fully understand how `IOptionsMonitor` works, let's try to change `IOptions` to `IOptionsMonitor` in our background service:

```csharp
public class BlogBackgroundServiceWithIOptionsMonitor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<BlogPostConfiguration> _optionsMonitor;
    private readonly ILogger<BlogBackgroundServiceWithIOptionsMonitor> _logger;

    public BlogBackgroundServiceWithIOptionsMonitor(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<BlogPostConfiguration> optionsMonitor,
        ILogger<BlogBackgroundServiceWithIOptionsMonitor> logger)
    {
        _scopeFactory = scopeFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _optionsMonitor.OnChange(newConfig =>
        {
            _logger.LogInformation("Configuration changed. ScheduleInterval - {ScheduleInterval}, PublishCount - {PublishCount}",
                newConfig.ScheduleInterval, newConfig.PublishCount);
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            // ...

            var blogs = await dbContext.BlogPosts
                .Take(_optionsMonitor.CurrentValue.PublishCount)
                .ToListAsync(cancellationToken: stoppingToken);

            _logger.LogInformation("Publish {BlogsCount} blogs: {@Blogs}",
                blogs.Count, blogs.Select(x => x.Title));

            var delay = TimeSpan.FromSeconds(_optionsMonitor.CurrentValue.ScheduleInterval);
            await Task.Delay(delay, stoppingToken);
        }
    }
}
```

Here are few important points worth mentioning. Despite `IOptionsMonitor` being a Singleton class it always returns an up-to-date configuration value using `_optionsMonitor.CurrentValue` property.

This class has a `OnChange` method with a delegate that fires when an appsettings.json is saved. This method can be called twice:

```
info: OptionsPattern.HostedServices.BlogBackgroundServiceWithIOptionsMonitor[0]
      Configuration changed. ScheduleInterval - 2, PublishCount - 2
info: OptionsPattern.HostedServices.BlogBackgroundServiceWithIOptionsMonitor[0]
      Configuration changed. ScheduleInterval - 2, PublishCount - 2
```

This can happen depending on the file system, that can trigger `IOptionsMonitor` to update the configuration on file saved and file closed events from the operating system.

[](#validation-in-options-pattern)

## Validation in Options Pattern

As we mentioned before, Options Pattern in ASP.NET Core supports validation. It supports 2 types of validation: data annotations and custom validation.

Data annotations validation is based on attribute validation which I am not a fan of. This type of validation breaks a single responsibility principle by polluting the configuration classes with a validation logic.

I prefer using custom validation. Let's have a look how to add validation for `BlogPostConfiguration`.

First, let's extend the configuration registration in DI container and add `ValidateDataAnnotations` and `ValidateOnStart` method calls:

```csharp
builder.Services.AddOptions<BlogPostConfiguration>()
    .Bind(builder.Configuration.GetSection(nameof(BlogPostConfiguration)))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

Regardless of chosen validation type, we need to call the `ValidateDataAnnotations` method.

`ValidateOnStart` method triggers validation on ASP.NET Core app startup and when the configuration is updated in appsettings.json. This is particularly useful to catch errors early before the application is started.

For validation, we are going to use **FluentValidation** library:

```csharp
public class BlogPostConfigurationValidator : AbstractValidator<BlogPostConfiguration>
{
    public BlogPostConfigurationValidator()
    {
        RuleFor(x => x.ScheduleInterval).GreaterThan(0);
        RuleFor(x => x.PublishCount).GreaterThan(0);
    }
}
```

Now let's create our custom options validator by implementing the `IValidateOptions<T>` interface:

```csharp
public class BlogPostConfigurationValidationOptions : IValidateOptions<BlogPostConfiguration>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BlogPostConfigurationValidationOptions(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public ValidateOptionsResult Validate(string? name, BlogPostConfiguration options)
    {
        using var scope = _scopeFactory.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<BlogPostConfiguration>>();

        var result = validator.Validate(options);
        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        var errors = result.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}").ToList();
        return ValidateOptionsResult.Fail(errors);
    }
}
```

`BlogPostConfigurationValidationOptions` must be registered a singleton, that's why we resolve scoped `IValidator<BlogPostConfiguration>` from the service scope factory.

Finally, you need to register validator and the validation options in DI:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining(typeof(BlogPostConfigurationValidator));

builder.Services.AddSingleton<IValidateOptions<BlogPostConfiguration>, BlogPostConfigurationValidationOptions>();
```

The `Validate` method is called in the following cases:

*   application startup
*   configuration was updated in appsettings.json

[](#using-options-pattern-to-manage-configuration-from-other-files)

## Using Options Pattern to Manage Configuration From Other Files

The real power of the Options Pattern in ASP.NET Core is that you can resolve configuration from any source using Options classes.

In all examples above, we were managing configuration within a standard appsettings.json. In the same way, you can manage configuration from any other JSON files.

Let's create a "custom.settings.json" file:

```json
{
  "BlogLimitsConfiguration": {
    "MaxBlogsPerDay": 3
  }
}
```

Then we can add this file to the `Configuration` object and add options for its configuration:

```csharp
builder.Configuration.AddJsonFile("custom.settings.json", true, true);

builder.Services.AddOptions<BlogLimitsConfiguration>()
    .Bind(builder.Configuration.GetSection(nameof(BlogLimitsConfiguration)));
```

Now we can use `BlogLimitsConfiguration` with any of the Options classes, for example:

```csharp
app.MapGet("/api/configuration-custom", (IOptions<BlogLimitsConfiguration> options) =>
{
    var configuration = options.Value;
    return Results.Ok(configuration);
});
```

You can even create custom Options configuration providers that read configuration from the database, redis or any other store. There are many ready-made configuration providers from external Nuget packages, for example, to access configuration from Azure, AWS using the Options classes.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/master-configuration-in-asp-net-core-with-the-options-pattern)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fmaster-configuration-in-asp-net-core-with-the-options-pattern&title=Master%20Configuration%20in%20ASP.NET%20Core%20With%20The%20Options%20Pattern)[X](https://twitter.com/intent/tweet?text=Master%20Configuration%20in%20ASP.NET%20Core%20With%20The%20Options%20Pattern&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fmaster-configuration-in-asp-net-core-with-the-options-pattern)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fmaster-configuration-in-asp-net-core-with-the-options-pattern)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.