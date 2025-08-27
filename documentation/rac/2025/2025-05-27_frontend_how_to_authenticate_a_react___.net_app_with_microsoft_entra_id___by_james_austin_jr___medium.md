```yaml
---
title: "How to Authenticate a React & .NET App with Microsoft Entra ID | by James Austin Jr | Medium"
source: https://medium.com/@jaustinjr.blog/how-to-authenticate-a-react-net-app-with-microsoft-entra-id-f8b35f75e1bf
date_published: 2025-05-27T15:02:40.088Z
date_captured: 2025-08-29T10:34:13.432Z
domain: medium.com
author: James Austin Jr
category: frontend
technologies: [React, .NET Core, Microsoft Entra ID, MSAL, Node.js, Vite, React Router, Microsoft.Identity.Web, npm, dotnet CLI, jwt.io, Postman, Swagger, ASP.NET Core, OAuth 2.0, PKCE, CORS]
programming_languages: [JavaScript, C#, JSON]
tags: [authentication, authorization, microsoft-entra-id, react, dotnet, web-api, single-page-application, rbac, msal, security]
key_concepts: [identity-and-access-management, single-page-application, app-registration, authorization-code-flow-with-pkce, role-based-access-control, access-tokens, id-tokens, cross-origin-resource-sharing]
code_examples: false
difficulty_level: intermediate
summary: |
  [This guide provides a comprehensive walkthrough for implementing authentication and authorization in a React and .NET application using Microsoft Entra ID and the Microsoft Authentication Library (MSAL). It details the process of configuring App Registrations in Entra ID, setting up the Authorization Code Flow with PKCE for the React client, and securing the .NET Web API with JWT Bearer authentication. A key focus is placed on establishing Role-Based Access Control (RBAC) within the application. The article also offers valuable considerations regarding token types, various authentication flows, and testing strategies, serving as an updated resource for developers.]
---
```

# How to Authenticate a React & .NET App with Microsoft Entra ID | by James Austin Jr | Medium

# How to Authenticate a React & .NET App with Microsoft Entra ID

## A guide to build authentication and authorization in cross-origin apps with MSAL.

[

![James Austin Jr](https://miro.medium.com/v2/da:true/resize:fill:64:64/0*lFrwT1vvr6dgMsgP)





](/@jaustinjr.blog?source=post_page---byline--f8b35f75e1bf---------------------------------------)

[James Austin Jr](/@jaustinjr.blog?source=post_page---byline--f8b35f75e1bf---------------------------------------)

Follow

12 min read

·

May 27, 2025

29

1

Listen

Share

More

![](https://miro.medium.com/v2/resize:fit:700/0*OuH735Zm-aBtJQvH)
A visual representation of security, showing two padlocks connected by a chain, with one smaller, open padlock in the foreground, all under green and red lighting.

Photo by [FlyD](https://unsplash.com/@flyd2069?utm_source=medium&utm_medium=referral) on [Unsplash](https://unsplash.com/?utm_source=medium&utm_medium=referral)

## Overview

Microsoft Entra ID is an Identity & Access Management (IAM) service that replaces Azure Active Directory. Today I’ll be guiding you through a Single-Page Application setup for a React & .NET application.

> **Read the article for free by visiting the** [**friend link**](/@jaustinjr.blog/f8b35f75e1bf?sk=9cc56361a47b91878135391c43ca1307)**! 🥂**

I have shared this topic with you before, check out the [first version of this guide](/@jaustinjr.blog/how-to-integrate-microsoft-entra-id-in-a-single-page-application-5f5f01b65cf3) if you’re interested. A few things I include in this second version are:

*   Simplified code
*   More walkthrough screenshots
*   Role-based access control (RBAC) setup
*   Comprehensive Microsoft documentation list

The Microsoft documentation does enough explaining of their technologies, so I’ll spare you the lecture on [Microsoft Entra ID](https://learn.microsoft.com/en-us/entra/fundamentals/whatis), Authentication, and Authorization.

All you need to know is that Microsoft Entra ID is the IAM we will be using to implement our authentication and authorization which basically manages who has access and what access to an application. For more information, see these articles:

*   [Authentication in Microsoft Entra](https://learn.microsoft.com/en-us/entra/identity/authentication/overview-authentication)
*   [OAuth 2.0 Authorization Framework](https://datatracker.ietf.org/doc/html/rfc6749)
*   [Access Tokens in Microsoft Entra](https://learn.microsoft.com/en-us/entra/identity-platform/access-tokens)

### Technologies

I include the version numbers for posterity, they are not required to complete the implementation, however, MSAL version 2+ is necessary.

*   [Node JS v20.15](https://nodejs.org/en)
*   [React v18](https://react.dev/)
*   [Vite v6.2](https://vite.dev/guide/)

```
npm create vite@latest // Scaffolds a new application
```

*   [React Router v7.5](https://reactrouter.com/start/declarative/routing)

```
npm install react-router
```

*   [Microsoft Authentication Library (MSAL) v](https://github.com/AzureAD/microsoft-authentication-library-for-js)3

```
npm install @azure/msal-browser @azure/msal-react
```

*   [.NET Core v9](https://dotnet.microsoft.com/en-us/download/dotnet)
*   [Microsoft.Identity.Web v3.8](https://www.nuget.org/packages/Microsoft.Identity.Web)

```
dotnet add package Microsoft.Identity.Web
```

## Getting Started

### App Registration

Go to [Microsoft Entra](https://entra.microsoft.com/#home) and navigate to the Identity platform to create a new App Registration. An App Registration will create a corresponding Enterprise Application. These two categories will be used to configure the application settings. App Registration covers application-specific settings for the web and Enterprise Application covers tenant-specific settings pertaining to the application.

Press enter or click to view image in full size

![](https://miro.medium.com/v2/resize:fit:700/1*06X9eqEMjee7kGWChMz5Eg.png)
Screenshot of the Microsoft Entra admin center, showing the "App registrations" page with the "New registration" button highlighted, indicating where to start a new application registration.

Microsoft Entra Identity: Create New App Registration

Upon creation of a new App Registration, Entra will provide quickstart tutorials for setting up the new application. We are using the _Single-Page Application_ (SPA) setup for the React application. This does not affect how we configure the Web API in the .NET Core application.

We need to configure the App Registration for our use case. This includes the authentication flow, access token settings, and scopes.

The authentication flow we are using is the _Authorization Code flow with PKCE_ (Proof Key for Code Exchange), and no change to the **Authentication** settings is necessary after setting up the SPA redirect URI. You should see the following below the redirect URI.

![](https://miro.medium.com/v2/resize:fit:421/1*43a5_Bl2XS31E-dJ73q97A.png)
Screenshot of the Microsoft Entra App Registration settings, confirming that the "Authorization Code Flow with PKCE" is granted for the application.

Authentication: Authorization Code Flow with PKCE Granted

Navigate to the **Manifest** settings and locate the `accessTokenAcceptedVersion` property. It will be set to `null` which defaults to version 1.0, we want to set this to `2` to grant the user a version 2.0 access token. You can verify the token version using [jwt.io](https://jwt.io/) and checking the `ver` property in the decrypted token. For more information check out this [Microsoft forum](https://learn.microsoft.com/en-us/answers/questions/639834/how-to-get-access-token-version-2-0).

```json
{  
  // ...,  
  "accessTokenAcceptedVersion": 2,  
  // ...  
}
```

Finally for this section on the App Registration, we’ll want a custom scope for this application to ensure we are retrieving a version 2.0 access token. Unfortunately setting the `accessTokenAcceptedVersion` property is not enough, we need to let the user grant consent to retrieving this token via the `User.Access` scope that we are going to create.

Navigate to the **Expose an API** settings to create a new scope that users can consent to without admin approval. This scope will then be attached to a version 2.0 access token that the user will receive. Note that the _Application ID URI_ `api://<client-id>` has to be set first before creating the scope.

Press enter or click to view image in full size

![](https://miro.medium.com/v2/resize:fit:700/1*QfznETGF6srYd5fvvuZshQ.png)
Screenshot of the "Expose an API" section in Microsoft Entra, showing the process of creating a new custom scope, with the "Add a scope" button highlighted.

Expose an API: Create a New Scope

### React Client Application

Create the React application if you have not already done so with the `npm create vite@latest` command — note that you don’t have to use Vite. Install the technologies listed above and add the following code to the `main.jsx` file or whatever file you are using for the root element.

```javascript
import { StrictMode } from 'react'  
import { createRoot } from 'react-dom/client'  
  
import App from './App.jsx'  
import AuthCallback from './AuthCallback.jsx'  
  
import { msalInstance } from './auth/config.js'  
import { MsalProvider } from '@azure/msal-react';  
import { BrowserRouter, Routes, Route } from "react-router";  
  
createRoot(document.getElementById('root')).render(  
  <StrictMode>  
    <MsalProvider instance={ msalInstance }>  
      <BrowserRouter>  
        <Routes>  
          <Route path="/" element={<App />} />  
          <Route path="/callback" element={<AuthCallback />} />  
        </Routes>  
      </BrowserRouter>  
    </MsalProvider>  
  </StrictMode>,  
)
```

The `<App />` element is being used for the application’s landing page and the `<AuthCallback />` element will handle the login redirect promise. Make sure the `<AuthCallback />` route matches the redirect URI set in the App Registration, otherwise the login redirect promise will not be caught and the user will not be able to login.

The `<AuthCallback />` is really simple and it leaves room for customizations such as an elaborate loading UX, global context implementations, and custom token storage.

```javascript
import { useEffect } from "react";  
  
import { useMsal } from "@azure/msal-react";  
  
  
function AuthCallback() {  
    const { instance } = useMsal();  
  
    useEffect(() => {  
        instance  
            .handleRedirectPromise()  
            .then((res) => {  
                if (res) {  
                    instance.setActiveAccount(res.account)  
                    window.location.replace("/");  
                }  
            })  
            .catch((err) => {  
                console.error(err);  
            });  
    }, [])  
  
    return (  
        <>  
        Loading...  
        </>  
    )  
}  
  
export default AuthCallback
```

The instance of MSAL is crucial for making this callback work and an example of what your configuration could look like is below. Refer to the [MSAL Configuration](https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-node/docs/configuration.md) documentation for customizations.

```javascript
import { PublicClientApplication, LogLevel, CacheLookupPolicy, BrowserCacheLocation } from "@azure/msal-browser";  
  
const clientId = <copy-from-App-Registration-Overview>  
const authority = "https://login.microsoftonline.com/" + <copy-from-App-Registration-Overview>  
  
// MSAL configuration object will connect to Microsoft Entra and set behavior in the browser  
// Refer to available configuration at https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md  
const msalConfig = {  
    auth: {  
        clientId: clientId,  
        authority: authority,  
        redirectUri: "<replace-with-app-base-url>/callback",  
        postLogoutRedirectUri: "<replace-with-app-base-url>/",  
        navigateToLoginRequestUrl: false  
    },  
    cache: {  
        // Select cache location according to application requirements  
        // Refer to MSAL caching at https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/caching.md  
        cacheLocation: BrowserCacheLocation.SessionStorage,  
        // It is not advised to store auth state in cookies in general  
        // Unless the application is experiencing authentication issues with IE11, Edge, or Firefox browsers then set to true to mitigate these issues  
        storeAuthStateInCookie: false  
    },  
    system: {  
        loggerOptions: {  
            loggerCallback: (level, message, piiLoggingEnabled) => {  
                if (piiLoggingEnabled) return;  
  
                if (level === LogLevel.Error) {  
                    console.error(message);  
                } else if (level === LogLevel.Warning) {  
                    console.warn(message);  
                } else {  
                    console.log(message);  
                }  
            },  
            logLevel: LogLevel.Warning,  
            piiLoggingEnabled: false, // Keep disabled for any hosted environment  
        },  
    }  
};  
  
console.log(msalConfig)  
  
// default scopes: openid profile email  
// v1.0 tokens will acquire these by default, but can be requested with .default  
// v2.0 tokens cannot combine the .default scope with resource-specific scopes in a single request  
// Users.Access scope is used to enable v2.0 access token which will be valid for the API  
export const loginRequest = {  
    scopes: [`api://${msalConfig.auth.clientId}/Users.Access`]  
};  
  
// Necessary for MSAL 3.0 initialization  
// Refer to https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/initialization.md#initializing-the-publicclientapplication-object  
export const msalInstance = await PublicClientApplication.createPublicClientApplication(msalConfig);
```

I suggest hiding sensitive information of your application and tenant in an environment file `.env` to populate the variables on build. That way you are not checking in those IDs in the source control or displaying them publicly on the web.

The login request to Entra has to be initiated from somewhere. For simplicity we’ll use the default landing page. We’ll need to define the `loginRedirect` and the `handleLogoutRedirect` APIs, then we can retrieve the access token by using the `acquireToken*` API. For each of the APIs, the code will look something like this.

```javascript
// Redirects to Microsoft authentication portal  
const handleLoginRedirect = () => {  
  instance.loginRedirect(loginRequest).catch((error) => console.error(error));  
};  
  
// Redirects to Microsoft authentication portal then to the logout URI  
// FYI Entra sometimes does not redirect back to the application  
const handleLogoutRedirect = () => {  
  instance.setActiveAccount(null);  
  instance.logoutRedirect().catch((error) => console.error(error));  
};  
  
// Acquire the access token before sending a request to the Web API  
const acquireToken = async () => {  
  // The acquireToken* APIs will use the active account to acquire the token  
  const res = await instance.acquireTokenSilent(loginRequest)  
    .catch(err => {  
      if (err instanceof InteractionRequiredAuthError) {  
        let redirectRequest = {...loginRequest, loginHint: instance.getActiveAccount().username}  
        instance.acquireTokenRedirect(redirectRequest);  
      } else {  
        console.error("Something went wrong:", err);  
        setResponseText("Could not refresh token. Check if an account is active.")  
      }  
    });  
  
  return res.accessToken;  
}
```

When creating a request for the Web API, make sure to use the result of `acquireToken` to set the `Authorization` header of the request. And in case you’re interested, check out my article for [alternative methods of token retrieval](/@jaustinjr.blog/3-methods-to-refresh-access-tokens-in-react-with-msal-9f28c0995ee7).

[

## 3 Methods to Refresh Access Tokens In React & Microsoft Entra ID

### A guide on leveraging the MSAL 2.0 API to refresh access tokens in a React SPA registered in Microsoft Entra ID.

medium.com



](/@jaustinjr.blog/3-methods-to-refresh-access-tokens-in-react-with-msal-9f28c0995ee7?source=post_page-----f8b35f75e1bf---------------------------------------)

This completes the Authentication portion of the guide. Now the client application is interacting with Microsoft Entra to authenticate the user’s session. Next we configure the Web API to interact with Microsoft Entra to confirm the user’s authorization for the application.

### .NET Core Web API

Create the Web API if you have not already done so with the `dotnet new webapi` command — note that you don’t have to use this template and can view more using the `dotnet new list` command. Install the technologies listed above and we’ll begin with the `Program.cs` file.

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;  
using Microsoft.Identity.Web;  
using Microsoft.IdentityModel.Logging;  
  
var builder = WebApplication.CreateBuilder(args);  
  
// CORS  
// Necessary to allow the client application to interact with the Web API  
builder.Services.AddCors(options =>  
{  
    // Replace this policy with your application's origin  
    options.AddPolicy(name: "AllowAnyOrigins",  
                        policy  =>  
                        {  
                            policy.AllowAnyOrigin()  
                                .AllowAnyHeader()  
                                .AllowAnyMethod();  
                        });  
});  
  
// Authentication  
// The Web API uses the AzureAd configuration to verify with Microsoft Entra that the access token is valid  
// All this information is in the App Registration portal Overview pane  
// The configuration is used to verify tokens with the tenant and registered application  
// (i.e. https://login.microsoftonline.com/<tenant_id>/discovery/keys?appid=<client_id>)  
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));  
  
// Authorization  
// Create roles for the application in the App Registration portal in the App Roles pane  
// Assign roles to Users/Groups or Applications (by service principal) in the Enterprise Application portal in the Users and Groups pane  
// To expose this application as an API, do so in the App Registration portal in the Expose an API pane under the Authorized Client Applications  
builder.Services.AddAuthorization(options => {  
    options.AddPolicy("GeneralPolicy", policy => {  
        policy.RequireRole([ "ADMIN", "GENERAL" ]);  
    });  
    options.AddPolicy("LimitedPolicy", policy => {  
        policy.RequireRole([ "ADMIN", "GENERAL" ]);  
    });  
    options.AddPolicy("AdminPolicy", policy => {  
        policy.RequireRole([ "ADMIN" ]);  
    });  
});  
  
// ...  
  
// AAD Security Logging  
if (app.Environment.IsDevelopment())  
{  
    // Keep disabled for hosted environments  
    IdentityModelEventSource.ShowPII = true;  
    IdentityModelEventSource.LogCompleteSecurityArtifact = true;  
}  
  
// Middleware  
// UseCors() comes before useAuthentication() and useAuthorization()  
// Refer to https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0#enable-cors  
app.UseCors("AllowAnyOrigins");  
app.UseAuthentication();  
app.UseAuthorization();
```

Add each section of code to the `Program.cs` file and define the following `AzureAd` object in the `appsettings.json` file. Define different Entra configurations based on the environment by denoting the file name as `appsettings.<environment>.json`.

```json
{  
  // ...,  
  "AzureAd": {  
    "Instance": "https://login.microsoftonline.com/",  
    "TenantId": "<copy-from-App-Registration-Overview>",  
    "ClientId": "<copy-from-App-Registration-Overview>"  
  },  
  // ...  
}
```

Set up your endpoints how you prefer whether that be minimal or classic API controllers, then we’ll need to use the `[Authorize]` attribute to enable the token verification with Entra.

```csharp
using Microsoft.AspNetCore.Authorization;  
using Microsoft.AspNetCore.Mvc;  
  
namespace Controllers;  
  
[ApiController]  
[Authorize]  
[Route("[controller]")]  
public class SampleController : Controller  
{  
  // ...  
}
```

However, the `[Authorize]` attribute only verifies the validity of an access token, and you may have noticed the authorization policies in the `Program.cs` file. Those are for the role-based access control that we’ll set up in Entra now.

### Role-Based Access Control

Navigate to the application’s App Registration and create the necessary roles you’d like to use in the **App Roles** settings. For this guide, we’ll be using the `General` and `Admin` roles.

Press enter or click to view image in full size

![](https://miro.medium.com/v2/resize:fit:700/1*yi_Gmeby10JmNfex93UzIg.png)
Screenshot of the "App roles" section in Microsoft Entra, displaying existing "General" and "Admin" roles and highlighting the "Create app role" button for defining new roles.

App Roles: Create a New App Role

We can start assigning app roles to users in the Enterprise Application, so let’s go over there, navigate to the **Users and Groups** settings, and click on _Add user/group_. Role assignments can go to anybody registered with your tenant so for testing purposes I recommend assigning yourself for now.

Note that Microsoft Entra does not support multiple role assignment in the _Add user/group_ page, but you can assign one role to multiple users/groups and repeat the process for other roles. For more information see this [Microsoft forum post](https://learn.microsoft.com/en-us/answers/questions/1293647/assign-multiple-app-roles-to-a-user).

Now we’re ready to test out these roles, just need to enforce them using the policies we created in the `Program.cs` file. The `General` policy is self-explanatory, everyone that is assigned a role can access the API endpoints. The `Admin` policy narrows access to only admin users. And although it’s same as the `General` policy, I like to use the `Limited` policy to indicate which API endpoints handle either of the roles differently.

```csharp
using Microsoft.AspNetCore.Authorization;  
using Microsoft.AspNetCore.Mvc;  
  
namespace Controllers;  
  
[ApiController]  
[Authorize(Policy = "GeneralPolicy")]  
[Route("[controller]")]  
public class SampleController : Controller  
{  
// ...  
  
    [HttpGet]  
    public ActionResult Get()  
    {  
        // ...  
    }  
  
    [HttpPost]  
    [Authorize(Policy = "LimitedPolicy")]  
    public ActionResult Create()  
    {  
        if(HttpContext.User.IsInRole("GENERAL"))  
        {  
            // ...  
        }  
  
        // ...  
    }  
  
    [HttpDelete]  
    [Authorize(Policy = "AdminPolicy")]  
    public ActionResult Delete()  
    {  
        // ...  
    }  
}
```

The controller’s `[Authorize(Policy = "GeneralPolicy")]` applies to all endpoints, but can be overridden like you see with the `HttpPost` and `HttpDelete` endpoints. Take a look at the `HttpPost` to understand what I mean by handling the roles differently in an endpoint.

`HttpContext.User` contains information about the authenticated user’s session and token — refer to the [claims reference](https://learn.microsoft.com/en-us/entra/identity-platform