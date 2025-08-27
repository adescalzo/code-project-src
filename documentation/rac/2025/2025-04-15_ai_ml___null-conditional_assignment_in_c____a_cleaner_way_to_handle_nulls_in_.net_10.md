```yaml
---
title: 💡Null-Conditional Assignment in C# – A Cleaner Way to Handle Nulls in .NET 10
source: https://www.arungudelli.com/csharp-tips/null-conditional-assignment-in-csharp/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2040
date_published: 2025-04-15T23:00:00.000Z
date_captured: 2025-08-12T12:18:41.114Z
domain: www.arungudelli.com
author: Unknown
category: ai_ml
technologies: [.NET 10 Preview 3, C# 14, Visual Studio 2022 v17.14 Preview 2, dotnet CLI]
programming_languages: [C#]
tags: [csharp, dotnet, null-conditional, language-feature, null-safety, code-conciseness, preview-feature, csharp-14]
key_concepts: [null-conditional assignment, null-safety, language features, code conciseness, compiler errors, preview features]
code_examples: false
difficulty_level: intermediate
summary: |
  The article introduces the null-conditional assignment feature in C# 14, part of .NET 10 Preview 3, which streamlines handling nulls by allowing direct assignment to properties of potentially null objects using the `?.` operator. It demonstrates how this new syntax `object?.Property = value;` replaces traditional `if (object is not null)` checks, leading to cleaner, more concise, and safer code that prevents `NullReferenceException`. The post includes practical code examples with `UserProfile` and `ProfileService` classes, illustrating its real-world application. It also provides clear instructions on how to enable and test this preview feature using the .NET SDK and `dotnet` CLI, noting current limitations with Visual Studio IDE support.
---
```

# 💡Null-Conditional Assignment in C# – A Cleaner Way to Handle Nulls in .NET 10

1.  [Home](/)
2.  [Csharp tips](/csharp-tips/)
3.  💡Null-Conditional Assignment in C# – A Cleaner Way to Handle Nulls in .NET 10

# 💡Null-Conditional Assignment in C# – A Cleaner Way to Handle Nulls in .NET 10

_**Published:** Apr 16, 2025 **Last Updated:** Apr 16, 2025_

---

On this page ExpandCollapse

***   [🌱 The Problem: Repetitive Null Checks](#-the-problem-repetitive-null-checks)*****   [✨ The New Way: Null-Conditional Assignment](#-the-new-way-null-conditional-assignment)
    ***   [⚠️ Note: This Would Fail in Earlier Versions](#-note-this-would-fail-in-earlier-versions)*******   [✅ What This Means for Your Code](#-what-this-means-for-your-code)*****   [🧪 Real-World Example](#-real-world-example)
    ***   [🧱 Supporting Classes: `UserProfile` and `ProfileService`](#-supporting-classes-userprofile-and-profileservice)*****   [🚀 Main Program: Using the Classes with Null-Safe Updates](#-main-program-using-the-classes-with-null-safe-updates)*******   [⚙️ How to Try This Feature](#-how-to-try-this-feature)
    ***   [✅ Install .NET 10 Preview 3](#-install-net-10-preview-3)*****   [✅ Set the Language Version to Preview](#-set-the-language-version-to-preview)*****   [⚠️ Not Yet Supported in Visual Studio](#-not-yet-supported-in-visual-studio)*****   [✅ Use the `dotnet` CLI](#-use-the-dotnet-cli)*******   [✍️ Final Thoughts](#-final-thoughts)*****   [📌 TL;DR](#-tldr)**

### On this page

***   [🌱 The Problem: Repetitive Null Checks](#-the-problem-repetitive-null-checks)*****   [✨ The New Way: Null-Conditional Assignment](#-the-new-way-null-conditional-assignment)
    ***   [⚠️ Note: This Would Fail in Earlier Versions](#-note-this-would-fail-in-earlier-versions)*******   [✅ What This Means for Your Code](#-what-this-means-for-your-code)*****   [🧪 Real-World Example](#-real-world-example)
    ***   [🧱 Supporting Classes: `UserProfile` and `ProfileService`](#-supporting-classes-userprofile-and-profileservice)*****   [🚀 Main Program: Using the Classes with Null-Safe Updates](#-main-program-using-the-classes-with-null-safe-updates)*******   [⚙️ How to Try This Feature](#-how-to-try-this-feature)
    ***   [✅ Install .NET 10 Preview 3](#-install-net-10-preview-3)*****   [✅ Set the Language Version to Preview](#-set-the-language-version-to-preview)*****   [⚠️ Not Yet Supported in Visual Studio](#-not-yet-supported-in-visual-studio)*****   [✅ Use the `dotnet` CLI](#-use-the-dotnet-cli)*******   [✍️ Final Thoughts](#-final-thoughts)*****   [📌 TL;DR](#-tldr)**

While working with the latest **.NET 10 Preview 3**, I came across a new language feature in C# 14 that simplifies how we assign values to nullable objects: **null-conditional assignment**.

It’s a small addition, but it helps reduce repetitive null checks and makes your code cleaner.

Let me show you how it works with a simple example.

---

## 🌱 The Problem: Repetitive Null Checks [#](#-the-problem-repetitive-null-checks)

```csharp
public class Customer
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class UpdateCustomer
{
    public static void UpdateAge(Customer? customer, int newAge)
    {
        if (customer is not null)
        {
            customer.Age = newAge;
        }
    }
}
```

This is a perfectly valid approach—but it feels a bit verbose for such a simple task.

We’re just ensuring the object isn’t null before performing an assignment.

---

![C# Null-Conditional Assignment: An image showing the C# logo and the syntax `customer?.Age = newAge;` on a purple background, illustrating the core concept of the article.](/images/null-conditional-assignment-csharp_huacffb5bb7ff7431b6d1127723d876c47_335850_1536x0_resize_q75_h2_box_3.webp)

Null-Conditional Assignment C#

## ✨ The New Way: Null-Conditional Assignment [#](#-the-new-way-null-conditional-assignment)

```csharp
public static void UpdateAge(Customer? customer, int newAge)
{
    customer?.Age = newAge;
}
```

The `?.` operator ensures that the assignment happens **only if `customer` is not null**.

If it is null, nothing happens—no exception, no error, just a silent skip.

### ⚠️ Note: This Would Fail in Earlier Versions [#](#-note-this-would-fail-in-earlier-versions)

If you try to use this syntax in earlier C# versions (prior to C# 14), you’ll get a compiler error like:

```fallback
The left-hand side of an assignment must be a variable, property or indexer
```

That’s because `customer?.Age` wasn’t considered a valid assignable target until C# 14.

---

## ✅ What This Means for Your Code [#](#-what-this-means-for-your-code)

*   🔍 Removes the need for manual null checks
*   🧼 Makes assignments more concise
*   🛡️ Protects from `NullReferenceException`
*   💡 Supported by IDEs (once fully available)

Example use:

```csharp
order?.Status = "Shipped";
user?.LastLogin = DateTime.UtcNow;
config?.RetryCount = 3;
```

---

## 🧪 Real-World Example [#](#-real-world-example)

Let’s simulate a practical use case: an application receives optional user profile data from an API, and we want to update it only if the data exists.

### 🧱 Supporting Classes: `UserProfile` and `ProfileService` [#](#-supporting-classes-userprofile-and-profileservice)

```csharp
using System;

namespace NullConditionalAssignmentDemo
{
    public class UserProfile
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public int Age { get; set; }

        public void Print()
        {
            Console.WriteLine($"DisplayName: {DisplayName ?? "N/A"}");
            Console.WriteLine($"Bio: {Bio ?? "N/A"}");
            Console.WriteLine($"Age: {Age}");
        }
    }

    public class ProfileService
    {
        public UserProfile? GetProfileFromApi(string username)
        {
            if (username.ToLower() == "ghost") return null;

            return new UserProfile
            {
                DisplayName = "Arun Gudelli",
                Bio = "C# Developer | Tech Blogger",
                Age = 29
            };
        }

        public void UpdateProfile(UserProfile? profile, string? newBio, int? newAge)
        {
            profile?.Bio = newBio;
            if (newAge.HasValue)
                profile?.Age = newAge.Value;
        }
    }
}
```

**Explanation:**

*   `UserProfile` is a basic data model class with nullable properties and a `Print()` method for displaying values.
*   `ProfileService` simulates external API behavior. If the input is `"ghost"`, it returns `null`, representing a failed or empty response.
*   The `UpdateProfile` method uses null-conditional assignment to update the profile safely, without any `if (profile != null)` checks.

One important takeaway here is that we can still safely call `UpdateProfile` even if the profile is `null`.

Normally, this might lead to a `NullReferenceException`, but thanks to **null-conditional assignment**, the assignments are silently skipped if the object is `null`.

In the `UpdateProfile` method, we use statements like `profile?.Bio = newBio` which ensures that the assignment only occurs if profile is not `null`.

Even if the caller passes a `null` profile (like when the `user` is `ghost`), the method runs without any exceptions.

The same goes for age—if we have a non-null value (`int?`), we assign it to `profile?.Age = newAge.Value`, which again runs only if the profile is not `null`.

This design pattern simplifies calling logic—no need to check for null before every update.

The update method itself safely handles it, keeping the code both neat and expressive.

---

### 🚀 Main Program: Using the Classes with Null-Safe Updates [#](#-main-program-using-the-classes-with-null-safe-updates)

```csharp
using System;

namespace NullConditionalAssignmentDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new ProfileService();

            string username1 = "arun";
            string username2 = "ghost";

            var profile1 = service.GetProfileFromApi(username1);
            var profile2 = service.GetProfileFromApi(username2);

            string? updatedBio = "Updated bio from UI";
            int? updatedAge = 30;

            service.UpdateProfile(profile1, updatedBio, updatedAge);
            service.UpdateProfile(profile2, updatedBio, updatedAge);

            Console.WriteLine("Profile 1 (arun):");
            profile1?.Print();

            Console.WriteLine("Profile 2 (ghost):");
            profile2?.Print();
        }
    }
}
```

**Explanation:**

*   In this main program, we simulate two scenarios:
    *   One where a valid profile is returned (`arun`)
    *   Another where the user does not exist and the method returns `null` (`ghost`)
*   Both profiles are passed to the `UpdateProfile` method.
*   The method contains null-conditional assignments like `profile?.Bio = newBio`, which means the update only happens if the profile is not null.
*   Even if `profile` is `null`, the method executes safely without throwing any exception.
*   This eliminates the need for the caller to check if the object is null before calling the method.
*   Finally, we use `profile?.Print()` to display the result. For `arun`, it prints the updated profile. For `ghost`, nothing is printed, but no error is thrown.
*   This demonstrates how null-conditional access and assignment provide safe and readable ways to deal with possibly-null objects.

**Output**:

```fallback
Profile 1 (arun):
DisplayName: Arun Gudelli
Bio: Updated bio from UI
Age: 30
Profile 2 (ghost):
```

---

## ⚙️ How to Try This Feature [#](#-how-to-try-this-feature)

### ✅ Install .NET 10 Preview 3 [#](#-install-net-10-preview-3)

Get the SDK from [.NET Downloads](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

### ✅ Set the Language Version to Preview [#](#-set-the-language-version-to-preview)

Update your `.csproj`:

```xml
<PropertyGroup>
  <LangVersion>preview</LangVersion>
</PropertyGroup>
```

---

### ⚠️ Not Yet Supported in Visual Studio [#](#-not-yet-supported-in-visual-studio)

As of now, **Visual Studio 2022 v17.14 Preview 2** still shows:

```fallback
The left-hand side of an assignment must be a variable, property or indexer
```

Even with the correct SDK and LangVersion.

---

### ✅ Use the `dotnet` CLI [#](#-use-the-dotnet-cli)

Instead, build your project using:

```bash
dotnet build
```

This works perfectly with null-conditional assignment using C# 14.

So until Visual Studio catches up with full support in the editor, use the `dotnet` CLI to test it out.

---

## ✍️ Final Thoughts [#](#-final-thoughts)

This small feature reduces code noise, improves clarity, and protects against null reference errors—all while staying elegant.

---

## 📌 TL;DR [#](#-tldr)

*   ✅ Introduced in .NET 10 Preview 3
*   🔁 Assign values only when the object is non-null
*   🧪 Use `<LangVersion>preview</LangVersion>` in `.csproj`
*   🧱 Not yet fully supported in Visual Studio
*   ✅ Use `dotnet build` CLI to test

Don’t be a stranger! Follow me on social media

*   [GitHub](https://github.com/arungudelli/)
*   [Twitter](https://twitter.com/arungudelli)
*   [Linkedin](https://www.linkedin.com/in/arungudelli/)
*   [Facebook](https://www.facebook.com/gudelliarun)

### 👋 Stay in the loop

Get a short & sweet tutorials delivered to your inbox every couple of days. No spam ever. Unsubscribe any time.

Subscribe

 

Your subscription has been successful.

Your subscription could not be saved. Please try again.

[

Positional Patterns in C#: The What, Why, and How →



](/csharp-tips/positional-patterns-csharp/)