```yaml
---
title: How to Sync Users Logged in by Google and GitHub to Your Database With Neon Auth for Free
source: https://antondevtips.com/blog/how-to-sync-users-logged-in-by-google-and-github-to-your-database-with-neon-auth-for-free
date_published: 2025-04-22T07:45:23.464Z
date_captured: 2025-08-06T16:47:37.909Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [Neon Auth, PostgreSQL, Google, GitHub, Stack Auth, Next.js, NodeJS, VS Code, Rider, .NET, ASP.NET Core, Entity Framework Core, Azure Marketplace]
programming_languages: [C#, SQL, JavaScript, Bash]
tags: [authentication, user-management, database-sync, postgresql, dotnet, web-api, data-access, cloud, saas, developer-tools]
key_concepts: [external-authentication, data-synchronization, database-integration, orm, api-development, automated-sync, user-profiles]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article demonstrates how to automatically synchronize user data from external authentication providers like Google and GitHub to a PostgreSQL database using Neon Auth. It highlights the complexities of manual synchronization and how Neon Auth simplifies this process by creating and managing a `neon_auth.users_sync` table. The guide includes steps for setting up Neon Auth with a Next.js frontend and integrating the synchronized user data into a .NET API application using Entity Framework Core. The core benefit is eliminating the need for custom code, webhooks, or extra tables for user profile management.]
---
```

# How to Sync Users Logged in by Google and GitHub to Your Database With Neon Auth for Free

![A dark blue and purple abstract background with "dev tips" logo and the title "HOW TO SYNC USERS LOGGED IN BY GOOGLE AND GITHUB TO YOUR DATABASE WITH NEON AUTH FOR FREE"](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fdotnet%2Fcover_dotnet_neon_auth.png&w=3840&q=100)

# How to Sync Users Logged in by Google and GitHub to Your Database With Neon Auth for Free

Apr 22, 2025

[Download source code](/source-code/how-to-sync-users-logged-in-by-google-and-github-to-your-database-with-neon-auth-for-free)

5 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Manually keeping your database in sync with external authentication providers can be a complex task. You often need webhooks, extra tables, and custom code just to handle user signups and profile updates.

[Neon Auth](https://neon.tech/docs/guides/neon-auth?refcode=44WD03UH) solves this problem by automatically synchronizing user data to your Postgres database in near real-time. No extra code. No fuss. And it's free.

Today I want to show you how I used [Neon Auth](https://neon.tech/docs/guides/neon-auth?refcode=44WD03UH) to synchronize users logged in by Google and GitHub to my Postgres database.

[](#how-neon-auth-simplifies-user-synchronization-to-the-database)

## How Neon Auth Simplifies User Synchronization to the Database

When you rely on external providers like Stack Auth for user authentication, you typically face synchronization challenges:

*   Where do you store user data?
*   Manual synchronization methods implementation like webhooks, polling, or login-time sync

This process might look as follows: ![A flowchart illustrating a complex manual user synchronization process from an Auth Provider to a Postgres Database, involving Webhook Endpoints, Job Queues, Sync Workers, and Retry & Error Handling.](https://antondevtips.com/media/code_screenshots/dotnet/neon-auth/img_8.svg)

**Neon Auth** addresses these headaches by linking your authentication provider directly with your Neon PostgreSQL database. As users log in or update their accounts, the changes automatically propagate into your database: no custom code, no separate webhooks needed.

Key Benefits:

*   Provision and manage your auth projects from the Neon Console.
*   Automated syncing of user profiles into the `neon_auth.users_sync` table.
*   Access your user data like any other table: simplifying relationships, queries, and maintenance.

With Neon Auth user synchronization is a breeze: ![A simplified flowchart showing user synchronization with Neon Auth: Auth Provider -> Neon Auth -> Postgres Database, indicating automated sync.](https://antondevtips.com/media/code_screenshots/dotnet/neon-auth/img_9.svg)

[](#setting-up-neon-auth)

## Setting Up Neon Auth

First, [sign in](https://console.neon.tech?refcode=44WD03UH) or [create](https://console.neon.tech?refcode=44WD03UH) your free account in Neon.

If you come from Azure, you can get started with Neon for free in the [Azure Marketplace](https://azuremarketplace.microsoft.com/en-us/marketplace/apps/neon1722366567200.neon_serverless_postgres_azure_prod?tab=overview&refcode=44WD03UH).

Select "Get It Now" and select a free tier to get started with.

To setup auth, follow these steps:

1.  Create a project and navigate to the Auth Tab, click on "Setup instructions": ![Screenshot of the Neon Console's Auth tab, showing "Setup instructions" for adding Stack Auth to an application, with options to clone a template or add to an existing project.](https://antondevtips.com/media/code_screenshots/dotnet/neon-auth/img_1.png)
    
2.  [Download](https://github.com/neondatabase-labs/neon-auth-nextjs-template) the sample Next.js Frontend Application you can use for authentication.
    
3.  Open a frontend application in an IDE of your choice (for example, VS Code or Rider). Ensure you have NodeJS 18+ installed.
    
4.  Copy authentication details from the Neon and paste them into the `.env.local` file: ![Screenshot of a code editor showing a `.env.local` file with placeholder values for `NEXT_PUBLIC_STACK_PROJECT_ID`, `NEXT_PUBLIC_STACK_PUBLISHABLE_CLIENT_KEY`, `STACK_SECRET_SERVER_KEY`, and `DATABASE_URL`.](https://antondevtips.com/media/code_screenshots/dotnet/neon-auth/img_2.png)
    

```
# Stack Auth keys
NEXT_PUBLIC_STACK_PROJECT_ID=
NEXT_PUBLIC_STACK_PUBLISHABLE_CLIENT_KEY=
STACK_SECRET_SERVER_KEY=

# For the `neondb_owner` role.
DATABASE_URL=
```

5.  Run the following commands in the terminal in the application root directory:

```bash
npm install
npm run dev
```

6.  Open the link `http://localhost:3000` link in your web browser, and in a few seconds you will see the following page: ![Screenshot of a web browser displaying the Next.js sample application's homepage, with options to "Sign Up" or "Sign In" using email, Google, or GitHub.](https://antondevtips.com/media/code_screenshots/dotnet/neon-auth/img_3.png)
    
7.  Click "Sign Up" button and authorize with either Google or GitHub: ![Screenshot of the Next.js sample application's sign-up page, showing options to sign up with Google or GitHub.](https://antondevtips.com/media/code_screenshots/dotnet/neon-auth/img_4.png)
    

After a successful authentication you will see your profile loaded: ![Screenshot of the Next.js sample application showing a successfully authenticated user's profile, displaying "Hello Anton Martyniuk" and a "Sign Out" button.](https://antondevtips.com/media/code_screenshots/dotnet/neon-auth/img_5.png)

8.  Navigate to the Neon Console and check the `users_sync` table: ![Screenshot of the Neon Console showing the `neon_auth.users_sync` table with one entry, displaying columns like `id`, `email`, `name`, `created_at`, `updated_at`, `deleted_at`, and `raw_json`.](https://antondevtips.com/media/code_screenshots/dotnet/neon-auth/img_6.png) ![A zoomed-in screenshot of the Neon Console's `users_sync` table, highlighting the `raw_json` column content for a user, showing detailed profile information.](https://antondevtips.com/media/code_screenshots/dotnet/neon-auth/img_7.png)

Our setup is ready.

[](#how-neon-auth-works)

## How Neon Auth Works

When you enable Neon Auth, it creates a dedicated `neon_auth` schema in your database. In this schema, you'll find a `users_sync` table that automatically mirrors user profiles from the connected auth provider.

The flow looks like this:

*   Users authenticate via Google, GitHub in Stack Auth.
*   The user's profile (ID, email, name, timestamps, etc.) lives in the auth provider.
*   Neon Auth syncs that data into `neon_auth.users_sync` for you.

`users_sync` table has the following structure:

*   **id**: Unique ID of the user
*   **email**: Primary email address
*   **name**: The user's display name
*   **raw\_json**: Full user profile in JSON
*   **created\_at**: Timestamp of user signup
*   **updated\_at**: Timestamp of user's last update (nullable)
*   **deleted\_at**: Timestamp of user deletion (nullable)

Here is the raw JSON from my auth by GitHub:

```json
{
  "id": "[guid_here]",
  "display_name": "Anton Martyniuk",
  "has_password": false,
  "is_anonymous": false,
  "primary_email": "[email_here]",
  "selected_team": null,
  "auth_with_email": false,
  "client_metadata": null,
  "oauth_providers": [
    {
      "id": "github",
      "email": "[email_here]",
      "account_id": "[id_here]"
    }
  ],
  "server_metadata": null,
  "otp_auth_enabled": false,
  "selected_team_id": null,
  "profile_image_url": "https://avatars.githubusercontent.com/u/[id_here]?v=4",
  "requires_totp_mfa": false,
  "signed_up_at_millis": 1111,
  "passkey_auth_enabled": false,
  "last_active_at_millis": 1111,
  "primary_email_verified": true,
  "client_read_only_metadata": null,
  "primary_email_auth_enabled": true
}
```

As you can see, passwords and other sensitive information is not synced to Neon. These data stay private within Auth providers.

With Neon Auth, there's no need to create and manage your own users table or implement webhook handlers for synchronization. Neon Auth simplifies this process by handling user synchronization for you, making it easier to build applications that depend on user data. You can directly reference the `neon_auth.users_sync` table in your schema and queries, with updates to user profiles automatically reflected, eliminating the need for you to write synchronization code.

However, since the `neon_auth.users_sync` table is updated asynchronously, there may be a slight delay (typically less than 1 second) before a user's data appears in the table. Keep this delay in mind when deciding whether to use foreign keys in your schema.

Now let's build a .NET Application that works with Neon users.

[](#building-a-net-application-with-neon-users)

## Building a .NET Application with Neon Users

I have built a Products API web application that has the following entities:

*   Product
*   ProductCart
*   ProductCartItem

This application lets you manage products and create a shopping cart with multiple items.

I want to connect this application with real users. With Neon Auth providing users it has never been easier.

Let's create a `User` entity:

```csharp
public class User
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public string RawJson { get; set; } = string.Empty;
}
```

Here is how it maps to the `users_sync` table:

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users_sync", options => options.ExcludeFromMigrations());

        builder.HasKey(u => u.Id);

        builder.Property(u => u.RawJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(u => u.Id)
            .HasMaxLength(255);

        builder.Property(u => u.Email)
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
```

Note that I am calling `options.ExcludeFromMigrations())` to exclude users entity from migrations as this table is already created by Neon Auth.

I have connected `ProductCart` entity to the Users:

```csharp
public class ProductCartConfiguration : IEntityTypeConfiguration<ProductCart>
{
    public void Configure(EntityTypeBuilder<ProductCart> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Id)
            .ValueGeneratedOnAdd();

        builder.HasOne(pc => pc.User)
            .WithMany()
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(pc => pc.CreatedOn);
    }
}
```

Here is an API endpoints to create a product cart for the online store with our connected user:

```csharp
POST http://localhost:5001/product-carts
Content-Type: application/json

{
  "userId": "b5efd1d4-f8fa-4cd9-8c9f-2cbb4ea4f165",
  "productCartItems": [
    {
      "productId": "019600db-3612-785a-be46-4ce312597393",
      "quantity": 2
    }
  ]
}
```

If you want to learn more about Neon Auth - check their [documentation](https://neon.tech/docs/guides/neon-auth-tutorial?refcode=44WD03UH).

[](#summary)

## Summary

**Neon Auth** revolutionizes how you manage user data from external providers by automatically syncing profiles into your Postgres database. Rather than coding your own user tables and sync logic, you simply rely on the `neon_auth.users_sync` table for up-to-date emails, names, IDs, and more.

From there, you can join or reference these user records in your own schema — just like any normal table.

Key Takeaways:

*   Saves time and complexity by eliminating manual webhooks and user tables.
*   Provides near real-time user synchronization for a consistent view of who's using your app.
*   Integrates with the Neon Console for easy provisioning, management, and future expansions.

Want to cut down on your auth integration overhead? Give Neon Auth a try and see how much simpler life can be when user profiles just sync automatically.

[Get started](https://neon.tech/?refcode=44WD03UH) with Neon for free.

Disclaimer: this newsletter is sponsored by [Neon](https://neon.tech/).

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-sync-users-logged-in-by-google-and-github-to-your-database-with-neon-auth-for-free)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-sync-users-logged-in-by-google-and-github-to-your-database-with-neon-auth-for-free&title=How%20to%20Sync%20Users%20Logged%20in%20by%20Google%20and%20GitHub%20to%2Fyour%2FDatabase%2FWith%2FNeon%2FAuth%2Ffor%2FFree)[X](https://twitter.com/intent/tweet?text=How%20to%20Sync%20Users%20Logged%20in%20by%20Google%20and%20GitHub%20to%20Your%20Database%20With%20Neon%20Auth%20for%20Free&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-sync-users-logged-in-by%2Fgoogle%2Fand%2Fgithub%2Fto%2Fyour%2Fdatabase%2Fwith%2Fneon%2Fauth%2Ffor%2Ffree)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-sync-users-logged-in-by-google-and-github-to-your-database-with-neon-auth-for-free)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.