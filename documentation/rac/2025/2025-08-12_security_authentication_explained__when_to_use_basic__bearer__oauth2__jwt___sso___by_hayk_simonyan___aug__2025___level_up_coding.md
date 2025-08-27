```yaml
---
title: "Authentication Explained: When to Use Basic, Bearer, OAuth2, JWT & SSO | by Hayk Simonyan | Aug, 2025 | Level Up Coding"
source: https://levelup.gitconnected.com/authentication-explained-when-to-use-basic-bearer-oauth2-jwt-sso-c3fb0aa083ef
date_published: 2025-08-13T02:28:32.770Z
date_captured: 2025-08-18T12:58:34.512Z
domain: levelup.gitconnected.com
author: Hayk Simonyan
category: security
technologies: [OAuth2, JWT, GitHub, Stripe, Firebase]
programming_languages: []
tags: [authorization, access-control, security, rbac, abac, acl, oauth2, jwt, web-security, api-security, identity-management]
key_concepts: [authorization, authentication-vs-authorization, role-based-access-control, attribute-based-access-control, access-control-lists, delegated-authorization, access-tokens, json-web-tokens]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive explanation of authorization, clearly differentiating it from authentication. It details the three primary authorization models: Role-Based Access Control (RBAC), Attribute-Based Access Control (ABAC), and Access Control Lists (ACL), outlining their characteristics, examples, and trade-offs. The post further explains how real-world applications implement these models using practical mechanisms like OAuth2 for delegated authorization and JSON Web Tokens (JWTs) for token-based access. It emphasizes that most complex systems combine these models and mechanisms to achieve flexible and secure access control.
---
```

# Authentication Explained: When to Use Basic, Bearer, OAuth2, JWT & SSO | by Hayk Simonyan | Aug, 2025 | Level Up Coding

# Authentication Explained: When to Use Basic, Bearer, OAuth2, JWT & SSO

[

![Hayk Simonyan profile picture](https://miro.medium.com/v2/resize:fill:64:64/1*shFuXtkBu9viAG261Fg2iA.png)

](https://hayk-simonyan.medium.com/?source=post_page---byline--c3fb0aa083ef---------------------------------------)

[Hayk Simonyan](https://hayk-simonyan.medium.com/?source=post_page---byline--c3fb0aa083ef---------------------------------------)

Follow

5 min read

·

5 days ago

127

4

Listen

Share

More

![Flow chart showing Login Request leading to Authentication, then Authorization Check, which results in Allowed Actions or Denied Actions.](https://miro.medium.com/v2/resize:fit:700/1*Or_0C6fq8p352Ug4PfCTmg.png)

**Authorization** is the step that comes _after_ authentication.

Once a login request is approved, meaning the system knows who the user is, the next question becomes: **What are they allowed to do?**

That’s where authorization kicks in. It’s the foundation of controlling access, enforcing security, and protecting privacy across modern systems.

![Title slide for "Authorization 101" with three boxes representing RBAC, ABAC, and ACL, each containing a small diagram.](https://miro.medium.com/v2/resize:fit:700/1*fKY-tRIaHtdGpYU1n_EKJw.png)

In this post, you’ll learn how real applications manage permissions using the 3 most common authorization models — **RBAC**, **ABAC**, and **ACL** — plus how tools like **OAuth2** and **JWTs** help enforce these rules in production.

If you prefer video format, I cover all of this in detail here: [Watch the full Authorization Explained video on YouTube](https://youtu.be/9JPnN1Z_iSY)

# What Is Authorization?

Authorization is the process of determining which actions or resources a user is allowed to access **after** they’ve been authenticated.

*   **Authentication** = Who is this user?
*   **Authorization** = What are they allowed to do?

![Flow chart showing Login Request leading to Authentication (who the user is) and then to Authorization (what they can do).](https://miro.medium.com/v2/resize:fit:700/1*I-a1RGxHK_Rv73Ct0L1a8A.png)

# Real-World Example

Let’s say you log into GitHub.

![Diagram illustrating a GitHub example with different users (User A, User B, Admin) having varying permissions for a Repository, explaining authentication vs. authorization.](https://miro.medium.com/v2/resize:fit:700/1*W5JgOecMwW2Fh-HUJseD_w.png)

Authentication confirms your identity.

But whether you can **push code**, **review pull requests**, or **delete a repo,** that’s all defined by GitHub’s **authorization** rules.

# The 3 Main Authorization Models

Most systems use one or a combination of the following models:

![Table summarizing the "Common Authorization Models" (RBAC, ABAC, ACL) with their descriptions and key characteristics.](https://miro.medium.com/v2/resize:fit:700/1*8fgqkYn9MUVNl_ekq-GPvw.png)

# 1\. Role-Based Access Control (RBAC)

In RBAC, users are assigned roles, and roles have specific permissions.

**Example:**

*   Admin → full access
*   Editor → can update content
*   Viewer → read-only

![Diagram illustrating Role-Based Access Control (RBAC), showing users assigned to roles (Admin, Editor, Viewer) which then have specific permissions.](https://miro.medium.com/v2/resize:fit:700/1*Ks8F6pRCDr07cQTaHxHBHg.png)

RBAC is widely used because it’s simple, scalable, and easy to manage. You’ll see it in systems like Stripe dashboards, CMS tools, and most admin panels.

# 2\. Attribute-Based Access Control (ABAC)

ABAC defines access based on user or resource **attributes**, and even **contextual factors** like time or location.

**Example:**

allow if user.department === 'HR' && time < 6PM

![Diagram illustrating Attribute-Based Access Control (ABAC), showing how access is determined by evaluating attributes of the user, resource, and environment against a policy.](https://miro.medium.com/v2/resize:fit:700/1*O0l0FxS__aCD7rs_Rp--oQ.png)

This makes it highly flexible. You could:

*   Restrict access to features by department
*   Limit actions to certain hours
*   Combine user and resource metadata for precise policies

Downside? Complexity. You’ll need a policy engine and a way to manage rules at scale.

# 3\. Access Control Lists (ACL)

In ACL-based systems, **each resource has its own list of permissions** for specific users or groups.

![Diagram illustrating Access Control Lists (ACL), showing a resource with an attached list of permissions for specific users or groups.](https://miro.medium.com/v2/resize:fit:700/1*agBcqY05gKQDEcxEwz5uRg.png)

**Example:** In Google Drive, every file has its own ACL defining who can view, comment, or edit.

This approach is highly granular, but harder to scale when managing millions of resources — unless well-abstracted.

# How Real Apps Use Authorization

Here’s how popular platforms implement these models:

*   **GitHub**: Combines RBAC (owner, collaborator) with repo-level permissions
*   **Stripe**: Defines roles like developer, support, or billing admin
*   **Firebase**: Uses a rule-based engine where developers define access conditions for each resource (a flexible mix of RBAC + ABAC)

Most large systems **mix models** to balance flexibility and control.

# Implementing Authorization in Practice

Now that you understand the _models_, let’s talk about the **mechanisms**.

This is where things like **OAuth2**, **JWTs**, and **access tokens** come in.

# OAuth2: Delegated Authorization

OAuth2 is a protocol that lets users **grant access to their data** in one system to another system _without sharing their credentials_.

**Example:** You let a third-party app read your GitHub repos. GitHub doesn’t give them your password; it gives them a **token** with specific permissions.
OAuth2 defines that flow securely.

![Diagram illustrating the OAuth2 flow, showing a user granting a third-party app access to a service provider's resources via an authorization server.](https://miro.medium.com/v2/resize:fit:700/1*oK80kjmj43gjbE3gudRtPg.png)

This is known as **delegated authorization**, and it’s used in “Login with Google”, “Connect to Slack”, and many APIs.

# Token-Based Authorization (JWTs, Bearer Tokens)

After a user logs in, most systems issue an **access token** (often a JWT) that contains key information like:

*   User ID
*   Roles or scopes
*   Expiration time

![Diagram illustrating Token-Based Authorization, showing a client sending a request with an access token to a backend, which verifies the token and checks permissions.](https://miro.medium.com/v2/resize:fit:700/1*VS7KchFKCw80aHWYr8xcxA.png)

When the client sends this token with a request, the backend:

1.  Verifies it
2.  Reads the claims
3.  Checks what actions the user is allowed to perform (using RBAC, ABAC, etc.)

**Tokens carry identity, but your backend defines permission logic.**

Tokens are a transport mechanism, not a decision-making engine.

# Putting It All Together

Authorization isn’t just about who got in, it’s about controlling what happens _after_.

To recap:

*   **RBAC**: Define roles and assign permissions (simple, scalable)
*   **ABAC**: Use attributes and conditions for fine-grained rules
*   **ACL**: Attach access lists to each resource (granular, but harder to scale)
*   **OAuth2** and **JWTs**: Enforce those rules across systems and clients

Most real systems blend these models to balance flexibility, performance, and security.

# Want to go deeper with real-world examples?

I cover all of this, plus examples, in my YouTube video below:

It’s a practical walkthrough designed to help you use this in interviews and real projects.