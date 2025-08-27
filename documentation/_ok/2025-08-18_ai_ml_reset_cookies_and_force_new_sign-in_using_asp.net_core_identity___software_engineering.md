```yaml
---
title: "Reset Cookies and force new sign-in using ASP.NET Core Identity | Software Engineering"
source: https://damienbod.com/2025/08/18/reset-cookies-and-force-new-sign-in-using-asp-net-core-identity/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2112
date_published: 2025-08-18T05:16:16.000Z
date_captured: 2025-08-25T10:15:02.625Z
domain: damienbod.com
author: damienbod
category: ai_ml
technologies: [ASP.NET Core, Duende IdentityServer, ASP.NET Core Identity, Entra ID, OpenID Connect, Razor Pages, GitHub, ILogger, Microsoft accounts, HTTP, Cookies, Clear-Site-Data]
programming_languages: [C#, HTML]
tags: [authentication, security, cookies, aspnet-core, identity-server, openid-connect, entra-id, web-development, federated-identity]
key_concepts: [cookie-management, authentication-flow, federated-identity, openid-connect-protocol, remote-authentication-errors, sign-out-vs-cookie-reset, razor-pages, identity-provider]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details a method for implementing a cookie reset and forcing a new sign-in in an ASP.NET Core application. It specifically addresses scenarios where local cookies become problematic due to size or remote authentication errors, leveraging Duende IdentityServer federated with Entra ID. The solution involves deleting local cookies via Razor Pages and initiating a new OpenID Connect authentication flow, which can result in silent re-authentication if the user remains logged into the external identity provider. The post clarifies that this is a local cookie cleanup, distinct from a full sign-out, and discusses considerations for server-side sessions and external identity provider control.]
---
```

# Reset Cookies and force new sign-in using ASP.NET Core Identity | Software Engineering

# Reset Cookies and force new sign-in using ASP.NET Core Identity

August 18, 2025 · by [damienbod](https://damienbod.com/author/damienbod/ "Posts by damienbod") · in [Uncategorized](https://damienbod.com/category/uncategorized/) · [3 Comments](https://damienbod.com/2025/08/18/reset-cookies-and-force-new-sign-in-using-asp-net-core-identity/#comments)

This post looks at implementing a cookie reset in an ASP.NET Core application using Duende identity server which federates to Entra ID. Sometimes cookies need to be reset for end users due to size problems, or unknown remote authentication server errors. The cookies can be cleared and a new sign in can be forced.

**Code**: [https://github.com/damienbod/DuendeProfileServiceAspNetCoreIdentity](https://github.com/damienbod/DuendeProfileServiceAspNetCoreIdentity)

## Setup

The ASP.NET Core web application is setup to authenticate using an identity server implemented using ASP.NET Core Identity and Duende IdentityServer. The identity provider federates to Entra ID using OpenID Connect. In our use case, Microsoft accounts are invited into the Entra ID tenant as guest and the login.live.com is used to authenticate the Microsoft Account users. We would like to provide a cookie reset for the two ASP.NET Core applications and force a sign-in flow. The user might still be authenticated in Entra ID or Live and the user will automatically be authenticated again. This just provides a cookie clean up on your application. If you want to sign-out, then the standard SignOut method can be used with the correct required schemes.

![Diagram illustrating a federated identity flow. An OIDC Client connects to an identity provider (ASP.NET Core Identity, Duende IdentityServer). Both have associated local cookies. The identity provider then connects to Entra ID, which in turn connects to live.com. Both Entra ID and live.com also have their own associated cookies. The OIDC Client and Identity Provider are enclosed in a dashed box, indicating the scope of the local application.](https://damienbod.com/wp-content/uploads/2025/05/federation.png?w=630)

## Reset

An ASP.NET core Razor Page is used to send an cookie reset POST request.

```html
<form method="post">
    <input type="submit" class="btn btn-warning" name="Reset">
</form>
```

The POST request deletes all cookies for the identity provider application and redirects to the OpenID Connect client using a HTTP redirect.

```csharp
public IActionResult OnPost()
{
    // clear cache if needed
    foreach (var cookie in Request.Cookies.Keys)
    {
        Response.Cookies.Delete(cookie);
    }
    // bubble up to a UI application if required
    return Redirect("[https://localhost:5015/resetcache](https://localhost:5015/resetcache)");
}
```

The HTTP GET redirect deletes all the cookies on the OpenID Connect client application and redirects to the default page which requires an authenticated user. The default challenge kicks in and starts an authentication flow. If the user is authenticated on Entra ID, the identity is silently authenticated again.

```csharp
[AllowAnonymous]
public class Index : PageModel
{
    public IActionResult OnGet()
    {
        // clear the cookie cache
        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
        }
        // Force a sign-in
        return Redirect("/");
    }
}
```

## Handle remote errors

The cookie reset can be used to handle unknown OpenID Connect remote authentication errors which sometimes fail for unknown reasons and the user cannot recover without resetting the local cookies.

```csharp
OnRemoteFailure = async context =>
{
    var logger = context.HttpContext
        .RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation(
        "OnRemoteFailure from identity provider. Scheme: {Scheme: }",
        context.Scheme.Name);
    if (context.Failure != null)
    {
        //server_error
        context.HandleResponse();
        context.Response.Redirect(
            $"/Home/Reset?remoteError={context.Failure.Message}");
    }
    await Task.CompletedTask;
}
```

## Notes

Normally this should not be required. The default sign out logic should be used if a logout is required. This logic is only for a local reset and does not logout the user. Changing state on a HTTP GET is also not recommended but this required for this to work.

### Why not just signout?

Using the SignOut method is the correct way to sign out of an ASP.NET core application. Using the different schemes, each one can be signed out. Removing cookies from the browser does not sign out the user. This just cleans up the local cookies and whatever is saved behind them.

### What about cache?

If the session is stored in a server session or a server cache, then you would need to clean this up and not just the cookies.

### What happens on the external identity provider?

We have no control over this directly and cannot control the sessions here. To remove the session, you would need to send an **endsession** request to the OpenID Connect server.

## Links

*   [https://docs.duendesoftware.com/identityserver/reference/services/profile-service](https://docs.duendesoftware.com/identityserver/reference/services/profile-service)
*   [https://duendesoftware.com/products/identityserver](https://duendesoftware.com/products/identityserver)
*   [https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
*   [https://learn.microsoft.com/en-us/aspnet/core/security/authentication/claims](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/claims)
*   [https://github.com/damienbod/MulitipleClientClaimsMapping](https://github.com/damienbod/MulitipleClientClaimsMapping)
*   [https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/)