```yaml
---
title: "How To Implement API Key Authentication In ASP.NET Core in .NET 9 | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/how-to-implement-api-key-authentication-in-asp-net-core-in-net-9-d193be7e8b34
date_published: 2025-08-03T17:01:45.725Z
date_captured: 2025-08-12T11:15:56.227Z
domain: medium.com
author: Michael Maurice
category: security
technologies: [ASP.NET Core, .NET 9, SQL Server, Entity Framework Core, MemoryCache, DistributedCache, OpenAPI, Swagger, xUnit, Moq, HttpClient, WebApplicationFactory]
programming_languages: [C#, SQL]
tags: [api-key, authentication, aspnet-core, dotnet, security, middleware, authorization, web-api, caching, rate-limiting]
key_concepts: [api-key-authentication, custom-authentication-handler, middleware-pattern, endpoint-filters, action-filters, dependency-injection, rate-limiting, claims-based-authorization]
code_examples: false
difficulty_level: intermediate
summary: |
  This comprehensive guide demonstrates multiple approaches to implementing API key authentication in ASP.NET Core 9. It covers integrating a custom authentication handler for robust security, utilizing middleware for global protection, applying endpoint filters for minimal APIs, and employing action filters for controller-based APIs. The article provides detailed code examples for each method, alongside services for API key validation, data persistence, and configuration. Furthermore, it delves into advanced features such as rate limiting, API key management, OpenAPI/Swagger integration, and testing strategies, emphasizing crucial security and performance best practices.
---
```

# How To Implement API Key Authentication In ASP.NET Core in .NET 9 | by Michael Maurice | Aug, 2025 | Medium

# How To Implement API Key Authentication In ASP.NET Core in .NET 9

API key authentication is one of the most straightforward yet effective ways to secure your APIs in .NET 9. Unlike complex authentication schemes like OAuth or JWT, API keys provide a simple mechanism for validating client requests while maintaining good security practices. This comprehensive guide will show you multiple approaches to implementing API key authentication in ASP.NET Core 9, from basic middleware solutions to fully integrated authentication handlers.

## Understanding API Key Authentication

API key authentication is a simple security mechanism where clients include a unique identifier (the API key) with each request to prove they’re authorized to access your API endpoints. The server validates this key against a known list of valid keys before processing the request.

## Why Use API Keys in .NET 9?

*   **Simplicity**: Easy to implement and understand, requiring minimal setup compared to OAuth or JWT
*   **Stateless**: No server-side sessions or complex token management required
*   **Performance**: Fast validation with minimal computational overhead
*   **Platform Agnostic**: Works across all platforms and programming languages
*   **Perfect for APIs**: Ideal for service-to-service communication and third-party integrations
*   **Control**: Easy to revoke, rotate, or limit access on a per-client basis

![API Key Auth Flow - .NET 9](https://miro.medium.com/v2/resize:fit:700/1*19AeorDdcoxrmeuz9JtSxw.png)
*Image: A Sankey diagram illustrating the API Key Authentication Flow in .NET 9. It shows an API Client sending an HTTP Request with a Key to ASP.NET Core. The request then passes through Authentication Middleware, which interacts with a Key Validation Service. The Key Validation Service checks against a DB/Cache and a Key Repository. If the key is valid, the request proceeds to API Access; otherwise, it results in a 401 Unauthorized response.*

## Method 1: Custom Authentication Handler (Recommended)

The most robust approach is implementing a custom authentication handler that integrates seamlessly with ASP.NET Core’s authentication system.

### Step 1: Create the Authentication Options

```csharp
namespace ECommerceApi.Authentication;  
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions  
{  
    public const string DefaultScheme = "ApiKey";  
    public string ApiKeyHeaderName { get; set; } = "X-API-Key";  
    public string Scheme => DefaultScheme;  
}  
public static class ApiKeyAuthenticationDefaults  
{  
    public const string AuthenticationScheme = "ApiKey";  
}
```

### Step 2: Implement the Authentication Handler

```csharp
namespace ECommerceApi.Authentication;  
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>  
{  
    private readonly IApiKeyValidationService _apiKeyValidationService;  
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;  
    public ApiKeyAuthenticationHandler(        IOptionsMonitor<ApiKeyAuthenticationOptions> options,  
        ILoggerFactory logger,  
        UrlEncoder encoder,  
        IApiKeyValidationService apiKeyValidationService)  
        : base(options, logger, encoder)  
    {  
        _apiKeyValidationService = apiKeyValidationService;  
        _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();  
    }  
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()  
    {  
        // Check if the API key header is present  
        if (!Request.Headers.ContainsKey(Options.ApiKeyHeaderName))  
        {  
            return AuthenticateResult.NoResult();  
        }  
        var apiKey = Request.Headers[Options.ApiKeyHeaderName].FirstOrDefault();  
        if (string.IsNullOrWhiteSpace(apiKey))  
        {  
            return AuthenticateResult.Fail("API key is missing");  
        }  
        try  
        {  
            var validationResult = await _apiKeyValidationService.ValidateApiKeyAsync(apiKey);  
            if (!validationResult.IsValid)  
            {  
                _logger.LogWarning("Invalid API key attempted: {ApiKey}", apiKey[..8] + "...");  
                return AuthenticateResult.Fail("Invalid API key");  
            }  
            // Create claims for the authenticated API key  
            var claims = new List<Claim>  
            {  
                new(ClaimTypes.Name, validationResult.ClientName),  
                new(ClaimTypes.NameIdentifier, validationResult.ClientId),  
                new("ApiKeyId", validationResult.ApiKeyId),  
                new("ClientType", validationResult.ClientType.ToString())  
            };  
            // Add role claims if present  
            if (validationResult.Roles?.Any() == true)  
            {  
                claims.AddRange(validationResult.Roles.Select(role =>   
                    new Claim(ClaimTypes.Role, role)));  
            }  
            // Add permission claims  
            if (validationResult.Permissions?.Any() == true)  
            {  
                claims.AddRange(validationResult.Permissions.Select(permission =>   
                    new Claim("permission", permission)));  
            }  
            var identity = new ClaimsIdentity(claims, Scheme.Name);  
            var principal = new ClaimsPrincipal(identity);  
            var ticket = new AuthenticationTicket(principal, Scheme.Name);  
            _logger.LogInformation("API key authentication successful for client: {ClientName}",   
                validationResult.ClientName);  
            return AuthenticateResult.Success(ticket);  
        }  
        catch (Exception ex)  
        {  
            _logger.LogError(ex, "Error occurred during API key validation");  
            return AuthenticateResult.Fail("Authentication error occurred");  
        }  
    }  
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)  
    {  
        Response.StatusCode = StatusCodes.Status401Unauthorized;  
        Response.Headers.Append("WWW-Authenticate", $"{Options.Scheme} realm=\"API Key Required\"");  
        return Task.CompletedTask;  
    }  
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)  
    {  
        Response.StatusCode = StatusCodes.Status403Forbidden;  
        return Task.CompletedTask;  
    }  
}
```

### Step 3: Create the API Key Validation Service

```csharp
namespace ECommerceApi.Services;  
public interface IApiKeyValidationService  
{  
    Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey);  
}  
public class ApiKeyValidationResult  
{  
    public bool IsValid { get; set; }  
    public string ClientId { get; set; } = string.Empty;  
    public string ClientName { get; set; } = string.Empty;  
    public string ApiKeyId { get; set; } = string.Empty;  
    public ClientType ClientType { get; set; }  
    public List<string>? Roles { get; set; }  
    public List<string>? Permissions { get; set; }  
    public DateTime? ExpiresAt { get; set; }  
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;  
}  
public enum ClientType  
{  
    Internal,  
    Partner,  
    Public,  
    Admin  
}  
public class ApiKeyValidationService : IApiKeyValidationService  
{  
    private readonly IConfiguration _configuration;  
    private readonly IApiKeyRepository _apiKeyRepository;  
    private readonly IMemoryCache _cache;  
    private readonly ILogger<ApiKeyValidationService> _logger;  
    public ApiKeyValidationService(        IConfiguration configuration,  
        IApiKeyRepository apiKeyRepository,  
        IMemoryCache cache,  
        ILogger<ApiKeyValidationService> logger)  
    {  
        _configuration = configuration;  
        _apiKeyRepository = apiKeyRepository;  
        _cache = cache;  
        _logger = logger;  
    }  
    public async Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey)  
    {  
        if (string.IsNullOrWhiteSpace(apiKey))  
        {  
            return new ApiKeyValidationResult { IsValid = false };  
        }  
        // Check cache first for performance  
        var cacheKey = $"apikey_{apiKey.GetHashCode()}";  
        if (_cache.TryGetValue(cacheKey, out ApiKeyValidationResult? cachedResult))  
        {  
            if (cachedResult!.IsExpired)  
            {  
                _cache.Remove(cacheKey);  
            }  
            else  
            {  
                return cachedResult;  
            }  
        }  
        // Validate against database  
        var apiKeyInfo = await _apiKeyRepository.GetApiKeyInfoAsync(apiKey);  
          
        if (apiKeyInfo == null)  
        {  
            return new ApiKeyValidationResult { IsValid = false };  
        }  
        if (apiKeyInfo.IsRevoked || apiKeyInfo.IsExpired)  
        {  
            return new ApiKeyValidationResult { IsValid = false };  
        }  
        var result = new ApiKeyValidationResult  
        {  
            IsValid = true,  
            ClientId = apiKeyInfo.ClientId,  
            ClientName = apiKeyInfo.ClientName,  
            ApiKeyId = apiKeyInfo.Id,  
            ClientType = apiKeyInfo.ClientType,  
            Roles = apiKeyInfo.Roles,  
            Permissions = apiKeyInfo.Permissions,  
            ExpiresAt = apiKeyInfo.ExpiresAt  
        };  
        // Cache the result for 5 minutes  
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));  
        // Update last used timestamp (fire and forget)  
        _ = Task.Run(async () =>   
        {  
            try  
            {  
                await _apiKeyRepository.UpdateLastUsedAsync(apiKeyInfo.Id, DateTime.UtcNow);  
            }  
            catch (Exception ex)  
            {  
                _logger.LogWarning(ex, "Failed to update last used timestamp for API key {ApiKeyId}",   
                    apiKeyInfo.Id);  
            }  
        });  
        return result;  
    }  
}
```

### Step 4: Create the API Key Repository

```csharp
namespace ECommerceApi.Data;  
public interface IApiKeyRepository  
{  
    Task<ApiKeyInfo?> GetApiKeyInfoAsync(string apiKey);  
    Task UpdateLastUsedAsync(string apiKeyId, DateTime lastUsed);  
    Task<List<ApiKeyInfo>> GetApiKeysForClientAsync(string clientId);  
    Task<string> CreateApiKeyAsync(CreateApiKeyRequest request);  
    Task RevokeApiKeyAsync(string apiKeyId);  
}  
public class ApiKeyInfo  
{  
    public string Id { get; set; } = string.Empty;  
    public string HashedKey { get; set; } = string.Empty;  
    public string ClientId { get; set; } = string.Empty;  
    public string ClientName { get; set; } = string.Empty;  
    public ClientType ClientType { get; set; }  
    public List<string> Roles { get; set; } = new();  
    public List<string> Permissions { get; set; } = new();  
    public DateTime CreatedAt { get; set; }  
    public DateTime? ExpiresAt { get; set; }  
    public DateTime? LastUsedAt { get; set; }  
    public bool IsRevoked { get; set; }  
    public string Description { get; set; } = string.Empty;  
      
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;  
}  
public class CreateApiKeyRequest  
{  
    public string ClientName { get; set; } = string.Empty;  
    public ClientType ClientType { get; set; }  
    public List<string> Roles { get; set; } = new();  
    public List<string> Permissions { get; set; } = new();  
    public DateTime? ExpiresAt { get; set; }  
    public string Description { get; set; } = string.Empty;  
}  
public class ApiKeyRepository : IApiKeyRepository  
{  
    private readonly ECommerceDbContext _context;  
    private readonly ILogger<ApiKeyRepository> _logger;  
    public ApiKeyRepository(ECommerceDbContext context, ILogger<ApiKeyRepository> logger)  
    {  
        _context = context;  
        _logger = logger;  
    }  
    public async Task<ApiKeyInfo?> GetApiKeyInfoAsync(string apiKey)  
    {  
        // Hash the API key for comparison (store hashed keys in database)  
        var hashedKey = HashApiKey(apiKey);  
        var apiKeyEntity = await _context.ApiKeys  
            .Where(k => k.HashedKey == hashedKey && !k.IsRevoked)  
            .Select(k => new ApiKeyInfo  
            {  
                Id = k.Id,  
                HashedKey = k.HashedKey,  
                ClientId = k.ClientId,  
                ClientName = k.ClientName,  
                ClientType = k.ClientType,  
                Roles = k.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),  
                Permissions = k.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),  
                CreatedAt = k.CreatedAt,  
                ExpiresAt = k.ExpiresAt,  
                LastUsedAt = k.LastUsedAt,  
                IsRevoked = k.IsRevoked,  
                Description = k.Description  
            })  
            .FirstOrDefaultAsync();  
        return apiKeyEntity;  
    }  
    public async Task UpdateLastUsedAsync(string apiKeyId, DateTime lastUsed)  
    {  
        await _context.ApiKeys  
            .Where(k => k.Id == apiKeyId)  
            .ExecuteUpdateAsync(setters => setters  
                .SetProperty(k => k.LastUsedAt, lastUsed));  
    }  
    public async Task<string> CreateApiKeyAsync(CreateApiKeyRequest request)  
    {  
        var apiKey = GenerateApiKey();  
        var hashedKey = HashApiKey(apiKey);  
        var entity = new ApiKeyEntity  
        {  
            Id = Guid.NewGuid().ToString(),  
            HashedKey = hashedKey,  
            ClientId = Guid.NewGuid().ToString(),  
            ClientName = request.ClientName,  
            ClientType = request.ClientType,  
            Roles = string.Join(",", request.Roles),  
            Permissions = string.Join(",", request.Permissions),  
            CreatedAt = DateTime.UtcNow,  
            ExpiresAt = request.ExpiresAt,  
            Description = request.Description,  
            IsRevoked = false  
        };  
        _context.ApiKeys.Add(entity);  
        await _context.SaveChangesAsync();  
        _logger.LogInformation("Created new API key for client: {ClientName}", request.ClientName);  
        return apiKey; // Return the plain API key (only time it's available)  
    }  
    public async Task RevokeApiKeyAsync(string apiKeyId)  
    {  
        await _context.ApiKeys  
            .Where(k => k.Id == apiKeyId)  
            .ExecuteUpdateAsync(setters => setters  
                .SetProperty(k => k.IsRevoked, true)  
                .SetProperty(k => k.RevokedAt, DateTime.UtcNow));  
    }  
    public async Task<List<ApiKeyInfo>> GetApiKeysForClientAsync(string clientId)  
    {  
        return await _context.ApiKeys  
            .Where(k => k.ClientId == clientId)  
            .Select(k => new ApiKeyInfo  
            {  
                Id = k.Id,  
                ClientId = k.ClientId,  
                ClientName = k.ClientName,  
                ClientType = k.ClientType,  
                CreatedAt = k.CreatedAt,  
                ExpiresAt = k.ExpiresAt,  
                LastUsedAt = k.LastUsedAt,  
                IsRevoked = k.IsRevoked,  
                Description = k.Description  
                // Don't return the hashed key  
            })  
            .ToListAsync();  
    }  
    private static string GenerateApiKey()  
    {  
        // Generate a cryptographically secure random API key  
        using var rng = RandomNumberGenerator.Create();  
        var bytes = new byte[32];  
        rng.GetBytes(bytes);  
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');  
    }  
    private static string HashApiKey(string apiKey)  
    {  
        // Use a secure hashing algorithm  
        using var sha256 = SHA256.Create();  
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));  
        return Convert.ToBase64String(hashedBytes);  
    }  
}
```

### Step 5: Configure Services and Authentication

```csharp
// Program.cs  
using ECommerceApi.Authentication;  
using ECommerceApi.Services;  
using ECommerceApi.Data;  
var builder = WebApplication.CreateSlimBuilder(args);  
// Add services  
builder.Services.AddDbContext<ECommerceDbContext>(options =>  
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));  
// Register API key services  
builder.Services.AddScoped<IApiKeyValidationService, ApiKeyValidationService>();  
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();  
builder.Services.AddMemoryCache();  
// Configure authentication  
builder.Services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)  
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(  
        ApiKeyAuthenticationDefaults.AuthenticationScheme,   
        options =>   
        {  
            options.ApiKeyHeaderName = "X-API-Key";  
        });  
builder.Services.AddAuthorization(options =>  
{  
    options.AddPolicy("RequireApiKey", policy =>  
        policy.RequireAuthenticatedUser()  
              .AddAuthenticationSchemes(ApiKeyAuthenticationDefaults.AuthenticationScheme));  
      
    options.AddPolicy("AdminOnly", policy =>  
        policy.RequireAuthenticatedUser()  
              .RequireRole("Admin")  
              .AddAuthenticationSchemes(ApiKeyAuthenticationDefaults.AuthenticationScheme));  
});  
builder.Services.AddControllers();  
builder.Services.AddOpenApi();  
var app = builder.Build();  
// Configure pipeline  
if (app.Environment.IsDevelopment())  
{  
    app.MapOpenApi();  
    app.UseSwaggerUI();  
}  
app.UseAuthentication();  
app.UseAuthorization();  
app.MapControllers();  
app.Run();
```

## Method 2: Middleware Approach

For simpler scenarios or when you need global API key protection, a middleware approach works well.

### Global API Key Middleware

```csharp
namespace ECommerceApi.Middleware;  
public class ApiKeyMiddleware  
{  
    private readonly RequestDelegate _next;  
    private readonly IApiKeyValidationService _validationService;  
    private readonly ILogger<ApiKeyMiddleware> _logger;  
    private readonly ApiKeyMiddlewareOptions _options;  
    public ApiKeyMiddleware(        RequestDelegate next,  
        IApiKeyValidationService validationService,  
        ILogger<ApiKeyMiddleware> logger,  
        IOptions<ApiKeyMiddlewareOptions> options)  
    {  
        _next = next;  
        _validationService = validationService;  
        _logger = logger;  
        _options = options.Value;  
    }  
    public async Task InvokeAsync(HttpContext context)  
    {  
        // Skip API key validation for excluded paths  
        if (_options.ExcludedPaths.Any(path =>   
            context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))  
        {  
            await _next(context);  
            return;  
        }  
        // Check for API key in header  
        if (!context.Request.Headers.TryGetValue(_options.ApiKeyHeaderName, out var apiKeyHeader))  
        {  
            await WriteUnauthorizedResponse(context, "API key is required");  
            return;  
        }  
        var apiKey = apiKeyHeader.FirstOrDefault();  
        if (string.IsNullOrWhiteSpace(apiKey))  
        {  
            await WriteUnauthorizedResponse(context, "API key is required");  
            return;  
        }  
        var validationResult = await _validationService.ValidateApiKeyAsync(apiKey);  
        if (!validationResult.IsValid)  
        {  
            _logger.LogWarning("Invalid API key attempted from IP: {RemoteIpAddress}",   
                context.Connection.RemoteIpAddress);  
            await WriteUnauthorizedResponse(context, "Invalid API key");  
            return;  
        }  
        // Add client information to HttpContext for downstream use  
        context.Items["ClientId"] = validationResult.ClientId;  
        context.Items["ClientName"] = validationResult.ClientName;  
        context.Items["ClientType"] = validationResult.ClientType;  
        context.Items["ApiKeyId"] = validationResult.ApiKeyId;  
        await _next(context);  
    }  
    private static async Task WriteUnauthorizedResponse(HttpContext context, string message)  
    {  
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;  
        context.Response.ContentType = "application/json";  
        var response = new  
        {  
            error = "Unauthorized",  
            message = message,  
            timestamp = DateTime.UtcNow  
        };  
        await context.Response.WriteAsJsonAsync(response);  
    }  
}  
public class ApiKeyMiddlewareOptions  
{  
    public string ApiKeyHeaderName { get; set; } = "X-API-Key";  
    public List<string> ExcludedPaths { get; set; } = new()  
    {  
        "/health",  
        "/metrics",  
        "/openapi",  
        "/swagger"  
    };  
}  
// Extension method for easy registration  
public static class ApiKeyMiddlewareExtensions  
{  
    public static IApplicationBuilder UseApiKeyAuthentication(        this IApplicationBuilder builder,   
        Action<ApiKeyMiddlewareOptions>? configureOptions = null)  
    {  
        var options = new ApiKeyMiddlewareOptions();  
        configureOptions?.Invoke(options);  
          
        return builder.UseMiddleware<ApiKeyMiddleware>(Options.Create(options));  
    }  
}
```

## Method 3: Endpoint Filters for Minimal APIs

For .NET 9 minimal APIs, endpoint filters provide a clean way to apply API key authentication selectively.

### API Key Endpoint Filter

```csharp
namespace ECommerceApi.Filters;  
public class ApiKeyEndpointFilter : IEndpointFilter  
{  
    private readonly IApiKeyValidationService _validationService;  
    private readonly ILogger<ApiKeyEndpointFilter> _logger;  
    public ApiKeyEndpointFilter(IApiKeyValidationService validationService, ILogger<ApiKeyEndpointFilter> logger)  
    {  
        _validationService = validationService;  
        _logger = logger;  
    }  
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)  
    {  
        const string apiKeyHeader = "X-API-Key";  
          
        var httpContext = context.HttpContext;  
          
        if (!httpContext.Request.Headers.TryGetValue(apiKeyHeader, out var apiKeyHeaderValue))  
        {  
            return Results.Unauthorized();  
        }  
        var apiKey = apiKeyHeaderValue.FirstOrDefault();  
        if (string.IsNullOrWhiteSpace(apiKey))  
        {  
            return Results.Unauthorized();  
        }  
        var validationResult = await _validationService.ValidateApiKeyAsync(apiKey);  
        if (!validationResult.IsValid)  
        {  
            _logger.LogWarning("Invalid API key attempt from {RemoteIpAddress}",   
                httpContext.Connection.RemoteIpAddress);  
            return Results.Unauthorized();  
        }  
        // Store client info for use in the endpoint  
        httpContext.Items["ClientId"] = validationResult.ClientId;  
        httpContext.Items["ClientName"] = validationResult.ClientName;  
        httpContext.Items["ClientType"] = validationResult.ClientType;  
        return await next(context);  
    }  
}  
// Extension method for easy application  
public static class EndpointRouteBuilderExtensions  
{  
    public static RouteHandlerBuilder RequireApiKey(this RouteHandlerBuilder builder)  
    {  
        return builder.AddEndpointFilter<ApiKeyEndpointFilter>();  
    }  
}
```

### Using Endpoint Filters

```csharp
// Program.cs - Minimal API endpoints with API key authentication  
app.MapGet("/api/products", async (IProductService productService) =>  
{  
    var products = await productService.GetProductsAsync();  
    return Results.Ok(products);  
}).RequireApiKey();  
app.MapPost("/api/products", async (CreateProductRequest request, IProductService productService, HttpContext context) =>  
{  
    var clientId = context.Items["ClientId"]?.ToString();  
    var product = await productService.CreateProductAsync(request, clientId);  
    return Results.Created($"/api/products/{product.Id}", product);  
}).RequireApiKey();  
// Group endpoints that all require API key authentication  
var apiGroup = app.MapGroup("/api")  
    .AddEndpointFilter<ApiKeyEndpointFilter>();  
apiGroup.MapGet("/orders", async (IOrderService orderService) =>  
{  
    var orders = await orderService.GetOrdersAsync();  
    return Results.Ok(orders);  
});  
apiGroup.MapPost("/orders", async (CreateOrderRequest request, IOrderService orderService) =>  
{  
    var order = await orderService.CreateOrderAsync(request);  
    return Results.Created($"/api/orders/{order.Id}", order);  
});
```

## Method 4: Action Filters for Controllers

For controller-based APIs, action filters provide attribute-based API key authentication.

### API Key Action Filter

```csharp
namespace ECommerceApi.Filters;  
public class ApiKeyAuthorizationFilter : IAsyncAuthorizationFilter  
{  
    private readonly IApiKeyValidationService _validationService;  
    private readonly ILogger<ApiKeyAuthorizationFilter> _logger;  
    public ApiKeyAuthorizationFilter(IApiKeyValidationService validationService, ILogger<ApiKeyAuthorizationFilter> logger)  
    {  
        _validationService = validationService;  
        _logger = logger;  
    }  
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)  
    {  
        const string apiKeyHeader = "X-API-Key";  
        if (!context.HttpContext.Request.Headers.TryGetValue(apiKeyHeader, out var apiKeyHeaderValue))  
        {  
            context.Result = new UnauthorizedObjectResult(new { error = "API key is required" });  
            return;  
        }  
        var apiKey = apiKeyHeaderValue.FirstOrDefault();  
        if (string.IsNullOrWhiteSpace(apiKey))  
        {  
            context.Result = new UnauthorizedObjectResult(new { error = "API key is required" });  
            return;  
        }  
        var validationResult = await _validationService.ValidateApiKeyAsync(apiKey);  
        if (!validationResult.IsValid)  
        {  
            _logger.LogWarning("Invalid API key attempt from {RemoteIpAddress}",   
                context.HttpContext.Connection.RemoteIpAddress);  
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid API key" });  
            return;  
        }  
        // Store client information  
        context.HttpContext.Items["ClientId"] = validationResult.ClientId;  
        context.HttpContext.Items["ClientName"] = validationResult.ClientName;  
        context.HttpContext.Items["ClientType"] = validationResult.ClientType;  
    }  
}  
// Attribute for easy application  
public class RequireApiKeyAttribute : ServiceFilterAttribute  
{  
    public RequireApiKeyAttribute() : base(typeof(ApiKeyAuthorizationFilter))  
    {  
    }  
}
```

### Using Action Filters

```csharp
namespace ECommerceApi.Controllers;  
[ApiController]  
[Route("api/[controller]")]  
public class ProductsController : ControllerBase  
{  
    private readonly IProductService _productService;  
    public ProductsController(IProductService productService)  
    {  
        _productService = productService;  
    }  
    [HttpGet]  
    [RequireApiKey]  
    public async Task<ActionResult<List<ProductDto>>> GetProducts()  
    {  
        var products = await _productService.GetProductsAsync();  
        return Ok(products);  
    }  
    [HttpPost]  
    [RequireApiKey]  
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductRequest request)  
    {  
        var clientId = HttpContext.Items["ClientId"]?.ToString();  
        var product = await _productService.CreateProductAsync(request, clientId);  
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);  
    }  
    // Public endpoint - no API key required  
    [HttpGet("public")]  
    public async Task<ActionResult<List<ProductDto>>> GetPublicProducts()  
    {  
        var products