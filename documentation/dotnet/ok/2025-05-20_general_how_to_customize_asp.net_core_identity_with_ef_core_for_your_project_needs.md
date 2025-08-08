```yaml
---
title: How to Customize ASP.NET Core Identity With EF Core for Your Project Needs
source: https://antondevtips.com/blog/how-to-customize-aspnet-core-identity-with-efcore-for-your-project-needs
date_published: 2025-05-20T07:45:06.339Z
date_captured: 2025-08-06T16:41:06.486Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core Identity, Entity Framework Core, PostgreSQL, JWT, .NET, Microsoft.AspNetCore.Identity.EntityFrameworkCore, Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.EntityFrameworkCore, Microsoft.IdentityModel.Tokens, System.IdentityModel.Tokens.Jwt, EFCore.NamingConventions]
programming_languages: [C#, SQL, Bash]
tags: [aspnet-core, identity, ef-core, authentication, authorization, jwt, database, security, dotnet, data-access]
key_concepts: [aspnet-core-identity-customization, entity-framework-core-mapping, jwt-authentication, role-based-authorization, claim-based-authorization, database-seeding, dependency-injection, minimal-apis]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide on customizing ASP.NET Core Identity with Entity Framework Core to fit specific project requirements. It covers extending built-in Identity entities, configuring database schema, and managing user authentication with JWT tokens. The guide also demonstrates how to implement role and claim-based authorization, along with practical steps for seeding initial roles and claims. It offers detailed code examples for a robust and flexible security setup in ASP.NET Core applications.]
---
```

# How to Customize ASP.NET Core Identity With EF Core for Your Project Needs

![Cover image for the article "How to Customize ASP.NET Core Identity With EF Core for Your Project Needs", featuring a white code icon on a dark blue background with abstract purple shapes.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fefcore%2Fcover_efcore_identity.png&w=3840&q=100)

# How to Customize ASP.NET Core Identity With EF Core for Your Project Needs

May 20, 2025

[Download source code](/source-code/how-to-customize-aspnet-core-identity-with-efcore-for-your-project-needs)

7 min read

Security and authentication are one of the most important aspects of any application. Understanding and applying proven tools is critical to prevent common vulnerabilities such as unauthorized access and data leaks.

**ASP.NET Core Identity** offers developers a powerful way to manage users, roles, claims, and perform user authentication for web apps. Identity provides ready-to-use solutions and APIs out of the box. But often you need to make adjustments to fit your specific needs or requirements.

Today I want to show you practical approaches to customizing ASP.NET Identity step-by-step.

We will explore:

*   How to adapt the built-in Identity tables to your database schema
*   How to register and log users with JWT tokens
*   How to update user roles and claims with Identity
*   How to seed initial roles and claims using Identity.

Let's dive in!

## Getting Started with ASP.NET Identity

**ASP.NET Core Identity** is a set of tools that adds login functionality to ASP.NET Core applications. It handles tasks like creating new users, hashing passwords, validating user credentials, and managing roles or claims.

Add the following packages to your project to get started with ASP.NET Core Identity:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

Here is how you can configure the default Identity:

```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => {})
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});
```

Here is the list of available entities provided by Identity:

*   User
*   Role
*   Claim
*   UserClaim
*   UserRole
*   RoleClaim
*   UserToken
*   UserLogin

However, in many apps you may need to make the following customizations:

*   Add own custom fields to the `User` entity
*   Add own custom fields to the `Role` entity
*   Reference user and role child entities when using `IdentityDbContext`
*   Use JWT Tokens instead of Bearer Tokens (yes, ASP.NET Core Identity returns Bearer tokens when using ready Web APIs)

Let's explore how you can customize the database schema for Identity with EF Core.

## Customizing ASP.NET Identity with EF Core

One of the great features of ASP.NET Core Identity is that you can customize the entities and their corresponding database tables.

You can create your own class and inherit from the `IdentityUser` to add new fields (FullName and JobTitle):

```csharp
public class User : IdentityUser
{
    public string FullName { get; set; }
    
    public string JobTitle { get; set; }
}
```

Next you need to inherit from the `IdentityDbContext` and specify the type of your custom user class:

```csharp
public class BooksDbContext : IdentityDbContext<User>
{
}
```

You can reference a User entity in other entities just as usual:

```csharp
public class Author
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public List<Book> Books { get; set; } = [];

    public string? UserId { get; set; }

    public User? User { get; set; }
}

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("authors");
        builder.HasKey(x => x.Id);

        // ...

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<Author>(x => x.UserId);
    }
}
```

Let's extend a `User` entity further to be able to navigate to user's claims, roles, logins and tokens:

```csharp
public class User : IdentityUser
{
    public ICollection<UserClaim> Claims { get; set; }

    public ICollection<UserRole> UserRoles { get; set; }

    public ICollection<UserLogin> UserLogins { get; set; }

    public ICollection<UserToken> UserTokens { get; set; }
}
```

We need to override the mapping for the `User` entity to specify the relationships:

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Each User can have many UserClaims
        builder.HasMany(e => e.Claims)
            .WithOne(e => e.User)
            .HasForeignKey(uc => uc.UserId)
            .IsRequired();

        // Each User can have many UserLogins
        builder.HasMany(e => e.UserLogins)
            .WithOne(e => e.User)
            .HasForeignKey(ul => ul.UserId)
            .IsRequired();

        // Each User can have many UserTokens
        builder.HasMany(e => e.UserTokens)
            .WithOne(e => e.User)
            .HasForeignKey(ut => ut.UserId)
            .IsRequired();

        // Each User can have many entries in the UserRole join table
        builder.HasMany(e => e.UserRoles)
            .WithOne(e => e.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
    }
}
```

If you want to have all the relations between Identity entities, you need to extend other classes too:

```csharp
public class Role : IdentityRole
{
    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<RoleClaim> RoleClaims { get; set; }
}

public class RoleClaim : IdentityRoleClaim<string>
{
    public Role Role { get; set; }
}

public class UserRole : IdentityUserRole<string>
{
    public User User { get; set; }
    public Role Role { get; set; }
}

public class UserClaim : IdentityUserClaim<string>
{
    public User User { get; set; }
}

public class UserLogin : IdentityUserLogin<string>
{
    public User User { get; set; }
}

public class UserToken : IdentityUserToken<string>
{
    public User User { get; set; }
}
```

Here is the mapping for the entities:

```csharp
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        // Each Role can have many entries in the UserRole join table
        builder.HasMany(e => e.UserRoles)
            .WithOne(e => e.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        // Each Role can have many associated RoleClaims
        builder.HasMany(e => e.RoleClaims)
            .WithOne(e => e.Role)
            .HasForeignKey(rc => rc.RoleId)
            .IsRequired();
    }
}

public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.ToTable("role_claims");
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RoleId });
    }
}

public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable("user_claims");
    }
}

public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.ToTable("user_logins");
    }
}

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("user_tokens");
    }
}
```

It may seem much, but you need to get done this once and then use it in every project.

> P.S.: you can download the full source code at the end of the article.

Here is how the `BooksDbContext` changes:

```csharp
public class BooksDbContext : IdentityDbContext<User, Role, string,
    UserClaim, UserRole, UserLogin,
    RoleClaim, UserToken>
{
}
```

Finally, you need to register Identity in DI:

```csharp
services.AddDbContext<BooksDbContext>((provider, options) =>
{
    options
        .UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable(DatabaseConsts.MigrationTableName,
                DatabaseConsts.Schema);
        })
        .UseSnakeCaseNamingConvention();
});

services
    .AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<BooksDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
```

By default, all the tables and columns in the database will be in PascalCase. If you prefer consistent and automatic naming patterns, consider the package `EFCore.NamingConventions`:

```bash
dotnet add package EFCore.NamingConventions
```

Once installed, you simply need to plug in the naming convention support into your DbContext configuration. In my DbContext registration above I used `UseSnakeCaseNamingConvention()` for my Postgres database. So the table and column names would look like: `user_claims`, `user_id`, etc.

## Registering Users with Identity

Now that Identity is wired up, you can expose an endpoint to let new users register. Here is a Minimal API example:

```csharp
public record RegisterUserRequest(string Email, string Password);

app.MapPost("/api/register", async (
    [FromBody] RegisterUserRequest request,
    UserManager<User> userManager) =>
{
    var existingUser = await userManager.FindByEmailAsync(request.Email);
    if (existingUser != null)
    {
        return Results.BadRequest("User already exists.");
    }

    var user = new User
    {
        UserName = request.Email,
        Email = request.Email
    };

    var result = await userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
    {
        return Results.BadRequest(result.Errors);
    }
    
    result = await userManager.AddToRoleAsync(user, "DefaultRole");
    
    if (!result.Succeeded)
    {
        return Results.BadRequest(result.Errors);
    }

    var response = new UserResponse(user.Id, user.Email);
    return Results.Created($"/api/users/{user.Id}", response);
});
```

You can use the `UserManager<User>` class to manage users.

Registration involves the following steps:

1.  You need to check if the user already exists by calling `FindByEmailAsync` method.
2.  If the user does not exist, you can create a new user by calling `CreateAsync` method.
3.  You can add the user to a role by calling `AddToRoleAsync` method.
4.  In case of error during registration - you can return a `BadRequest` response with the error message from Identity.

## How to Log in Users with Identity

Once a user is created, they can authenticate. Here is how you can implement authentication using Identity:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public record LoginUserRequest(string Email, string Password);

app.MapPost("/api/login", async (
    [FromBody] LoginUserRequest request,
    IOptions<AuthConfiguration> authOptions,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<Role> roleManager) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user is null)
    {
        return Results.NotFound("User not found");
    }

    var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
    if (!result.Succeeded)
    {
        return Results.Unauthorized();
    }

    var roles = await userManager.GetRolesAsync(user);
    var userRole = roles.FirstOrDefault() ?? "user";

    var role = await roleManager.FindByNameAsync(userRole);
    var roleClaims = role is not null ? await roleManager.GetClaimsAsync(role) : [];

    var token = GenerateJwtToken(user, authOptions.Value, userRole, roleClaims);
    return Results.Ok(new { Token = token });
});
```

The authentication process involves the following steps:

1.  You need to check if the user exists by calling `FindByEmailAsync` method.
2.  You can check the user's password by calling `CheckPasswordSignInAsync` method from `SignInManager<User>`.
3.  You can get the user's roles by calling `GetRolesAsync` method from `UserManager<User>`.
4.  You can get the role's claims by calling `GetClaimsAsync` method from `RoleManager<Role>`.
5.  In case of error during login - you can return a `BadRequest` response with the error message from Identity.

You can issue a JWT token upon successful login:

```csharp
private static string GenerateJwtToken(User user,
    AuthConfiguration authConfiguration,
    string userRole,
    IList<Claim> roleClaims)
{
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfiguration.Key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    List<Claim> claims = [
        new(JwtRegisteredClaimNames.Sub, user.Email!),
        new("userid", user.Id),
        new("role", userRole)
    ];

    foreach (var roleClaim in roleClaims)
    {
        claims.Add(new Claim(roleClaim.Type, roleClaim.Value));
    }

    var token = new JwtSecurityToken(
        issuer: authConfiguration.Issuer,
        audience: authConfiguration.Audience,
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

Each role in my application has a set of claims, I add these claims to the JWT token. Here is what an issued JWT token may look like:

```csharp
{
  "sub": "admin@test.com",
  "userid": "dc233fac-bace-4719-9a4f-853e199300d5",
  "role": "Admin",
  "users:create": "true",
  "users:update": "true",
  "users:delete": "true",
  "books:create": "true",
  "books:update": "true",
  "books:delete": "true",
  "exp": 1739481834,
  "iss": "DevTips",
  "aud": "DevTips"
}
```

You can use the claims to limit access to the endpoints, for example:

```csharp
app.MapPost("/api/books", Handle)
        .RequireAuthorization("books:create");
        
    app.MapDelete("/api/books/{id}", Handle)
        .RequireAuthorization("books:delete");
        
    app.MapPost("/api/users", Handle)
        .RequireAuthorization("users:create");
        
    app.MapDelete("/api/users/{id}", Handle)
        .RequireAuthorization("users:delete");
```

I explained how to set up Authentication and Authorization in ASP.NET Core [here](https://antondevtips.com/blog/authentication-and-authorization-best-practices-in-aspnetcore/?utm_source=antondevtips&utm_medium=email&utm_campaign=20-05-2025-newsletter).

## How to Seed Identity Data: Initialize Roles and Claims

Seeding is helpful when you want to set up an application with default roles and claims. A common approach is to seed data once on the application startup:

```csharp
var app = builder.Build();

// Register middlewares...

// Create and seed database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

    await DatabaseSeedService.SeedAsync(dbContext, userManager, roleManager);
}

await app.RunAsync();
```
```csharp
public static class DatabaseSeedService
{
    public static async Task SeedAsync(BooksDbContext dbContext, UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        await dbContext.Database.MigrateAsync();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        // Seed roles and claims here

        await dbContext.SaveChangesAsync();
    }
}
```

You can use the `RoleManager<Role>` to manage roles and their claims:

```csharp
var adminRole = new Role { Name = "Admin" };
var authorRole = new Role { Name = "Author" };

var result = await roleManager.CreateAsync(adminRole);
result = await roleManager.CreateAsync(authorRole);

result = await roleManager.AddClaimAsync(adminRole, new Claim("users:create", "true"));
result = await roleManager.AddClaimAsync(adminRole, new Claim("users:update", "true"));
result = await roleManager.AddClaimAsync(adminRole, new Claim("users:delete", "true"));

result = await roleManager.AddClaimAsync(adminRole, new Claim("books:create", "true"));
result = await roleManager.AddClaimAsync(adminRole, new Claim("books:update", "true"));
result = await roleManager.AddClaimAsync(adminRole, new Claim("books:delete", "true"));

result = await roleManager.AddClaimAsync(authorRole, new Claim("books:create", "true"));
result = await roleManager.AddClaimAsync(authorRole, new Claim("books:update", "true"));
result = await roleManager.AddClaimAsync(authorRole, new Claim("books:delete", "true"));
```

Here is how you can create a default user in your application:

```csharp
var adminUser = new User
{
    Id = Guid.NewGuid().ToString(),
    Email = "admin@test.com",
    UserName = "admin@test.com"
};

result = await userManager.CreateAsync(adminUser, "Test1234!");
result = await userManager.AddToRoleAsync(adminUser, "Admin");
```

It's important to change the default password after you are successfully logged in for the first time.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-customize-aspnet-core-identity-with-efcore-for-your-project-needs)