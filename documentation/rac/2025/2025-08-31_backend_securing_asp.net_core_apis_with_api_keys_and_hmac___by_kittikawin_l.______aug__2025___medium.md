```yaml
---
title: "Securing ASP.NET Core APIs with API Keys and HMAC | by Kittikawin L. 🍀 | Aug, 2025 | Medium"
source: https://medium.com/@kittikawin_ball/securing-asp-net-core-apis-with-api-keys-and-hmac-508665dc5e3d
date_published: 2025-08-31T15:01:38.399Z
date_captured: 2025-09-03T12:10:13.892Z
domain: medium.com
author: Kittikawin L. 🍀
category: backend
technologies: [ASP.NET Core, .NET, OAuth2, JWT, HMAC, User Secrets, Azure Key Vault, AWS Secrets Manager, Postman, curl, HTTPS]
programming_languages: [C#]
tags: [api-security, api-keys, hmac, authentication, dotnet, aspnet-core, middleware, web-api, security, cryptography]
key_concepts: [api-key-authentication, hmac-signing, middleware, replay-attack-prevention, tampering-protection, secret-management, timestamp-validation, request-buffering]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details how to secure ASP.NET Core APIs using a combination of API Keys and HMAC signing. It explains the limitations of API Keys alone and demonstrates how HMAC provides stronger protection against tampering and replay attacks. The guide provides C# code examples for implementing both API Key and HMAC validation as custom middleware. It also covers best practices for secure secret storage, key rotation, and timestamp validation, emphasizing the importance of HTTPS for production environments.]
---
```

# Securing ASP.NET Core APIs with API Keys and HMAC | by Kittikawin L. 🍀 | Aug, 2025 | Medium

![A silver key on a black background, symbolizing security and access.](https://miro.medium.com/v2/resize:fit:700/0*VBs9WS76QFNp2p2v)

Photo by [Matt Artz](https://unsplash.com/@mattartz?utm_source=medium&utm_medium=referral) on [Unsplash](https://unsplash.com/?utm_source=medium&utm_medium=referral)

Security is a fundamental requirement when building APIs. While _OAuth2_ and _JWT_ are widely adopted, there are cases where a lighter, secure alternative is more suitable. API Keys combined with _HMAC_ (Hash-based Message Authentication Code) provide a strong option for partner APIs, internal services, or situations where a single, end-to-end identity solution might be complex.

This article demonstrates how to implement API Key authentication in ASP.NET Core and strengthen it with _HMAC_ signing to protect against tampering and replay attacks.

## Why Use API Keys?

*   **Simple**: Easy to implement and maintain.
*   **Manageable**: Keys can be rotated or revoked per client.
*   **Lightweight**: Works without the complexity of _OAuth_ or _JWT_.

> **_Note_**_: Using API Keys alone for public APIs that expose a lot of data without additional protection is not recommended._

## Step 1: Implementing API Key Authentication

Create middleware to validate API keys from request headers.

```csharp
public class ApiKeyMiddleware  
{  
    private const string API_KEY_HEADER = "X-API-KEY";  
    private readonly RequestDelegate _next;  
    private readonly string _apiKey;  
      
    public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)  
    {  
        _next = next;  
        _apiKey = config["ApiKey"]!; // store securely, no need to hardcode it.  
    }  
  
    public async Task InvokeAsync(HttpContext context)  
    {  
        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey)  
            || !_apiKey.Equals(extractedApiKey))  
        {  
            context.Response.StatusCode = 401;  
            await context.Response.WriteAsync("Unauthorized");  
            return;  
        }  
        await _next(context);  
    }  
}
```

Register the middleware in **Program.cs**:

```csharp
app.UseMiddleware<ApiKeyMiddleware>();
```

At this point, clients must include the header `X-API-KEY` in their requests.

## Step 2: Adding HMAC Signing for Security

API Keys alone can be intercepted. To provide stronger protection, add **HMAC signing**, which ensures requests cannot be tampered with or resubmitted.

### HMAC Signing Workflow

1.  The client sends
    *   `X-API-KEY`
    *   `X-TIMESTAMP`
    *   `X-SIGNATURE = HMAC(secret, requestBody + timestamp)`
2.  The server verifies the signature using the shared secret.

### Middleware Example

```csharp
public class HmacMiddleware  
{  
    private readonly RequestDelegate _next;  
    private readonly string _secret;  
  
    public HmacMiddleware(RequestDelegate next, IConfiguration config)  
    {  
        _next = next;  
        _secret = config["ApiSecret"]!;  
    }  
  
    public async Task InvokeAsync(HttpContext context)  
    {  
        var request = context.Request;  
        if (!request.Headers.TryGetValue("X-API-KEY", out var apiKey) ||  
            !request.Headers.TryGetValue("X-SIGNATURE", out var signature) ||  
            !request.Headers.TryGetValue("X-TIMESTAMP", out var timestamp))  
        {  
            context.Response.StatusCode = 401;  
            await context.Response.WriteAsync("Missing authentication headers");  
            return;  
        }  
  
        // Validate timestamp (allow up to 5 minutes drift)  
        if (Math.Abs((DateTime.UtcNow - DateTime.Parse(timestamp)).TotalMinutes) > 5)  
        {  
            context.Response.StatusCode = 401;  
            await context.Response.WriteAsync("Request expired");  
            return;  
        }  
  
        // Compute HMAC  
        request.EnableBuffering();  
        using var reader = new StreamReader(request.Body, leaveOpen: true);  
        var body = await reader.ReadToEndAsync();  
        request.Body.Position = 0; // reset for next middleware  
        var payload = body + timestamp;  
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(_secret));  
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));  
        if (!hash.Equals(signature))  
        {  
            context.Response.StatusCode = 401;  
            await context.Response.WriteAsync("Invalid signature");  
            return;  
        }  
  
        await _next(context);  
    }  
}
```

## Best Practices

*   **Store secrets securely** (e.g., User Secrets, Azure Key Vault, AWS Secrets Manager).
*   **Rotate keys regularly** to minimize risk.
*   **Validate timestamps** to prevent replay attacks.
*   **Test with tools** such as _Postman_ or _curl_ to ensure correctness.

## Conclusion

API Keys provide a simple way to protect APIs, but they are even more effective when combined with **HMAC signing**. With just a little additional middleware, ASP.NET Core APIs can be protected from unauthorized access, tampering, and replay attacks.

For production use, this approach should always be paired with **HTTPS**, regular key rotation, and continuous monitoring.