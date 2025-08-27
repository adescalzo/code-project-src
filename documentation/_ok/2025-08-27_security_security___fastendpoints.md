```yaml
---
title: "Security | FastEndpoints"
source: https://fast-endpoints.com/docs/security#declarative-security-policies
date_published: unknown
date_captured: 2025-08-27T13:36:35.126Z
domain: fast-endpoints.com
author: Unknown
category: security
technologies: [FastEndpoints, ASP.NET Core, JWT Bearer, Cookie Authentication, ASP.NET Identity, FastEndpoints.Security, Auth0, OAuth2, OpenID Connect, Microsoft.AspNetCore.Antiforgery, FastEndpoints.Generator, Dependency Injection, CancellationToken, HttpContext, IConfiguration, IAntiforgery, RequestDelegate, ClaimsPrincipal, AuthorizationPolicyBuilder, IAuthenticationHandler, TimeSpan, DateTime, IEnumerable, List, Task, HeaderNames]
programming_languages: [C#]
tags: [security, authentication, authorization, jwt, cookie-auth, fastendpoints, aspnet-core, refresh-tokens, csrf, access-control-list]
key_concepts: [jwt-authentication, cookie-authentication, endpoint-authorization, refresh-tokens, token-revocation, access-control-lists, csrf-protection, aspnet-core-middleware]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article details security features in FastEndpoints, built upon ASP.NET Core's authentication and authorization middleware. It covers setting up JWT Bearer and Cookie authentication with convenience wrappers, along with methods for generating and managing tokens, including refresh tokens and revocation. Endpoint authorization is extensively explained, allowing restriction by policies, claims, roles, permissions, and scopes, with support for both pre-built and declarative policies. Furthermore, the content introduces source-generated Access Control Lists for managing permissions and implements CSRF protection using antiforgery tokens for form submissions. It also discusses handling multiple authentication schemes and custom providers, making FastEndpoints a comprehensive solution for securing web APIs.]
---
```

# Security | FastEndpoints

# Security

## Introduction

The security aspects in FastEndpoints is built around the same authentication & authorization middleware that you're used to in ASP.NET such as **JWT Bearer**, **Cookie**, **Identity**, etc. Convenience wrappers are provided for **JWT** and **Cookie** schemes as described below.

Once auth middleware is configured, authorization requirements for endpoints can be specified in a convenient manner inside endpoints themselves. Roles/Claims/Permissions/Policies are all supported.

Endpoints are secure by default. You'd have to explicitly call **AllowAnonymous()** in the configuration if unauthenticated access is to be allowed to a particular endpoint.

---

## JWT Bearer Authentication

Support for easy JWT Bearer Authentication is provided. Install the **FastEndpoints.Security** package and register it in the middleware pipeline like so:

```bash
dotnet add package FastEndpoints.Security
```

```csharp
using FastEndpoints;
using FastEndpoints.Security; //add this

var bld = WebApplication.CreateBuilder();
bld.Services
   .AddAuthenticationJwtBearer(s => s.SigningKey = "The secret used to sign tokens") //add this
   .AddAuthorization() //add this
   .AddFastEndpoints();

var app = bld.Build();
app.UseAuthentication() //add this
   .UseAuthorization() //add this
   .UseFastEndpoints();
app.Run();
```

The action supplied to the **AddAuthenticationJwtBearer()** method allows you to configure other [advanced bearer token consumption](https://gist.github.com/dj-nitehawk/27550c40475ea528f5c187050fca9fba) options as needed.

### Generating JWT Tokens

JWTs can be easily generated with an endpoint that signs in users such as the following:

```csharp
public class UserLoginEndpoint : Endpoint<LoginRequest>
{
    public override void Configure()
    {
        Post("/api/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        if (await myAuthService.CredentialsAreValid(req.Username, req.Password, ct))
        {
            var jwtToken = JwtBearer.CreateToken(
                o =>
                {
                    o.SigningKey = "A secret token signing key";
                    o.ExpireAt = DateTime.UtcNow.AddDays(1);
                    o.User.Roles.Add("Manager", "Auditor");
                    o.User.Claims.Add(("UserName", req.Username));
                    o.User["UserId"] = "001"; //indexer based claim setting
                });

            await Send.OkAsync(
                new
                {
                    req.Username,
                    Token = jwtToken
                });
        }
        else
            ThrowError("The supplied credentials are invalid!");
    }
}
```

**JwtBearer.CreateToken()** static method can be used to supply the necessary arguments such as any user roles, claims, permissions, issuer, audience, etc. for generating a token. Alternatively, an instance of **JwtCreationOptions** can be configured globally as follows:

```csharp
bld.Services.Configure<JwtCreationOptions>( o =>  o.SigningKey = "..." );
```

and then, you can specify just the relevant settings during token creation like so:

```csharp
var token = JwtBearer.CreateToken( 
     o => 
     { 
         o.ExpireAt = DateTime.UtcNow.AddHours(1); 
         o.User.Claims.Add(("UserId", "001")); 
     });
```

It is also possible to programmatically update the signing keys of both **JwtCreationOptions** and **JwtSigningOptions** during runtime as shown [here](https://gist.github.com/dj-nitehawk/65b78b08075fae3070e9d30e2a59f4c1).

---

## Cookie Authentication

If your client applications have support for cookies, you can use cookies for auth instead of JWTs. By default, the following enables cookies (**http-only & samesite-lax**), so you can store user claims in the encrypted ASP.NET cookie without having to worry about the safety of front-end application storage choice.

```csharp
using FastEndpoints;
using FastEndpoints.Security; //add this

var bld = WebApplication.CreateBuilder();
bld.Services
   .AddAuthenticationCookie(validFor: TimeSpan.FromMinutes(10)) //configure cookie auth
   .AddAuthorization() //add this
   .AddFastEndpoints();

var app = bld.Build();
app.UseAuthentication() //add this
   .UseAuthorization() //add this
   .UseFastEndpoints();
app.Run();
```

Once the cookie auth middleware is configured, you can sign users in from within an endpoint handler by calling the following static method:

```csharp
CookieAuth.SignInAsync(u =>
{
    u.Roles.Add("Admin");
    u.Permissions.AddRange(new[] { "Create_Item", "Delete_Item" });
    u.Claims.Add(new("Address", "123 Street"));

    //indexer based claim setting
    u["Email"] = "abc@def.com";
    u["Department"] = "Administration";
});
```

The above method will embed a **ClaimsPrincipal** with the supplied roles/permissions/claims in an encrypted cookie and add it to the response. **CookieAuth.SignOutAsync ()** can be used to sign users out.

---

## Endpoint Authorization

Once an authentication middleware is registered such as **JWT Bearer**, or **Cookie** as shown above, access can be restricted to users based on the following:

*   **Policies**
*   **Claims**
*   **Roles**
*   **Permissions**
*   **Scopes**

## Pre-Built Security Policies

Security policies can be pre-built and registered during app startup and endpoints can choose to allow access to users based on the registered policy names like so:

```csharp
bld.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManagersOnly", x => x.RequireRole("Manager").RequireClaim("ManagerID"));
});
```

```csharp
public class UpdateUserEndpoint : Endpoint<UpdateUserRequest>
{
    public override void Configure()
    {
        Put("/api/users/update");
        Policies("ManagersOnly");
    }
}
```

## Declarative Security Policies

Instead of registering each security policy at startup, you can selectively specify security requirements for each endpoint in the endpoint configuration itself like so:

```csharp
public class RestrictedEndpoint : Endpoint<RestrictedRequest>
{
    public override void Configure()
    {
        Post("/api/restricted");
        Claims("AdminID", "EmployeeID");
        Roles("Admin", "Manager");
        Permissions("UpdateUsersPermission", "DeleteUsersPermission");
        Policy(x => x.RequireAssertion(...));
    }
}
```

### Claims() method

With this method you are specifying that if a [claims principal](https://andrewlock.net/introduction-to-authentication-with-asp-net-core/#multiple-identities) has ANY of the specified claims, access should be granted. If the requirement is to allow access only if ALL specified claims are present, you can use the **ClaimsAll()** method.

### Permissions() method

If access should be granted only if the user has ALL the said permissions, use the **PermissionsAll()** method, otherwise access is granted if ANY of the permissions are present. The **Claim Type** which this requirement is matched against can be changed like so:

```csharp
app.UseFastEndpoints(c => c.Security.PermissionsClaimType = "...") //defaults to 'permissions'
```

### Scopes() method

To restrict access based on [Scopes](https://oauth.net/2/scope) in tokens (e.g., from OAuth2/OpenID Connect IDPs), specify required scopes using the **Scopes()** method:

```csharp
public override void Configure()
{
    Get("/item");
    Scopes("item:read", "item:write");
}
```

This allows access if the user's **"scope"** claim includes ANY of the listed values. To require ALL scopes, use **ScopesAll()** instead.

By default, scopes are read from the **"scope"** claim, which can be changed like so:

```csharp
app.UseFastEndpoints(c => c.Security.ScopeClaimType = "scp")
```

If scope values aren't space-separated, customize parsing like so:

```csharp
app.UseFastEndpoints(c => c.Security.ScopeParser = input =>
{
    //extract scope values and return a collection of strings
})
```

### Roles() method

This method specifies roles which the current claims principal must possess in order to be allowed access. If the user has ANY of the given roles, access is granted. The **Claim Type** which this requirement is matched against can be changed like so:

```csharp
app.UseFastEndpoints(c => c.Security.RoleClaimType = "...") //defaults to 'role'
```

### Policy() method

You can specify an action to be performed on an **AuthorizationPolicyBuilder** for specifying any other authorization requirements that can't be satisfied by the above methods.

### AllowAnonymous() method

Call this method if unauthenticated users are to be allowed access to a particular endpoint.

It is also possible to specify which http verbs you'd like to allow anonymous access to if your endpoint is listening on [multiple verbs & routes](misc-conveniences#multiple-verbs-routes) like so:

```csharp
public class RestrictedEndpoint : Endpoint<RestrictedRequest>
{
    public override void Configure()
    {
        Verbs(Http.POST, Http.PUT, Http.PATCH);
        Routes("/api/restricted");
        AllowAnonymous(Http.POST);
    }
}
```

The above endpoint is listening for all 3 http methods on the same route but only **POST** method is allowed to be accessed anonymously. It is useful for example when you'd like to use the same handler logic for create/replace/update scenarios and create operation is allowed to be done by anonymous users. Using just **AllowAnonymous()** without any arguments means all verbs are allowed anonymous access.

---

## Other Auth Providers

All auth providers compatible with the ASP.NET middleware pipeline can be registered and used like above.

> INFO
> Here's an [example project](https://github.com/dj-nitehawk/FastEndpoints-Auth0-Demo) using [Auth0](https://auth0.com/access-management) with permission based authorization.

---

## Multiple Authentication Schemes

Multiple schemes can be configured as you'd typically do in the ASP.NET middleware pipeline and specify per endpoint which schemes are to be used for authenticating incoming requests.

```csharp
bld.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options => options.SlidingExpiration = true) // cookie auth
.AddJwtBearer(options =>                                // jwt bearer auth
{
    options.Authority = $"https://{bld.Configuration["Auth0:Domain"]}/";
    options.Audience = bld.Configuration["Auth0:Audience"];
});
```

```csharp
public override void Configure()
{
    Get("/account/profile");
    AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
}
```

In the above example, both **Cookie** and **JWT Bearer** schemes are being registered. However, the endpoint specifies **only JWT Bearer** scheme should be used for authenticating incoming requests. Multiple schemes can be specified in ths manner. If an incoming request isn't using any of the said schemes, access will be denied.

## The Default Authentication Scheme

When using the provided convenience methods such as **AddAuthenticationJwtBearer()** and **AddAuthenticationCookie()** together, whichever scheme that is registered last becomes the default scheme. For example, if JWT was registered first and Cookie last, then Cookie auth becomes the default. If you'd like to be explicit about what the default scheme should be, you can do so like below:

```csharp
bld.Services.AddAuthenticationJwtBearer(...);
bld.Services.AddAuthenticationCookie(...);
bld.Services.AddAuthentication(o => //must be the last auth call
{
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
});
```

#### Default scheme with ASP.NET Identity

Explicitly setting the default auth scheme as above is essential when using [Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-7.0&tabs=visual-studio) as well as [customizing Identity models](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-7.0) because the **Add\*Identity<>()** call makes **Cookie** auth the default scheme. This may not be the desired effect when your application also registers **JWT** auth and you'd expect JWT to be the default. In which case, simply register your auth pipeline as follows:

```csharp
bld.Services.AddIdentity<MyUser,MyRole>(...);
bld.Services.AddAuthenticationJwtBearer(...);
bld.Services.AddAuthentication(o => 
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; );
```

[See here](https://github.com/FastEndpoints/Jwt-And-Identity-Auth-Demo) for a demo project using the default identity together with JWT and making JWT the default scheme.

## Combined Authentication Scheme

Here's an example of how you'd create a custom combined auth scheme which would combine both Cookie and JWT auth when using the wrapper methods offered by FastEndpoints:

```csharp
bld.Services
   .AddAuthenticationCookie(validFor: TimeSpan.FromMinutes(60))
   .AddAuthenticationJwtBearer(s => s.SigningKey = "Token signing key")
   .AddAuthentication(o =>
   {
       o.DefaultScheme = "Jwt_Or_Cookie";
       o.DefaultAuthenticateScheme = "Jwt_Or_Cookie";
   })
   .AddPolicyScheme("Jwt_Or_Cookie", "Jwt_Or_Cookie", o =>
   {
       o.ForwardDefaultSelector = ctx =>
       {
           if (ctx.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeader) &&
               authHeader.FirstOrDefault()?.StartsWith("Bearer ") is true)
           {
               return JwtBearerDefaults.AuthenticationScheme;
           }
           return CookieAuthenticationDefaults.AuthenticationScheme;
       };
   });
```

## Custom Authentication Schemes

Creating and using custom authentication schemes is no different to how you'd typically configure them in ASP.Net using an [IAuthenticationHandler](https://source.dot.net/#Microsoft.AspNetCore.Authentication.Abstractions/IAuthenticationHandler.cs) implementation. See below links for examples:

*   [ApiKey authentication with Swagger UI support](https://gist.github.com/dj-nitehawk/4efe5ef70f813aec2c55fff3bbb833c0)
*   [Session based auth with Swagger UI support](https://gist.github.com/dj-nitehawk/ef60db792a56afc23537238e79257d13)

---

## JWT Refresh Tokens

Implementing refresh tokens in FastEndpoints is a simple 2-step process.

### Step 1 - Login Endpoint:

Create a user login endpoint which checks the supplied user credentials such as username/password and issues an initial pair of access & refresh tokens.

```csharp
public class LoginEndpoint : EndpointWithoutRequest<TokenResponse>
{
    public override void Configure()
    {
        Get("/api/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        //user credential checking has been omitted for brevity

        Response = await CreateTokenWith<MyTokenService>("user-id-001", u =>
        {
            u.Roles.AddRange(new[] { "Admin", "Manager" });
            u.Permissions.Add("Update_Something");
            u.Claims.Add(new("UserId", "user-id-001"));
        });
    }
}
```

The interesting bits of info here would be the following:

*   **CreateTokenWith<TTokenService>()**: This is a method supplied by the endpoint base class which can be used to generate the initial response dto containing the access/refresh token pair. The token service is discussed below. The parameters of the method would be the user-id and an action for configuring which user privileges (roles/claims/permissions) are to be embedded in the generated access token.
*   **MyTokenService**: This is your implementation of a specialized abstract endpoint class which is configured with the relevant settings such as singing key/ audience/ issuer/ expiry times/ etc. See example below.
*   **TokenResponse**: This is the response dto that the token service will return when token generation succeeds.

### Step 2 - Token Service:

A token service is created by implementing the **RefreshTokenService<TRequest, TResponse>** abstract class. This class is a bit different from the typical endpoint classes that it is configured by calling **Setup()** in the constructor as shown below. Also, the request and response dto generic arguments are constrained to **TokenRequest** & **TokenResponse** even though you are free to subclass those types if you need to add more properties. In addition to the endpoint setup, you need to implement 3 abstract methods as explained below. There is no **HandleAsync()** method like in a regular endpoint.

```csharp
public class MyTokenService : RefreshTokenService<TokenRequest, TokenResponse>
{
    public MyTokenService(IConfiguration config)
    {
        Setup(o =>
        {
            o.TokenSigningKey = config["TokenSigningKey"];
            o.AccessTokenValidity = TimeSpan.FromMinutes(5);
            o.RefreshTokenValidity = TimeSpan.FromHours(4);

            o.Endpoint("/api/refresh-token", ep =>
            {
                ep.Summary(s => s.Summary = "this is the refresh token endpoint");
            });
        });
    }

    public override async Task PersistTokenAsync(TokenResponse response)
    {
        await Data.StoreToken(response);

        // this method will be called whenever a new access/refresh token pair is being generated.
        // store the tokens and expiry dates however you wish for the purpose of verifying
        // future refresh requests.        
    }

    public override async Task RefreshRequestValidationAsync(TokenRequest req)
    {
        if (!await Data.TokenIsValid(req.UserId, req.RefreshToken))
            AddError(r => r.RefreshToken, "Refresh token is invalid!");

        // validate the incoming refresh request by checking the token and expiry against the
        // previously stored data. if the token is not valid and a new token pair should
        // not be created, simply add validation errors using the AddError() method.
        // the failures you add will be sent to the requesting client. if no failures are added,
        // validation passes and a new token pair will be created and sent to the client.        
    }

    public override Task SetRenewalPrivilegesAsync(TokenRequest request, UserPrivileges privileges)
    {
        privileges.Roles.Add("Manager");
        privileges.Claims.Add(new("ManagerID", request.UserId));
        privileges.Permissions.Add("Manage_Department");

        // specify the user privileges to be embedded in the jwt when a refresh request is
        // received and validation has passed. this only applies to renewal/refresh requests
        // received to the refresh endpoint and not the initial jwt creation.        
    }
}
```

> INFO
> Here's an [example project](https://github.com/FastEndpoints/Refresh-Tokens-Demo) showcasing refresh token usage.

> TIP
> There are a couple of [optional hooks](https://github.com/FastEndpoints/FastEndpoints/blob/5afe7db3628e08fc4515af17701410b4a35f182b/Src/Security/RefreshTokens/RefreshTokenService.cs#L55-L91) that can be tapped in to if you'd like to modify Jwt token creation parameters per request, and also modify the token response per request before it's sent to the client. Per request token creation parameter modification may be useful when allowing the client to decide the validity of tokens.

---

## JWT Token Revocation

Token revocation can be easily implemented with the provided abstract middleware class. Override the **JwtTokenIsValidAsync()** method and return false if the supplied token is no longer valid after checking it against a database or cache of revoked tokens.

```csharp
public class MyBlacklistChecker(RequestDelegate next) : JwtRevocationMiddleware(next)
{
    protected override Task<bool> JwtTokenIsValidAsync(string jwtToken, CancellationToken ct)
    { 
        //return true if the supplied token is still valid
    }
}
```

Register your implementation before any auth related middleware:

```csharp
app.UseJwtRevocation<MyBlacklistChecker>() //must come before auth middleware
   .UseAuthentication()
   .UseAuthorization();
```

The default response can be overridden like so:

```csharp
protected override async Task SendTokenRevokedResponseAsync(HttpContext ctx)
{
    await ctx.Response.SendStringAsync("This token is revoked!", 401);
}
```

---

## Source Generated Access Control Lists

In a typical application that uses permission based authorization, you'd be creating either an enum or static class such as the following to define all of the different permissions the application has:

```csharp
public static class Allow
{
    public const string Article_Create = "001";
    public const string Article_Approve = "002";
    public const string Article_Reject = "003";
}
```

You'd then use this class to specify permission requirements for endpoints like this:

```csharp
public override void Configure()
{
    Post("/article");
    Permissions(Allow.Article_Create);
}
```

And, in order to assign this permission to an author upon login, you'