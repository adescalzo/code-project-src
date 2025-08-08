## Summary
This article addresses the challenge of including ASP.NET Core health checks within an OpenAPI specification, a common requirement for clients generating APIs from the spec. It explains how to move beyond the default `MapHealthChecks` method by implementing a custom Minimal API endpoint that performs health checks. The post details how to return rich problem reports with failure information and how to properly annotate this custom endpoint for OpenAPI generation using `.Produces`, `.WithName`, and `.WithTags`.

---

```markdown
# Health Checks in Your OpenAPI Specs

**Source:** https://blog.wildermuth.com/2024/06/17/health-checks-in-your-open-api/
**Date Captured:** 2025-07-28T19:36:37.776Z
**Domain:** blog.wildermuth.com
**Author:** Unknown
**Category:** backend
```

---

![Health Checks in Your OpenAPI Specs](https://wilderminds.blob.core.windows.net/img/2024/06/17/cover.jpg)

*Originally published by [Shawn Wildermuth](/#contact) on June 17, 2024.*
*Tags: [.NET](/tags/.net/), [ASP.NET Core](/tags/asp.net-core/), [Minimal APIs](/tags/minimal-apis/), [OpenAPI](/tags/openapi/)*

I recently was working on a project where the client wanted the health checks to be part of the OpenAPI specification. Here’s how you can do it.

> **Announcement**: In case you’re new here, I’ve just launched my blog with a new combined website. Take a gander around and tell me what you think!

## Standard ASP.NET Core Health Checks

ASP.NET Core supports health checks out of the box. You can add them to your project by adding the `Microsoft.AspNetCore.Diagnostics.HealthChecks` NuGet package. Once you’ve added the package, you can add health checks dependencies to your project by adding them to the `ServiceCollection`:

```csharp
builder.Services.AddHealthChecks();
```

Once you’ve added the health checks, you need to map the health checks to an endpoint (usually `/health`). You do that by calling `MapHealthChecks`:

```csharp
app.MapHealthChecks("/health");
```

This works great. If you need to use the health checks, you can just call the `/health` endpoint.

## The Challenge: OpenAPI Integration

At the client, our APIs were generated via some tooling by reading the OpenAPI spec. The client wanted the health checks to be part of the OpenAPI specification so that the client could call it with the same generated code. But how to get it to work?

## Custom Health Check Endpoint for OpenAPI

The solution is to not use the `MapHealthChecks` method, but instead to build an API (in my case, Minimal APIs) to perform the health checks. Here’s how you can do it:

```csharp
builder.MapGet("/health", async (HealthCheckService healthCheck, 
  IHttpContextAccessor contextAccessor) =>
{
    var report = await healthCheck.CheckHealthAsync();
    if (report.Status == HealthStatus.Healthy)
    {
        return Results.Ok(new { Success = true });
    }
    else 
    {
        return Results.Problem("Unhealthy", statusCode: 500);
    }
});
```

This works great. One of the reasons I decided to do it this way is that instead of just a string, I wanted to return some context about the health check. This way, the client can know what is wrong with the health check.

### Returning Detailed Failure Information

> **NOTE**: Returning reasons for the failure can be a security risk. Be careful not to return any sensitive information.

I found the best way to do this is to create a problem report with some information:

```csharp
var report = await healthCheck.CheckHealthAsync();
if (report.Status == HealthStatus.Healthy)
{
  return Results.Ok(new { Success = true });
}
else
{
  var failures = report.Entries
    .Select(e => e.Value.Description)
    .ToArray();

  var details = new ProblemDetails()
  {
      Instance = contextAccessor.HttpContext.Request.GetServerUrl(),
      Status = 503,
      Title = "Healthcheck Failed",
      Type = "healthchecks",
      Detail = string.Join(Environment.NewLine, failures)
  };
  return Results.Problem(details);
}
```

By creating a problem detail, I can specify what the URL was used, the status code to use (503 in this case), and a list of the failures. The report that the `CheckHealthAsync` method returns has a dictionary of the health checks. I’m just using the description of the health check as the failure reason.

### Adding Specific Health Checks

Remember when you call `AddHealthChecks` you can add additional checks like this one for testing the DbContext connection string:

```csharp
builder.Services.AddHealthChecks()
  // From the Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore package
  .AddDbContextCheck<ShoeContext>();
```

### Enhancing OpenAPI Specification

Then you can add some additional information for the OpenAPI specification:

```csharp
builder.MapGet("/health", async (HealthCheckService healthCheck, 
  IHttpContextAccessor contextAccessor) => { /* ... */ })
            .Produces(200)
            .ProducesProblem(503)
            .WithName("HealthCheck")
            .WithTags("HealthCheck")
            .AllowAnonymous();
```

Make sense? Let me know what you think!

---

## Licensing and Author Information

[![Creative Commons License](http://i.creativecommons.org/l/by-nc-nd/3.0/88x31.png)](http://creativecommons.org/licenses/by-nc-nd/3.0/)

This work by [Shawn Wildermuth](//wildermuth.com) is licensed under a [Creative Commons Attribution-NonCommercial-NoDerivs 3.0 Unported License](http://creativecommons.org/licenses/by-nc-nd/3.0/). Based on a work at [wildermuth.com](//wildermuth.com).

[Shawn Wildermuth](https://wildermuth.com/hireme) is an Instructor, Author, Filmmaker, Microsoft MVP, and Docker Captain.

## Additional Resources

### Popular Tags

*   [databases](/tags/databases)
*   [ado](/tags/ado)
*   [recordset](/tags/recordset)
*   [datasets](/tags/datasets)
*   [xmldatadocument](/tags/xmldatadocument)
*   [dotnet](/tags/dotnet)
*   [xml](/tags/xml)
*   [xmldocument](/tags/xmldocument)

### Learn From Me

*   [Training Classes](https://wilderminds.com/training)
*   [Pluralsight Videos](https://shawnl.ink/psauthor)
*   [YouTube Channel](https://shawnl.ink/yt)
*   [Individual Coaching](https://wilderminds.com/hireme/coaching/)
*   [Code Reviews](https://wilderminds.com/hireme/codereviews/)

---

*Privacy Policy: [wildermuth.com/privacy](/privacy)*
*©2007-2025 Wilder Minds LLC & Wilder Minds BV*
```