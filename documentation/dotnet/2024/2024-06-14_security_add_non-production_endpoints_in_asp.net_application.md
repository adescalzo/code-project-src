## Summary
This article demonstrates how to implement "Non-Production Endpoints" in an ASP.NET application, which are useful for debugging, testing, and development but should not be exposed in production. It details the creation of a custom `ActionFilterAttribute` that restricts access to specific endpoints based on the application's environment. The post provides practical examples, including how to use this filter to create an endpoint for securely viewing application configuration in non-production environments.

---

# Add Non-Production Endpoints in ASP.NET Application

```markdown
**Source:** https://harshmatharu.com/blog/add-non-production-endpoints-in-aspnet-app?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=how-to-use-c-to-follow-oauth-authentication-flows
**Date Captured:** 2025-07-28T19:35:25.061Z
**Domain:** harshmatharu.com
**Author:** Unknown
**Category:** security
**Published On:** Saturday, June 15, 2024
```

---

## Table of Contents

*   [Why do we need Non-Production Endpoints?](#why-do-we-need-non-production-endpoints)
*   [Create Action Filter](#create-action-filter)
*   [Using the Action Filter](#using-the-action-filter)
*   [Viewing application configuration](#viewing-application-configuration)
*   [Conclusion](#conclusion)
*   [Related Posts](#related-posts)

Non-Production Endpoints are the endpoints that are used for debugging, testing, and development purposes. These endpoints are not exposed to the public and are only accessible in the development environment.

## Why do we need Non-Production Endpoints?

Non-Production Endpoints can be used for the following scenarios but not limited to:

*   Viewing application configuration.
*   Adding/Removing data.
*   Generating access tokens with longer validity for testing.
*   For adding messages into the message broker.

These are just few examples, you can add more endpoints based on your requirements. These endpoints will make your life easier while debugging and testing the application. Now, let's see how we can add Non-Production Endpoints in an ASP.NET application.

## Create Action Filter

First, create a simple ASP.NET Web API application.

**Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
```

Then, create the following action filter.

**NonProductionAttribute.cs**

```csharp
public class NonProductionAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IHostEnvironment environment = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
        if (environment.IsProduction())
        {
            context.Result = new NotFoundResult();
            return;
        }

        base.OnActionExecuting(context);
    }
}
```

It will check if the request is coming from the production environment or not. If the request is coming from the production environment then it will return a 404 status code. Otherwise, it will allow the request to pass through.

## Using the Action Filter

Now, let's see how we can use this action filter in our controller to create a Non-Production Endpoint.

Since the action filter is an attribute, add this on top of the controller or the action method.

**DemoController.cs**

```csharp
[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    [HttpGet]
    [NonProduction]
    public IActionResult HelloWorld()
    {
        return Ok("Hello, World!");
    }
}
```

Now, if you run the application and try to access the `GET /demo` endpoint, it will work normally because the environment is set to development.

![Hitting endpoint in development environment](https://harshmatharustorage.blob.core.windows.net/public/images/add-non-production-endpoints-in-aspnet/hit-endpoint.jpg)

Now let's change the environment to production. Open the `Properties/launchSettings.json` file and set the `ASPNETCORE_ENVIRONMENT` to `Production`. Please ensure that you set the environment variable in the profile which you are using to run the application. For example, if you are using `http` profile to run the application then set the environment variable in the `http` profile.

![Set environment variable](https://harshmatharustorage.blob.core.windows.net/public/images/add-non-production-endpoints-in-aspnet/set-env-variable.jpg)

Now, if you try to access the `GET /demo` endpoint, it will return a 404 status code.

![Endpoint not found](https://harshmatharustorage.blob.core.windows.net/public/images/add-non-production-endpoints-in-aspnet/not-found.jpg)

## Viewing application configuration

Now, let's see how we can add an endpoint to view the application configuration.

**DemoController.cs**

```csharp
[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    private readonly IConfiguration configuration;

    public DemoController(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    [HttpGet("config")]
    [NonProduction]
    public IActionResult Config()
    {
        return Ok(configuration.GetChildren().Select(c => new { c.Key, c.Value }));
    }
}
```

Now, if you run the application and try to access the `GET /demo/config` endpoint, you will be able to view the application configuration.

As this endpoint spits out all the configuration values, it will also include the secret values. You can remove or encode secret values if you want them to stay hidden.

![Application configuration](https://harshmatharustorage.blob.core.windows.net/public/images/add-non-production-endpoints-in-aspnet/app-configuration.jpg)

## Conclusion

In this post, we learned how to add Non-Production Endpoints in an ASP.NET application. These endpoints are very useful for debugging, testing, and development purposes.

## Related Posts

*   [Export Excel in WebApi](https://harshmatharu.com/blog/excel-export-in-aspnet)