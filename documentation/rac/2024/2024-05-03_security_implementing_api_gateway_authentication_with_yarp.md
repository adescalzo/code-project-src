```yaml
---
title: Implementing API Gateway Authentication With YARP
source: https://www.milanjovanovic.tech/blog/implementing-api-gateway-authentication-with-yarp
date_published: 2024-05-04T00:00:00.000Z
date_captured: 2025-09-02T22:25:56.789Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: security
technologies: [YARP, ASP.NET Core, .NET]
programming_languages: [C#]
tags: [api-gateway, authentication, authorization, yarp, dotnet, aspnet-core, microservices, reverse-proxy, security, web-api]
key_concepts: [API Gateway, Authentication, Authorization, Reverse Proxy, Microservices, Custom Authorization Policies, Middleware, Claims-based authorization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details how to implement API gateway authentication using YARP (Yet Another Reverse Proxy) for .NET applications. It explains the crucial role of API gateways in streamlining client interactions and securing microservices by acting as a central entry point. The content covers configuring authentication and authorization middleware within YARP, demonstrating how to enforce default or custom authorization policies based on user claims. By leveraging ASP.NET Core's security features, YARP provides a flexible and robust solution for granular access control at the gateway level, reducing the load on backend services.]
---
```

# Implementing API Gateway Authentication With YARP

![Cover image for the article 'Implementing API Gateway Authentication With YARP.'](/blog-covers/mnw_088.png?imwidth=3840)

# Implementing API Gateway Authentication With YARP

4 min read · May 04, 2024

API gateways provide clients with a single point of entry. This streamlines their interactions with your system and ensures the security of your microservices or distributed system.

One critical aspect of API gateways is authentication - ensuring only authorized users and applications can access your valuable data and resources.

In this newsletter, we'll explore how you can implement API gateway authentication using [YARP](https://microsoft.github.io/reverse-proxy/index.html) (Yet Another Reverse Proxy), a powerful and flexible reverse proxy library for .NET applications.

Here's what we will cover:

*   The role of API gateways
*   Configuring authentication with YARP
*   Creating custom authorization policies

Let's dive in.

## The Role of API Gateways

An [**API gateway**](implementing-an-api-gateway-for-microservices-with-yarp) is the "front door" to your backend services and APIs. It acts as an intermediary layer, handling client requests and routing them to the appropriate destinations.

The key benefits of API gateways are:

*   **Centralized access**: All incoming requests must first pass through the gateway. This simplifies management and monitoring.
*   **Service abstraction**: Clients interact only with the gateway. We can hide the complexity of the backend architecture from clients.
*   **Performance enhancement**: Implement techniques like caching and [**load balancing**](horizontally-scaling-aspnetcore-apis-with-yarp-load-balancing) to optimize API performance.
*   **Authentication and Authorization**: API gateways verify user and application identities, enforcing whether a request is allowed or not.

![Diagram illustrating the role of an API Gateway as a central entry point for clients interacting with multiple backend services.](/blogs/mnw_088/api_gateway.png?imwidth=3840)

Source: [Modular Monolith Architecture](/modular-monolith-architecture)

## Configuring Authentication With YARP

We can use the API gateway to authenticate and authorize requests before they are proxied to the destination servers. This can reduce the load on the destination servers, and introduce a layer of security. Implementing authentication on the API gateway ensures consistent policies are implemented across your applications.

If you're new to YARP, I recommend first reading about [**how to implement an API gateway with YARP**](implementing-an-api-gateway-for-microservices-with-yarp).

By default, YARP won't authenticate or authorize requests unless enabled in the route or application configuration.

We can start by introducing [authentication](https://docs.microsoft.com/aspnet/core/security/authentication/) and [authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction) middleware:

```csharp
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
```

This allows us to configure the authorization policy by providing the `AuthorizationPolicy` value in the route configuration.

There are two special values we can specify in a route's authorization parameter:

*   `default` - The route will require an authenticated user.
*   `anonymous` - The route will not require authorization regardless of any other configuration.

Here's how we can enforce that all incoming requests must be authenticated:

```json
{
  // This is how we define reverse proxy routes.
  "Routes": {
    "api-route": {
      "ClusterId": "api-cluster",
      "AuthorizationPolicy": "default",
      "Match": {
        "Path": "api/{**catch-all}"
      }
    }
  }
}
```

We want to authorize any incoming request as soon as it hits the API gateway. However, the destination server may still need to know who the user is (authentication) and what they can do (authorization).

YARP will pass any credentials to the proxied request. By default, cookies, bearer tokens, and API keys will flow to the destination server.

## Creating Custom Authentication Policies

YARP can utilize the powerful [authorization policies](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies) feature in ASP.NET Core. We can specify a policy per route in the proxy configuration, and the rest is handled by existing ASP.NET Core authentication and authorization components.

```json
{
  // This is how we define auth policies for reverse proxy routes.
  "Routes": {
    "api-route1": {
      "ClusterId": "api-cluster",
      "AuthorizationPolicy": "is-vip",
      "Match": {
        "Path": "api/hello-vip"
      }
    },
    "api-route2": {
      "ClusterId": "api-cluster",
      "AuthorizationPolicy": "default",
      "Match": {
        "Path": "api/{**catch-all}"
      }
    }
  }
}
```

Here's how we can create a custom `is-vip` policy with two components. It requires an authenticated user and `vip` claim with one of the defined allowed values to be present . To use this policy, we can just specify it as the value for the `AuthorizationPolicy` in the route configuration.

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("is-vip", policy =>
        policy
            .RequireAuthenticatedUser()
            .RequireClaim("vip", allowedValues: true.ToString()));
});
```

## Summary

API gateways provide a unified access point, streamlining client interactions and securing your backend services. Authentication is an essential element of API gateway security, controlling who can access your resources.

YARP offers a versatile solution for building .NET API gateways. By integrating with ASP.NET Core's authentication and authorization frameworks, YARP enables robust security mechanisms.

This flexibility really shines with support for custom authorization policies. This allows you to define [**granular access control**](master-claims-transformation-for-flexible-aspnetcore-authorization) based on user roles, claims, or other attributes.

Thanks for reading, and I'll see you next week.

**P.S.** Here's the complete [source code](https://github.com/m-jovanovic/yarp-authentication) for this article if you want to try it out.