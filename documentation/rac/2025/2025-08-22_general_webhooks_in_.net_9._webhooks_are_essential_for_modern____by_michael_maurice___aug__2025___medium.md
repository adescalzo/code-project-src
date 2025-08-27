```yaml
---
title: "Webhooks in .NET 9. Webhooks are essential for modern… | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/webhooks-in-net-9-1d09092f50a0
date_published: 2025-08-22T16:56:14.326Z
date_captured: 2025-08-27T11:17:29.600Z
domain: medium.com
author: Michael Maurice
category: general
technologies: [ASP.NET Core, .NET 9, Minimal APIs, Entity Framework Core, SQL Server, Hangfire, FluentValidation, Serilog, Swagger, OpenAPI, HttpClient, CORS, GitHub, Stripe, HMAC-SHA256, ResilienceHandler, Microsoft.AspNetCore.Diagnostics.HealthChecks]
programming_languages: [C#, SQL, Bash, JSON]
tags: [webhooks, dotnet, event-driven, api, security, retry-mechanisms, minimal-apis, data-access, background-jobs, logging]
key_concepts: [event-driven-architecture, webhook-implementation, hmac-authentication, retry-mechanisms, minimal-apis, middleware, dependency-injection, database-persistence, background-job-processing]
code_examples: false
difficulty_level: intermediate
summary: |
  [This comprehensive guide details the implementation of production-ready webhooks in .NET 9, covering both sending and receiving functionalities. It emphasizes secure authentication using HMAC, robust retry mechanisms with exponential backoff, and efficient delivery. The solution leverages modern .NET 9 features like Minimal APIs, built-in resilience, Entity Framework Core for data persistence, and Hangfire for background job processing. The article provides a detailed project structure, core domain models, authentication middleware, and API endpoints for managing subscriptions and processing events. It also includes practical examples for integrating with third-party webhooks such as GitHub and Stripe, ensuring a scalable and resilient event-driven system.]
---
```

# Webhooks in .NET 9. Webhooks are essential for modern… | by Michael Maurice | Aug, 2025 | Medium

# Webhooks in .NET 9

![Architectural diagram illustrating a secure, resilient, event-driven webhook system in .NET 9. It shows event producers (OrderService) signing events, which are then processed by .NET 9 Minimal APIs with HMAC authentication middleware, a WebhookService, and a WebhookSender with resilience. Events are persisted in EF Core (Subscriptions, Deliveries) and processed by Hangfire for retries. Receivers (Generic, GitHub, Stripe) verify signatures and respond, with failed deliveries triggering exponential backoff retries. Serilog and OpenAPI are also integrated.](https://miro.medium.com/v2/resize:fit:700/1*vztA8xvJdbxuRAgdEct-9Q.png)

Webhooks are essential for modern applications, enabling real-time event-driven communication between services. This comprehensive guide covers implementing production-ready webhooks in .NET 9, including secure authentication, retry mechanisms, and advanced patterns using Minimal APIs and the latest .NET features.

## Understanding Webhooks

### What Are Webhooks?

Webhooks are automated HTTP callbacks that notify external systems when specific events occur in your application. Instead of constantly polling for updates, systems can register webhook URLs to receive real-time notifications.

Key Characteristics:

*   Event-driven: Triggered by specific application events
*   HTTP POST requests: Deliver data via standard HTTP
*   Real-time: Immediate notification when events occur
*   Asynchronous: Non-blocking communication pattern

### Webhook vs API Polling

Traditional API Polling:

```text
Client → GET /api/orders/status → Server
(Every 30 seconds, regardless of changes)
```

Webhook Pattern:

```text
Server → POST /webhook/order-status → Client
(Only when status actually changes)
```

## Setting Up .NET 9 Webhook Infrastructure

### Project Setup and Dependencies

Create a comprehensive webhook system with .NET 9:

```bash
# Create new .NET 9 project
dotnet new webapi -n WebhookSystem.NET9 -f net9.0
# Add required packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Hangfire.AspNetCore
dotnet add package FluentValidation.AspNetCore
dotnet add package Serilog.AspNetCore
```

### Enhanced Project Structure

```text
WebhookSystem.NET9/
├── Models/
│   ├── WebhookSubscription.cs
│   ├── WebhookEvent.cs
│   ├── WebhookDelivery.cs
│   └── WebhookPayload.cs
├── Services/
│   ├── IWebhookService.cs
│   ├── WebhookService.cs
│   ├── IWebhookSender.cs
│   ├── WebhookSender.cs
│   └── HmacAuthenticationService.cs
├── Data/
│   ├── WebhookDbContext.cs
│   └── Repositories/
├── Endpoints/
│   ├── WebhookEndpoints.cs
│   └── WebhookReceiverEndpoints.cs
├── Middleware/
│   ├── WebhookAuthenticationMiddleware.cs
│   └── WebhookLoggingMiddleware.cs
└── Program.cs
```

## Core Domain Models

### Webhook Domain Models

Define comprehensive models for webhook management:

```csharp
// Models/WebhookSubscription.cs
namespace WebhookSystem.NET9.Models;
public class WebhookSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? Description { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(1);
    public Dictionary<string, string> Headers { get; set; } = new();
      
    // Navigation properties
    public List<WebhookDelivery> Deliveries { get; set; } = new();
}
// Models/WebhookEvent.cs
public class WebhookEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0";
    public Dictionary<string, object> Metadata { get; set; } = new();
}
// Models/WebhookDelivery.cs
public class WebhookDelivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubscriptionId { get; set; }
    public Guid EventId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public int HttpStatusCode { get; set; }
    public string? Response { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public int AttemptNumber { get; set; } = 1;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan? ResponseTime { get; set; }
      
    // Navigation properties
    public WebhookSubscription Subscription { get; set; } = null!;
}
// Models/WebhookPayload.cs
public record WebhookPayload(    string Id,
    string Event,
    string Source,
    DateTime Timestamp,
    string Version,
    object Data,
    Dictionary<string, object>? Metadata = null);
```

### Data Context Configuration

Set up Entity Framework with .NET 9 enhancements:

```csharp
// Data/WebhookDbContext.cs
using Microsoft.EntityFrameworkCore;
using WebhookSystem.NET9.Models;
using System.Text.Json;
namespace WebhookSystem.NET9.Data;
public class WebhookDbContext : DbContext
{
    public WebhookDbContext(DbContextOptions<WebhookDbContext> options) : base(options)
    {
    }
    public DbSet<WebhookSubscription> Subscriptions { get; set; }
    public DbSet<WebhookDelivery> Deliveries { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // WebhookSubscription configuration
        modelBuilder.Entity<WebhookSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Secret).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
              
            // JSON conversion for complex properties
            entity.Property(e => e.Events)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            entity.Property(e => e.Headers)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
            entity.HasIndex(e => e.Url);
            entity.HasIndex(e => e.IsActive);
        });
        // WebhookDelivery configuration
        modelBuilder.Entity<WebhookDelivery>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Payload).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Response).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.HasOne(d => d.Subscription)
                .WithMany(s => s.Deliveries)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.AttemptedAt);
            entity.HasIndex(e => e.IsSuccessful);
        });
        base.OnModelCreating(modelBuilder);
    }
}
```

## HMAC Authentication Implementation

### Secure HMAC Authentication Service

Implement robust HMAC-based webhook authentication:

```csharp
// Services/HmacAuthenticationService.cs
using System.Security.Cryptography;
using System.Text;
namespace WebhookSystem.NET9.Services;
public interface IHmacAuthenticationService
{
    string GenerateSignature(string payload, string secret, string algorithm = "sha256");
    bool ValidateSignature(string payload, string signature, string secret, string algorithm = "sha256");
    string GenerateTimestamp();
    bool ValidateTimestamp(string timestamp, TimeSpan tolerance = default);
}
public class HmacAuthenticationService : IHmacAuthenticationService
{
    private readonly ILogger<HmacAuthenticationService> _logger;
    public HmacAuthenticationService(ILogger<HmacAuthenticationService> logger)
    {
        _logger = logger;
    }
    public string GenerateSignature(string payload, string secret, string algorithm = "sha256")
    {
        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            using var hmac = algorithm.ToLowerInvariant() switch
            {
                "sha1" => HMAC.Create("HMACSHA1"),
                "sha256" => HMAC.Create("HMACSHA256"),
                "sha512" => HMAC.Create("HMACSHA512"),
                _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
            };
            hmac!.Key = keyBytes;
            var hashBytes = hmac.ComputeHash(payloadBytes);
              
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HMAC signature");
            throw;
        }
    }
    public bool ValidateSignature(string payload, string signature, string secret, string algorithm = "sha256")
    {
        try
        {
            // Remove algorithm prefix if present (e.g., "sha256=" from GitHub)
            var cleanSignature = signature.Contains('=')   
                ? signature.Split('=')[1]   
                : signature;
            var expectedSignature = GenerateSignature(payload, secret, algorithm);
              
            // Use time-constant comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(cleanSignature),
                Encoding.UTF8.GetBytes(expectedSignature));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating HMAC signature");
            return false;
        }
    }
    public string GenerateTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
    }
    public bool ValidateTimestamp(string timestamp, TimeSpan tolerance = default)
    {
        if (tolerance == default)
            tolerance = TimeSpan.FromMinutes(5); // Default 5-minute tolerance
        try
        {
            if (!long.TryParse(timestamp, out var unixTimestamp))
                return false;
            var providedTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            var now = DateTimeOffset.UtcNow;
              
            return Math.Abs((now - providedTime).TotalMilliseconds) <= tolerance.TotalMilliseconds;
        }
        catch
        {
            return false;
        }
    }
}
```

### Webhook Authentication Middleware

Create middleware for automatic webhook authentication:

```csharp
// Middleware/WebhookAuthenticationMiddleware.cs
using Microsoft.Extensions.Primitives;
using WebhookSystem.NET9.Services;
using System.Text;
namespace WebhookSystem.NET9.Middleware;
public class WebhookAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHmacAuthenticationService _hmacService;
    private readonly ILogger<WebhookAuthenticationMiddleware> _logger;
    public WebhookAuthenticationMiddleware(        RequestDelegate next,
        IHmacAuthenticationService hmacService,
        ILogger<WebhookAuthenticationMiddleware> logger)
    {
        _next = next;
        _hmacService = hmacService;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply to webhook endpoints
        if (!context.Request.Path.StartsWithSegments("/webhooks"))
        {
            await _next(context);
            return;
        }
        // Skip authentication for GET requests (health checks, etc.)
        if (context.Request.Method == HttpMethods.Get)
        {
            await _next(context);
            return;
        }
        try
        {
            // Enable request body buffering for multiple reads
            context.Request.EnableBuffering();
            // Read the request body
            var body = await ReadRequestBodyAsync(context.Request);
            // Extract authentication headers
            var signature = ExtractSignature(context.Request.Headers);
            var timestamp = ExtractTimestamp(context.Request.Headers);
            var secret = ExtractSecret(context.Request.Headers);
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret))
            {
                _logger.LogWarning("Missing required authentication headers");
                await WriteUnauthorizedResponse(context);
                return;
            }
            // Validate timestamp if provided
            if (!string.IsNullOrEmpty(timestamp))
            {
                if (!_hmacService.ValidateTimestamp(timestamp))
                {
                    _logger.LogWarning("Invalid or expired timestamp: {Timestamp}", timestamp);
                    await WriteUnauthorizedResponse(context);
                    return;
                }
            }
            // Validate HMAC signature
            var isValid = _hmacService.ValidateSignature(body, signature, secret);
            if (!isValid)
            {
                _logger.LogWarning("Invalid HMAC signature for webhook request");
                await WriteUnauthorizedResponse(context);
                return;
            }
            // Reset body position for downstream middleware
            context.Request.Body.Position = 0;
            // Authentication successful, continue pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during webhook authentication");
            await WriteErrorResponse(context);
        }
    }
    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }
    private static string? ExtractSignature(IHeaderDictionary headers)
    {
        // Support multiple signature header formats
        return headers.TryGetValue("X-Webhook-Signature", out var signature) ? signature.ToString() :
               headers.TryGetValue("X-Hub-Signature-256", out var githubSig) ? githubSig.ToString() :
               headers.TryGetValue("Authorization", out var auth) ? auth.ToString() :
               null;
    }
    private static string? ExtractTimestamp(IHeaderDictionary headers)
    {
        return headers.TryGetValue("X-Webhook-Timestamp", out var timestamp) ? timestamp.ToString() : null;
    }
    private static string? ExtractSecret(IHeaderDictionary headers)
    {
        return headers.TryGetValue("X-API-Key", out var apiKey) ? apiKey.ToString() :
               headers.TryGetValue("X-Webhook-Secret", out var secret) ? secret.ToString() :
               null;
    }
    private static async Task WriteUnauthorizedResponse(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
          
        var response = new { error = "Unauthorized", message = "Invalid webhook authentication" };
        await context.Response.WriteAsJsonAsync(response);
    }
    private static async Task WriteErrorResponse(HttpContext context)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
          
        var response = new { error = "Internal Server Error", message = "Webhook authentication error" };
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## Webhook Service Implementation

### Comprehensive Webhook Management Service

Implement the core webhook management functionality:

```csharp
// Services/IWebhookService.cs
namespace WebhookSystem.NET9.Services;
public interface IWebhookService
{
    Task<WebhookSubscription> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<WebhookSubscription?> GetSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookSubscription>> GetSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<WebhookSubscription?> UpdateSubscriptionAsync(Guid id, UpdateSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
    Task TriggerEventAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookDelivery>> GetDeliveryHistoryAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
public record CreateSubscriptionRequest(    string Url,
    List<string> Events,
    string? Description = null,
    Dictionary<string, string>? Headers = null,
    int MaxRetries = 3,
    int RetryDelayMinutes = 1);
public record UpdateSubscriptionRequest(    string? Url = null,
    List<string>? Events = null,
    bool? IsActive = null,
    string? Description = null,
    Dictionary<string, string>? Headers = null,
    int? MaxRetries = null,
    int? RetryDelayMinutes = null);
// Services/WebhookService.cs
using Microsoft.EntityFrameworkCore;
using WebhookSystem.NET9.Data;
using WebhookSystem.NET9.Models;
using System.Security.Cryptography;
using System.Text;
public class WebhookService : IWebhookService
{
    private readonly WebhookDbContext _context;
    private readonly IWebhookSender _webhookSender;
    private readonly ILogger<WebhookService> _logger;
    public WebhookService(        WebhookDbContext context,
        IWebhookSender webhookSender,
        ILogger<WebhookService> logger)
    {
        _context = context;
        _webhookSender = webhookSender;
        _logger = logger;
    }
    public async Task<WebhookSubscription> CreateSubscriptionAsync(        CreateSubscriptionRequest request,   
        CancellationToken cancellationToken = default)
    {
        var subscription = new WebhookSubscription
        {
            Url = request.Url,
            Events = request.Events,
            Description = request.Description,
            Headers = request.Headers ?? new Dictionary<string, string>(),
            MaxRetries = request.MaxRetries,
            RetryDelay = TimeSpan.FromMinutes(request.RetryDelayMinutes),
            Secret = GenerateSecret()
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created webhook subscription {SubscriptionId} for URL {Url}",   
            subscription.Id, subscription.Url);
        return subscription;
    }
    public async Task<WebhookSubscription?> GetSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Deliveries.OrderByDescending(d => d.AttemptedAt).Take(100))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
    public async Task<IEnumerable<WebhookSubscription>> GetSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.IsActive)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<WebhookSubscription?> UpdateSubscriptionAsync(  
        Guid id,   
        UpdateSubscriptionRequest request,   
        CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions.FindAsync(new object[] { id }, cancellationToken);
        if (subscription == null)
            return null;
        if (!string.IsNullOrEmpty(request.Url))
            subscription.Url = request.Url;
        if (request.Events != null)
            subscription.Events = request.Events;
        if (request.IsActive.HasValue)
            subscription.IsActive = request.IsActive.Value;
        if (request.Description != null)
            subscription.Description = request.Description;
        if (request.Headers != null)
            subscription.Headers = request.Headers;
        if (request.MaxRetries.HasValue)
            subscription.MaxRetries = request.MaxRetries.Value;
        if (request.RetryDelayMinutes.HasValue)
            subscription.RetryDelay = TimeSpan.FromMinutes(request.RetryDelayMinutes.Value);
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated webhook subscription {SubscriptionId}", subscription.Id);
        return subscription;
    }
    public async Task<bool> DeleteSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions.FindAsync(new object[] { id }, cancellationToken);
        if (subscription == null)
            return false;
        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted webhook subscription {SubscriptionId}", id);
        return true;
    }
    public async Task TriggerEventAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _context.Subscriptions
            .Where(s => s.IsActive && s.Events.Contains(webhookEvent.EventType))
            .ToListAsync(cancellationToken);
        if (!subscriptions.Any())
        {
            _logger.LogDebug("No active subscriptions found for event type {EventType}", webhookEvent.EventType);
            return;
        }
        _logger.LogInformation("Triggering webhook event {EventType} to {Count} subscriptions",   
            webhookEvent.EventType, subscriptions.Count);
        var tasks = subscriptions.Select(subscription =>   
            _webhookSender.SendWebhookAsync(subscription, webhookEvent, cancellationToken));
        await Task.WhenAll(tasks);
    }
    public async Task<IEnumerable<WebhookDelivery>> GetDeliveryHistoryAsync(  
        Guid subscriptionId,   
        CancellationToken cancellationToken = default)
    {
        return await _context.Deliveries
            .Where(d => d.SubscriptionId == subscriptionId)
            .OrderByDescending(d => d.AttemptedAt)
            .Take(1000) // Limit to last 1000 deliveries
            .ToListAsync(cancellationToken);
    }
    private static string GenerateSecret()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
```

## Webhook Sender with Retry Logic

### Advanced Webhook Delivery Service

Implement robust webhook delivery with retry mechanisms:

```csharp
// Services/IWebhookSender.cs
namespace WebhookSystem.NET9.Services;
  
public interface IWebhookSender
{
    Task SendWebhookAsync(WebhookSubscription subscription, WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
    Task RetryFailedWebhookAsync(Guid deliveryId, CancellationToken cancellationToken = default);
}
// Services/WebhookSender.cs
using System.Net;
using System.Text;
using System.Text.Json;
using WebhookSystem.NET9.Data;
using WebhookSystem.NET9.Models;
public class WebhookSender : IWebhookSender
{
    private readonly HttpClient _httpClient