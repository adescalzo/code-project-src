```yaml
---
title: "Dynamic Htmx Islands with ASP.NET Core | Khalid Abuhakmeh"
source: https://khalidabuhakmeh.com/dynamic-htmx-islands-with-aspnet-core?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=what-s-new-in-c-13&_bhlid=efd9fe2eec2bc00843447c23ee2cfbd19d05af2e
date_published: 2024-11-19T00:00:00.000Z
date_captured: 2025-08-08T16:18:13.859Z
domain: khalidabuhakmeh.com
author: Khalid Abuhakmeh
category: testing
technologies: [ASP.NET Core, Htmx, Razor, .NET]
programming_languages: [C#, HTML, JavaScript, CSS]
tags: [aspnet-core, htmx, web-development, caching, performance, frontend, backend, tag-helpers, razor-pages, server-side-rendering]
key_concepts: [server-islands, htmx, tag-helpers, response-caching, output-caching, dom-manipulation, lazy-loading, web-performance]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article demonstrates implementing "Htmx Islands" in ASP.NET Core applications, a technique for rendering dynamic content sections within largely static pages. Inspired by Astro's server islands, this approach leverages ASP.NET Core Tag Helpers to create a reusable `island` component. The `island` component uses Htmx to fetch dynamic content on demand, triggered by events like `load`, `revealed`, or `intersect`, optimizing content delivery. The post also details how to integrate ASP.NET Core's response and output caching to enhance overall page performance. This method offers flexibility, allowing dynamic content to be sourced from various backends, not solely tied to the ASP.NET Core server.]
---
```

# Dynamic Htmx Islands with ASP.NET Core | Khalid Abuhakmeh

November 19, 2024

# Dynamic Htmx Islands with ASP.NET Core

![A vibrant image with a gradient background on the left, displaying the title 'Dynamic Htmx Islands with ASP.NET Core' and 'khalidabuhakmeh.com'. The right side features an aerial view of small, lush green islands surrounded by clear turquoise water, with a faint boat wake visible.](https://res.cloudinary.com/abuhakmeh/image/fetch/c_limit,f_auto,q_auto,w_800/https://khalidabuhakmeh.com/assets/images/posts/misc/dynamic-htmx-islands-aspnet-core-taghelpers.jpg) Photo by [Denys Nevozhai](https://unsplash.com/@dnevozhai)

I’m a big fan of static site renderers, and they are still one of the missing elements that would make ASP.NET Core more compelling for users across the vast ecosystem divide. In the meantime, developers must rely on tools like Astro, Jekyll, and 11ty to build static site experiences. Recently, I read about Astro 5.0 and its [server island implementation](https://5-0-0-beta.docs.astro.build/en/guides/server-islands/). Server islands allow for mostly static pages to carve out portions of the page that will be rendered on-demand by, you guessed it, the server. This allows site authors to deliver fast, statically rendered pages that benefit the user while allowing for dynamic user-specific content.

In this post, we’ll see how to implement a similar island approach in ASP.NET Core applications that utilize **response and output caching** for performance increases while still carving out small page sections for dynamic content. We’ll use Htmx to trigger requests for dynamic content based on three exclusive page events.

## What is an Island?

As described in the introduction, an island is part of the document object model (DOM) that is loaded after the initial page load. This allows the shared DOM across users to be cached while dynamic content specific to a user session is loaded afterward. While a post-load event is commonly used to retrieve dynamic content, islands can also be lazily loaded and take advantage of the `revealed` or `intersect` events. These will only trigger requests if the user sees this DOM element on the page. Optimizing pages by selecting island events can reduce unnecessary service processing. In general, islands are a powerful technique when building web applications.

Examples of islands may include a profile name in a page’s navigation, customized recommendations on a storefront, user-based statistics for a dashboard, and more. These islands typically comprise a small yet critical part of the user experience.

Now, let’s implement an `island` component for ASP.NET Core web applications.

## The Island Plan

An island has three parts: the initial content, the triggering event, and the endpoint that returns the dynamic content.

Let’s start by seeing what our `island` implementation will look like on a Razor page.

```razor
<island url="/profile/avatar">
    <div class="alert alert-info d-flex justify-content-center vertical-align-center">
        <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
    </div>
</island>
```

The `island` element has a URL attribute and some inner content that will be used as a placeholder while dynamic content is loaded. The handler for our dynamic content is a straightforward endpoint, but it could be any backend you choose. What you don’t see in this initial example is a configurable event. We’ll see that in action later in the post.

```csharp
app.MapGet("/profile/avatar", () => Results.Content(
    //lang=html
    $"""
     <div class="alert alert-info">
        <p class="fs-1 fw-bold">🌴 Welcome to the island Khalid!</p>
        <p class="fs-3">You arrived on ({DateTime.Now.ToLongTimeString()})</p>
     </div>
     """)
);
```

OK, let’s build it!

## Creating an Island TagHelper

While I initially attempted to do this with a `ViewComponent` I found it needing more in what it could accomplish. Tag Helpers are vastly more powerful and capable in this case.

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Razor.TagHelpers;

public enum IslandEvents
{
    Load,
    Revealed,
    Intersect
}

[HtmlTargetElement("island")]
public class IslandTagHelper : TagHelper
{
    [HtmlAttributeName("url"), Required]
    public string? Url { get; set; }

    [HtmlAttributeName("event")] 
    public IslandEvents Event { get; set; } = IslandEvents.Load;
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Changing the tag name to "div"
        output.TagName = "div";

        var @event = Event switch
        {
            IslandEvents.Load => "load",
            IslandEvents.Revealed => "revealed",
            IslandEvents.Intersect => "intersect once",
            _ => "load"
        };

        output.Attributes.SetAttribute("hx-get", Url);
        output.Attributes.SetAttribute("hx-trigger", @event);
        output.Attributes.SetAttribute("hx-swap", "outerHTML");

        // Retrieve the inner content
        var childContent = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(childContent);

        // Ensuring the tag is not self-closing
        output.TagMode = TagMode.StartTagAndEndTag;
    }
}
```

Remember to register the tag helper in `_ViewImports.cshtml`. Since we’re using [Htmx](https://htmx.org), you must add the script reference to your `_Layout.cshtml`.

```html
<script src="https://unpkg.com/htmx.org@2.0.3"></script>
```

You’ll notice we have an `enum` of `IslandEvents`. Let’s discuss what each person does and how their behavior differs.

*   **Load**: After the initial page load, the page will retrieve the dynamic content.
*   **Revealed**: The dynamic content will be retrieved only after the element is visible to the user.
*   **Intersect**: If the element is in an `overflow`, the page will only retrieve the dynamic content after it intersects with the visible part of the page.

Let’s look at how to change the default loading behavior.

```razor
<div style="margin-top: 2000px">
    <island url="/profile/avatar" event="Revealed">
        <div class="alert alert-info d-flex justify-content-center vertical-align-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    </island>
</div>
```

In the example, the `margin-top` is `2000px`. This forces us to scroll down to reveal the element, and only then will we call for dynamic content.

## Response and Output Caching

While not the main focus of this post, response and output caching will be essential to using islands. As mentioned, you want to share as much as possible across user sessions to reap the benefits of these techniques.

You must update your `Program` file to include the following components to add output and response caching—first, the service registrations.

```csharp
builder.Services.AddOutputCache();
builder.Services.AddResponseCaching();
```

Then, as part of your ASP.NET Core pipeline, you’ll need to add the following middleware calls.

```csharp
app.UseResponseCaching();
app.UseOutputCache();
```

Once registered, you can apply output caching to the endpoint, which displays most of the content. Here is an example of its use on a Razor page.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

[OutputCache(Duration = 100),
 ResponseCache(
     Duration = 100,
     Location = ResponseCacheLocation.Any,
     NoStore = false)]
public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    private readonly ILogger<IndexModel> logger = logger;

    public void OnGet()
    {
    }
}
```

Feel free to apply caching to the dynamic endpoints, being mindful of cache-busting based on a user’s session variables.

```csharp
app.MapGet("/profile/avatar", () => Results.Content(
    //lang=html
    $"""
     <div class="alert alert-info">
        <p class="fs-1 fw-bold">🌴 Welcome to the island Khalid!</p>
        <p class="fs-3">You arrived on ({DateTime.Now.ToLongTimeString()})</p>
     </div>
     """))
    .CacheOutput(policy => { /* apply caching policy here */ });
```

If you are unfamiliar with caching in ASP.NET Core, [I recommend that you read the official documentation on the topic](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-8.0).

There you have it. ASP.NET Core Islands using Htmx.

## Conclusion

Some folks may wonder why they should not just do all the caching and reuse on the server, including techniques like [donut caching and donut hole caching](https://www.computerworld.com/article/1604649/what-exactly-is-donut-caching.html). The advantage of this approach is that dynamic content can come from **anywhere** and isn’t explicitly tied to your ASP.NET Core server backend. You can deliver dynamic content from CDNs, function as service endpoints, use other static sites, and more. It’s a technique that benefits from ASP.NET Core but isn’t tied to it.

If I spent more time on this implementation, I’d likely integrate islands more closely to ASP.NET Core’s routing mechanisms, allowing users to specify pages, handlers, MVC actions, and more. That said, a simple `url` attribute works just fine.

Finally, TagHelpers are an underrated feature of the ASP.NET Core stack, and I think people should revisit them.

If you’d like to see a running sample of this project, visit my **[GitHub repository and try it out for yourself](https://github.com/khalidabuhakmeh/aspnetcore-htmx-islands)**. Cheers.

Tags: [aspnet](/tag/aspnet/) [htmx](/tag/htmx/)

![A headshot of Khalid Abuhakmeh, the author.](/assets/images/authorimage.jpg)

## About Khalid Abuhakmeh

Khalid is a developer advocate at JetBrains focusing on .NET technologies and tooling.

## Read Next

October 22, 2024

### [Update HTML Elements with Htmx Triggers and ASP.NET Core](/update-html-elements-with-htmx-triggers-and-aspnet-core)

![Thumbnail for an article titled 'Update HTML Elements with Htmx Triggers and ASP.NET Core', featuring a stylized representation of HTML tags and a trigger icon.](https://res.cloudinary.com/abuhakmeh/image/fetch/c_limit,f_auto,q_auto,w_500/https://khalidabuhakmeh.com/assets/images/posts/misc/htmx-aspnetcore-hx-trigger-html.jpg)

December 17, 2024

### [Building a Persistent Counter with Alpine.Js](/building-a-persistent-counter-with-alpinejs)

![Thumbnail for an article titled 'Building a Persistent Counter with Alpine.Js', showing a stylized counter interface.](https://res.cloudinary.com/abuhakmeh/image/fetch/c_limit,f_auto,q_auto,w_500/https://khalidabuhakmeh.com/assets/images/posts/misc/building-persistent-counter-alpinejs-javascript.jpg)