```yaml
---
title: Keycloak Tutorial for .NET Developers
source: https://juliocasal.com/blog/keycloak-tutorial-for-net-developers?utm_source=convertkit&utm_medium=email&utm_campaign=Welcome%20To%20The%20.NET%20Saturday!%20-%208807570
date_published: 2025-02-01T08:00:00.000Z
date_captured: 2025-08-27T14:51:44.010Z
domain: juliocasal.com
author: Julio Casal
category: ai_ml
technologies: [Keycloak, ASP.NET Core, OpenID Connect, Docker, Docker Desktop, .NET, NuGet, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.Authentication.OpenIdConnect, Blazor, HTTP Client, .NET Aspire, Microsoft Entra ID]
programming_languages: [C#, YAML, Bash]
tags: [keycloak, oidc, authentication, authorization, dotnet, aspnet-core, docker, identity-management, security, web-api]
key_concepts: [OpenID Connect (OIDC), Identity and Access Management (IAM), JWT Bearer authentication, Authorization Code Flow, Keycloak Realms, Keycloak Clients, Keycloak Client Scopes, Audience (JWT)]
code_examples: false
difficulty_level: intermediate
summary: |
  [This tutorial guides .NET developers through securing ASP.NET Core applications using Keycloak and OpenID Connect (OIDC) for local development. It covers setting up Keycloak via Docker, configuring realms, users, and clients for both backend APIs and frontend applications. The article details the necessary C# code for integrating JWT Bearer authentication in the backend and OIDC middleware in a Blazor frontend. Finally, it demonstrates how to enable authenticated requests from the frontend to the backend using a delegating handler, providing a complete, secure local development environment.]
---
```

# Keycloak Tutorial for .NET Developers

# Keycloak Tutorial for .NET Developers

Feb 1, 2025

_Read time: 15 minutes_

In one of my recent newsletters, I claimed OpenID Connect (OIDC) is the right way to configure authentication and authorization for your ASP.NET Core apps. No need to reinvent the wheel.

However, ASP.NET Core does not include a built-in OIDC server, so you are on your own trying to figure out what to use out of dozens of possible free, paid, local, or hosted options.

If you deploy your apps to Azure, a great option is Microsoft Entra ID, which I covered [here](https://juliocasal.com/blog/Securing-Aspnet-Core-Applications-With-OIDC-And-Microsoft-Entra-ID). But, for local development, I find that to be overkill.

You should be able to run one command in your box and have everything needed to test your app with all the OIDC goodness without ever having to leave your box or pay for cloud services.

So today I’ll show you how to secure your ASP.NET Core Apps with OIDC and Keycloak, from scratch.

Let’s dive in.

### **What is Keycloak?**

Keycloak is an open-source identity and access management (IAM) tool that helps secure applications and services.

![Keycloak logo](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-h8wB5QC64bLNQNVL82jran.jpeg)

I love to use it primarily for local development because:

*   **It’s easy to run it locally** using nothing more than Docker.
*   **It has no cloud dependencies**, making it fast and free from cloud-related costs.
*   **It has a simple web admin UI** for managing users, roles, clients, scopes, and many other things.
*   **It is free and open-source**, so you can go straight to the source if needed.
*   **It is OpenID Connect certified**, so it must support all the OIDC goodness.

Now let’s see how to configure it for local development.

### **Step 1: Run Keycloak via Docker**

Make sure you have already installed and started **Docker Desktop** in your box, and then create a **docker-compose.yml** file somewhere in your repo with these contents:

```yaml
services:
  keycloak:
    image: quay.io/keycloak/keycloak:26.1.0
    container_name: keycloak
    ports:
      - "8080:8080"
    environment:
      - KC_BOOTSTRAP_ADMIN_USERNAME=admin
      - KC_BOOTSTRAP_ADMIN_PASSWORD=admin
    volumes:
      - keycloak-data:/opt/keycloak/data
    command: ["start-dev"]

volumes:
  keycloak-data:
```
![Docker Compose configuration for Keycloak](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-uz4nuuHe7qgzKLXqixvWfX.jpeg)

That configures the container to run Keycloak version **26.1**, exposing its admin portal on **port 8080**, using **admin** for user and password, and with a **volume** to not lose our settings when stopping the container.

We also configure it to run in **dev mode** (start-dev), which avoids a few restrictions that should be enforced in Production only.

Open a terminal wherever you saved this file and run this:

```bash
docker compose up -d
```
![Terminal command for starting Docker Compose](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-qsm5UogKm4g3AZ7dKkpHzP.jpeg)

Now open your browser and navigate to this page:

[http://localhost:8080](http://localhost:8080/)

![Keycloak login page in a web browser](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-8kAMYMeYtfgbcqT1asVfbi.jpeg)

Sign in with **admin** for user and password and you’ll land on Keycloak’s home page:

![Keycloak admin console home page](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-tyiTBsoYrKgk6aGENUDiA1.jpeg)

Now let’s configure the realm.

### **Step 2: Create the realm**

A Keycloak Realm is a logical space for managing users, roles, groups, and authentication configurations within a Keycloak instance.

You can create one by clicking on **Create realm** in the realm drop-down:

![Keycloak realm dropdown with 'Create realm' button highlighted](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-pejjpY7anW8xSYZDer67eA.jpeg)

In the next screen enter a realm name that matches what your app is about and hit **Create:**

![Keycloak 'Create realm' form with realm name input](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-9XrJS3FWvJcDn4AWNf75Fh.jpeg)

The realm for our Game Store app is ready. Next, let’s add a user.

### **Step 3: Add a user**

User management is very straightforward in Keycloak. Just head to the **Users** section and click **Create new user**:

![Keycloak 'Users' section with 'Create new user' button highlighted](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-busXuCAneosffd5JYPX1Hq.jpeg)

In the next screen, enter your user details, turn on _Email verified_ (to keep things simple), and hit **Create**:

![Keycloak 'Create user' form with user details and 'Email verified' toggle](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-9wHpiWDeMb8SH1DCuFVH7P.jpeg)

Here you also want to set a password for your user, so next click on the **Credentials** tab and click on **Set password**:

![Keycloak user 'Credentials' tab with 'Set password' button](https://juliocasal.com/assets/images/2025-02-01/4ghDFAZYvbFtvU3CTR72ZN-sx9exmw9vKjgjbmKeBR46m.jpeg)

Any password will do, since this is for local dev. Also, no need to make it temporary.

Next, let’s register our first client.

### **Step 4: