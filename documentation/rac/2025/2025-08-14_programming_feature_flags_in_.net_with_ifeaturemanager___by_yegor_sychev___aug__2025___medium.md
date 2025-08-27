```yaml
---
title: "Feature flags in .NET with IFeatureManager | by Yegor Sychev | Aug, 2025 | Medium"
source: https://medium.com/@yegor-sychev/feature-flags-in-net-with-ifeaturemanager-c08391f9fdeb
date_published: 2025-08-14T10:51:22.229Z
date_captured: 2025-08-25T10:14:08.269Z
domain: medium.com
author: Yegor Sychev
category: programming
technologies: [.NET, Microsoft.FeatureManagement, IFeatureManager, Microsoft.FeatureManagement.FeatureFilters, TimeWindowFilter, ASP.NET Core, Swagger, SwaggerUI]
programming_languages: [C#, JSON]
tags: [feature-flags, dotnet, configuration, web-api, a/b-testing, dependency-injection, release-management, software-development, api, runtime-control]
key_concepts: [feature-flags, dependency-injection, configuration-management, a/b-testing, conditional-logic, runtime-configuration, feature-filters, continuous-delivery]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces feature flags in .NET using the `IFeatureManager` interface from the `Microsoft.FeatureManagement` package. It explains how feature flags enable dynamic control over application logic at runtime, useful for scenarios like A/B testing, temporary promotions, or hotfixes. The content provides practical code examples demonstrating how to register feature management in Dependency Injection, configure flags in `appsettings.json` (including advanced filters like `TimeWindowFilter`), and check a flag's state within an ASP.NET Core controller. The author highlights benefits such as reducing release risks and gaining more control over production functionality without redeployment.
---
```

# Feature flags in .NET with IFeatureManager | by Yegor Sychev | Aug, 2025 | Medium

# Feature flags in .NET with IFeatureManager

![Conceptual diagram showing a .NET application using a feature flag. A toggle switch labeled "Feature Flag: New UI" is set to "ON", overlaid on C# code that conditionally executes "New UI code" or "Old UI code" based on `if (FeatureFlag.IsEnabled("NewUI"))`. A .NET logo is also visible.](https://miro.medium.com/v2/resize:fit:700/1*4cVgvMrqJ5eQlNUVIWrAUQ.png)

`IFeatureManager` from the `Microsoft.FeatureManagement` package manages feature flags in .NET.

It lets you choose the right branch of logic while the application is running.

Where it’s useful:

*   temporary enablement or quick disabling of a feature — for example, during a new release or a hotfix
*   disabling problematic functionality during failures
*   A/B testing
*   temporary scenarios, such as promotions

How it works:

Register Feature Management in DI, Add filters if needed. For example, `TimeWindow` activates a flag only within a specific time range.

```csharp
using Microsoft.FeatureManagement;  
using Microsoft.FeatureManagement.FeatureFilters;  
  
var builder = WebApplication.CreateBuilder(args);  
  
builder.Services.AddFeatureManagement()  
// If you need a time window filter, add the following line  
    .AddFeatureFilter<TimeWindowFilter>();  
  
builder.Services.AddControllers();  
builder.Services.AddEndpointsApiExplorer();  
builder.Services.AddSwaggerGen(c =>  
{  
    c.SwaggerDoc("v1", new() { Title = "Feature Flags Demo API", Version = "v1" });  
});  
  
var app = builder.Build();  
  
if (app.Environment.IsDevelopment())  
{  
    app.UseSwagger();  
    app.UseSwaggerUI();  
}  
  
app.MapControllers();  
  
app.Run();
```

Set values in `appsettings.json`:

```json
{  
  "Logging": {  
    "LogLevel": {  
      "Default": "Information",  
      "Microsoft.AspNetCore": "Warning"  
    }  
  },  
  "AllowedHosts": "*",  
  "FeatureManagement": {  
    "NewDashboard": true,  
    "HolidayPromo": {  
      "EnabledFor": [  
        {  
          "Name": "TimeWindow",  
          "Parameters": {  
            "Start": "2024-12-01T00:00:00Z",  
            "End": "2025-12-31T23:59:59Z"  
          }  
        }  
      ]  
    }  
  }  
}
```

Check a flag’s state through `IFeatureManager`:

```csharp
using Microsoft.AspNetCore.Mvc;  
using Microsoft.FeatureManagement;  
  
namespace FeatureFlagsDemo.Api.Controllers;  
  
[ApiController]  
[Route("api/[controller]")]  
public class DashboardController(IFeatureManager featureManager) : ControllerBase  
{  
    [HttpGet]  
    public async Task<IActionResult> GetDashboard()  
    {  
        var useNewDashboard = await featureManager.IsEnabledAsync("NewDashboard");  
  
        if (useNewDashboard)  
        {  
            return Ok(new  
            {  
                Version = "New",  
                Data = GetNewDashboardData(),  
            });  
        }  
  
        return Ok(new  
        {  
            Version = "Legacy",  
            Data = GetLegacyDashboardData(),  
        });  
    }  
  
    [HttpGet("promo")]  
    public async Task<IActionResult> GetHolidayPromo()  
    {  
        if (!await featureManager.IsEnabledAsync("HolidayPromo"))  
        {  
            return Ok(new { Message = "No active promotions" });  
        }  
  
        return Ok(new  
        {  
            Message = "Holiday Special Offer!",  
            Discount = "50% OFF",  
            ValidUntil = "December 31, 2025",  
            PromoCode = "HOLIDAY2025"  
        });  
    }  
  
    private static object GetNewDashboardData() => new  
    {  
        Charts = new[]  
        {  
            new { Name = "Revenue Chart", Type = "Line", Interactive = true },  
            new { Name = "User Activity", Type = "Heatmap", Interactive = true },  
            new { Name = "Performance Metrics", Type = "Gauge", Interactive = true }  
        },  
        Layout = "Modern Grid",  
        Theme = "Dark Mode Available",  
        Features = new[] { "Real-time Updates", "Custom Widgets", "Export Options" }  
    };  
  
    private static object GetLegacyDashboardData() => new  
    {  
        Charts = new[]  
        {  
            new { Name = "Basic Revenue", Type = "Bar", Interactive = false },  
            new { Name = "Simple Stats", Type = "Table", Interactive = false }  
        },  
        Layout = "Classic List",  
        Theme = "Light Only",  
        Features = new[] { "Static Reports" }  
    };  
}
```

As a result, you can change application behavior via configuration, without redeploying.

This approach saves time, reduces the risk of release errors, and gives the team more control over functionality directly in production.