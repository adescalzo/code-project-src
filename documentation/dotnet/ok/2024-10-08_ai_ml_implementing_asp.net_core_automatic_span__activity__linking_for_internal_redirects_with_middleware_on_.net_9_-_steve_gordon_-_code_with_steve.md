```yaml
---
title: Implementing ASP.NET Core Automatic Span (Activity) Linking for Internal Redirects with Middleware on .NET 9 - Steve Gordon - Code with Steve
source: https://www.stevejgordon.co.uk/implementing-aspnetcore-span-linking-for-redirects-with-middleware?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=using-windows-error-reporting-in-net&_bhlid=abff0569ed3f9844e5c1b15332e0e5868c70949e
date_published: 2024-10-08T07:20:31.000Z
date_captured: 2025-08-08T15:49:01.681Z
domain: www.stevejgordon.co.uk
author: Steve Gordon
category: ai_ml
technologies: [.NET 9, ASP.NET Core, OpenTelemetry, System.Diagnostics.DiagnosticSource, Microsoft.AspNetCore.Http.Extensions, Microsoft.AspNetCore.Http.Features, Microsoft.AspNetCore.Mvc.ViewFeatures, System.Collections.Frozen, ITempDataDictionaryFactory, Elastic APM agent]
programming_languages: [C#]
tags: [aspnet-core, opentelemetry, tracing, observability, middleware, dotnet, http-redirects, span-linking, web-development, distributed-tracing]
key_concepts: [span-linking, distributed-tracing, middleware-pattern, tempdata, activity-api, http-redirects, causality, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  This article details the implementation of automatic span (Activity) linking for internal HTTP redirects within ASP.NET Core applications, specifically targeting .NET 9. The author addresses the challenge of maintaining trace causality when browsers follow redirects, which typically breaks standard trace context propagation. The proposed solution involves a custom middleware that utilizes ASP.NET Core's `TempData` mechanism to securely store the originating request's activity ID, timestamp, and target URL. On the subsequent redirected request, the middleware retrieves this information and uses the new `Activity.AddLink` API in .NET 9 to establish a link between the current span and the parent span, thereby enriching the tracing data and improving observability for complex user flows like OAuth.
---
```

# Implementing ASP.NET Core Automatic Span (Activity) Linking for Internal Redirects with Middleware on .NET 9 - Steve Gordon - Code with Steve

![Blog title image for Implementing ASP.NET Core Automatic Span (Activity) Linking for Internal Redirects with Middleware on .NET 9](https://www.stevejgordon.co.uk/wp-content/uploads/2024/10/Implementing-ASP.NET-Core-Automatic-Span-Activity-Linking-for-Internal-Redirects-Middleware-on-.NET-9-750x410.png)
*Image description: A blog post title image with a teal to green gradient background. White text reads ".NET" at the top, followed by "IMPLEMENTING ASP.NET CORE AUTO SPAN (ACTIVITY) LINKING FOR INTERNAL REDIRECTS ON .NET 9". At the bottom, "www.stevejgordon.co.uk | @stevejgordon" is displayed.*

# Implementing ASP.NET Core Automatic Span (Activity) Linking for Internal Redirects with Middleware on .NET 9

[14th March 2025](https://www.stevejgordon.co.uk/implementing-aspnetcore-span-linking-for-redirects-with-middleware) [Steve Gordon](https://www.stevejgordon.co.uk/author/stevejgordon) [.NET 9](https://www.stevejgordon.co.uk/category/net-9), [ASP.NET Core](https://www.stevejgordon.co.uk/category/asp-net-core), [OpenTelemetry](https://www.stevejgordon.co.uk/category/opentelemetry)

Today, I’ll continue a current theme for my content based on my experiences implementing OpenTelemetry instrumentation in practice for .NET applications. In this post, I want to focus on a minor enhancement I recently added to a project that enables span links between request traces on ASP.NET Core during internal redirects.

> NOTE: This code relies on a new API added in .NET 9, which allows us to add links to an existing Activity after creation. Therefore, it will not work on applications built on .NET 8 and earlier without explicitly depending on `System.Diagnostics.DiagnosticSource` 9.0.0 or newer.

It’s not uncommon for us to want to redirect the user to an alternative endpoint within our application during request handling. One example includes the OAuth flow when authenticating a user via a third-party identity provider. We may receive several HTTP requests in such cases as the user flows through authentication. The first redirect may be from an endpoint requiring an authorised user to send the browser to the login page. Another may include handling the OAuth callback before redirecting the user to their initially requested destination.

In these situations, I wanted to trace the causality of requests such that when viewing a trace for one request, I could navigate to the original request that issued the redirect. When a redirect is returned that the browser follows, the headers used for trace propagation are no longer helpful. We could tackle passing the trace context through the redirect in a few ways. For example, we could include **traceparent** information on the query string or in a cookie. With ASP.NET Core, we can also leverage the `TempData` mechanism (which by default uses a cookie-based provider) to preserve data across requests, which is the approach I’ll use for this post.

> NOTE: The code shown in this post is for illustrative purposes. While I expect it to work reasonably well, it has not been extensively battle-tested. Feel free to copy and paste it, but take care to validate it before deploying it to production! There are some unhandled edge cases that could be improved in a final implementation.

Let’s begin with the initial outline of the middleware. We’ll build this up in stages so I can explain each code block as we add it.

```csharp
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Frozen;
using System.Diagnostics;

namespace MyApp.Middleware;

public class RedirectActivityLinkingMiddleware(
    RequestDelegate next,
    ITempDataDictionaryFactory tempDataDictionaryFactory)
{
    private readonly RequestDelegate _next = next;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory = tempDataDictionaryFactory;
    private const string RedirectParentActivityId = nameof(RedirectParentActivityId);
    private const string RedirectTimestamp = nameof(RedirectTimestamp);
    private const string RedirectTarget = nameof(RedirectTarget);
    private static readonly FrozenSet<int> RedirectStatusCodes = FrozenSet.ToFrozenSet([301, 302, 303, 307, 308]);

    public async Task Invoke(HttpContext context)
    {
        var tempData = _tempDataDictionaryFactory.GetTempData(context);
        // more code to follow…
        await _next(context);
    }
}
```

This implementation relies on the availability of an `ITempDataDictionaryFactory`, which should be available from the service provider for applications created from the ASP.NET Core templates and is injected into the constructor.

The preceding code sets up some string constants and a `FrozenSet` initialised with the response status codes where our activity propagation logic should run.

In the commented section, we’ll add some logic to store the current activity ID and retrieve it on subsequent requests.

```csharp
context.Response.OnStarting(static state =>
{
    var context = (HttpContext)state;
    var activity = context.Features.Get<IHttpActivityFeature>()?.Activity;
    if (activity is null || !activity.Recorded)
        return Task.CompletedTask;
    if (RedirectStatusCodes.Contains(context.Response.StatusCode))
    {
        if (context.Response.Headers.Location.Count != 1)
            return Task.CompletedTask;
        var location = context.Response.Headers.Location[0];
        // This is a basic check to apply this to only relative URLs.
        // This should be sufficient for our use cases.
        if (string.IsNullOrEmpty(location) || location[0] != '/')
            return Task.CompletedTask;
        var factory = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
        var tempData = factory.GetTempData(context);
        // This information is stored in temp data because it is then encrypted and therefore can't be spoofed.
        tempData[RedirectParentActivityId] = activity.Id;
        // Sadly the ParentActivityId can't handle long values so we have to store it as a string
        tempData[RedirectTimestamp] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        tempData[RedirectTarget] = location;
        tempData.Save();
    }
    return Task.CompletedTask;
}, context);
```

This next important block of code registers a delegate for `Response.OnStarting`. Our logic needs to know the status code and headers of the response to function, but it also needs to store temp data. By default, the cookie-based TempData provider is used, so we need to be able to add a cookie to the outgoing response.

The preceding code is responsible for accessing the current trace information and, when an internal redirect occurs, adding that to the temp data to apply span linking on the subsequent request. This code uses an overload that accepts an object, providing the state used within the lambda. This avoids creating a closure and is, therefore, slightly more performant. As such, we can apply the static keyword to ensure we don’t capture any variables outside the lambda code’s scope. In this case, the state we pass in the `HttpContext` for the current request.

For this code to work, we need to access the current `Activity` (think span in OpenTelemetry terminology) for the request that will issue the redirect. We can retrieve this via the `IHttpActivityFeature` added to the `HttpContext` for the request. The activity may be null if there is no observer for the ASP.NET Core instrumentation code, so we must handle that scenario.

We also need to access the status code set on the response. We’ll check if this code matches any potential HTTP status codes for a redirect. We don’t want to set the `TempData` data for regular non-redirect responses. For efficiency, this middleware uses a static `FrozenSet` of redirect HTTP codes. We first validate that the response matches one of these codes before storing our temp data.

A second check we make is to avoid setting `TempData` for redirects outside our application. We only want to attempt to set this data when we know the user is being redirected to a relative local path on our site. To achieve this, the code checks to see if the URL from the “Location” header on the response starts with a forward slash.

We’re now ready to store data in the `ITempDataDictionary`, so we resolve it from `RequestServices` in this block. We could also have considered passing this as state, perhaps using a `ValueTuple`, although that would have incurred boxing, which we avoid in this code. I’ve not profiled this code to determine which approach has the lowest overhead.

We call `GetTempData` on the `ITempDataDictionaryFactory` to get an instance of `ITempDataDictionary`. The implementation is reasonably efficient and will return the same dictionary that got created when we called `GetTempData` outside of this local function. We add three items to the dictionary. The first is the ID of the `Activity` for the current request, equivalent to the value used for a standard **traceparent** header in distributed trace propagation. The second thing we add is the millisecond epoch timestamp when adding the temp data. This will be useful later to avoid us incorrectly attributing links to future requests where something strange happens and there’s a long delay. Unfortunately, we can’t store a `long` value in the temp data, so we must create a string representation instead. Finally, we add the target of the redirect, which will be used to help us validate that we only link under the expected circumstances. This somewhat mitigates an edge case where a user may browse the application from two tabs, and we may accidentally associate an incorrect link. After adding these three items, we save the temp data.

We’re left with one last chunk of code that will handle the span linking for requests.

```csharp
var activity = Activity.Current;
if (activity?.IsAllDataRequested == true
    && tempData.TryGetValue(RedirectParentActivityId, out var parentActivityIdObject)
    && tempData.TryGetValue(RedirectTimestamp, out var tempDataTimestampObject)
    && tempData.TryGetValue(RedirectTarget, out var redirectTargetObject)
    && parentActivityIdObject is string parentId
    && tempDataTimestampObject is string timestamp
    && redirectTargetObject is string redirectTarget
    && string.Equals(context.Request.GetEncodedPathAndQuery(), redirectTarget, StringComparison.Ordinal)
    && ActivityContext.TryParse(parentId, activity.TraceStateString, isRemote: false, out var ctx)
    && long.TryParse(timestamp, out var dateSet))
{
    var millisecondsDifference = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - dateSet;
    if (millisecondsDifference < (Debugger.IsAttached ? 60000 : 5000))
    {
        activity.AddLink(new ActivityLink(ctx));
    }
}
await _next(context);
```

This code first accesses the current `Activity` and checks its `IsAllDataRequested` property returns true, indicating the `Activity` is sampled and should be enriched. It then accesses the activity ID, timestamp, and target data we stored as part of the redirect to try and extract the activity ID, timestamp, and target data. The remaining conditions ensure we can cast and parse the objects for the values into the expected types.

If so, we first check that this request was made within 5 seconds of the redirect. This is another value you may want to tweak. I felt 5 seconds was sufficient for the redirect to hit the browser and the new page to be requested. Hopefully, it’s short enough to avoid misappropriating the link. We don’t want the data to be valid for too long in case the user never gets redirected for some reason but makes a new request to another URL or refreshes the page. Our additional safety net comes into play, too, as we check that the URL for this request matches the value we stored before the redirect. This at least ensures we only try to add links to pages we know to have resulted from the expected internal redirect. Again, there are some possible edge cases this code doesn’t handle and might need to be considered for production.

It’s important to note that

In all scenarios, we remove the two temp data elements. This shouldn’t strictly be necessary as these get removed after we read them via `TryGetValue`, but I added them to ensure they are gone as soon as possible. Feel free to skip those lines of code.

And that’s it; we now have a single middleware that ensures all requests triggered by an internal redirect can be linked to the originating request. This can be useful to track and is another way to enrich the value of tracing data that applications emit.

With the middleware defined, we can add it to the request pipeline. We’ll want to do this early on before most other middleware runs.

```csharp
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseMiddleware<RedirectActivityLinkingMiddleware>();
// Any other application-specific middleware
app.MapDefaultControllerRoute();
app.Run();
```

## Considerations

I’ve tried to be over-cautious in the above code to prevent invalid data. There are still edge cases where this could fall down, but those seem unlikely for a regular browsing session. As a result of the choices above, span links might be missed for high-latency scenarios where the redirect isn’t received within the timeframe you expect. The timeout could be increased if those occur frequently. It would even be possible to consider adding metrics to tally the setting and retrieval of these temp data values that could be used to tally whether any appear to be lost.

## Conclusion

In this post, I presented one possible way to achieve span (Activity) linking between a request resulting in an internal redirect and the request for the redirect location. It has a few minor trade-offs, and in very rare edge cases, it might result in an incorrect link, but I think it’s sufficient for most scenarios. It feels like a reasonable way to enrich spans to show the causal relationship between multiple requests.

---

Have you enjoyed this post and found it useful? If so, please consider supporting me:

[.NET 9](https://www.stevejgordon.co.uk/tag/net-9) [Activity](https://www.stevejgordon.co.uk/tag/activity) [ASP.NET Core](https://www.stevejgordon.co.uk/tag/asp-net-core) [observability](https://www.stevejgordon.co.uk/tag/observability) [tracing](https://www.stevejgordon.co.uk/tag/tracing)

#### [Steve Gordon](https://www.stevejgordon.co.uk/author/stevejgordon)

Steve Gordon is a Pluralsight author, 7x Microsoft MVP, and a .NET engineer at [Elastic](https://www.elastic.co) where he maintains the .NET APM agent and related libraries. Steve is passionate about community and all things .NET related, having worked with ASP.NET for over 21 years. Steve enjoys sharing his knowledge through his blog, in videos and by presenting talks at user groups and conferences. Steve is excited to participate in the active .NET community and founded .NET South East, a .NET Meetup group based in Brighton. He enjoys contributing to and maintaining OSS projects. You can find Steve on most social media platforms as [@stevejgordon](https://twitter.com/stevejgordon)

## Post navigation

[An Efficient Dictionary for IPAddress Tracking using .NET 9 with AlternateLookup and IAlternateEqualityComparer](https://www.stevejgordon.co.uk/efficient-dictionary-for-ipaddress-tracking-using-net-9-with-alternatelookup-and-ialternateequalitycomparer)

[A Brief Introduction to the .NET Muxer (aka dotnet.exe)](https://www.stevejgordon.co.uk/a-brief-introduction-to-the-dotnet-muxer)