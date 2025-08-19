```yaml
---
title: "TheCodeMan | Master .NET Technologies"
source: https://thecodeman.net/posts/better-error-handling-with-problemdetails?utm_source=Newsletter&utm_medium=email&utm_campaign=Deep%20Dive%20into%20Source%20Generators%20in%20C%23
date_published: unknown
date_captured: 2025-08-17T22:08:44.079Z
domain: thecodeman.net
author: Unknown
category: ai_ml
technologies: [ASP.NET Core, Minimal API, ProblemDetails, .NET, JetBrains ReSharper, Visual Studio Code, JSON, HTTP]
programming_languages: [C#, JavaScript]
tags: [error-handling, api, web-api, dotnet, minimal-api, structured-errors, http-status-codes, middleware, exception-handling, rfc-7807]
key_concepts: [error-handling, structured-errors, api-design, global-exception-handling, minimal-api, rfc-7807, http-status-codes]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article emphasizes the criticality of robust error handling in APIs, advocating for the use of ProblemDetails (RFC 7807) to provide standardized, machine-readable error responses. It guides readers through building a simple .NET Web API using Minimal API, demonstrating how to return structured errors for both specific business logic failures and unhandled exceptions. Key implementation steps include defining product logic, creating a custom exception handling middleware, and configuring the application in `Program.cs`. The post highlights the advantages of this approach, such as improved clarity for frontend developers and adherence to industry standards.]
---
```

# TheCodeMan | Master .NET Technologies

[Stefan Đokić](/) Menu

*   [Home](/)
*   [Get for Free](#)
    
    [Pass Interview Prep Kit](/pass-your-interview)[Builder Pattern Chapter](/builder-pattern-free-stuff)[RAG System in .NET](/rag-system-dotnet)
    
*   [Blog](/blog)
*   [Ebooks](#)
    
    [Ebook Simplified](/design-patterns-simplified)[5 Patterns Ebook](/design-patterns-that-deliver-ebook)
    
*   [For Sponsors](/sponsorship)

document.addEventListener("DOMContentLoaded", function() { "use strict"; // Burger Menu function burgerMenu() { document.body.addEventListener('click', function(event) { if (event.target.matches('.js-fh5co-nav-toggle')) { var nav = document.getElementById('ftco-nav'); if (nav.style.display === 'block' || getComputedStyle(nav).display === 'block') { event.target.classList.remove('active'); nav.style.display = 'none'; } else { event.target.classList.add('active'); nav.style.display = 'block'; } event.preventDefault(); } }); } burgerMenu(); // Dropdown hover var dropdowns = document.querySelectorAll('nav .dropdown'); dropdowns.forEach(function(dropdown) { dropdown.addEventListener('mouseover', function() { this.classList.add('show'); const link = this.querySelector('a'); if (link) { link.setAttribute('aria-expanded', true); } const dropdownMenu = this.querySelector('.dropdown-menu'); if (dropdownMenu) { dropdownMenu.classList.add('show'); } }); dropdown.addEventListener('mouseout', function() { this.classList.remove('show'); const link = this.querySelector('a'); if (link) { link.setAttribute('aria-expanded', false); } const dropdownMenu = this.querySelector('.dropdown-menu'); if (dropdownMenu) { dropdownMenu.classList.remove('show'); } }); }); // Scroll Window function scrollWindow() { window.addEventListener('scroll', function() { var st = window.scrollY, navbar = document.querySelector('.ftco\_navbar'), sd = document.querySelector('.js-scroll-wrap'); if (st > 150) { if (!navbar.classList.contains('scrolled')) { navbar.classList.add('scrolled'); } } if (st < 150) { if (navbar.classList.contains('scrolled')) { navbar.classList.remove('scrolled', 'sleep'); } } if (st > 350) { if (!navbar.classList.contains('awake')) { navbar.classList.add('awake'); } if (sd) { sd.classList.add('sleep'); } } if (st < 350) { if (navbar.classList.contains('awake')) { navbar.classList.remove('awake'); navbar.classList.add('sleep'); } if (sd) { sd.classList.remove('sleep'); } } }); } scrollWindow(); });

## Better Error Handling in .NET using ProblemDetails

June 23 2025

##### JetBrains is bringing the power of ReSharper to Visual Studio Code! Here’s your chance to influence its future – [join the public preview](https://jb.gg/rs-vsc-thecodeman-newsletter) to get early access, test powerful new tools, and share your feedback directly with the development team.

##### [Join now](https://jb.gg/rs-vsc-thecodeman-newsletter)

   
 

### Background

   
 

##### Let’s be honest - error handling is usually the last thing we think about when building APIs. But it should be one of the first.

##### Imagine this:

##### Your frontend calls an API, and gets this in return:

##### "Object reference not set to an instance of an object."

##### Not helpful.

##### Now imagine getting this instead:

```json
{
    "title": "Something went wrong",
    "status": 500,
    "detail": "Please contact support.",
    "instance": "/products/0"
}
```

##### Now that’s helpful and clean. And that’s exactly what ProblemDetails gives us.

   
 

### What is ProblemDetails?

   
 

##### It’s a **standard way** of returning error responses in APIs, defined in [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807). Instead of random text or inconsistent JSON, you return structured errors like this:

```json
{
    "title": "Product not found",
    "status": 404,
    "detail": "No product with ID 42.",
    "instance": "/products/42"
}
```

##### ASP.NET has built-in support for this - and it works great with Minimal APIs too.

   
 

### Let’s Build It with Minimal API

   
 

##### We’ll create a simple Web API where you can:

##### • Get a product by ID

##### • Return errors using ProblemDetails

##### • Handle exceptions globally

##### All using Minimal API style.

#### 1: Define the Product Logic

##### Let’s fake a product lookup that throws an error if the ID is invalid or not found.

```csharp
public record Product(int Id, string Name);
```

#### 2: Add Global Error Handling Middleware

##### We’ll catch all unhandled exceptions and return a structured ProblemDetails response.

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            var problem = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Please contact support.",
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problem.Status.Value;

            var json = JsonSerializer.Serialize(problem);
            await context.Response.WriteAsync(json);
        }
    }
}
```

#### 4: Wire Everything Up in Program.cs

##### This is where Minimal API really shines - everything in one file:

```csharp
using Microsoft.AspNetCore.Mvc;
using ProblemDetailsMinimalApi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Use custom error handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// In-memory data for testing
var products = new List<Product>
{
    new Product(1, "Laptop"),
    new Product(2, "Phone"),
    new Product(3, "Keyboard")
};

// GET /products/{id}
app.MapGet("/products/{id:int}", (int id, HttpContext http) =>
{
    if (id <= 0)
        throw new ArgumentOutOfRangeException(nameof(id), "Product ID must be greater than zero.");

    var product = products.FirstOrDefault(p => p.Id == id);

    if (product is null)
    {
        var notFoundProblem = new ProblemDetails
        {
            Title = "Product not found",
            Status = StatusCodes.Status404NotFound,
            Detail = $"No product found with ID {id}.",
            Instance = http.Request.Path
        };

        return Results.Problem(
            title: notFoundProblem.Title,
            detail: notFoundProblem.Detail,
            statusCode: notFoundProblem.Status,
            instance: notFoundProblem.Instance
        );
    }

    return Results.Ok(product);
});

app.Run();
```

##### Then try:

##### • ✅ GET /products/1 - returns product

##### • ❌ GET /products/0 - throws exception → returns 500 ProblemDetails

##### • ❌ GET /products/999 - returns 404 ProblemDetails

   
 

### Optional: Add Custom Fields

   
 

##### You can extend ProblemDetails with your own data:

```csharp
public class CustomProblemDetails : ProblemDetails
{
    public string ErrorCode { get; set; } = default!;
}
```

##### Then return it with Results.Problem(...) and pass additional metadata.

   
 

### Benefits of This Approach

   
 

##### • Clean error responses

##### • Easy to understand for frontend devs

##### • Standards-based (RFC 7807)

##### • Built into .NET

   
 

### Wrapping Up

   
 

##### Never return ex.ToString() to the user - it may leak sensitive info.

##### ✅Log full exception

##### ❌Show minimal, generic details in the API response

##### With just a few lines of code, you now have a Minimal API that returns beautif

##### That's all from me today.

##### P.S. Follow me on [YouTube](https://www.youtube.com/@thecodeman_).

### **There are 3 ways I can help you:**

#### My Design Patterns Ebooks

[1\. Design Patterns that Deliver](/design-patterns-that-deliver-ebook?utm_source=website)

This isn’t just another design patterns book. Dive into real-world examples and practical solutions to real problems in real applications.[Check out it here.](/design-patterns-that-deliver-ebook?utm_source=website)

  

[1\. Design Patterns Simplified](/design-patterns-simplified?utm_source=website)

Go-to resource for understanding the core concepts of design patterns without the overwhelming complexity. In this concise and affordable ebook, I've distilled the essence of design patterns into an easy-to-digest format. It is a Beginner level. [Check out it here.](/design-patterns-simplified?utm_source=website)

  

#### [Join TheCodeMan.net Newsletter](/)

Every Monday morning, I share 1 actionable tip on C#, .NET & Arcitecture topic, that you can use right away.

  

#### [Sponsorship](/sponsorship)

Promote yourself to 17,150+ subscribers by sponsoring this newsletter.

  

  

Master .NET Technologies

Join 17,150+ subscribers to improve your .NET Knowledge.

\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\] { --text-colour:#212529; --font-family:"Helvetica Neue", Helvetica, Arial, Verdana, sans-serif; } \[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\] .emailoctopus-form { --label-size:12px; --field-colour:#FFFFFF; --label-colour:#ffffff; --button-colour:#ffbd39; --button-text-colour:#000; --field-border-colour:#CED4DA; } @charset "UTF-8";\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].modal-container{align-items:center;background:rgba(77,77,77,.8);display:flex;flex-direction:column;inset:0;justify-content:center;opacity:0;overflow:hidden;position:fixed;transition:opacity .3s ease-in;width:100vw;z-index:999999}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].modal-container.active{opacity:1}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].modal-container .modal-container-inner{background:#fefefe;border-radius:5px;max-width:600px;padding:20px;position:relative}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].modal-container .modal-container-inner button.close{align-items:center;background-color:rgba(0,0,0,.8);border-color:transparent;border-radius:50%;border-width:0;color:#fff;display:flex;height:34px;margin:0;opacity:1;padding:6px;position:absolute;right:-17px;top:-17px;transition:background-color .5s;width:34px;z-index:2}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].modal-container .modal-container-inner button.close:hover{background-color:#000;cursor:pointer}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].modal-container .modal-container-inner button.close:focus{border-color:rgba(72,99,156,.3);box-shadow:0 0 0 .2rem rgba(0,123,255,.33);outline:none}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].modal-container .modal-container-inner button.close svg{height:22px;text-align:center;width:22px}@media (max-width:700px){\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].modal-container .modal-container-inner{margin:22px}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].modal-container .modal-container-inner>div{max-height:90vh;overflow:scroll}}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\].inline-container{max-width:600px!important;position:relative}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container{align-items:center;background:rgba(77,77,77,.8);display:flex;flex-direction:column;inset:0;opacity:0;overflow:hidden;position:fixed;transition:opacity .3s ease-in;width:100vw;z-index:999999}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container.active,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container.active{opacity:1}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container.active .slide-in-container-inner,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container.active .slide-in-container-inner{right:1%;width:90%}@media (max-width:700px){\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container.active .slide-in-container-inner,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container.active .slide-in-container-inner{right:5%}}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container.active.left .slide-in-container-inner,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container.active.left .slide-in-container-inner{left:1%}@media (max-width:700px){\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container.active.left .slide-in-container-inner,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container.active.left .slide-in-container-inner{left:5%}}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container .slide-in-container-inner,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container .slide-in-container-inner{background:var(--background-colour);border-radius:5px;bottom:1%;max-width:400px;padding:36px 1em 1em;position:fixed;right:-400px;transition:.3s;width:400px;z-index:999999}@media (max-width:700px){\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container .slide-in-container-inner,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container .slide-in-container-inner{bottom:2.5%}}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container .slide-in-container-inner button.close,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container .slide-in-container-inner button.close{align-items:center;background-color:transparent;border-color:transparent;border-width:0;color:gray;cursor:pointer;display:flex;height:34px;margin:0;opacity:1;padding:6px;position:absolute;right:6px;top:6px;width:34px;z-index:2}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container .slide-in-container-inner button.close svg,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container .slide-in-container-inner button.close svg{height:22px;text-align:center;width:22px}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="1"\].slide-in-container.left .slide-in-container-inner,\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="2"\].slide-in-container.left .slide-in-container-inner{left:-400px}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container{z-index:999999}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container.active{opacity:1}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container.active .slide-in-container-inner{right:2%;width:90%}@media (max-width:700px){\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container.active .slide-in-container-inner{right:5%}}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container.active.left .slide-in-container-inner{left:1%}@media (max-width:700px){\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container.active.left .slide-in-container-inner{left:5%}}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container .slide-in-container-inner{background:var(--background-colour);border-radius:5px;bottom:1%;max-width:400px;padding:36px 1em 1em;position:fixed;right:-400px;transition:.3s;width:400px;z-index:999999}@media (max-width:700px){\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container .slide-in-container-inner{bottom:2.5%}}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container .slide-in-container-inner button.close{align-items:center;background-color:rgba(0,0,0,.8);border-color:transparent;border-radius:50%;border-width:0;color:#fff;display:flex;height:34px;margin:0;opacity:1;padding:6px;position:absolute;right:-17px;top:-17px;transition:background-color .5s;width:34px;z-index:2}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container .slide-in-container-inner button.close:hover{background-color:#000;cursor:pointer}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container .slide-in-container-inner button.close:focus{border-color:rgba(72,99,156,.3);box-shadow:0 0 0 .2rem rgba(0,123,255,.33);outline:none}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container .slide-in-container-inner button.close svg{height:22px;text-align:center;width:22px}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]\[data-version="3"\].slide-in-container.left .slide-in-container-inner{left:-400px}\[data-form="861505f8-b3f8-11ef-896f-474a313dbc14"\]{display:block;font-family:var(--font-family);--eo-form-font-family:var(--font-family);--eo-form-font-size:var(--base-font-size,16px);--eo-form-font-color:#071c35;--eo-form-font-weight:400;--eo-form-dropdown-font-size:var(--base-font-size,16px);--eo-form-label-color:var(--label-colour,#071c35);--eo-form-label-weight:400;--eo-form-placeholder:#495057;--eo-form-input-background:var(--field-colour,#fff);--eo-form-padding-top:0.375rem;--eo-form-padding-left:0.75rem;--eo-form-border-color:var(--field-border-colour,#d4d4d4);--eo-form-icon-color:#8d8d8d;--eo-form-icon-hover:#737373;--eo-form-icon-active:#5c5c5c;--configured-primary-color:var(--primary-brand-colour,#3c3c3c