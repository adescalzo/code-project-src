```yaml
---
title: "🔐 How I Secured a .NET Core API Without JWT or Cookies — The Power of Custom Authentication | by Sunita Rawat | Jul, 2025 | Medium"
source: https://medium.com/@sunita.rawat.cgi/how-i-secured-a-net-core-api-without-jwt-or-cookies-the-power-of-custom-authentication-99a48257c852
date_published: 2025-07-12T15:30:13.775Z
date_captured: 2025-08-12T21:02:58.070Z
domain: medium.com
author: Sunita Rawat
category: security
technologies: [ASP.NET Core, .NET Core, Azure, Azure Key Vault, JWT, OAuth, Identity]
programming_languages: [C#, SQL]
tags: [authentication, authorization, aspnet-core, security, web-api, custom-authentication, dotnet, webhook, api-security]
key_concepts: [custom-authentication-handler, authentication-schemes, jwt-authentication, cookie-authentication, claims-based-identity, dependency-injection, configuration-management]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores implementing custom authentication in ASP.NET Core APIs, addressing scenarios where standard JWT or cookie-based methods are insufficient, such as securing third-party webhooks with a shared secret. It provides a step-by-step guide on creating a `CustomHeaderAuthenticationHandler` to validate an `X-Client-Secret` header. The post demonstrates how to register this custom handler within `Program.cs` and apply it to specific endpoints using the `[Authorize]` attribute. It also emphasizes best practices like avoiding hardcoded secrets by utilizing `IConfiguration` or Azure Key Vault, and the flexibility of combining custom schemes with existing ones for hybrid authentication scenarios.
---
```

# 🔐 How I Secured a .NET Core API Without JWT or Cookies — The Power of Custom Authentication | by Sunita Rawat | Jul, 2025 | Medium

# 🔐 How I Secured a .NET Core API Without JWT or Cookies — The Power of Custom Authentication

_What if_ `[Authorize]` _just isn’t enough?_

Welcome to real-world software. Where best practices meet edge cases, and boilerplate doesn’t cut it.

When we started building our backend for a PaaS solution using **.NET Core** on **Azure**, we assumed authentication would be simple:
**JWT for mobile, cookies for web, done and dusted.**

But then came reality.

# 🎯 The Problem No One Talks About

Here’s what we actually needed:

*   ✅ **JWT-based auth** for our mobile apps
*   ✅ **Cookie-based login** for our internal dashboards
*   ✅ **Custom secret header check** for third-party webhooks
    (which didn’t support JWT, OAuth, or anything modern)

That’s when I realized:

> In .NET Core, you can write your own **authentication handler** from scratch.
>
> You’re not locked into JWT, OAuth, or Identity.

# 🧩 Why Not Just Stick with JWT or Cookies?

They’re amazing — no doubt.
But sometimes, they just **don’t fit**:

*   You’re working with partners who can only send a shared secret.
*   Your webhook has to be validated by a single `X-Client-Secret` header.
*   You want a lightweight, no-token overhead way to secure specific endpoints.

That’s where **custom authentication handlers** shine.

# ✅ Real-World Use Case: Securing a Webhook with `X-Client-Secret`

We had an endpoint like:

POST /api/vendor/update-inventory

Our partner couldn’t send a token. They could only send this:

X-Client-Secret: abc123

No JWT. No OAuth.
Just a plain, old, shared secret.
So we built a custom solution, cleanly integrated into ASP.NET Core.

# 🛠 Step 1: Build the Custom Authentication Handler

```csharp
public class CustomHeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public CustomHeaderAuthenticationHandler(        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock) {}

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Client-Secret", out var secret))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Client-Secret header."));
        }
        if (secret != "abc123") // ❌ Avoid hardcoding in real apps
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid client secret."));
        }
        var claims = new[] { new Claim(ClaimTypes.Name, "TrustedVendor") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

# ⚙️ Step 2: Register It in `Program.cs`

```csharp
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "CustomHeader";
        options.DefaultChallengeScheme = "CustomHeader";
    })
    .AddScheme<AuthenticationSchemeOptions, CustomHeaderAuthenticationHandler>("CustomHeader", null);
```

# 🔐 Step 3: Protect Your Endpoint

```csharp
[Authorize(AuthenticationSchemes = "CustomHeader")]
[HttpPost("vendor/update-inventory")]
public IActionResult UpdateInventory()
{
    return Ok("Inventory updated securely.");
}
```

Now its💥 — done.
Now only requests with the correct `X-Client-Secret` can access this endpoint.

# 💡 Lessons Learned

*   🔍 **Always log failed attempts** in the handler — it saves lives in production.
*   🧩 **Combine with other schemes** like JWT and Cookies using:

```csharp
[Authorize(AuthenticationSchemes = "Jwt,Cookies,CustomHeader")]
```

*   🔐 **Never hardcode secrets** — use `IConfiguration` or pull from **Azure Key Vault**.
*   🧼 Use `AuthenticateResult.NoResult()` to allow fallback to other schemes if needed.

# 🔄 Supporting Hybrid Authentication

Want to mix and match?

```csharp
services.AddAuthentication()
    .AddJwtBearer("Jwt", options => { /*...*/ })
    .AddCookie("Cookies", options => { /*...*/ })
    .AddScheme<AuthenticationSchemeOptions, CustomHeaderAuthenticationHandler>("CustomHeader", null);
```

Now you can do:

```csharp
[Authorize(AuthenticationSchemes = "Jwt,Cookies,CustomHeader")]
```

Perfect for APIs that serve mobile, web, and third-party vendors all in one.

# 🚀 Why This Changed Everything for Us

✅ Third-party partners could integrate without complex token setups
✅ No compromises on security
✅ Everything stayed modular, clean, and inside our `.NET Core` codebase
✅ No custom middleware hacks or workarounds

It made our backend **more flexible**, **easier to maintain**, and **future-proof**.