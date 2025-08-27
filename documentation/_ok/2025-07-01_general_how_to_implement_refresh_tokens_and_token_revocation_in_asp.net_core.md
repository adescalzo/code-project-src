```yaml
---
title: How to Implement Refresh Tokens and Token Revocation in ASP.NET Core
source: https://antondevtips.com/blog/how-to-implement-refresh-tokens-and-token-revocation-in-aspnetcore
date_published: 2025-07-01T07:45:15.127Z
date_captured: 2025-08-27T13:15:16.462Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core, JWT, Entity Framework Core, Carter, MemoryCache, Redis, JwtSecurityTokenHandler, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.Identity, HTTPS, SQL Server, .NET]
programming_languages: [C#, SQL]
tags: [authentication, authorization, jwt, refresh-tokens, token-revocation, aspnetcore, security, web-api, dotnet, caching]
key_concepts: [JWT authentication, Refresh token flow, Token revocation, Stateless sessions, Secure token storage, Token rotation, ASP.NET Core Middleware, Background services]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to implementing refresh tokens and token revocation in ASP.NET Core applications. It explains the JWT authentication flow, highlighting the roles of short-lived access tokens and long-lived refresh tokens for enhanced security and user experience. The post includes detailed C# code examples for token generation, validation, and refreshing, along with mechanisms for invalidating tokens due to security events or permission changes. Furthermore, it addresses critical security considerations like secure token storage, token rotation, and detecting token theft to build a robust authentication system.]
---
```

# How to Implement Refresh Tokens and Token Revocation in ASP.NET Core

![Cover image for the article "How to Implement Refresh Tokens and Token Revocation in ASP.NET Core". It features a white code icon (</>) on a dark blue background with abstract purple shapes.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_refresh_tokens.png&w=3840&q=100)

# How to Implement Refresh Tokens and Token Revocation in ASP.NET Core

Jul 1, 2025

[Download source code](/source-code/how-to-implement-refresh-tokens-and-token-revocation-in-aspnetcore)

7 min read

### Newsletter Sponsors

Learn how to build production-ready [REST APIs](https://www.milanjovanovic.tech/pragmatic-rest-apis?affcode=1486372_j2vpyytw) using the latest ASP.NET Core features and best practices. This is by far the best and the most comprehensive REST APIs course I have seen.  
Use my affliate link and start learning how to build [REST APIs](https://www.milanjovanovic.tech/pragmatic-rest-apis?affcode=1486372_j2vpyytw).

[Sponsor my newsletter to reach 12,000+ readers](/sponsorship)

Nowadays **JWT** (JSON Web Token) authentication is the industry standard for maintaining stateless and secure user sessions.

JWTs have changed how we handle authentication in modern web applications. Unlike traditional session-based authentication that stores session data on the server, JWTs carry all necessary user information within the tokens themselves. This approach enhances scalability and performance.

However, the real challenge isn't implementing basic JWT authentication; it's managing security and user experience when tokens expire.

In today's post, we will explore:

*   What are Refresh Tokens and how they work
*   Implementing Refresh Tokens
*   Ensuring security best practices
*   Revoking Refresh Tokens to dynamically update user permissions

Let's dive in!

## What Are Refresh Tokens and How They Work?

Typically, JWT authentication involves two tokens: an **access token** and a **refresh token**.

The **access token** grants permission to access protected resources but is short-lived, often between 5 and 10 minutes.

A short lifespan reduces risk if access tokens are compromised. But if your access token lives only for a few minutes, users would have to log in over and over. This is a terrible user experience.

Here's where refresh tokens come in handy. The **refresh token** has a single purpose: obtaining a new access token when the current one expires – without forcing users to log in again. Typically, refresh tokens are long-lived, lasting days or weeks.

Here's the authentication flow using Refresh Tokens:

**1. Login:**

*   User logs in and receives an access token and a refresh token.

**2. Store both tokens:**

*   Tokens should be stored securely in HttpOnly cookies or encrypted storage.

**3. Check Token Expiry:**

*   When the access token expires or returns a 401 response, the client initiates a token refresh.

**4. Call Refresh Endpoint:**

*   The client sends a pair of access and refresh tokens to a special refresh URL over HTTPS.

**5. Server Checks:**

*   Server verifies the refresh token is valid, unexpired, and not revoked.

**6. New Tokens Issued:**

*   If it's all good, the server gives you a new pair of access and a refresh token.

**7. Update Client:**

*   The client replaces the old tokens and continues without asking the user to log in again.

Let's explore how to implement this in code.

## How to Implement Refresh Tokens

If you're new to JWT or ASP.NET Core Authentication, check out my detailed [article](https://antondevtips.com/blog/authentication-and-authorization-best-practices-in-aspnetcore/?utm_source=antondevtips&utm_medium=own&utm_campaign=newsletter) first.

In this [article](https://antondevtips.com/blog/authentication-and-authorization-best-practices-in-aspnetcore/?utm_source=antondevtips&utm_medium=own&utm_campaign=newsletter) you can get familiar with the codebase we will be expanding today.

Here's a brief overview of our authentication setup:

```csharp
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = configuration["AuthConfiguration:Issuer"],
    ValidAudience = configuration["AuthConfiguration:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(configuration["AuthConfiguration:Key"]!))
};

services.AddSingleton(tokenValidationParameters);

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = tokenValidationParameters;
});

services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireRole("Admin");
    });

    options.AddPolicy("Author", policy =>
    {
        policy.RequireRole("Author");
    });
});
```

First, let's create a `RefreshToken` entity and connect it to a user with a foreign key:

```csharp
public class RefreshToken
{
    public string Token { get; set; } = null!;

    public string JwtId { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public bool Invalidated { get; set; }

    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;
}

public void Configure(EntityTypeBuilder<RefreshToken> builder)
{
    builder.ToTable("refresh_tokens");

    builder.HasKey(e => e.Token);

    builder.Property(e => e.JwtId).IsRequired();
    builder.Property(e => e.ExpiryDate).IsRequired();
    builder.Property(e => e.Invalidated).IsRequired();
    builder.Property(e => e.UserId).IsRequired();
    builder.Property(e => e.CreatedAtUtc).IsRequired();
    builder.Property(e => e.UpdatedAtUtc);

    builder.HasOne(e => e.User)
        .WithMany()
        .HasForeignKey(e => e.UserId);
}
```

Here are our basic authentication models:

```csharp
public sealed record LoginUserRequest(string Email, string Password);
public sealed record LoginUserResponse(string Token, string RefreshToken);
```

When a user logs in, the server returns both tokens.

Here is our endpoint for refreshing tokens:

```csharp
public sealed record RefreshTokenRequest(string Token, string RefreshToken);
public sealed record RefreshTokenResponse(string Token, string RefreshToken);

public class RefreshTokenEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/refresh", Handle);
    }

    private static async Task<IResult> Handle(
        [FromBody] RefreshTokenRequest request,
        IClientAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var result = await authorizationService.RefreshTokenAsync(
            request.Token,
            request.RefreshToken,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.Problem(
                statusCode: 400,
                detail: result.Errors?[0].Message,
                title: result.Errors?[0].Code);
        }

        var response = new RefreshTokenResponse(result.Data!.Token, result.Data.RefreshToken);
        return Results.Ok(response);
    }
}
```

Let's explore the implementation of `authorizationService.RefreshTokenAsync` in details:

```csharp
public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(
    string token,
    string refreshToken,
    CancellationToken cancellationToken = default)
{
    // Validates the digital signature of the provided access token
    var validatedToken = GetPrincipalFromToken(token, _tokenValidationParameters);
    if (validatedToken is null)
    {
        return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "Invalid token");
    }

    // Extracts JWT id from the claims
    var jti = validatedToken.Claims
        .SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
    
    if (string.IsNullOrEmpty(jti))
    {
        return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken, "Invalid token");
    }

    // Verifies if refresh token exists in the database
    var storedRefreshToken = await _dbContext.RefreshTokens
        .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);
    
    if (storedRefreshToken is null)
    {
        return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken,
            "This refresh token does not exist");
    }

    // Confirms that the refresh token hasn't expired yet
    if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
    {
        return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken,
            "This refresh token has expired");
    }

    // Verifies that the refresh token hasn't been manually invalidated (revoked)
    if (storedRefreshToken.Invalidated)
    {
        return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken,
            "This refresh token has been invalidated");
    }

    // Ensures the refresh token is paired with the correct JWT by comparing the IDs
    if (storedRefreshToken.JwtId != jti)
    {
        return Result<RefreshTokenResponse>.Failure(ErrorInvalidToken,
            "This refresh token does not match this JWT");
    }

    var userId = validatedToken.Claims.FirstOrDefault(x => x.Type == "userid")?.Value;
    if (userId is null)
    {
        return Result<RefreshTokenResponse>.Failure(ErrorUserNotFound,
            "Current user is not found");
    }

    // Search for a user in the database
    var user = await _userManager.FindByIdAsync(userId);
    if (user is null)
    {
        return Result<RefreshTokenResponse>.Failure(ErrorUserNotFound,
            "Current user is not found");
    }

    // Creates a brand new pair of tokens (access token and refresh token)
    var (newToken, newRefreshToken) = await GenerateJwtAndRefreshTokenAsync(user, refreshToken);
    
    var response = new RefreshTokenResponse(newToken, newRefreshToken);
    return Result<RefreshTokenResponse>.Success();
}
```

You can use `JwtSecurityTokenHandler` to validate a digital signature of an access token:

```csharp
private static ClaimsPrincipal? GetPrincipalFromToken(
    string token,
    TokenValidationParameters parameters)
{
    var tokenHandler = new JwtSecurityTokenHandler();

    try
    {
        var tokenParameters = parameters.Clone();
        tokenParameters.ValidateLifetime = false;
        var principal = tokenHandler.ValidateToken(token, tokenParameters, out var validatedToken);
        return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
    }
    catch
    {
        return null;
    }
}

private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
    => validatedToken is JwtSecurityToken jwtSecurityToken
       && jwtSecurityToken.Header.Alg
            .Equals(SecurityAlgorithms.HmacSha256,StringComparison.InvariantCultureIgnoreCase);
```

Here is how to create an access (JWT) token:

```csharp
private async Task<(string token, string refreshToken)> GenerateJwtAndRefreshTokenAsync(
    User user,
    string? existingRefreshToken)
{
    var roles = await _userManager.GetRolesAsync(user);
    var userRole = roles.FirstOrDefault() ?? "user";

    var role = await _roleManager.FindByNameAsync(userRole);
    var roleClaims = role is not null ? await _roleManager.GetClaimsAsync(role) : [];

    var token = GenerateJwtToken(user, _authOptions.Value, userRole, roleClaims);
    var refreshToken = await GenerateRefreshTokenAsync(token, user, existingRefreshToken);

    return (token, refreshToken);
}
```

And a refresh token:

```csharp
private async Task<string> GenerateRefreshTokenAsync(
    string token,
    User user,
    string? existingRefreshToken)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var jwtToken = tokenHandler.ReadJwtToken(token);
    var jti = jwtToken.Id;

    var refreshToken = new RefreshToken
    {
        Token = Guid.NewGuid().ToString(),
        JwtId = jti,
        UserId = user.Id,
        ExpiryDate = DateTime.UtcNow.AddDays(7),
        CreatedAtUtc = DateTime.UtcNow,
    };

    if (!string.IsNullOrEmpty(existingRefreshToken))
    {
        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == existingRefreshToken);
        
        if (existingToken != null)
        {
            _dbContext.Set<RefreshToken>().Remove(existingToken);
        }
    }

    await _dbContext.AddAsync(refreshToken);
    await _dbContext.SaveChangesAsync();

    return refreshToken.Token;
}
```

Here we generate a Refresh Token that is valid for 7 days. If a user logs in before the token expires — a new token is generated (for another 7 days); otherwise, a user is logged out.

It's also important to ensure that old refresh tokens are deleted from the database — to prevent their reuse.

> You can download the full source at the end of the post

## Security Considerations for Token Refreshing

Implementing token refresh mechanisms introduces several security considerations that must be addressed to maintain a robust authentication system:

*   **Token Storage:** Store an access and a refresh token securely in HttpOnly cookies to prevent XSS attacks.
*   **Token Rotation:** Each refresh token use should invalidate the old token, limiting attackers' potential damage.
*   **Token Revocation:** Maintain a blacklist of invalidated tokens to handle logouts, password changes, and suspicious activity.
*   **Detecting Token Theft:** Implement mechanisms to detect potential token theft, such as tracking token usage patterns or implementing token binding to specific devices or IP ranges. Unusual patterns may indicate an unauthorized use of a stolen token.
*   **Scope Limitation:** Refresh tokens should have minimal privileges, limited solely to requesting new access tokens. They should not grant direct access to any protected resources or sensitive operations.

Access (JWT) tokens should be short-lived, ideally 5-10 minutes (the less - the better). While refresh tokens can live from a few days to several weeks.

For some web and mobile applications that require a user to log in only once per month — you can make refresh tokens expire after 1-2 months.

On the other hand, financial or sensitive applications might use even shorter durations for enhanced security. If the user is not active for 10-30 minutes — token is revoked and the user is logged out.

## Revoking Refresh Tokens for Dynamic Permission Update

Sometimes, you need to update permissions dynamically through token revocation.

When a role or set of claims is updated on the server - user automatically refreshes the token on the next request and receives updated permissions. The moment a user navigates or refreshes a page - he is granted new permissions and sees changes in the navigation menu.

Let's explore a `UpdateUserRoleEndpoint`:

```csharp
[Authorize(Roles = "admin")]
public class UpdateUserRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/{userId}/role", Handle)
            .RequireAuthorization("admin");
    }

    private static async Task<IResult> Handle(
        [FromRoute] string userId,
        [FromBody] UpdateUserRoleRequest request,
        IClientAuthorizationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateUserRoleAsync(userId, request.NewRole, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Errors?[0].Code == "user_not_found")
            {
                return Results.NotFound(new UpdateUserRoleResponse(false
                    result.Errors[0].Message));
            }

            return Results.BadRequest(new UpdateUserRoleResponse(false,
                result.Errors?[0].Message ?? "An error occurred"));
        }

        return Results.Ok(new UpdateUserRoleResponse(true, result.Data!));
    }
}
```

When roles or permissions change, mark the user's refresh tokens as invalidated and store them in MemoryCache or Distributed Cache (like Redis).

```csharp
// Invalidate all refresh tokens for this user
var refreshTokens = await _dbContext.RefreshTokens
    .Where(rt => rt.UserId == userId && !rt.Invalidated)
    .ToListAsync(cancellationToken);

foreach (var refreshToken in refreshTokens)
{
    refreshToken.Invalidated = true;
    refreshToken.UpdatedAtUtc = DateTime.UtcNow;

    // Add to memory cache for the middleware to check
    _memoryCache.Set(refreshToken.JwtId, RevocatedTokenType.RoleChanged);
}

await _dbContext.SaveChangesAsync(cancellationToken);
```

Every incoming request checks if the access token was revoked. If revoked, the server responds with a 401, triggering a token refresh. This updates the user's claims instantly.

```csharp
public class CheckRevocatedTokensMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _memoryCache;

    public CheckRevocatedTokensMiddleware(RequestDelegate next, IMemoryCache memoryCache)
    {
        _next = next;
        _memoryCache = memoryCache;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/login")
            || context.Request.Path.StartsWithSegments("/refresh"))
        {
            await _next(context);
            return;
        }

        var jwtId = context.User.FindFirst(JwtRegisteredClaimNames.Jti);
        var role = context.User.FindFirst(ClaimTypes.Role);
        if (jwtId is null || role is null)
        {
            await _next(context);
            return;
        }

        var revocationType = _memoryCache.Get<RevocatedTokenType?>(jwtId.Value);
        if (revocationType.HasValue)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(context);
    }
}
```

Remember to register the middleware:

```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CheckRevocatedTokensMiddleware>();

app.MapCarter();

await app.RunAsync();
```

Use a `HostedService` to load revoked tokens into MemoryCache on app startup to persist revocation across application restarts:

```csharp
public class InvalidatedTokensHostedService : IHostedService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceProvider _serviceProvider;

    public InvalidatedTokensHostedService(
        IMemoryCache memoryCache,
        IServiceProvider serviceProvider)
    {
        _memoryCache = memoryCache;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var invalidatedTokens = await dbContext.RefreshTokens
            .Where(x => x.Invalidated)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var token in invalidatedTokens)
        {
            _memoryCache.Set(token.JwtId, RevocatedTokenType.Invalidated);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

In the same way you can revoke the token and forbid further access on the next refresh attempt.

## Summary

Refresh tokens enable short-lived access tokens without repeatedly logging users. By implementing the strategies outlined in this post, your application will achieve optimal security, performance, and user experience. With careful planning, JWT refresh and revocation mechanisms can make your authentication system robust and secure.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-implement-refresh-tokens-and-token-revocation-in-aspnetcore)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-refresh-tokens-and-token-revocation-in-aspnetcore&title=How%20to%20Implement%20Refresh%20Tokens%20and%20Token%20Revocation%20in%20ASP.NET%20Core)[X](https://twitter.com/intent/tweet?text=How%20to%20Implement%20Refresh%20Tokens%20and%20Token%20Revocation%20in%20ASP.NET%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-refresh-tokens-and-token-revocation-in-aspnetcore)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-refresh-tokens-and-token-revocation-in-aspnetcore)

# Improve Your **.NET** and Architecture Skills

Join my community of **12,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 12,000+ Subscribers

Join 12,000+ developers already reading

No spam. Unsubscribe any time.