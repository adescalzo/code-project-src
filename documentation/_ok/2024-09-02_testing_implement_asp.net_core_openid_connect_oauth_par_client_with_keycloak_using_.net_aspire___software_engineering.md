```yaml
---
title: "Implement ASP.NET Core OpenID Connect OAuth PAR client with Keycloak using .NET Aspire | Software Engineering"
source: https://damienbod.com/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=effective-integration-testing-with-a-database-in-net&_bhlid=d248fe64a76b7bc9a3bec33c65e8a76f8cb3e4f0
date_published: 2024-09-02T04:23:31.000Z
date_captured: 2025-08-08T13:47:30.958Z
domain: damienbod.com
author: damienbod
category: testing
technologies: [ASP.NET Core, .NET Aspire, Keycloak, Docker, .NET 9, NuGet, Keycloak.AuthServices.Aspire.Hosting, System.IO.Hashing, Blazor, MVC, Razor Pages, Quarkus]
programming_languages: [C#]
tags: [openid-connect, oauth, authentication, security, dotnet, aspnet-core, keycloak, aspire, docker, web-development]
key_concepts: [openid-connect, oauth-par, identity-provider, containerization, distributed-applications, https-certificates, authentication-flows, client-configuration]
code_examples: false
difficulty_level: intermediate
summary: |
  This post demonstrates how to implement an ASP.NET Core application that uses OpenID Connect and OAuth PAR for authentication. It leverages Keycloak as the identity provider, which is hosted in a Docker container and orchestrated using .NET Aspire for local development. The article details the setup of the Keycloak container within the .NET Aspire AppHost project, including the configuration for HTTPS development certificates. It also covers the necessary client configuration within Keycloak and the ASP.NET Core application to enable the OAuth PAR flow, a new feature in .NET 9. The author highlights the benefits of .NET Aspire for containerized development while acknowledging areas for further learning regarding production deployment.
---
```

# Implement ASP.NET Core OpenID Connect OAuth PAR client with Keycloak using .NET Aspire | Software Engineering

# [Software Engineering](https://damienbod.com/ "Software Engineering")

*   [ASP.NET Core](https://damienbod.com/category/net/asp-net-core/)
*   [Azure](https://damienbod.com/category/azure/)
*   [Security](https://damienbod.com/category/security/)
*   [angular](https://damienbod.com/category/javascript/angular/)
*   [Web API](https://damienbod.com/category/topheadermenu/)
*   [MVC](https://damienbod.com/tag/mvc/)
*   [SignalR](https://damienbod.com/tag/signalr/)
*   [Validation](https://damienbod.com/category/validation/)
*   [Elasticsearch](https://damienbod.com/category/elasticsearch/)
*   [Entity Framework](https://damienbod.com/category/entity-framework/)
*   [SQL](https://damienbod.com/category/sql/)
*   [SQLite](https://damienbod.com/category/sqlite/)
*   [OData](https://damienbod.com/category/odata/)
*   [About](https://damienbod.com/about/)

Web development

# Implement ASP.NET Core OpenID Connect OAuth PAR client with Keycloak using .NET Aspire

September 2, 2024 · by [damienbod](https://damienbod.com/author/damienbod/ "Posts by damienbod") · in [.NET Core](https://damienbod.com/category/net/net-core/), [ASP.NET Core](https://damienbod.com/category/net/asp-net-core/), [MVC](https://damienbod.com/category/web/mvc/), [OAuth2](https://damienbod.com/category/security/oauth2/), [Security](https://damienbod.com/category/security/), [Web](https://damienbod.com/category/web/) · [1 Comment](https://damienbod.com/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/#comments)

This post shows how to implement an ASP.NET Core application which uses OpenID Connect and OAuth PAR for authentication. The client application uses [Keycloak](https://www.keycloak.org/) as the identity provider. The Keycloak application is hosted in a docker container. The applications are run locally using .NET Aspire. This makes it really easy to develop using containers.

Code: [https://github.com/damienbod/keycloak-backchannel](https://github.com/damienbod/keycloak-backchannel)

## Setup

The standard Aspire Microsoft template was used to setup the .NET Aspire **AppHost**, **ServiceDefaults** projects. The Keycloak container service was added to the AppHost project using the **Keycloak.AuthServices.Aspire.Hosting** Nuget package. An ASP.NET Core Razor Page project was added as the UI client, but any project can be used like Blazor or an MVC application.

![A diagram titled "Aspire" showing an "ASP.NET Core UI" (Blazor/Razor Pages/MVC) on the left, connected by a dashed arrow to a "Keycloak container" on the right. Below the dashed arrow, text indicates "OpenID Connect confidential client code flow with PKCE using OAUTH PAR RFC 9126" and an OAuth 2.0 logo. This illustrates the architecture of the authentication flow.](https://damienbod.com/wp-content/uploads/2024/09/oauth-par-keycloak_01.png?w=882)

## Keycloak Setup

The Keycloak Container is completely setup in the **AppHost** project. The **Keycloak.AuthServices.Aspire.Hosting** Nuget package is used to add the integration to .NET Aspire. For this to work, [Docker Desktop](https://www.docker.com/products/docker-desktop/) needs to be installed in the development environment. I want to use the Keycloak preview features and initialized this using the WithArgs method. If using the Microsoft Keycloak package, the setup is almost identical.

```csharp
1
2
3
4
5
6
7
8
var userName = builder.AddParameter("userName");
var password = builder.AddParameter("password", secret: true);
var keycloak = builder.AddKeycloakContainer("keycloak",
            userName: userName, password: password, port: 8080)
    .WithArgs("--features=preview")
    .WithDataVolume()
    .RunWithHttpsDevCertificate(port: 8081);
```

I want to develop using HTTPS and so the Keycloak container needs to run in HTTPS as well. This was not so simple to setup, but [Damien Edwards](https://github.com/DamianEdwards) provided a [solution](https://github.com/dotnet/aspire-samples/blob/b741f5e78a86539bc9ab12cd7f4a5afea7aa54c4/samples/Keycloak/Keycloak.AppHost/HostingExtensions.cs) which works great.

The **RunWithHttpsDevCertificate** extension method was added using his code and adapted so that the port is fixed for the HTTPS Keycloak server. This implementation requires the **System.IO.Hashing** Nuget package.

```csharp
1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26
27
28
29
30
31
32
33
34
35
36
37
38
39
40
41
42
43
44
45
46
47
48
49
50
51
52
53
54
55
56
57
58
59
60
61
62
63
64
65
66
67
68
69
70
71
72
73
74
75
76
77
78
79
80
81
82
83
84
85
86
87
88
89
90
91
92
93
94
95
96
97
98
99
100
101
102
103
104
105
106
107
108
109
110
111
112
113
114
115
116
117
118
119
120
121
122
using System.Diagnostics;
using System.IO.Hashing;
using System.Text;
namespace Aspire.Hosting;
/// <summary>
/// Original src code:
/// [https://github.com/dotnet/aspire-samples/blob/b741f5e78a86539bc9ab12cd7f4a5afea7aa54c4/samples/Keycloak/Keycloak.AppHost/HostingExtensions.cs](https://github.com/dotnet/aspire-samples/blob/b741f5e78a86539bc9ab12cd7f4a5afea7aa54c4/samples/Keycloak/Keycloak.AppHost/HostingExtensions.cs)
/// </summary>
public static class HostingExtensions
{
    /// <summary>
    /// Injects the ASP.NET Core HTTPS developer certificate into the resource via the specified environment variables when
    /// <paramref name="builder"/>.<see cref="IResourceBuilder{T}.ApplicationBuilder">ApplicationBuilder</see>.
    /// <see cref="IDistributedApplicationBuilder.ExecutionContext">ExecutionContext</see>.<see cref="DistributedApplicationExecutionContext.IsRunMode">IsRunMode</see><c> == true</c>.<br/>
    /// If the resource is a <see cref="ContainerResource"/>, the certificate files will be bind mounted into the container.
    /// </summary>
    /// <remarks>
    /// This method <strong>does not</strong> configure an HTTPS endpoint on the resource. Use <see cref="ResourceBuilderExtensions.WithHttpsEndpoint{TResource}"/> to configure an HTTPS endpoint.
    /// </remarks>
    public static IResourceBuilder<TResource> RunWithHttpsDevCertificate<TResource>(this IResourceBuilder<TResource> builder, string certFileEnv, string certKeyFileEnv)
        where TResource : IResourceWithEnvironment
    {
        const string DEV_CERT_DIR = "/dev-certs";
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            // Export the ASP.NET Core HTTPS development certificate & private key to PEM files, bind mount them into the container
            // and configure it to use them via the specified environment variables.
            var (certPath, _) = ExportDevCertificate(builder.ApplicationBuilder);
            var bindSource = Path.GetDirectoryName(certPath) ?? throw new UnreachableException();
            if (builder.Resource is ContainerResource containerResource)
            {
                builder.ApplicationBuilder.CreateResourceBuilder(containerResource)
                    .WithBindMount(bindSource, DEV_CERT_DIR, isReadOnly: true);
            }
            builder
                .WithEnvironment(certFileEnv, $"{DEV_CERT_DIR}/dev-cert.pem")
                .WithEnvironment(certKeyFileEnv, $"{DEV_CERT_DIR}/dev-cert.key");
        }
        return builder;
    }
    /// <summary>
    /// Configures the Keycloak container to use the ASP.NET Core HTTPS development certificate created by <c>dotnet dev-certs</c> when
    /// <paramref name="builder"/><c>.ExecutionContext.IsRunMode == true</c>.
    /// </summary>
    /// <remarks>
    /// See <see href="[https://learn.microsoft.com/dotnet/core/tools/dotnet-dev-certs](https://learn.microsoft.com/dotnet/core/tools/dotnet-dev-certs)">[https://learn.microsoft.com/dotnet/core/tools/dotnet-dev-certs</see](https://learn.microsoft.com/dotnet/core/tools/dotnet-dev-certs</see)>
    /// for more information on the <c>dotnet dev-certs</c> tool.<br/>
    /// See <see href="[https://learn.microsoft.com/aspnet/core/security/enforcing-ssl#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos](https://learn.microsoft.com/aspnet/core/security/enforcing-ssl#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos)">
    /// [https://learn.microsoft.com/aspnet/core/security/enforcing-ssl</see](https://learn.microsoft.com/aspnet/core/security/enforcing-ssl</see)>
    /// for more information on the ASP.NET Core HTTPS development certificate.
    /// </remarks>
    public static IResourceBuilder<KeycloakResource> RunWithHttpsDevCertificate(this IResourceBuilder<KeycloakResource> builder, int port = 8081, int targetPort = 8443)
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            // Mount the ASP.NET Core HTTPS development certificate in the Keycloak container and configure Keycloak to it
            // via the KC_HTTPS_CERTIFICATE_FILE and KC_HTTPS_CERTIFICATE_KEY_FILE environment variables.
            builder
                .RunWithHttpsDevCertificate("KC_HTTPS_CERTIFICATE_FILE", "KC_HTTPS_CERTIFICATE_KEY_FILE")
                .WithHttpsEndpoint(port: port, targetPort: targetPort)
                .WithEnvironment("KC_HOSTNAME", "localhost")
                // Without disabling HTTP/2 you can hit HTTP 431 Header too large errors in Keycloak.
                // Related issues:
                // [https://github.com/keycloak/keycloak/discussions/10236](https://github.com/keycloak/keycloak/discussions/10236)
                // [https://github.com/keycloak/keycloak/issues/13933](https://github.com/keycloak/keycloak/issues/13933)
                // [https://github.com/quarkusio/quarkus/issues/33692](https://github.com/quarkusio/quarkus/issues/33692)
                .WithEnvironment("QUARKUS_HTTP_HTTP2", "false");
        }
        return builder;
    }
    private static (string, string) ExportDevCertificate(IDistributedApplicationBuilder builder)
    {
        // Exports the ASP.NET Core HTTPS development certificate & private key to PEM files using 'dotnet dev-certs https' to a temporary
        // directory and returns the path.
        // TODO: Check if we're running on a platform that already has the cert and key exported to a file (e.g. macOS) and just use those instead.
        var appNameHashBytes = XxHash64.Hash(Encoding.Unicode.GetBytes(builder.Environment.ApplicationName).AsSpan());
        var appNameHash = BitConverter.ToString(appNameHashBytes).Replace("-", "").ToLowerInvariant();
        var tempDir = Path.Combine(Path.GetTempPath(), $"aspire.{appNameHash}");
        var certExportPath = Path.Combine(tempDir, "dev-cert.pem");
        var certKeyExportPath = Path.Combine(tempDir, "dev-cert.key");
        if (File.Exists(certExportPath) && File.Exists(certKeyExportPath))
        {
            // Certificate already exported, return the path.
            return (certExportPath, certKeyExportPath);
        }
        else if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, recursive: true);
        }
        Directory.CreateDirectory(tempDir);
        var exportProcess = Process.Start("dotnet", $"dev-certs https --export-path \"{certExportPath}\" --format Pem --no-password");
        var exited = exportProcess.WaitForExit(TimeSpan.FromSeconds(5));
        if (exited && File.Exists(certExportPath) && File.Exists(certKeyExportPath))
        {
            return (certExportPath, certKeyExportPath);
        }
        else if (exportProcess.HasExited && exportProcess.ExitCode != 0)
        {
            throw new InvalidOperationException($"HTTPS dev certificate export failed with exit code {exportProcess.ExitCode}");
        }
        else if (!exportProcess.HasExited)
        {
            exportProcess.Kill(true);
            throw new InvalidOperationException("HTTPS dev certificate export timed out");
        }
        throw new InvalidOperationException("HTTPS dev certificate export failed for an unknown reason");
    }
}
```

**Note:** The **AppHost** project must reference all the services used in the solution.

## Keycloak client configuration

See the [razorpagepar.json](https://github.com/damienbod/keycloak-backchannel/blob/main/KeycloakClients/razorpagepar.json) file in the git repository. This is a Keycloak export of the whole client. This can be imported and updated.

The client is configured to use PAR.

![A screenshot of the Keycloak administration interface, specifically the client configuration section. The left sidebar shows "Manage" and "Clients" highlighted. The main content area shows toggles for "Pushed authorization request required" (set to On) and "Always use lightweight access token" (set to Off), along with "ACR to LoA Mapping". This image demonstrates how to enable PAR in Keycloak.](https://damienbod.com/wp-content/uploads/2024/09/oauth-par-keycloak_02.png?w=927)

## ASP.NET Core OpenID Connect client using OAuth PAR

The client application uses the standard OpenID Connect client and requires OAuth PAR for authentication. This is a new feature in .NET 9. The repo has a Razor Page OpenID Connect example as well as an MVC client sample. This would be the same for a Blazor application.

```csharp
1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26
27
28
29
30
31
32
33
services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = authConfiguration["StsServerIdentityUrl"];
    options.ClientSecret = authConfiguration["ClientSecret"];
    options.ClientId = authConfiguration["Audience"];
    options.ResponseType = "code";
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("offline_access");
    options.ClaimActions.Remove("amr");
    options.ClaimActions.MapJsonKey("website", "website");
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;
    options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Require;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = JwtClaimTypes.Name,
        RoleClaimType = JwtClaimTypes.Role,
    };
});
```

## Notes

.NET Aspire looks great and is easy to use in development. I am only learning this and must learn the details now. I have some issues using the containers and HTTPS and I don’t understand how the configuration works. I also don’t understand how this would work in production. Lots to learn.

## Links

[https://www.keycloak.org/](https://www.keycloak.org/)

[https://www.keycloak.org/server/features](https://www.keycloak.org/server/features)

[https://github.com/NikiforovAll/keycloak-authorization-services-dotnet](https://github.com/NikiforovAll/keycloak-authorization-services-dotnet)

[https://openid.net/specs/openid-connect-backchannel-1\_0.html](https://openid.net/specs/openid-connect-backchannel-1_0.html)

[https://github.com/dotnet/aspire-samples/tree/main/samples](https://github.com/dotnet/aspire-samples/tree/main/samples)

[https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)

### Share this:

*   [Click to share on X (Opens in new window) X](https://damienbod.com/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/?share=twitter&nb=1)
*   [Click to share on Facebook (Opens in new window) Facebook](https://damienbod.com/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/?share=facebook&nb=1)

Like Loading...

### _Related_

[Implement ASP.NET Core OpenID Connect with Keycloak to implement Level of Authentication (LoA) requirements](https://damienbod.com/2025/07/02/implement-asp-net-core-openid-connect-with-keykloak-to-implement-level-of-authentication-loa-requirements/ "Implement ASP.NET Core OpenID Connect with Keycloak to implement Level of Authentication (LoA) requirements")July 2, 2025In ".NET"

[Implement OpenID Connect Back-Channel Logout using ASP.NET Core, Keycloak and .NET Aspire](https://damienbod.com/2024/09/09/implement-openid-connect-back-channel-logout-using-asp-net-core-keycloak-and-net-aspire/ "Implement OpenID Connect Back-Channel Logout using ASP.NET Core, Keycloak and .NET Aspire")September 9, 2024In "ASP.NET Core"

[Implement an OpenIddict identity provider using ASP.NET Core Identity with Keycloak federation](https://damienbod.com/2022/05/02/implement-an-openiddict-identity-provider-using-asp-net-core-identity-with-keycloak-federation/ "Implement an OpenIddict identity provider using ASP.NET Core Identity with Keycloak federation")May 2, 2022In ".NET"

Tags: [.NET](https://damienbod.com/tag/net/), [ASP.NET Core](https://damienbod.com/tag/asp-net-core/), [aspire](https://damienbod.com/tag/aspire/), [dotnet](https://damienbod.com/tag/dotnet/), [MVC](https://damienbod.com/tag/mvc/), [OAuth2](https://damienbod.com/tag/oauth2/), [OIDC](https://damienbod.com/tag/oidc/), [OpenId connect](https://damienbod.com/tag/openid-connect/)

### One comment

1.  [Dew Drop–September 3, 2024 – Morning Dew by Alvin Ashcraft](https://www.alvinashcraft.com/2024/09/03/dew-drop-september-3-2024/) · [September 3, 2024 - 11:36](https://damienbod.com/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/#comment-166508) · [Reply](https://damienbod.com/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/?replytocom=166508#respond)→
    
    […\] Implement ASP.NET Core OpenID Connect OAuth PAR client with Keycloak using .NET Aspire (Damien Bowden) […\]
    

### Leave a comment [Cancel reply](/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=effective-integration-testing-with-a-database-in-net&_bhlid=d248fe64a76b7bc9a3bec33c65e8a76f8cb3e4f0#respond)

Write a comment...

Log in or provide your name and email to leave a comment.

Email me new posts

InstantlyDailyWeekly

Email me new comments

Save my name, email, and website in this browser for the next time I comment.

Comment

   

Δdocument.getElementById( "ak\_js\_1" ).setAttribute( "value", ( new Date() ).getTime() );

This site uses Akismet to reduce spam. [Learn how your comment data is processed.](https://akismet.com/privacy/)

← [Add a Swagger UI using a .NET 9 Json OpenAPI file](https://damienbod.com/2024/08/12/add-a-swagger-ui-using-a-net-9-json-openapi-file/)

[Implement OpenID Connect Back-Channel Logout using ASP.NET Core, Keycloak and .NET Aspire](https://damienbod.com/2024/09/09/implement-openid-connect-back-channel-logout-using-asp-net-core-keycloak-and-net-aspire/) →

.widget.widget\_media\_image { overflow: hidden; }.widget.widget\_media\_image img { height: auto; max-width: 100%; }[![](https://damienbod.com/wp-content/uploads/2015/10/microsoftmvp2016.png)](https://mvp.microsoft.com/en-us/PublicProfile/5002218?fullName=Damien%20%20Bowden)

 

### Categories

Categories Select Category .NET .NET Core angular AngularJS AOP App Service ASP.NET Core aspire ASPNET5 Azure Azure Cognitive Search Azure functions Azure Key Vault blockchain Deployment devops Docker dotnet e-id Elasticsearch Enterprise Library Entity Framework Entra CIAM git graph javascript jQuery Logging Lucene Microsoft Entra External ID Microsoft Entra ID Mobile Monitoring MVC MVVM Nest NLog NoSQL NuGet OAuth2 OData OpenId Protobuf Security Self Sovereign Identity Semantic Logging Serverless SignalR SLAB SQL SQLite TopHeaderMenu Typescript UI Uncategorized Unity Validation Web Windows 8 Apps WiX WPF

/\* <![CDATA[ */ (function() { var dropdown = document.getElementById( "cat" ); function onCatChange() { if ( dropdown.options[ dropdown.selectedIndex ].value > 0 ) { dropdown.parentNode.submit(); } } dropdown.onchange = onCatChange; })(); /* ]]> */

### blogs 1 web, general

*   [.NET Blog](https://blogs.msdn.microsoft.com/dotnet/)
*   [Adam Storr](https://adamstorr.azurewebsites.net/)
*   [Andrew Lock](http://andrewlock.net/#open)
*   [Anthony Giretti](http://anthonygiretti.com/)
*   [ASP.NET Core blog](https://blogs.msdn.microsoft.com/webdev/)
*   [Benjamin Abt (de)](http://schwabencode.com/)
*   [Bryan Hogan](https://nodogmablog.bryanhogan.net)
*   [Chris Klug](https://www.fearofoblivion.com/)
*   [Claudio Bernasconi](https://www.claudiobernasconi.ch/)
*   [code-maze.com](https://code-maze.com/)
*   [codemurals](https://codemurals.blogspot.com/)
*   [David Pine](https://davidpine.net/)
*   [Davide Bellone](https://www.code4it.dev/)
*   [dotnetthoughts](http://dotnetthoughts.net/)
*   [Fabian Gosebrink](http://offering.solutions/)
*   [Filip WOJCIESZYN](http://www.strathweb.com/)
*   [Gérald Barré](https://www.meziantou.net/)
*   [Gunnar Peipman's](http://gunnarpeipman.com/ "ASP.NET ")
*   [haacked.com](http://haacked.com/)
*   [humankode](https://www.humankode.com/)
*   [Isaac Abraham](https://www.compositional-it.com/)
*   [Isaac Levin](https://www.isaaclevin.com/)