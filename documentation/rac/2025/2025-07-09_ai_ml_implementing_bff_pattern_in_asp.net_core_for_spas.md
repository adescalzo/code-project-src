```yaml
---
title: Implementing BFF Pattern in ASP.NET Core for SPAs
source: https://nestenius.se/net/implementing-bff-pattern-in-asp-net-core-for-spas/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2088
date_published: 2025-07-09T12:05:29.000Z
date_captured: 2025-08-17T22:12:16.866Z
domain: nestenius.se
author: Tore Nestenius
category: ai_ml
technologies: [ASP.NET Core, OpenID Connect, OAuth 2.0, Duende BFF Library, Duende IdentityServer, npm, HTTP-only cookies, Same-site protection]
programming_languages: [C#, JavaScript]
tags: [bff-pattern, security, authentication, authorization, openid-connect, oauth, aspnet-core, spa, web-security, dotnet]
key_concepts: [backend-for-frontend-pattern, single-page-applications, openid-connect, oauth-2.0, cross-site-scripting, token-management, session-management, attack-surface-reduction]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Backend-for-Frontend (BFF) pattern as a secure authentication solution for Single-Page Applications (SPAs) using ASP.NET Core. It highlights the significant security risks associated with direct OpenID Connect (OIDC) implementation in SPAs, particularly the storage and potential theft of tokens in the browser via XSS attacks. The BFF pattern mitigates these risks by centralizing token management in the backend and utilizing secure browser features like HTTP-only cookies for session handling. The author emphasizes that this approach reduces frontend complexity and aligns with official OAuth 2.0 recommendations for browser-based applications. This post serves as the first part of a series, laying the groundwork for a complete BFF implementation.]
---
```

# Implementing BFF Pattern in ASP.NET Core for SPAs

[PrevPrevious](https://nestenius.se/net/how-to-use-kurrentdb-for-event-sourcing-in-c-on-azure/)

[NextNext](https://nestenius.se/net/bff-in-asp-net-core-2-the-bff-pattern-explained/)

![Cover image for the blog post series, titled "Implementing BFF Pattern in ASP.NET Core for SPAs Part One: An Introduction" with a green and purple geometric background.](https://nestenius.se/wp-content/uploads/2025/07/Tore-blog-cover-images-BFF1.png)

# Implementing BFF Pattern in ASP.NET Core for SPAs

*   [July 9, 2025](https://nestenius.se/2025/07/09/)

This multi-part blog series will show you how to implement secure authentication for Single-Page Applications using the **Backend-for-Frontend (BFF)** pattern with ASP.NET Core.

We’ll explore why handling OpenID Connect directly in SPAs creates security risks, then build a complete BFF implementation that eliminates browser token storage and follows OAuth 2.0 best practices. In short, it will help you to build secure applications with less complexity in the frontend!

As this is a big topic, I’m planning to release a new post in this series approximately once a week, with each part building on the previous one. You can jump to the section you need, but for background and context, it’s best to start here:

*   **Part 1 – Introduction**
*   [Part 2 – Introducing the Backend-for-Frontend (BFF) pattern](https://nestenius.se/net/bff-in-asp-net-core-2-the-bff-pattern-explained/)
*   [Part 3 – Securing the Cookie Session](https://nestenius.se/net/bff-in-asp-net-core-3-the-bff-pattern-explained/)
*   [Part 4 – Implementing a BFF in ASP.NET Core](https://nestenius.se/net/bff-in-asp-net-core-4-implementing-a-bff-from-scratch/)
*   [Part 5 – Automatic Token Renewal](https://nestenius.se/net/bff-in-asp-net-core-5-automatic-token-renewal/)
*   [Part 6 – Securing the BFF using CORS](https://nestenius.se/net/bff-in-asp-net-core-6-securing-our-bff-with-cors/)
*   Part 7 – Introducing the Duende BFF Library (coming soon)

## The Problem with Direct OIDC in SPAs

In my work, I regularly encounter development teams implementing OpenID Connect authentication directly in their Single-Page Applications. The typical flow looks like this: 

1.  The browser-based frontend authenticates directly with the OIDC provider.
2.  The OIDC JavaScript library stores the received tokens in the browser.
3.  Uses them to call backend APIs.

![Diagram illustrating the insecure direct OpenID Connect flow in SPAs. A browser authenticates with an Identity Provider and then uses access tokens stored in the browser to directly call Payment API, Sales API, and Order API.](https://nestenius.se/wp-content/uploads/2025/07/Image-1.png)

This approach seems straightforward, but it introduces significant security risks and complexity.

**so what exactly are these risks?  
**

## The Problem With Tokens In the Browser

Let’s explore the key problems with this approach:

*   **Protecting the tokens:**  
    Storing tokens (access token, ID token, and refresh token) in the browser puts them at risk of theft through cross-site scripting (XSS) attacks or malicious third-party scripts. For example, a compromised browser extension could easily access tokens stored in localStorage, giving attackers full access to your APIs.
*   **Increased attack surface:**  
    Adding OpenID Connect support directly to your SPA means handling tokens, redirects, and callbacks in JavaScript. This prevents you from utilizing secure browser features, such as HTTP-only cookies and same-site protections, while significantly increasing your application’s attack surface.
*   **Added Complexity:**  
    OpenID Connect and token handling introduce a lot of complexity to your frontend. Even with libraries, you need to understand token flows, configure edge cases like renewal and error handling, and manage logout scenarios. This increases security-critical code that requires careful writing, testing, and review.
*   **Using a trusted library:**  
    Searching npm for “[oidc](https://www.npmjs.com/search?q=oidc)” returns thousands of JavaScript libraries, but only a small number are [certified](https://openid.net/developers/certified-openid-connect-implementations/) by the OpenID Foundation. Using an unmaintained or insecure library can introduce subtle bugs or serious security vulnerabilities.

These problems are well-documented in the security community. Rather than diving deep into every detail, I recommend these excellent talks that explain the issues clearly:

*   [Using the BFF pattern to secure SPA and Blazor Applications](https://www.youtube.com/watch?v=UBFx3MSu1Rc)
*   [alert‘OAuth 2 0’; // The impact of XSS on OAuth 2.0 in SPAs](https://www.youtube.com/watch?v=lEnbi4KClVw)
*   [Web Security and BFF with Philippe De Ryck](https://www.youtube.com/watch?v=urS9wstmN2U)  
    Philippe is one of the authors of the OAuth 2.0 for Browser-Based Applications guide.

**Curious about OpenID-Connect?**  
Head over to my [OpenID Connect for Developers](http://tn-data.se/openid-connect/) article for an accessible, in-depth introduction to OpenID Connect for developers.

## The Impact of Stolen Tokens

The impact extends far beyond browser access. Once an attacker exfiltrates these tokens, they gain portable, offline access to your APIs. The stolen access token can be used from any client application or system. The attacker doesn’t need continued access to the victim’s browser.  
  
This means they can:

*   Call your APIs from their own systems.
*   Use automated tools to exploit the token.
*   Continue accessing resources until the token expires (potentially hours or days later).
*   Potentially use the token across multiple applications if it has a broad scope.

Refresh tokens make this even worse. If a refresh token is also stolen, the attacker can:

*   Generate new access tokens indefinitely.
*   Maintain persistent access even after the original access token expires.
*   Continue the attack for weeks or months until the refresh token is revoked or expired.

This transforms a temporary browser compromise into a persistent, distributed security threat that’s much harder to detect and contain.

## The Solution: the Backend-for-Frontend (BFF) Pattern

So what’s the better approach? Instead of handling authentication directly in the SPA, we move all token management to the backend and use a simple session cookie for communication between the frontend and backend.

This creates a much cleaner and more secure architecture:

![Diagram illustrating the secure Backend-for-Frontend (BFF) pattern. A browser communicates with a BFF backend using a session cookie, and the BFF handles authentication with the Identity Provider and secure calls to backend APIs.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI4NjkiIGhlaWdodD0iNDYwIiB2aWV3Qm94PSIwIDAgODY5IDQ2MCI+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgc3R5bGU9ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)

## Key principles of the BFF approach:

*   **No tokens in the browser:**  
    Eliminating tokens from the frontend removes a major security risk and reduces complexity. No more worrying about XSS attacks stealing your tokens or trying to implement secure storage in JavaScript.
*   **Use the browser’s built-in security features:**  
    Without frontend token management, you can fully utilize HTTP-only cookies, same-site protection, and secure flags. All of these are battle-tested browser security mechanisms that work without additional configuration.
*   **Keep the frontend simple and focused:**  
    Your SPA can concentrate on what it does best: user interface and experience. Authentication complexity stays in the backend where it belongs.
*   **Centralize trust and session handling in the backend:**  
    The backend handles the OpenID Connect flow, manages sessions, and securely stores tokens. This makes logging, monitoring, and auditing much easier.
*   **Reducing complexity:**  
    Moving the authentication into the backend reduces the number of moving parts in the frontend and cuts down the attack surface. This also means fewer security concerns for frontend developers to worry about.

This approach is called the **Backend-for-Frontend (BFF)** **pattern**. It’s not a new concept, but it’s particularly effective for securing modern SPAs.

**What you’ll learn:** By the end of this series, you’ll have a solid starter implementation that eliminates browser token storage, leverages HTTP-only cookies, and centralizes authentication in ASP.NET Core, giving you a strong foundation for your own BFF journey.

## Official Recommendation

The BFF pattern isn’t just a theoretical improvement; it’s recognized as the current best practice by the OAuth working group. The IETF draft specification for [OAuth 2.0 for Browser-Based Apps](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-browser-based-apps) explicitly states:  
  
_“This architecture is strongly recommended for business applications, sensitive applications, and applications that handle personal data.”_  
  
This official endorsement reinforces the message that moving authentication to the backend is more than a matter of convenience; it is also about meeting established security standards suited to modern web applications in today’s world.

## Why I Created This Series

I wrote this series for two main reasons, both rooted in real-world experience.

First, I wanted to deepen my own understanding of BFF implementations. Reading about authentication patterns is one thing, but actually building, testing, and troubleshooting them reveals nuances you can’t get from just theory alone.

Second, I needed a comprehensive, working example for the security workshops I run with development teams. When I’m explaining secure SPA architectures, nothing beats having actual ASP.NET Core code that teams can examine, run, and adapt to their own projects.

You can learn more about [these security workshops here](https://tn-data.se/courses/).

These workshops focus on practical security implementation: the kind of hands-on knowledge that helps teams avoid common pitfalls and build more secure applications from the start.

## Ready for the Deep Dive?

This deep dive follows that same hands-on philosophy – we’ll build a complete BFF implementation together so you can see exactly how it works under the hood (and hopefully with fewer headaches, too!).

In Part 2, we’ll explore exactly how the BFF pattern works and why it solves these security challenges.  
  
**To [part 2 – The BFF Pattern Explained](https://nestenius.se/net/bff-in-asp-net-core-2-the-bff-pattern-explained/).**

## About the author

Hey! I’m [Tore](https://www.linkedin.com/in/torenestenius/) 👋 I’m an independent consultant, trainer, and coach for developers in the world of authentication, authorization, and beyond. I help teams to build secure, scalable, and visible applications with a specialism in Duende IdentityServer, OIDC, and OAuth. Interested in [training](https://tn-data.se/courses/), consulting, or an extra pair of hands for your web security skills or project? You can find out more [here](https://tn-data.se/duende-identityserver-consulting-services/). 

## More blog posts by the author

*   [IdentityServer – IdentityResource vs. ApiResource vs. ApiScope](https://nestenius.se//net/identityserver-identityresource-vs-apiresource-vs-apiscope/)
*   [Debugging OpenID Connect Claim Problems in ASP.NET Core](https://nestenius.se//net/missing-openid-connect-claims-in-asp-net-core/)
*   [Pushed Authorization Requests (PAR) in ASP.NET Core 9](https://nestenius.se//net/demystifying-openid-connects-state-and-nonce-parameters-in-asp-net-core/)
*   [Demystifying OpenID Connect’s State and Nonce Parameters in ASP.NET Core](https://nestenius.se//net/demystifying-openid-connects-state-and-nonce-parameters-in-asp-net-core/)

## Share This Story

## About The Author

![Placeholder for author's profile image.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMDAiIGhlaWdodD0iMzAwIiB2aWV3Qm94PSIwIDAgMzAwIDMwMCI+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgc3R5bGU9ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)

Hi, I’m Tore! I have been fascinated by computers since I unpacked my first Commodore VIC-20. I am a Microsoft MVP in .NET and  I provide freelance **[application development,](https://tn-data.se/consulting/)** developer **[training](https://tn-data.se/courses/)** and **[coaching](https://tn-data.se/coaching/)** services, focusing on ASP.NET Core, IdentityServer, OpenID Connect, Architecture, and Web Security. Let’s connect on [**LinkedIn**](https://www.linkedin.com/in/torenestenius) and **[Twitter!](https://twitter.com/Tndata)**

## Related Posts

[

![Placeholder for a related blog post cover image.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDI0IiBoZWlnaHQ9IjUxMiI vZlZld0JveD0iMCAwIDEwMjQgNTEyIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT0iZmlsbDojY2ZkNGRiO2ZpbGwtb3BhY2l0eTogMC4xOyIvPjwvc3ZnPg==)

](https://nestenius.se/net/bff-in-asp-net-core-6-securing-our-bff-with-cors/)

### [BFF in ASP.NET Core #6 – Securing our BFF with CORS](https://nestenius.se/net/bff-in-asp-net-core-6-securing-our-bff-with-cors/)

[Read More »](https://nestenius.se/net/bff-in-asp-net-core-6-securing-our-bff-with-cors/ "Read more about BFF in ASP.NET Core #6 – Securing our BFF with CORS")

[

![Placeholder for a related blog post cover image.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDI0IiBoZWlnaHQ9IjUxMiIgdmlld0JveD0iMCAwIDEwMjQgNTEyIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT0iZmlsbDojY2ZkNGRiO2ZpbGwtb3BhY2l0eTogMC4xOyIvPjwvc3ZnPg==)

](https://nestenius.se/net/bff-in-asp-net-core-5-automatic-token-renewal/)

### [BFF in ASP.NET Core #5 – Automatic Token Renewal](https://nestenius.se/net/bff-in-asp-net-core-5-automatic-token-renewal/)

[Read More »](https://nestenius.se/net/bff-in-asp-net-core-5-automatic-token-renewal/ "Read more about BFF in ASP.NET Core #5 – Automatic Token Renewal")

[

![Placeholder for a related blog post cover image.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDI0IiBoZWlnaHQ9IjUxMiIgdmlld0JveD0iMCAwIDEwMjQgNTEyIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT0iZmlsbDojY2ZkNGRiO2ZpbGwtb3BhY2l0eTogMC4xOyIvPjwvc3ZnPg==)

](https://nestenius.se/net/bff-in-asp-net-core-4-implementing-a-bff-from-scratch/)

### [BFF in ASP.NET Core #4 – Implementing a BFF from scratch](https://nestenius.se/net/bff-in-asp-net-core-4-implementing-a-bff-from-scratch/)

[Read More »](https://nestenius.se/net/bff-in-asp-net-core-4-implementing-a-bff-from-scratch/ "Read more about BFF in ASP.NET Core #4 – Implementing a BFF from scratch")

## Do You Want Tore To Be Your Mentor?

[Hire Me](https://tn-data.se/consulting/#coaching)

## Services 🚀

I offer training and coaching for professional developers and consulting services for startups and enterprises. Find out more on my business website. 

[Find Out More](https://tn-data.se/consulting/)

## Tore’s Newsletter

**Be the First to Know!** Get notified about my latest blog posts, upcoming presentations, webinars, and more — **subscribe today!**

[](https://tores-dev-newsletter.beehiiv.com)

[Let me read it first!](https://tores-dev-newsletter.beehiiv.com)

![Cartoon illustration of Tore Nestenius.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxNzgiIGhlaWdodD0iMTc4IiB2aWV3Qm94PSIwIDAgMTc4IDE3OCI+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgc3R5bGU9ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)

![Profile picture of Tore Nestenius.](https://nestenius.se/wp-content/uploads/2022/01/Tore-Nestenius-6-1-opt.jpg.webp)

![Microsoft MVP Badge.](https://nestenius.se/wp-content/uploads/2024/10/Tore-Microsoft-MVP-Badge-2-300x121.png.webp)

## About me

Hi! I’m Tore Nestenius. I’m a trainer and senior software developer focusing on Architecture, Security & Identity,