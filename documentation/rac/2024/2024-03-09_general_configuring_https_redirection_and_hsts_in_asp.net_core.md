```yaml
---
title: Configuring HTTPS Redirection and HSTS in ASP.NET Core
source: https://antondevtips.com/blog/configuring-https-redirection-and-hsts-in-aspnet-core
date_published: 2024-03-09T20:18:18.717Z
date_captured: 2025-08-06T17:13:39.123Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core, HTTPS, HSTS, .NET]
programming_languages: [C#]
tags: [web-security, https, hsts, aspnet-core, middleware, dotnet, web-development, security]
key_concepts: [HTTPS Redirection, HTTP Strict Transport Security, Middleware, Web Security, SSL Certificate, Man-in-the-middle attacks]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the importance of HTTPS for web security, detailing how it protects data integrity and confidentiality. It provides a step-by-step guide on configuring HTTPS Redirection in ASP.NET Core using built-in middleware, ensuring all HTTP traffic is encrypted. The article further demonstrates how to enable HTTP Strict Transport Security (HSTS) in ASP.NET Core to force browsers to use HTTPS, thereby preventing man-in-the-middle attacks. Practical C# code examples are included for both configurations, showing how to adjust settings like redirection status codes and HSTS options. The content emphasizes these configurations as crucial steps for enhancing the security of ASP.NET Core applications.]
---
```

# Configuring HTTPS Redirection and HSTS in ASP.NET Core

![Cover image for the article titled "HTTPS AND HSTS IN ASP.NET CORE" with a "dev tips" logo and abstract purple shapes on a dark blue background.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_hsts.png&w=3840&q=100)

# Configuring HTTPS Redirection and HSTS in ASP.NET Core

Mar 9, 2024

2 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

## Essentials of web security

When it comes to web security - **HTTPS** is one of the most important aspects and a starting point for almost every website. HTTPS (Hypertext Transfer Protocol Secure) is an internet communication protocol that protects the integrity and confidentiality of data sent from the user's web browser to the website. HTTPS encrypts data transferred over the internet, which helps to secure transactions, prevent hacking, and protect sensitive information. This protocol is especially crucial for websites that handle sensitive data, such as e-commerce sites, online banking, and any site that requires login and password credentials.

## Understanding HTTPS redirection and HSTS

**HTTPS Redirection:** is a technique where HTTP requests are automatically redirected to their HTTPS counterparts, ensuring that all communication between the client and server is encrypted.

**HTTP Strict Transport Security (HSTS):** is a web security policy mechanism that helps to protect websites against man-in-the-middle attacks by forcing web browsers to access the website over HTTPS only.

## Configuring HTTPS Redirection in ASP.NET Core

Asp.Net Core provides built-in support for HTTPS redirection using a middleware. Lets have a look how to setup it:

**1\. Ensure HTTPS is enabled** First, ensure that your Asp.Net Core application has an HTTPS endpoint. Check your appsettings.json and launchSettings.json. Out of the box Asp.Net Core includes a self-signed certificate for development purposes. For production, make sure to provide a valid SSL certificate.

**2\. Configure HTTPS redirection middleware**

Add **UseHttpsRedirection** middleware in Program.cs or Startup.cs inside **Configure** method (for old projects):

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

await app.RunAsync();
```

By adding **UseHttpsRedirection** middleware you instruct your webapp to redirect all calls to HTTP endpoints to their HTTPS counterparts.

**3\. Configure temporary or permanent HTTPS redirection** The default behaviour of **UseHttpsRedirection** middleware can be adjusted by calling **AddHttpsRedirection** method when creating a DI container:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpsRedirection(options =>
{
    // options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
    // options.HttpsPort = 5001;
    options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
    options.HttpsPort = 443;
});

var app = builder.Build();

app.UseHttpsRedirection();

await app.RunAsync();
```

Permanent redirection is recommended for production environments.

## Configuring HSTS in ASP.NET Core

After configuring HTTPS redirection you can enable HSTS in Asp.Net Core to force web browsers to access the website over HTTPS only.

**1\. Enable HSTS middleware** Update your Program.cs or Startup.cs to add HSTS:

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHsts(); // Add this line
app.UseHttpsRedirection();

await app.RunAsync();
```

**2\. Adjust default HSTS settings**

The default behaviour of **UseHsts** middleware can be adjusted by calling **AddHsts** method when creating a DI container:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(60);

    options.ExcludedHosts.Add("test.com");
});

var app = builder.Build();

app.UseHttpsRedirection();

await app.RunAsync();
```

## Summary

Configuring HTTPS is crucial for enhancing website security. HTTPS ensures that all traffic between web browser and server is encrypted, which helps to secure transactions, prevent hacking, and protect sensitive information.

In Asp.Net Core you can configure HTTPS redirection ensuring that all traffic is secured.

To make a step further in web security - HSTS can be enabled to force web browsers to access the website over HTTPS only. Asp.Net Core is extensible in this part and you can adjust HTTPS redirection and HSTS settings to match your needs.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fconfiguring-https-redirection-and-hsts-in-aspnet-core&title=Configuring%20HTTPS%20Redirection%20and%20HSTS%20in%20ASP.NET%20Core)[X](https://twitter.com/intent/tweet?text=Configuring%20HTTPS%20Redirection%20and%20HSTS%20in%20ASP.NET%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fconfiguring-https-redirection-and-hsts-in-aspnet-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fconfiguring-https-redirection-and-hsts-in-aspnet-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.