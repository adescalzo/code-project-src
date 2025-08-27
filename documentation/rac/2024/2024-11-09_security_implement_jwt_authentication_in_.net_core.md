```yaml
---
title: Implement JWT Authentication in .NET Core
source: https://www.nikolatech.net/blogs/implement-jwt-authentication-in-dotnet
date_published: 2024-11-09T17:06:29.376Z
date_captured: 2025-09-05T11:31:40.958Z
domain: www.nikolatech.net
author: Unknown
category: security
technologies: [.NET Core, ASP.NET Core, JWT, NuGet, Microsoft.AspNetCore.Authentication.JwtBearer, JsonWebTokenHandler, Swagger, Minimal APIs, IOptions, IConfiguration, SymmetricSecurityKey, HTTP/HTTPS]
programming_languages: [C#, Bash]
tags: [jwt, authentication, dotnet, aspnet-core, security, api-security, web-api, middleware, swagger, configuration]
key_concepts: [JWT authentication, token generation, token validation, authentication middleware, authorization middleware, claims-based authorization, API security, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to implementing JWT (JSON Web Token) authentication in .NET Core applications. It explains the structure and common flow of JWTs, highlighting their role in securing stateless APIs. The guide covers practical steps from installing the necessary NuGet package and generating tokens using `JsonWebTokenHandler`, to configuring authentication middleware in `Program.cs`. Furthermore, it demonstrates how to secure API endpoints using `[Authorize]` attributes for controllers and `RequireAuthorization` for Minimal APIs, and how to integrate JWT support into Swagger UI.
---
```

# Implement JWT Authentication in .NET Core

![Banner: A blue and white abstract background with geometric shapes. The text "JWT AUTHENTICATION" is prominently displayed at the top, with "Secure your API in .NET" below it. A stylized "NK" logo is in the top left corner, and a colorful star-like icon is in the bottom left.](https://coekcx.github.io/BlogImages/banners/implement-jwt-authentication-in-dotnet-banner.png)

**JWT (JSON Web Token)** is an open standard for securely exchanging JSON data between parties.

Tokens are compact and trustworthy due to its digital signature, ideal for authentication in stateless APIs.

A JWT consists of three parts:

*   **Header**: Specifies the token type (JWT) and the signing algorithm (e.g., HMAC SHA256).
*   **Payload**: Contains the claims, which are statements about an entity and additional metadata. Claims can be standard (such as sub or iat) or custom for specific use cases.
*   **Signature**: Verifies the integrity of the message and confirms that it was issued by a trusted source, ensuring that it has not been tampered with.

**Common Flow**: When a user logs in, the server creates a JWT and sends it to the client. The client then attaches the JWT to its requests, enabling the server to validate the token and authorize access based on its claims.

On most projects I've worked on, we've used JWT authentication, and I'd like to share how you can implement it as well.

## Getting Started

To get started with JWT, you need to install the NuGet package. You can do this via the NuGet Package Manager or by running the following command in the Package Manager Console:

```bash
Install-Package Microsoft.AspNetCore.Authentication.JwtBearer
```

Once installed, you can start imlementing your JWT generator.

## Implementation

First, you need to define a class that will be responsible for generating the JWT. Inside the class, you will inject either IConfiguration, IOptions or another mechanism to access sensitive information.

Here is an example of what appsettings.json might look like:

```json
{
  "Jwt": {
    "Secret": "Dummy secret, must be at least 16 bytes! :)",
    "Issuer": "https://nikolatech.net",
    "Audience": "https://nikolatech.net",
    "ExpiresInMinutes": 60
  }
}
```

The method responsible for generating the token should include user details (e.g. Id, Email, etc.) as parameters, as they are needed to create the token.

To generate the token, we use a **JsonWebTokenHandler**, which will create the JWT based on our token descriptor.

The token descriptor enables us to define the claims for the token. Below are the key data elements in the descriptor:

*   **Issuer**: This is typically the URL or identity provider responsible for issuing the token.
*   **Audience**: This defines the intended recipients, typically as a URL. If token doesn’t have the expected audience, the token is invalid.
*   **SigningCredentials**: Specifies how the token is signed. It includes the security key and the signing algorithm used to secure the JWT.
*   **Expires**: Defines the expiration time of the token. After this time, the token is no longer valid. It's important to set a reasonable expiration time to reduce security risks.
*   **Subject**: This is a claims identity, holding claims related to the entity (such as a user). The "sub" claim contains the user's Id, email claim holds the user's email. You can also add custom claims, such as roles or permissions.

Here is an example of what implementation looks like:

```csharp
internal sealed class JwtGenerator : IJwtGenerator
{
    private readonly JwtOptions _jwtOptions;

    public JwtGenerator(IOptions<JwtOptions> jwtOptions) =>
        _jwtOptions = jwtOptions.Value;

    public string Generate(Guid id, string email)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, id.ToString()),
            new(JwtRegisteredClaimNames.Email, email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes)
        };

        var handler = new JsonWebTokenHandler();

        return handler.CreateToken(tokenDescriptor);
    }
}
```

The issuer, audience and expiration fields come from the settings, while the subject is created from the user details. To create the token descriptor, the only additional step is to generate the signing credentials.

To create the signing credentials, we need a security key and the desired algorithm.

Using JwtOptions, I generated the security key from my secret. Since the **SymmetricSecurityKey** requires a byte array, the secret must be encoded first.

## Service Registration

Once the JWT implementation is done, we need to set up the authentication middleware in Program.cs to use JWT.

Here’s an example of how to configure JWT authentication:

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"]
        };
    });
```

AddAuthentication registers the authentication middleware and sets up JWT authentication as the default scheme.

AddJwtBearer configures the JWT bearer token validation.

When RequireHttpsMetadata is set to true, it ensures that metadata is retrieved over HTTPS.

TokenValidationParameters are used to validate the token itself and you need to ensure that the issuer, signing key and audience match what was set in our JwtGenerator class.

You also need to add **UseAuthentication** to enable middleware that validates the incoming token based on your configuration.

**UseAuthorization** verifies if the user's claims satisfy any authorization policies defined in the application, such as required roles or permissions to access specific routes.

```csharp
app.UseAuthentication();

app.UseAuthorization();
```

**NOTE**: The order matters. **app.UseAuthentication()** must come before **app.UseAuthorization()** to ensure the authentication step completes before checking for permissions.

## Securing Endpoints

To finalize, we need to specify which controllers or endpoints require authentication.

Let's start with controllers. By using the `[Authorize]` attribute, we can mark an entire controller to require authorization for all of its endpoints.

If we place this attribute only above specific endpoints, then only those endpoints will require authorization.

If the entire controller is marked with `[Authorize]`, but we want some endpoints to be publicly accessible, we can mark those endpoints with the `[AllowAnonymous]` attribute, which overrides the authorization requirement.

Here's an example of how it looks in code:

```csharp
[Authorize]
[Route("api/[controller]")]
public sealed class UsersController : BaseController
{

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var command = request.Adapt<LoginUserRequest>();

        var response = await Sender.Send(command, cancellationToken);

        return Ok(response);
    }

    // Other endpoints will require authentication
}
```

In Minimal APIs, applying authentication and authorization is straightforward. Simply chain the RequireAuthorization method to the endpoint definition.

Here’s an example of applying RequireAuthorization to a single route:

```csharp
public sealed class DeleteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("products/{id:guid}", async (ISender sender, Guid id, CancellationToken cancellationToken) =>
        {
            var command = new DeleteProductCommand(id);

            var result = await sender.Send(command, cancellationToken);

            return result;
        }).WithTags(Tags.Products)
        .RequireAuthorization();
    }
}
```

## Swagger

Additionally, if you are using Swagger this is a helpful section.

To add JWT support in Swagger, we need to configure it to accept and handle bearer tokens in the header.

It will enable us to interact with endpoints that require authorization directly within the Swagger UI.

Here’s a breakdown of the code:

```csharp
services.AddSwaggerGen(o =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Auth",
        Description = "Place JWT token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    };

    o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            []
        }
    };

    o.AddSecurityRequirement(securityRequirement);
});
```

## Conclusion

In conclusion, implementing JWT (JSON Web Token) authentication in a .NET Core application is a powerful way to secure APIs and protect sensitive data. JWTs are compact, digitally signed tokens, making them ideal for stateless API interactions.

This approach is fully supported for both controllers and minimal APIs.

Additionally, adding authorization on top of authentication is straightforward. By adding additional context through claims in the token, we can easily implement role-based or permission-based authorization. Perhaps in the next blog, we will dive deeper into authorization.

To check out the source code and try the app, visit my example:

[Source Code](https://www.nikolatech.net/codes/jwt-authentication-examples-in-dotnet)