```yaml
---
title: "How To Build a Load Balancer In .NET With YARP Reverse Proxy | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/how-to-build-a-load-balancer-in-net-with-yarp-reverse-proxy-bf116933afd5
date_published: 2025-08-14T17:01:47.182Z
date_captured: 2025-08-22T11:19:50.074Z
domain: medium.com
author: Michael Maurice
category: frontend
technologies: [YARP, .NET, ASP.NET Core, Docker, Redis, Prometheus, K6, Kestrel, JWT Bearer Authentication, Microsoft.Extensions.Diagnostics.HealthChecks, HttpClientFactory, CORS, System.Text.Json]
programming_languages: [C#, JavaScript, YAML, Shell Script]
tags: [load-balancing, reverse-proxy, yarp, dotnet, aspnet-core, microservices, web-api, docker, health-checks, authentication]
key_concepts: [load-balancing-algorithms, reverse-proxy, health-checks, jwt-authentication, request-transformation, response-transformation, observability, performance-optimization, docker-compose]
code_examples: false
difficulty_level: intermediate
summary: |
  [This comprehensive guide demonstrates how to build production-ready load balancers using Microsoft's YARP (Yet Another Reverse Proxy) library in .NET applications. It covers fundamental concepts of load balancing and reverse proxies, followed by detailed instructions on YARP setup, including basic and advanced load balancing strategies, custom policies, and robust health check implementations. The article further explores securing the gateway with JWT authentication and authorization, implementing request/response transformations, and deploying the entire solution using Docker Compose. It concludes with essential insights into monitoring, performance optimization, and best practices for creating scalable and high-performance application architectures.]
---
```

# How To Build a Load Balancer In .NET With YARP Reverse Proxy | by Michael Maurice | Aug, 2025 | Medium

Member-only story

# **How To Build a Load Balancer In .NET With YARP Reverse Proxy**

[

![Michael Maurice](https://miro.medium.com/v2/resize:fill:64:64/1*Vydee41-YhCgiyTaA_dPoA.png)

](/@michaelmaurice410?source=post_page---byline--bf116933afd5---------------------------------------)

[Michael Maurice](/@michaelmaurice410?source=post_page---byline--bf116933afd5---------------------------------------)

Follow

15 min read

·

Aug 14, 2025

4

Listen

Share

More

Press enter or click to view image in full size

![Diagram illustrating how client requests are handled by a .NET YARP Reverse Proxy, which then distributes them to multiple backend WebApi services (WebApi1, WebApi2, WebApi3) running in Docker containers. The YARP proxy also integrates with features like Health Checks, JWT Authentication, Request/Response Transformations, and Session Affinity, while feeding data into a Metrics system for observability.](https://miro.medium.com/v2/resize:fit:700/1*O1sNmGcbVZIbHXh8mcKb1A.png)

**If you want the full source code, download it from this link:** [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)

Load balancing is crucial for building scalable, high-performance applications that can handle increasing traffic and provide fault tolerance. YARP (Yet Another Reverse Proxy) is Microsoft’s powerful reverse proxy library that makes implementing load balancers in .NET applications remarkably simple while offering enterprise-grade features. This comprehensive guide demonstrates how to build production-ready load balancers using YARP, from basic round-robin distribution to advanced configurations with health checks, authentication, and custom policies.

# Understanding Load Balancing and Reverse Proxies

# What is Load Balancing?

Load balancing is the process of distributing incoming requests across multiple backend servers to optimize resource utilization, maximize throughput, minimize response time, and avoid overloading any single server. A load balancer acts as a traffic director, sitting between clients and backend servers to ensure optimal distribution of workload.

# Benefits of Load Balancing

Horizontal Scalability: Add more servers to handle increased traffic rather than upgrading hardware

High Availability: If one server fails, others continue serving requests

Performance Optimization: Distribute load to prevent server overload and reduce response times

Fault Tolerance: Automatically route traffic away from unhealthy servers

Zero Downtime Deployments: Update servers individually without service interruption

# Reverse Proxy vs Load Balancer

While often used interchangeably, these serve different purposes:

Reverse Proxy: Acts as an intermediary between clients and servers, hiding backend complexity and providing features like SSL termination, caching, and request transformation

Load Balancer: Specifically focuses on distributing incoming requests across multiple backend instances

YARP combines both capabilities, functioning as a reverse proxy that can perform load balancing among other features.

# Introduction to YARP

# What is YARP?

YARP (Yet Another Reverse Proxy) is a highly customizable reverse proxy library built by Microsoft specifically for .NET applications. Unlike traditional standalone proxy servers, YARP is designed as a library that integrates directly into your ASP.NET Core applications, providing maximum flexibility and customization.

# Key YARP Features

High Performance: Built on ASP.NET Core’s high-performance networking stack

Customizability: Extensive extensibility points for custom logic and middleware

Configuration Flexibility: Support for file-based, code-based, and dynamic configuration

Load Balancing Algorithms: Multiple built-in strategies with support for custom implementations

Health Checks: Active and passive health monitoring for backend services

Request Transformation: Modify requests and responses as they flow through the proxy

Authentication & Authorization: Integrate with ASP.NET Core’s security features

# Setting Up YARP for Load Balancing

# Project Structure

Create a well-organized project structure for your load balancer:

📁 YarpLoadBalancer/  
├── 📁 API/  
│   ├── 📁 WebApi1/  
│   │   ├── Controllers/  
│   │   ├── Program.cs  
│   │   └── appsettings.json  
│   ├── 📁 WebApi2/  
│   └── 📁 WebApi3/  
├── 📁 Gateway/  
│   ├── 📁 YarpLoadBalancer/  
│   │   ├── Program.cs  
│   │   ├── appsettings.json  
│   │   ├── Middleware/  
│   │   └── Services/  
└── 📁 docker-compose.yml

# Installing YARP

Add the YARP reverse proxy package to your gateway project:

```bash
# Create the load balancer project  
dotnet new webapi -n YarpLoadBalancer  
cd YarpLoadBalancer  
# Install YARP NuGet package  
dotnet add package Yarp.ReverseProxy --version 2.3.0  
# Optional: Add health checks and monitoring  
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks  
dotnet add package Microsoft.AspNetCore.Diagnostics.HealthChecks
```

# Basic YARP Configuration

Configure YARP in your `Program.cs`:

```csharp
using Yarp.ReverseProxy;  
var builder = WebApplication.CreateBuilder(args);  
// Add YARP reverse proxy services  
builder.Services.AddReverseProxy()  
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));  
// Add health checks  
builder.Services.AddHealthChecks();  
// Add logging and monitoring  
builder.Services.AddLogging();  
var app = builder.Build();  
// Configure middleware pipeline  
app.UseRouting();  
// Map health check endpoint  
app.MapHealthChecks("/health");  
// Map YARP reverse proxy  
app.MapReverseProxy();  
app.Run();
```

# Implementing Basic Load Balancing

# Configuration-Based Load Balancing

Define your load balancer configuration in `appsettings.json`:

```json
{  
  "Logging": {  
    "LogLevel": {  
      "Default": "Information",  
      "Microsoft.AspNetCore": "Warning",  
      "Yarp": "Information"  
    }  
  },  
  "ReverseProxy": {  
    "Routes": {  
      "api-route": {  
        "ClusterId": "api-cluster",  
        "Match": {  
          "Path": "/api/{\*\*catch-all}"  
        },  
        "Transforms": [  
          {  
            "RequestHeader": "X-Load-Balancer",  
            "Set": "YARP-Gateway"  
          }  
        ]  
      },  
      "health-route": {  
        "ClusterId": "api-cluster",   
        "Match": {  
          "Path": "/health"  
        }  
      }  
    },  
    "Clusters": {  
      "api-cluster": {  
        "LoadBalancingPolicy": "RoundRobin",  
        "Destinations": {  
          "api1": {  
            "Address": "https://localhost:7001/"  
          },  
          "api2": {  
            "Address": "https://localhost:7002/"  
          },  
          "api3": {  
            "Address": "https://localhost:7003/"  
          }  
        }  
      }  
    }  
  }  
}
```

# Creating Backend API Services

Create multiple API instances to load balance between:

```csharp
// WebApi1/Program.cs  
var builder = WebApplication.CreateBuilder(args);  
builder.Services.AddControllers();  
builder.Services.AddEndpointsApiExplorer();  
builder.Services.AddSwaggerGen();  
var app = builder.Build();  
// Configure the HTTP request pipeline  
if (app.Environment.IsDevelopment())  
{  
    app.UseSwagger();  
    app.UseSwaggerUI();  
}  
app.UseHttpsRedirection();  
app.UseAuthorization();  
app.MapControllers();  
// Add identification endpoint  
app.MapGet("/api/server-info", () =>   
{  
    return Results.Ok(new   
    {   
        Server = "WebApi1",  
        Port = "7001",  
        Timestamp = DateTime.UtcNow,  
        MachineName = Environment.MachineName,  
        ProcessId = Environment.ProcessId  
    });  
});  
// Health check endpoint  
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "WebApi1" }));  
app.Run("https://localhost:7001");
```

```csharp
// WebApi2/Program.cs - Similar configuration with different port  
app.Run("https://localhost:7002");  
// WebApi3/Program.cs - Similar configuration with different port    
app.Run("https://localhost:7003");
```

# Testing Basic Load Balancing

Create a simple test to verify load balancing functionality:

```csharp
// Create a test client to verify load balancing  
public class LoadBalancerTester  
{  
    private readonly HttpClient _httpClient;  
      
    public LoadBalancerTester()  
    {  
        _httpClient = new HttpClient  
        {  
            BaseAddress = new Uri("https://localhost:5000") // Load balancer address  
        };  
    }  
      
    public async Task TestLoadBalancing(int requestCount = 10)  
    {  
        var results = new List<string>();  
          
        for (int i = 0; i < requestCount; i++)  
        {  
            try  
            {  
                var response = await _httpClient.GetAsync("/api/server-info");  
                var content = await response.Content.ReadAsStringAsync();  
                var serverInfo = System.Text.Json.JsonSerializer.Deserialize<ServerInfo>(content);  
                  
                results.Add($"Request {i + 1}: {serverInfo.Server} (Port: {serverInfo.Port})");  
                Console.WriteLine($"Request {i + 1} routed to: {serverInfo.Server}");  
                  
                await Task.Delay(100); // Small delay between requests  
            }  
            catch (Exception ex)  
            {  
                Console.WriteLine($"Request {i + 1} failed: {ex.Message}");  
            }
        }  
          
        // Analyze distribution  
        var distribution = results.GroupBy(r => r.Split(':')[36].Trim().Split(' '))  
                                 .ToDictionary(g => g.Key, g => g.Count());  
          
        Console.WriteLine("\nLoad Balancing Distribution:");  
        foreach (var kvp in distribution)  
        {  
            Console.WriteLine($"{kvp.Key}: {kvp.Value} requests ({kvp.Value * 100.0 / requestCount:F1}%)");  
        }  
    }  
      
    public record ServerInfo(string Server, string Port, DateTime Timestamp, string MachineName, int ProcessId);  
}
```

# Advanced Load Balancing Strategies

# Built-in Load Balancing Policies

YARP provides several load balancing algorithms out of the box:

```json
{  
  "ReverseProxy": {  
    "Clusters": {  
      "round-robin-cluster": {  
        "LoadBalancingPolicy": "RoundRobin",  
        "Destinations": {  
          "api1": { "Address": "https://localhost:7001/" },  
          "api2": { "Address": "https://localhost:7002/" },  
          "api3": { "Address": "https://localhost:7003/" }  
        }  
      },  
      "least-requests-cluster": {  
        "LoadBalancingPolicy": "LeastRequests",   
        "Destinations": {  
          "api1": { "Address": "https://localhost:7001/" },  
          "api2": { "Address": "https://localhost:7002/" }  
        }  
      },  
      "power-of-two-cluster": {  
        "LoadBalancingPolicy": "PowerOfTwoChoices",  
        "Destinations": {  
          "api1": { "Address": "https://localhost:7001/" },  
          "api2": { "Address": "https://localhost:7002/" },  
          "api3": { "Address": "https://localhost:7003/" }  
        }  
      },  
      "random-cluster": {  
        "LoadBalancingPolicy": "Random",  
        "Destinations": {  
          "api1": { "Address": "https://localhost:7001/" },  
          "api2": { "Address": "https://localhost:7002/" }  
        }  
      },  
      "first-alphabetical-cluster": {  
        "LoadBalancingPolicy": "FirstAlphabetical",  
        "Destinations": {  
          "primary": { "Address": "https://localhost:7001/" },  
          "secondary": { "Address": "https://localhost:7002/" }  
        }  
      }  
    }  
  }  
}
```

# Weighted Load Balancing

Implement weighted distribution for servers with different capacities:

```json
{  
  "ReverseProxy": {  
    "Clusters": {  
      "weighted-cluster": {  
        "LoadBalancingPolicy": "WeightedRoundRobin",  
        "Destinations": {  
          "powerful-server": {  
            "Address": "https://localhost:7001/",  
            "Metadata": {  
              "Weight": "70"  
            }  
          },  
          "standard-server": {  
            "Address": "https://localhost:7002/",  
            "Metadata": {  
              "Weight": "30"   
            }  
          }  
        }  
      }  
    }  
  }  
}
```

# Custom Load Balancing Policy

Create a custom load balancing policy for specialized requirements:

```csharp
using Yarp.ReverseProxy.LoadBalancing;  
using Yarp.ReverseProxy.Model;  
public class CustomLoadBalancingPolicy : ILoadBalancingPolicy  
{  
    public string Name => "CustomPolicy";  
    public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations)  
    {  
        if (availableDestinations.Count == 0)  
        {  
            return null;  
        }  
        // Custom logic: Route based on user ID in header  
        if (context.Request.Headers.TryGetValue("X-User-ID", out var userIdHeader))  
        {  
            if (int.TryParse(userIdHeader.FirstOrDefault(), out var userId))  
            {  
                // Route even user IDs to first server, odd to second server  
                var destinationIndex = userId % 2;  
                if (destinationIndex < availableDestinations.Count)  
                {  
                    return availableDestinations[destinationIndex];  
                }  
            }  
        }  
        // Fallback to round-robin  
        var atomicCounter = cluster.Model.Destinations.Count;  
        var index = (int)(Interlocked.Increment(ref atomicCounter) % availableDestinations.Count);  
        return availableDestinations[index];  
    }  
}  
// Register the custom policy  
builder.Services.AddSingleton<ILoadBalancingPolicy, CustomLoadBalancingPolicy>();
```

# Health Checks Implementation

# Active Health Checks

Configure active health monitoring to proactively check service health:

```json
{  
  "ReverseProxy": {  
    "Clusters": {  
      "monitored-cluster": {  
        "LoadBalancingPolicy": "PowerOfTwoChoices",  
        "HealthCheck": {  
          "Active": {  
            "Enabled": "true",  
            "Interval": "00:00:10",  
            "Timeout": "00:00:10",   
            "Policy": "ConsecutiveFailures",  
            "Path": "/health",  
            "Query": "?detailed=true"  
          },  
          "AvailableDestinationsPolicy": "HealthyOrPanic"  
        },  
        "Metadata": {  
          "ConsecutiveFailuresHealthPolicy.Threshold": "3"  
        },  
        "Destinations": {  
          "api1": {  
            "Address": "https://localhost:7001/",  
            "Health": "https://localhost:7001/health"  
          },  
          "api2": {  
            "Address": "https://localhost:7002/",  
            "Health": "https://localhost:7002/health"  
          },  
          "api3": {  
            "Address": "https://localhost:7003/",  
            "Health": "https://localhost:7003/health"  
          }  
        }  
      }  
    }  
  }  
}
```

# Passive Health Checks

Implement passive health monitoring based on request failures:

```json
{  
  "ReverseProxy": {  
    "Clusters": {  
      "passive-monitored-cluster": {  
        "LoadBalancingPolicy": "RoundRobin",  
        "HealthCheck": {  
          "Passive": {  
            "Enabled": "true",  
            "Policy": "TransportFailureRateHealthPolicy",  
            "ReactivationPeriod": "00:02:00"  
          }  
        },  
        "Metadata": {  
          "TransportFailureRateHealthPolicy.RateLimit": "0.5"  
        },  
        "Destinations": {  
          "api1": { "Address": "https://localhost:7001/" },  
          "api2": { "Address": "https://localhost:7002/" },  
          "api3": { "Address": "https://localhost:7003/" }  
        }  
      }  
    }  
  }  
}
```

# Custom Health Check Policy

Create custom health check logic for specific requirements:

```csharp
using Yarp.ReverseProxy.Health;  
using Yarp.ReverseProxy.Model;  
public class CustomHealthCheckPolicy : IActiveHealthCheckPolicy  
{  
    private readonly ILogger<CustomHealthCheckPolicy> _logger;  
    public string Name => "CustomHealthCheck";  
    public CustomHealthCheckPolicy(ILogger<CustomHealthCheckPolicy> logger)  
    {  
        _logger = logger;  
    }  
    public void ProbingCompleted(ClusterState cluster, IReadOnlyList<DestinationProbingResult> probingResults)  
    {  
        foreach (var result in probingResults)  
        {  
            var destination = result.Destination;  
            var response = result.Response;  
              
            // Custom health check logic  
            var isHealthy = response != null &&   
                           response.IsSuccessStatusCode &&   
                           await IsCustomHealthCheckPassed(response);  
            var newHealth = isHealthy ? DestinationHealth.Healthy : DestinationHealth.Unhealthy;  
              
            if (destination.Health.Active != newHealth)  
            {  
                _logger.LogInformation("Destination {DestinationId} health changed from {OldHealth} to {NewHealth}",  
                    destination.DestinationId, destination.Health.Active, newHealth);  
            }  
            destination.Health.Active = newHealth;  
        }  
    }  
    private async Task<bool> IsCustomHealthCheckPassed(HttpResponseMessage response)  
    {  
        try  
        {  
            var content = await response.Content.ReadAsStringAsync();  
            var healthStatus = System.Text.Json.JsonSerializer.Deserialize<HealthStatus>(content);  
              
            // Custom validation logic  
            return healthStatus?.Status?.Equals("Healthy", StringComparison.OrdinalIgnoreCase) == true &&  
                   healthStatus.Dependencies?.All(d => d.Status == "Up") == true;  
        }  
        catch (Exception)  
        {  
            return false;  
        }  
    }  
    public record HealthStatus(string Status, List<DependencyStatus> Dependencies);  
    public record DependencyStatus(string Name, string Status);  
}  
// Register custom health check policy  
builder.Services.AddSingleton<IActiveHealthCheckPolicy, CustomHealthCheckPolicy>();
```

# Authentication and Authorization

# JWT Bearer Authentication

Implement authentication at the gateway level:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;  
using Microsoft.IdentityModel.Tokens;  
using System.Text;  
var builder = WebApplication.CreateBuilder(args);  
// Add YARP  
builder.Services.AddReverseProxy()  
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));  
// Configure JWT authentication  
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)  
    .AddJwtBearer(options =>  
    {  
        options.TokenValidationParameters = new TokenValidationParameters  
        {  
            ValidateIssuer = true,  
            ValidateAudience = true,  
            ValidateLifetime = true,  
            ValidateIssuerSigningKey = true,  
            ValidIssuer = builder.Configuration["Jwt:Issuer"],  
            ValidAudience = builder.Configuration["Jwt:Audience"],  
            IssuerSigningKey = new SymmetricSecurityKey(  
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))  
        };  
    });  
// Configure authorization policies  
builder.Services.AddAuthorization(options =>  
{  
    options.AddPolicy("AdminOnly", policy =>  
        policy.RequireRole("Administrator"));  
      
    options.AddPolicy("UserOrAdmin", policy =>  
        policy.RequireRole("User", "Administrator"));  
});  
var app = builder.Build();  
// Enable authentication and authorization  
app.UseAuthentication();  
app.UseAuthorization();  
// Map authenticated reverse proxy  
app.MapReverseProxy().RequireAuthorization();  
app.Run();
```

# Route-Level Authorization

Configure different authorization policies for different routes:

```json
{  
  "ReverseProxy": {  
    "Routes": {  
      "admin-route": {  
        "ClusterId": "admin-cluster",  
        "AuthorizationPolicy": "AdminOnly",  
        "Match": {  
          "Path": "/api/admin/{\*\*catch-all}"  
        }  
      },  
      "user-route": {  
        "ClusterId": "user-cluster",   
        "AuthorizationPolicy": "UserOrAdmin",  
        "Match": {  
          "Path": "/api/user/{\*\*catch-all}"  
        }  
      },  
      "public-route": {  
        "ClusterId": "public-cluster",  
        "AuthorizationPolicy": "Anonymous",  
        "Match": {  
          "Path": "/api/public/{\*\*catch-all}"  
        }  
      }  
    },  
    "Clusters": {  
      "admin-cluster": {  
        "Destinations": {  
          "admin-api": { "Address": "https://localhost:7001/" }  
        }  
      },  
      "user-cluster": {  
        "Destinations": {  
          "user-api": { "Address": "https://localhost:7002/" }  
        }  
      },  
      "public-cluster": {  
        "Destinations": {  
          "public-api": { "Address": "https://localhost:7003/" }  
        }  
      }  
    }  
  }  
}
```

# Token Forwarding

Forward authentication tokens to backend services:

```csharp
builder.Services.AddReverseProxy()  
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))  
    .AddTransforms(context =>  
    {  
        context.AddRequestTransform(async requestContext =>  
        {  
            // Forward JWT token to backend services  
            if (requestContext.HttpContext.User.Identity?.IsAuthenticated == true)  
            {  
                var token = await requestContext.HttpContext.GetTokenAsync("access_token");  
                if (!string.IsNullOrEmpty(token))  
                {  
                    requestContext.ProxyRequest.Headers.Authorization =   
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);  
                }  
            }  
// Add user information as headers  
            if (requestContext.HttpContext.User.Identity?.IsAuthenticated == true)  
            {  
                var userId = requestContext.HttpContext.User.FindFirst("sub")?.Value;  
                var userName = requestContext.HttpContext.User.FindFirst("name")?.Value;  
                var userRole = requestContext.HttpContext.User.FindFirst("role")?.Value;  
                if (!string.IsNullOrEmpty(userId))  
                    requestContext.ProxyRequest.Headers.Add("X-User-ID", userId);  
                  
                if (!string.IsNullOrEmpty(userName))  
                    requestContext.ProxyRequest.Headers.Add("X-User-Name", userName);  
                  
                if (!string.IsNullOrEmpty(userRole))  
                    requestContext.ProxyRequest.Headers.Add("X-User-Role", userRole);  
            }  
        });  
    });
```

# Request and Response Transformations

# Built-in Transformations

Configure common request and response transformations:

```json
{  
  "ReverseProxy": {  
    "Routes": {  
      "transform-route": {  
        "ClusterId": "api-cluster",  
        "Match": {  
          "Path": "/api/{\*\*catch-all}"  
        },  
        "Transforms": [  
          {  
            "PathPattern": "/v2/api/{\*\*catch-all}"  
          },  
          {  
            "RequestHeader": "X-Forwarded-By",  
            "Set": "YARP-Gateway"  
          },  
          {  
            "RequestHeader": "X-Original-Host",  
            "Set": "{OriginalHost}"  
          },  
          {  
            "ResponseHeader": "X-Powered-By",  
            "Set": "YARP-LoadBalancer"  
          },  
          {  
            "ResponseHeader": "Server",  
            "Set": ""  
          }  
        ]  
      }  
    }  
  }  
}
```

# Custom Request Transformations

Implement custom request transformation logic:

```csharp
public class CustomRequestTransform : RequestTransform  
{  
    public override ValueTask ApplyAsync(RequestTransformContext context)  
    {  
        // Add correlation ID for request tracking  
        if (!context.ProxyRequest.Headers.Contains("X-Correlation-ID"))  
        {  
            var correlationId = Guid.NewGuid().ToString();  
            context.ProxyRequest.Headers.Add("X-Correlation-ID", correlationId);  
            context.HttpContext.Items["CorrelationId"] = correlationId;  
        }  
// Add timestamp header  
        context.ProxyRequest.Headers.Add("X-Request-Timestamp",   
            DateTimeOffset.UtcNow.ToString("O"));  
        // Modify query parameters  
        if (context.Query.ContainsKey("version"))  
        {  
            context.Query["api-version"] = context.Query["version"];  
            context.Query.Remove("version");  
        }  
        // Add client IP information  
        var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();  
        if (!string.IsNullOrEmpty(clientIp))  
        {  
            context.ProxyRequest.Headers.Add("X-Client-IP", clientIp);  
        }  
        return default;  
    }  
}  
// Register custom transform  
builder.Services.AddReverseProxy()  
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))  
    .AddTransforms(context =>  
    {  
        context.AddRequestTransform<CustomRequestTransform>();  
    });
```

# Response Transformations

Implement custom response transformation:

```csharp
public class CustomResponseTransform : ResponseTransform  
{  
    public override ValueTask ApplyAsync(ResponseTransformContext context)  
    {  
        // Add response timing header  
        if (context.HttpContext.Items.TryGetValue("RequestStartTime", out var startTimeObj) &&  
            startTimeObj is DateTimeOffset startTime)  
        {  
            var duration = DateTimeOffset.UtcNow - startTime;  
            context.HttpContext.Response.Headers.Add("X-Response-Time",   
                $"{duration.TotalMilliseconds}ms");  
        }  
// Add server identification  
        var serverHeader = context.ProxyResponse?.Headers.GetValues("X-Server-ID").FirstOrDefault();  
        if (!string.IsNullOrEmpty(serverHeader))  
        {  
            context.HttpContext.Response.Headers.Add("X-Handled-By", serverHeader);  
        }  
        // Remove sensitive headers  
        context.HttpContext.Response.Headers.Remove("X-Powered-By");  
        context.HttpContext.Response.Headers.Remove("Server");  
        return default;  
    }  
}  
// Middleware to track request timing  
public class RequestTimingMiddleware  
{  
    private readonly RequestDelegate _next;  
    public RequestTimingMiddleware(RequestDelegate next)  
    {  
        _next = next;  
    }  
    public async Task InvokeAsync(HttpContext context)  
    {  
        context.Items["RequestStartTime"] = DateTimeOffset.UtcNow;  
        await _next(context);  
    }  
}  
// Register middleware and transform  
app.UseMiddleware<RequestTimingMiddleware>();  
builder.Services.AddReverseProxy()  
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))  
    .AddTransforms(context =>  
    {  
        context.AddResponseTransform<CustomResponseTransform>();  
    });
```

# Docker Deployment

# Docker Compose Configuration