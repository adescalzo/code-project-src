# Only Admins Allowed: How to Lock Down Your REST API with Roles | by Code Crack | Dot Net, API & SQL Learning | Jul, 2025 | Medium

**Source:** https://medium.com/dot-net-sql-learning/only-admins-allowed-how-to-lock-down-your-rest-api-with-roles-b6845cf74b1e
**Date Captured:** 2025-07-28T16:15:51.698Z
**Domain:** medium.com
**Author:** Code Crack
**Category:** database

---

Member-only story

# Only Admins Allowed: How to Lock Down Your REST API with Roles

[

![Code Crack](https://miro.medium.com/v2/resize:fill:64:64/1*8CX9MO7Uy5K2at7LiQ4-Wg.png)





](/@CodeCrack?source=post_page---byline--b6845cf74b1e---------------------------------------)

[Code Crack](/@CodeCrack?source=post_page---byline--b6845cf74b1e---------------------------------------)

Follow

3 min read

·

Jul 20, 2025

15

1

Listen

Share

More

_“If a viewer can delete an admin’s resource — that’s not a bug, it’s a huge security vulnerability.”_

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*q0byOuzNIqe-m57IRGs2KA.jpeg)

Only Admins Allowed (Created in ChatGPT)

## Benefits of Role-Based Access:

*   **_Protects sensitive APIs_**
*   **_Shares user responsibilities_**
*   **_Prevents unnecessary access_**
*   **_Enables compliance with GDPR, HIPAA, or SOC2_**

# Real API Example

Let’s say you have created an API where users can be deleted via request:

DELETE /api/users/123  
Authorization: Bearer eyJhbGciOiJIUzI1...

You want only users with the “**Admin**” role to be able to do this. Then you need to verify the user’s role from the **JWT token** and provide access control.

# Implementation Guide (ASP.NET Core Example)

## Step 1: Add Role Claim to JWT Token

var claims = new List<Claim>  
{  
    new Claim(ClaimTypes.Name, user.Username),  
    new Claim(ClaimTypes.Role, "Admin") // <-- Roll added here  
};

## Step 2: Implement Role-Based Authorization in the API

\[Authorize(Roles = "Admin")\]  
\[HttpDelete("api/users/{id}")\]  
public IActionResult DeleteUser(int id)  
{  
    \_userService.Delete(id);  
    return Ok();  
}

If the user’s role is not “**Admin**”, they will not have access to this endpoint.

# What if you use Node.js?

## Express + JWT + Middleware example:

function authorizeRoles(...roles) {  
  return (req, res, next) => {  
    const user = req.user;  
    if (!roles.includes(user.role)) {  
      return res.status(403).json({ message: "Access denied" });  
    }  
    next();  
  };  
}

Usage:

app.delete('/api/users/:id', authenticateJWT, authorizeRoles('Admin'), deleteUser);

[

## How do you handle partial updates (PATCH vs PUT)?

### Do you know the correct way to update data in your REST API?

medium.com



](/dot-net-sql-learning/how-do-you-handle-partial-updates-patch-vs-put-25890bb6101c?source=post_page-----b6845cf74b1e---------------------------------------)

# Extra Security Layers (Advanced Level of Security)

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*u9bZLkbcNkENf-eE522Uzw.png)

# Avoid Common Mistakes

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*jBQs3lUlSimb4jyGrrPpRg.png)

# Keep an Audit Log

*   Who accessed which API and when?
*   Which role was used to access it?
*   What sensitive actions were taken?

Along with Role-Based Access, logging is also important.

# Real World Use Case

## Healthcare App

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*hKEPe1iJHBq6ZaTN7KuTWg.png)

# Comparison of performance and security

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*uWuQD5xcxBUovuLbv1qZCg.png)

# Briefly, things to remember

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*DNJlazlFtbvv3w1GVE_nZg.png)

In today’s era, API security is not limited to passwords or JWT tokens. If you want a secure, scalable, and compliance-ready REST API, Role-Based Authorization is a must.

Clearly define each resource —  
**Who can see it? Who can edit it? Who can delete it?**

**Read more blogs** 👉

[

## Think Your API Is Safe? Not Without Rate Limiting!

### In today’s digital world, every application, website, and mobile app is powered by a powerful API. But did you know —…

medium.com



](/dot-net-sql-learning/think-your-api-is-safe-not-without-rate-limiting-db1811607f9d?source=post_page-----b6845cf74b1e---------------------------------------)

[

## var vs dynamic: The Battle That’s Breaking Your C# Code!

### Every C# developer comes across these two keywords at some point in their lives — var and dynamic. At first glance, it…

medium.com



](/dot-net-sql-learning/var-vs-dynamic-the-battle-thats-breaking-your-c-code-f19936dbf35d?source=post_page-----b6845cf74b1e---------------------------------------)

[

## Revoking JWTs Is “Impossible”… Until You Learn This Trick!

### If you are developing any modern web or mobile application, then you must have heard the name JWT (JSON Web Token). It…

medium.com



](/@CodeCrack/revoking-jwts-is-impossible-until-you-learn-this-trick-595b74b47c39?source=post_page-----b6845cf74b1e---------------------------------------)

**_Disclaimer :_** _This article was written with the assistance of AI tools (e.g.,_ **_ChatGPT, Google Translator_**_), and further edited for clarity and originality. The purpose of this blog is solely for_ **_tutorial_** _and_ **_knowledge-sharing_**_, and it is not intended to_ **_copy_** _or_ **_infringe_** _upon any_ **_copyrighted material_**_._