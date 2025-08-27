```yaml
---
title: How to Handle Options in ASP.NET Core Better
source: https://okyrylchuk.dev/blog/how-to-handle-options-in-asp-net-core-better/
date_published: 2024-04-12T20:37:20.000Z
date_captured: 2025-08-20T18:56:48.703Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [ASP.NET Core, .NET 6, appsettings.json, IOptions, IOptionsSnapshot, IOptionsMonitor, OptionsBuilder]
programming_languages: [C#, JSON]
tags: [asp.net-core, configuration, options-pattern, dependency-injection, dotnet, web-development, settings, validation, live-updates]
key_concepts: [configuration-management, options-pattern, dependency-injection, configuration-binding, named-options, options-validation, live-configuration-updates, singleton-services, scoped-services]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores effective ways to handle configuration settings in ASP.NET Core applications, contrasting a common manual approach with the more robust Options pattern. It demonstrates how the Options pattern simplifies configuration binding and registration in the Dependency Injection container. The post details the differences and use cases for `IOptions`, `IOptionsSnapshot`, and `IOptionsMonitor`, highlighting their capabilities for live updates and named options. Additionally, it covers advanced features such as options validation using `DataAnnotations` and `OptionsBuilder`, and post-configuration modifications, presenting the Options pattern as a flexible and powerful solution.
---
```

# How to Handle Options in ASP.NET Core Better

# How to Handle Options in ASP.NET Core Better

By [Oleg Kyrylchuk](https://okyrylchuk.dev/blog/author/kyrylchuk-oleggmail-com/ "View all posts by Oleg Kyrylchuk") / April 12, 2024

Configuration settings are an integral part of applications. The default configuration file in ASP.NET Core is the **appsettings.json** file. There are several approaches to reading the settings for your application.

Let’s look at the common approach and how we can do it better.

Table Of Contents

1.  [Common Approach](#common-approach)
2.  [Options Pattern](#options-pattern)
3.  [Summary](#summary)

## Common Approach

In **appsettings.json**, we have the **ExternalServiceOptions** section.

```json
{
  "ExternalServiceOptions": {
    "Enabled": true,
    "Url": "http://localhost:3000",
    "ApiKey": "api-key"
  }
}
```

We need to create a class for this section.

```csharp
public class ExternalServiceOptions
{
    public string Url { get; set; }
    public bool Enabled { get; set; }
    public string ApiKey { get; set; }
}
```

The section reading and registration in the DI container are as follows.

```csharp
var builder = WebApplication.CreateBuilder(args);

var options = new ExternalServiceOptions();
builder.Configuration
     .GetRequiredSection(nameof(ExternalServiceOptions))
     .Bind(options);

builder.Services.AddSingleton(options);

var app = builder.Build();
```

The **GetRequiredSection** method was introduced in .NET 6. It throws an exception if a section is missing in the settings. For optional sections, you can use the **GetSection** method.

`System.InvalidOperationException: ‘Section ‘ExternalServiceOptions’ not found in configuration.’`

Now, you can inject **ExternalServiceOptions** into your endpoints or services.

As this approach looks straightforward, it still requires too many lines of code for such a simple thing. We must create an empty object, bind it with options, and register it in the DI Container.

Let’s see how we can do it better.

## Options Pattern

The Options pattern is an alternative approach to load configuration. It automatically binds options and registers in the DI container for us.

```csharp
var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
builder.Services.Configure<ExternalServiceOptions>(
    config.GetRequiredSection(nameof(ExternalServiceOptions)));

var app = builder.Build();

app.MapGet("/options", (IOptions<ExternalServiceOptions> options) =>
{
    var _ = options.Value; // <== ExternalServiceOptions object
});
```

As you can see, the Configure extension method of **IServiceCollection** does binding and registering in the DI container for us in one line.
However, it’s only an advantage of the Options Pattern.

Besides the **IOptions** interface, there is **IOptionsSnapshot**.

```csharp
app.MapGet("/options", (IOptionsSnapshot<ExternalServiceOptions> options) =>
{
    var _ = options.Value; // <== ExternalServiceOptions object
});
```

And **IOptionsMonitor**.

```csharp
app.MapGet("/options", (IOptionsMonitor<ExternalServiceOptions> options) =>
{
    var _ = options.CurrentValue; // <== ExternalServiceOptions object
});
```

You don’t need to change the options registration. Just inject the chosen interface.

**IOptions** interface doesn’t support the configuration reading after the application has started.

However, **IOptionsSnapshot** and **IOptionsMonitor** can update options while the application is running. The difference is that **IOptionsSnapshot** is registered as Scoped and can’t be used in the Singleton services, and **IOptionsMonitor** is registered as Singleton and can be used in every service.

Additionally, **IOptionsMonitor** has an **OnChange** method that allows you to subscribe for live configuration changes.

Also, both interfaces support Named options, unlike the **IOptions** interface. The named options are helpful when you have multiple configuration sections bind to the same properties.

For example, **SubscriptionPrices** has **Monthly** and **Yearly** prices with the same **Price** properties.

```json
"SubscriptionPrices": {
  "Monthly": {
    "Price": 10.0
  },
  "Yearly": {
    "Price": 100.0
  }
}
```

```csharp
builder.Services
     .Configure<SubscriptionPrices>("Monthly",
        config.GetRequiredSection("SubscriptionPrices:Monthly"))
     .Configure<SubscriptionPrices>("Yearly",
        config.GetRequiredSection("SubscriptionPrices:Yearly"));

app.MapGet("/options", (IOptionsMonitor<SubscriptionPrices> options) =>
{
    var monthlyPrice = options.Get("Monthly");
    var yearlyPrice = options.Get("Yearly");
});

public class SubscriptionPrices
{
    public decimal Price { get; set; }
}
```

The Options pattern also supports options validation. To do it, you need to use **OptionsBuilder** and change the options registration slightly.

```csharp
builder.Services
   .AddOptions()
   .Bind(config.GetRequiredSection(nameof(ExternalServiceOptions)))
   .ValidateDataAnnotations();

public class ExternalServiceOptions
{
    public string Url { get; set; }
    public bool Enabled { get; set; }

    [RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$")]
    public string ApiKey { get; set; }
}
```

The validation is done when the first use of options occurs. If you want to validate options on the application start, you can do that with the **ValidateOnStart** method.

```csharp
builder.Services
     .AddOptions<ExternalServiceOptions>()
     .Bind(config.GetRequiredSection(nameof(ExternalServiceOptions)))
     .ValidateDataAnnotations()
     .ValidateOnStart();
```

The **OptionsBuilder** is for streamlining the configuration of your options.

Also, if you need to update your options after configuration, you can do that with the **PostConfigure** method.

```csharp
builder.Services.PostConfigure<ExternalServiceOptions>(options =>
   {
       options.Enabled = true;
   });
```

The **PostConfigure** runs after all configurations.

## Summary

The Options pattern is flexible and powerful.

It makes it easy to configure your application options, allows options validation, supports Named options, and can do live options updates while your application runs.

Post navigation

[← Previous Post](https://okyrylchuk.dev/blog/getting-started-with-health-checks-in-asp-net-core/ "Getting started with Health Checks in ASP.NET Core")

[Next Post →](https://okyrylchuk.dev/blog/how-to-investigate-performance-counters-in-dotnet/ "How to Investigate Performance Counters in .NET")

## Related Posts

### [Records in C#](https://okyrylchuk.dev/blog/records-in-csharp/)

.NET, C# / February 2, 2024

### [Pattern Matching in C#](https://okyrylchuk.dev/blog/pattern-matching-in-csharp/)

.NET, C# / February 9, 2024

![Author's avatar](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)