```yaml
---
title: How do I secure a .NET Web API (JWT, OAuth, Identity)?
source: https://www.c-sharpcorner.com/article/how-do-i-secure-a-net-web-api-jwt-oauth-identity/?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-261
date_published: 2025-08-07T00:00:00.000Z
date_captured: 2025-08-13T11:17:01.294Z
domain: www.c-sharpcorner.com
author: Gaurav Kumar
category: security
technologies: [.NET Web API, JWT, OAuth 2.0, ASP.NET Core Identity, ASP.NET Core, NuGet, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.Identity.EntityFrameworkCore, SQL Server, Azure Active Directory, Auth0, IdentityServer, Google, Facebook, GitHub API, C# Corner, SharpGPT, Viberondemand, Beehiiv, Facebook, Twitter, LinkedIn, Reddit, WhatsApp]
programming_languages: [C#, SQL, JavaScript, Bash]
tags: [web-api, security, authentication, authorization, jwt, oauth, aspnet-core, identity, dotnet, csharp]
key_concepts: [JWT authentication, OAuth 2.0 authorization, ASP.NET Core Identity, Authentication vs. Authorization, Token-based security, Role-based authorization, Security best practices, Stateless APIs]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to securing .NET Web APIs using a combination of JWT, OAuth 2.0, and ASP.NET Core Identity. It explains the fundamental differences between authentication and authorization and details the importance of API security in modern architectures. The content offers step-by-step instructions and C# code examples for implementing JWT authentication, integrating OAuth 2.0 with external identity providers, and configuring ASP.NET Core Identity for robust user management. Additionally, it demonstrates how to combine Identity with JWT for token-based user management and outlines crucial security best practices like HTTPS, password hashing, and token expiration. This guide is a practical resource for developers aiming to implement strong security measures in their .NET Web API applications.
---
```

# How do I secure a .NET Web API (JWT, OAuth, Identity)?

# How do I secure a .NET Web API (JWT, OAuth, Identity)?

## Why Securing Your Web API Matters?

APIs are often the backbone of modern apps, especially in microservice and cloud-based architectures. If left unsecured, your API is vulnerable to attacks like data leaks, unauthorized access, and impersonation.

That’s where JWT (JSON Web Tokens), OAuth 2.0, and ASP.NET Core Identity come into play, providing a complete toolkit to authenticate and authorize users safely.

## Overview of Key Security Concepts

| Term                  | What It Means                                      |
| :-------------------- | :------------------------------------------------- |
| Authentication        | Verifying who the user is (e.g., login)            |
| Authorization         | Controlling what the user can do (e.g., access certain endpoints) |
| JWT                   | A secure, compact token used to carry claims about the user |
| OAuth 2.0             | An open standard for delegated authorization      |
| ASP.NET Core Identity | Framework for managing users, roles, and credentials |

## 1. JWT Authentication in ASP.NET Core Web API

JWT (JSON Web Token) is the most common way to secure APIs in a stateless way.

### Step-by-Step: Add JWT Authentication

**Step 1.** Add Required NuGet Packages.

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

**Step 2.** Configure JWT Authentication in Program.cs.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "yourapp.com",
            ValidAudience = "yourapp.com",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("super_secret_key_12345"))
        };
    });

builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
```

**Step 3.** Secure Your Endpoints.

```csharp
[Authorize]
[HttpGet("secure-data")]
public IActionResult GetSecureData()
{
    return Ok("You are authorized!");
}
```

**Step 4.** Issue a JWT Token on Login.

```csharp
[HttpPost("login")]
public IActionResult Login([FromBody] LoginModel login)
{
    // Dummy check
    if (login.Username == "admin" && login.Password == "password")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login.Username)
        };
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("super_secret_key_12345"));

        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "yourapp.com",
            audience: "yourapp.com",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
    return Unauthorized();
}
```

## 2. OAuth 2.0 Authorization

OAuth 2.0 is an authorization framework used to grant access to third-party apps without sharing passwords. It’s used widely in login with Google, Facebook, Microsoft, etc.

### Common OAuth Scenarios

*   Sign in users via Google/Facebook.
*   Authorize access to external services (like GitHub API).
*   Delegate API access using access tokens.

### Add OAuth2 to Your Web API

You usually combine OAuth2 with an Identity Provider like.

*   Azure Active Directory
*   Auth0
*   IdentityServer
*   Google/Facebook

**Example:** Add Google Authentication.

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = "your-google-client-id";
    options.ClientSecret = "your-google-client-secret";
});
```

## 3. ASP.NET Core Identity

ASP.NET Core Identity is a full user management system that supports:

*   Registration
*   Login/Logout
*   Password management
*   Role-based authorization

**Install Identity**

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

**Configure Identity in Program.cs**

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
```

**Secure an Endpoint with Role**

```csharp
[Authorize(Roles = "Admin")]
[HttpGet("admin-only")]
public IActionResult GetAdminData()
{
    return Ok("You are an Admin!");
}
```

## Combine Identity + JWT for Token-Based User Management

You can extend Identity to generate JWT tokens after login instead of using cookies.

**Example:** Identity Login Returning JWT

```csharp
[HttpPost("token")]
public async Task<IActionResult> Token([FromBody] LoginModel model)
{
    var user = await userManager.FindByNameAsync(model.Username);
    if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("your_super_secret_key");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new { token = tokenHandler.WriteToken(token) });
    }
    return Unauthorized();
}
```

## Extra Security Best Practices

| Best Practice             | Why It Matters                               |
| :------------------------ | :------------------------------------------- |
| 🔐 Use HTTPS              | Encrypts traffic between client and server   |
| 🧂 Hash & salt passwords  | Prevents password leaks if DB is compromised |
| ⌛ Set token expiration    | Reduces the risk of stolen tokens            |
| 🔁 Rotate keys regularly  | Improves security hygiene                    |
| 🧾 Use claims & roles properly | Enforces fine-grained authorization          |
| 🚪 Logout (Token revocation) | Invalidate sessions when needed              |

## Summary

| Feature  | Use When...                                    |
| :------- | :--------------------------------------------- |
| JWT      | You need stateless, fast, token-based auth     |
| OAuth2   | You allow third-party logins or services       |
| Identity | You manage your user accounts and roles        |

## Conclusion

Securing your .NET Web API isn’t optional; it’s essential. Whether you use JWT for stateless APIs, OAuth2 for third-party authorization, or ASP.NET Core Identity for user management, the key is to combine them wisely based on your app’s needs.

People also reading

*   JWT vs. OAuth 2.0: Which authentication/authorization method is more suitable for different API scenarios and why?
    
    This topic explores the trade-offs between JWT and OAuth 2.0. JWT is ideal for stateless authentication, where the server doesn't need to maintain session information. It's fast and scalable, suitable for microservices and APIs where rapid verification is crucial. The token contains all the necessary information about the user and their permissions. However, JWTs require careful management of secret keys and token expiration. Key considerations include token size impacting performance and the complexity of revocation. Conversely, OAuth 2.0 excels in delegated authorization, allowing third-party applications to access resources on behalf of a user without sharing credentials. It's commonly used for 'Login with Google/Facebook' scenarios or accessing external APIs. OAuth 2.0 introduces more overhead due to the need for authorization servers and access tokens, but it provides better security and control over access rights. Discussion points could include: stateless vs. stateful authentica [Read more](/sharpGPT?msg=jwt-vs-oauth-20-which-authenticationauthorization-method-is-more-suitable-for-different-api-scenarios-and-why)
    

![Prompt Engineering Training banner](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)
![.NET logo](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/dot_net_2015_060314128.png ".NET")
![C# Corner Ebook: Coding Principles cover](https://www.c-sharpcorner.com/UploadFile/EBooks/04092024094508AM/04092024095117AMCoding-Principles-resize.png)
![AI Trainings banner with a woman using a laptop, offering courses like Generative AI for Beginners and Mastering Prompt Engineering](https://www.c-sharpcorner.com/UploadFile/Ads/13.jpg)
![Mastering Prompt Engineering banner with a robotic head, highlighting instructor-led trainings](https://www.c-sharpcorner.com/UploadFile/Ads/14.jpg)