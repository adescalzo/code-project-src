# 🛠️ How I Used Custom Middleware to Solve 5 Real Problems in .NET Core | by Sunita Rawat | Jul, 2025 | Medium

**Source:** https://medium.com/@sunita.rawat.cgi/%ef%b8%8f-how-i-used-custom-middleware-to-solve-5-real-problems-in-net-core-6ed6b4a49805
**Date Captured:** 2025-07-28T16:16:22.092Z
**Domain:** medium.com
**Author:** Sunita Rawat
**Category:** general

---

Top highlight

Member-only story

# 🛠️ How I Used Custom Middleware to Solve 5 Real Problems in .NET Core

[

![Sunita Rawat](https://miro.medium.com/v2/resize:fill:32:32/1*T5r1LgkLNavO5BlyR6yu0g.jpeg)





](/@sunita.rawat.cgi?source=post_page---byline--6ed6b4a49805---------------------------------------)

[Sunita Rawat](/@sunita.rawat.cgi?source=post_page---byline--6ed6b4a49805---------------------------------------)

Follow

4 min read

·

Jul 18, 2025

51

2

Listen

Share

More

_Real-world examples you can use today_

# 🧩 Introduction: The Moment I Realized Middleware Is Underrated

When I first learned about middleware in .NET Core, it felt… basic.

We all write `app.UseRouting()` and `app.UseAuthentication()` in `Startup.cs` or `Program.cs` and move on.

But it wasn’t until I worked on a large multi-tenant product — where different clients had different behaviors, security needs, request policies, and maintenance rules — that I discovered how **custom middleware is one of the most underrated tools in .NET Core**.

Instead of bloating controllers with repeated logic, I built small, clean middleware components that:

*   Resolved tenant context
*   Blocked unauthorized IPs
*   Triggered maintenance mode
*   Logged full request & response bodies
*   Validated custom headers

This article is about those exact scenarios — shared in the most real, dev-to-dev way.

# ⚙️ But First — What Is Middleware?

Middleware in .NET Core is just a small component that sits in the request pipeline. It can:

*   Read the incoming request
*   Take an action (like logging, auth, header validation, etc.)
*   Decide whether to pass the request forward (or short-circuit it)

Think of middleware like airport security — checking passports, scanning bags, and then letting you pass to the gate.

# 🚧 Why Built-in Middleware Isn’t Always Enough

The framework gives you some amazing built-in middlewares — routing, static files, CORS, authentication, etc.

But real-world apps often need **cross-cutting behaviors that are too custom** for these defaults.

Things like:

*   “Block all requests from particular regions except admins”
*   “Only allow API access if `X-Partner-Token` is present"
*   “Log entire request/response bodies for troubleshooting”
*   “Resolve tenant from subdomain and attach its DB config”

Let’s dive into the five middlewares I wrote to solve such problems — with real code.

# 🔥 Middleware #1: Tenant Resolver (Multi-Tenant Support)

# 📌 Problem:

We had one API serving multiple customers, and each tenant had its own theme, DB connection, and API rules. We needed a way to **identify the tenant from the request** and inject it into the context.

# ✅ Solution:

Create middleware that:

*   Extracts the tenant ID from the subdomain or header
*   Validates it using a service
*   Stores tenant info in `HttpContext.Items`

# 🧠 Code:

public class TenantResolverMiddleware  
{  
    private readonly RequestDelegate \_next;  
    public TenantResolverMiddleware(RequestDelegate next) => \_next = next;  
    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)  
    {  
        var host = context.Request.Host.Host;  
        var tenantId = host.Split('.')\[0\]; // e.g., tenant1.myapp.com  
        var tenant = await tenantService.GetTenantAsync(tenantId);  
        if (tenant == null)  
        {  
            context.Response.StatusCode = 404;  
            await context.Response.WriteAsync("Tenant Not Found");  
            return;  
        }  
        context.Items\["Tenant"\] = tenant;  
        await \_next(context);  
    }  
}

# 🔥 Middleware #2: IP Whitelisting / Geo Blocking

# 📌 Problem:

The client wanted to allow API access **only from certain IPs** and **block traffic from specific countries**.

# ✅ Solution:

Build a middleware that checks the client IP and blocks unauthorized ones.

# 🧠 Code:

public class IpRestrictionMiddleware  
{  
    private readonly RequestDelegate \_next;  
    private readonly ILogger<IpRestrictionMiddleware> \_logger;  
    private readonly List<string\> allowedIps = new() { "192.168.1.1", "203.0.113.0" };  
    public IpRestrictionMiddleware(RequestDelegate next, ILogger<IpRestrictionMiddleware> logger)  
    {  
        \_next = next;  
        \_logger = logger;  
    }  
    public async Task InvokeAsync(HttpContext context)  
    {  
        var ip = context.Connection.RemoteIpAddress?.ToString();  
        if (!allowedIps.Contains(ip))  
        {  
            \_logger.LogWarning("Blocked IP: {IP}", ip);  
            context.Response.StatusCode = 403;  
            await context.Response.WriteAsync("Access Denied");  
            return;  
        }  
        await \_next(context);  
    }  
}

👉 _Pro Tip:_ For geo-blocking, integrate with a GeoIP API or MaxMind DB.

# 🔥 Middleware #3: Maintenance Mode Toggle

# 📌 Problem:

Sometimes, we need to put the app into maintenance mode without deploying a new build. Show a “503 Service Unavailable” to users — but let internal IPs or Admins in.

# ✅ Solution:

Use a middleware that checks a config flag and short-circuits the request if needed.

# 🧠 Code:

public class MaintenanceModeMiddleware  
{  
    private readonly RequestDelegate \_next;  
    private readonly IConfiguration \_config;  
    public MaintenanceModeMiddleware(RequestDelegate next, IConfiguration config)  
    {  
        \_next = next;  
        \_config = config;  
    }  
    public async Task InvokeAsync(HttpContext context)  
    {  
        bool isInMaintenance = \_config.GetValue<bool\>("AppSettings:MaintenanceMode");  
        if (isInMaintenance && !context.User.IsInRole("Admin"))  
        {  
            context.Response.StatusCode = 503;  
            await context.Response.WriteAsync("App is under maintenance.");  
            return;  
        }  
        await \_next(context);  
    }  
}

🔧 Toggle it in `appsettings.json` or Azure App Configuration dynamically.

# 🔥 Middleware #4: Request & Response Logging

# 📌 Problem:

We had critical endpoints failing silently. Devs couldn’t see request bodies or downstream responses.

# ✅ Solution:

Build a middleware that logs the **entire request and response body** for debugging (careful with sensitive info).

# 🧠 Code (Simplified):

public class RequestResponseLoggingMiddleware{    private readonly RequestDelegate \_next;    private readonly ILogger<RequestResponseLoggingMiddleware> \_logger;    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)    {  
        \_next = next;  
        \_logger = logger;  
    }    public async Task InvokeAsync(HttpContext context)    {        // Log Request        context.Request.EnableBuffering();        var buffer = new byte\[Convert.ToInt32(context.Request.ContentLength)\];        await context.Request.Body.ReadAsync(buffer);        var requestBody = Encoding.UTF8.GetString(buffer);  
        context.Request.Body.Position = 0;  
        \_logger.LogInformation("Request: {body}", requestBody);        // Log Response        var originalBody = context.Response.Body;        using var newBody = new MemoryStream();  
        context.Response.Body = newBody;        await \_next(context);  
        newBody.Seek(0, SeekOrigin.Begin);        var responseBody = await new StreamReader(newBody).ReadToEndAsync();  
        newBody.Seek(0, SeekOrigin.Begin);  
        \_logger.LogInformation("Response: {body}", responseBody);        await newBody.CopyToAsync(originalBody);  
    }  
}

# 🔥 Middleware #5: Enforcing Custom Headers

# 📌 Problem:

Some APIs were exposed to third-party vendors who **must send** `**X-Partner-Token**`. Missing that header should reject the request early.

# ✅ Solution:

Write a header enforcement middleware.

# 🧠 Code:

public class RequiredHeaderMiddleware  
{  
    private readonly RequestDelegate \_next;  
    public RequiredHeaderMiddleware(RequestDelegate next) => \_next = next;  
    public async Task InvokeAsync(HttpContext context)  
    {  
        if (!context.Request.Headers.ContainsKey("X-Partner-Token"))  
        {  
            context.Response.StatusCode = 400;  
            await context.Response.WriteAsync("Missing required header");  
            return;  
        }  
        await \_next(context);  
    }  
}

# 📎 Bonus Tips: Writing Clean Middleware

*   ✅ Keep each middleware **single-purpose**
*   ✅ Use `HttpContext.Items` to share state
*   ✅ Don’t swallow exceptions — let them bubble up unless you’re handling them
*   ✅ Call `await _next(context)` **unless you intentionally short-circuit**

# 📌 When Should You Use Middleware?

**Use middleware when you need to:**

*   Handle cross-cutting concerns like logging, auth, throttling
*   Enforce rules before controller executes
*   Keep your controller logic clean and focused
*   Share logic across multiple routes

**Avoid middleware when:**

*   You only need it for one controller
*   The logic depends on complex user input validation

# Final Thoughts

Middleware is one of those things in .NET Core that feels “optional” — until you actually need it.

It helped me clean up controllers, centralize behavior, and add flexibility to production apps without rewriting business logic.

If you’ve only ever used the built-in ones, try writing your own. Start small. You’ll soon realize that **middleware is a superpower** when used right.