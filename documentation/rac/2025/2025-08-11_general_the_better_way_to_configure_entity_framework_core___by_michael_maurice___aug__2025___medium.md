```yaml
---
title: "The Better Way to Configure Entity Framework Core | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/the-better-way-to-configure-entity-framework-core-3619023cdd04
date_published: 2025-08-11T17:02:33.933Z
date_captured: 2025-08-22T11:02:18.544Z
domain: medium.com
author: Michael Maurice
category: general
technologies: [Entity Framework Core, .NET, SQL Server, PostgreSQL, NUnit, Microsoft.EntityFrameworkCore, System.Text.Json, Microsoft.Extensions.Configuration, Microsoft.AspNetCore.Hosting, Microsoft.Extensions.DependencyInjection]
programming_languages: [C#, SQL]
tags: [ef-core, entity-framework, configuration, data-access, dotnet, csharp, architecture, testing, best-practices, database]
key_concepts: [IEntityTypeConfiguration pattern, separation-of-concerns, assembly-scanning, base-configuration-classes, provider-specific-configuration, advanced-relationship-mapping, unit-testing, database-migrations]
code_examples: false
difficulty_level: intermediate
summary: |
  [This comprehensive guide explores superior approaches to Entity Framework Core configuration, moving beyond the monolithic `OnModelCreating` method. It champions the `IEntityTypeConfiguration` pattern for improved maintainability, testability, and code organization. The article delves into advanced techniques like base configuration classes, automatic discovery via assembly scanning, provider-specific settings, and complex relationship mapping. It also covers performance optimization, unit and integration testing strategies, and migration considerations, providing best practices for building scalable data access layers.]
---
```

# The Better Way to Configure Entity Framework Core | by Michael Maurice | Aug, 2025 | Medium

# The Better Way to Configure Entity Framework Core

![Diagram illustrating the shift from a monolithic OnModelCreating method to a structured approach using IEntityTypeConfiguration classes and automatic assembly scanning in Entity Framework Core. The left side shows a messy, tangled code block labeled "Monolithic OnModelCreating" with a warning sign, representing poor maintainability. The right side shows individual configuration classes (UserTypeConfiguration, ProductConfiguration, OrderConfiguration, CategoryConfiguration) leading to a database icon, with an arrow pointing to "ApplyConfigurationsFromAssembly(...)" with a checkmark, symbolizing a cleaner, more organized, and automatically applied configuration.](https://miro.medium.com/v2/resize:fit:700/1*N62dsZcaHmVj8RR4UfKaLA.png)

Entity Framework Core configuration is often one of the most overlooked aspects of application architecture, yet it significantly impacts maintainability, testability, and code organization. Most developers settle for cramming all configuration logic into the `OnModelCreating` method, creating monolithic, hard-to-maintain code. This comprehensive guide explores superior approaches to EF Core configuration that promote separation of concerns, improve code organization, and enhance long-term maintainability through modern patterns and best practices.

# The Problem with Traditional Configuration Approaches

# The Monolithic OnModelCreating Anti-Pattern

Most EF Core tutorials and examples demonstrate configuration like this:

```csharp
public class ApplicationDbContext : DbContext  
{  
    public DbSet<User> Users { get; set; }  
    public DbSet<Product> Products { get; set; }  
    public DbSet<Order> Orders { get; set; }  
    public DbSet<OrderItem> OrderItems { get; set; }  
    public DbSet<Category> Categories { get; set; }  
protected override void OnModelCreating(ModelBuilder modelBuilder)  
    {  
        // User configuration  
        modelBuilder.Entity<User>(entity =>  
        {  
            entity.HasKey(e => e.Id);  
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);  
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);  
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);  
            entity.HasIndex(e => e.Email).IsUnique();  
        });  
        // Product configuration  
        modelBuilder.Entity<Product>(entity =>  
        {  
            entity.HasKey(e => e.Id);  
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);  
            entity.Property(e => e.Description).HasMaxLength(1000);  
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");  
            entity.HasOne(e => e.Category)  
                  .WithMany(e => e.Products)  
                  .HasForeignKey(e => e.CategoryId);  
        });  
        // Order configuration    
        modelBuilder.Entity<Order>(entity =>  
        {  
            entity.HasKey(e => e.Id);  
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);  
            entity.Property(e => e.OrderDate).IsRequired();  
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");  
            entity.HasOne(e => e.User)  
                  .WithMany(e => e.Orders)  
                  .HasForeignKey(e => e.UserId);  
        });  
        // ... hundreds more lines  
    }  
}
```

# Problems with This Approach

*   **Violation of Single Responsibility Principle**: The `DbContext` becomes responsible for both data access and entity configuration
*   **Poor Maintainability**: As applications grow, this method can easily exceed 500+ lines, making it impossible to navigate
*   **Difficult Testing**: Configuration logic is tightly coupled to the DbContext, making unit testing challenging
*   **Poor Code Organization**: Related configuration is scattered throughout one massive method
*   **Merge Conflicts**: Multiple developers modifying the same method leads to frequent conflicts
*   **Lack of Reusability**: Configuration cannot be shared across different contexts

# The Better Way: IEntityTypeConfiguration Pattern

# Introduction to IEntityTypeConfiguration

Entity Framework Core provides the `IEntityTypeConfiguration<TEntity>` interface specifically designed to address configuration organization issues:

```csharp
using Microsoft.EntityFrameworkCore;  
using Microsoft.EntityFrameworkCore.Metadata.Builders;  
namespace EFCoreConfig.Configurations  
{  
    public class UserConfiguration : IEntityTypeConfiguration<User>  
    {  
        public void Configure(EntityTypeBuilder<User> builder)  
        {  
            // Table configuration  
            builder.ToTable("Users", "Identity");  
              
            // Primary key  
            builder.HasKey(e => e.Id);  
              
            // Properties  
            builder.Property(e => e.Id)  
                   .HasDefaultValueSql("NEWID()");  
            builder.Property(e => e.Email)  
                   .IsRequired()  
                   .HasMaxLength(255)  
                   .HasColumnType("varchar(255)");  
            builder.Property(e => e.FirstName)  
                   .IsRequired()  
                   .HasMaxLength(100)  
                   .HasColumnType("nvarchar(100)");  
            builder.Property(e => e.LastName)  
                   .IsRequired()  
                   .HasMaxLength(100)  
                   .HasColumnType("nvarchar(100)");  
            builder.Property(e => e.DateCreated)  
                   .IsRequired()  
                   .HasDefaultValueSql("GETUTCDATE()");  
            builder.Property(e => e.IsActive)  
                   .IsRequired()  
                   .HasDefaultValue(true);  
            // Indexes  
            builder.HasIndex(e => e.Email)  
                   .IsUnique()  
                   .HasDatabaseName("IX_Users_Email");  
            builder.HasIndex(e => e.LastName)  
                   .HasDatabaseName("IX_Users_LastName");  
            // Relationships will be configured here later  
        }  
    }  
}
```

# Advanced Entity Configuration Example

Here’s a more complex configuration demonstrating advanced patterns:

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>  
{  
    public void Configure(EntityTypeBuilder<Order> builder)  
    {  
        // Table configuration  
        builder.ToTable("Orders", "Sales");  
          
        // Primary key  
        builder.HasKey(e => e.Id);  
          
        // Properties with advanced configuration  
        builder.Property(e => e.Id)  
               .HasDefaultValueSql("NEWID()")  
               .HasComment("Unique identifier for the order");  
        builder.Property(e => e.OrderNumber)  
               .IsRequired()  
               .HasMaxLength(50)  
               .HasColumnType("varchar(50)")  
               .HasComment("Human-readable order number");  
        builder.Property(e => e.OrderDate)  
               .IsRequired()  
               .HasColumnType("datetime2(7)")  
               .HasDefaultValueSql("GETUTCDATE()")  
               .HasComment("Date and time when order was created");  
        builder.Property(e => e.TotalAmount)  
               .IsRequired()  
               .HasColumnType("decimal(18,2)")  
               .HasComment("Total order amount including tax");  
        builder.Property(e => e.Status)  
               .IsRequired()  
               .HasConversion<string>()  
               .HasMaxLength(50)  
               .HasComment("Current order status");  
        // Computed columns  
        builder.Property(e => e.OrderYear)  
               .HasComputedColumnSql("YEAR([OrderDate])")  
               .HasComment("Computed year from order date for partitioning");  
        // Indexes for performance  
        builder.HasIndex(e => e.OrderNumber)  
               .IsUnique()  
               .HasDatabaseName("IX_Orders_OrderNumber");  
        builder.HasIndex(e => e.OrderDate)  
               .HasDatabaseName("IX_Orders_OrderDate");  
        builder.HasIndex(e => e.UserId)  
               .HasDatabaseName("IX_Orders_UserId");  
        builder.HasIndex(e => new { e.UserId, e.OrderDate })  
               .HasDatabaseName("IX_Orders_UserId_OrderDate");  
        // Relationships  
        builder.HasOne(e => e.User)  
               .WithMany(e => e.Orders)  
               .HasForeignKey(e => e.UserId)  
               .OnDelete(DeleteBehavior.Restrict)  
               .HasConstraintName("FK_Orders_Users");  
        builder.HasMany(e => e.OrderItems)  
               .WithOne(e => e.Order)  
               .HasForeignKey(e => e.OrderId)  
               .OnDelete(DeleteBehavior.Cascade)  
               .HasConstraintName("FK_OrderItems_Orders");  
        // Value objects and complex properties  
        builder.OwnsOne(e => e.ShippingAddress, address =>  
        {  
            address.Property(a => a.Street).HasMaxLength(200);  
            address.Property(a => a.City).HasMaxLength(100);  
            address.Property(a => a.State).HasMaxLength(50);  
            address.Property(a => a.PostalCode).HasMaxLength(20);  
            address.Property(a => a.Country).HasMaxLength(100);  
        });  
        builder.OwnsOne(e => e.BillingAddress, address =>  
        {  
            address.Property(a => a.Street).HasMaxLength(200);  
            address.Property(a => a.City).HasMaxLength(100);  
            address.Property(a => a.State).HasMaxLength(50);  
            address.Property(a => a.PostalCode).HasMaxLength(20);  
            address.Property(a => a.Country).HasMaxLength(100);  
        });  
        // Query filters for soft delete  
        builder.HasQueryFilter(e => !e.IsDeleted);  
    }  
}
```

# Organizing Configuration Classes

# Project Structure

Organize configuration classes in a logical structure:

```
📁 YourProject/  
├── 📁 Data/  
│   ├── 📁 Configurations/  
│   │   ├── 📁 Identity/  
│   │   │   ├── UserConfiguration.cs  
│   │   │   ├── RoleConfiguration.cs  
│   │   │   └── UserRoleConfiguration.cs  
│   │   ├── 📁 Catalog/  
│   │   │   ├── ProductConfiguration.cs  
│   │   │   ├── CategoryConfiguration.cs  
│   │   │   └── ProductTagConfiguration.cs  
│   │   ├── 📁 Sales/  
│   │   │   ├── OrderConfiguration.cs  
│   │   │   ├── OrderItemConfiguration.cs  
│   │   │   └── PaymentConfiguration.cs  
│   │   └── 📁 Common/  
│   │       ├── BaseEntityConfiguration.cs  
│   │       └── AuditableEntityConfiguration.cs  
│   └── ApplicationDbContext.cs
```

# Base Configuration Classes

Create base configurations for common patterns:

```csharp
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>  
    where TEntity : class, IBaseEntity  
{  
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)  
    {  
        // Common configuration for all entities  
        builder.HasKey(e => e.Id);  
          
        builder.Property(e => e.Id)  
               .HasDefaultValueSql("NEWID()");  
        ConfigureEntity(builder);  
    }  
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);  
}  
public abstract class AuditableEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity>  
    where TEntity : class, IAuditableEntity  
{  
    public override void Configure(EntityTypeBuilder<TEntity> builder)  
    {  
        base.Configure(builder);  
        // Audit fields configuration  
        builder.Property(e => e.CreatedBy)  
               .IsRequired()  
               .HasMaxLength(100);  
        builder.Property(e => e.CreatedAt)  
               .IsRequired()  
               .HasDefaultValueSql("GETUTCDATE()");  
        builder.Property(e => e.ModifiedBy)  
               .HasMaxLength(100);  
        builder.Property(e => e.ModifiedAt);  
        builder.Property(e => e.IsDeleted)  
               .IsRequired()  
               .HasDefaultValue(false);  
        // Soft delete query filter  
        builder.HasQueryFilter(e => !e.IsDeleted);  
        // Audit indexes  
        builder.HasIndex(e => e.CreatedAt)  
               .HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedAt");  
        builder.HasIndex(e => e.CreatedBy)  
               .HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedBy");  
    }  
}
```

# Implementing Configuration Classes

```csharp
public class ProductConfiguration : AuditableEntityConfiguration<Product>  
{  
    protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)  
    {  
        // Table configuration  
        builder.ToTable("Products", "Catalog");  
          
        // Properties  
        builder.Property(e => e.Name)  
               .IsRequired()  
               .HasMaxLength(200)  
               .HasColumnType("nvarchar(200)");  
        builder.Property(e => e.SKU)  
               .IsRequired()  
               .HasMaxLength(50)  
               .HasColumnType("varchar(50)");  
        builder.Property(e => e.Price)  
               .IsRequired()  
               .HasColumnType("decimal(18,2)")  
               .HasComment("Current selling price");  
        builder.Property(e => e.Cost)  
               .HasColumnType("decimal(18,2)")  
               .HasComment("Product cost for margin calculation");  
        // JSON column for metadata (SQL Server 2016+)  
        builder.Property(e => e.Metadata)  
               .HasColumnType("nvarchar(max)")  
               .HasConversion(  
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),  
                   v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null));  
        // Indexes  
        builder.HasIndex(e => e.SKU)  
               .IsUnique()  
               .HasDatabaseName("IX_Products_SKU");  
        builder.HasIndex(e => e.CategoryId)  
               .HasDatabaseName("IX_Products_CategoryId");  
        builder.HasIndex(e => e.Price)  
               .HasDatabaseName("IX_Products_Price");  
        // Relationships  
        builder.HasOne(e => e.Category)  
               .WithMany(e => e.Products)  
               .HasForeignKey(e => e.CategoryId)  
               .OnDelete(DeleteBehavior.Restrict);  
        // Many-to-many relationship with explicit join entity  
        builder.HasMany(e => e.Tags)  
               .WithMany(e => e.Products)  
               .UsingEntity<ProductTag>(  
                   j => j.HasOne(pt => pt.Tag)  
                         .WithMany()  
                         .HasForeignKey(pt => pt.TagId),  
                   j => j.HasOne(pt => pt.Product)  
                         .WithMany()  
                         .HasForeignKey(pt => pt.ProductId),  
                   j =>  
                   {  
                       j.HasKey(pt => new { pt.ProductId, pt.TagId });  
                       j.ToTable("ProductTags", "Catalog");  
                   });  
    }  
}
```

# Applying Configuration to DbContext

# Manual Configuration Application

The traditional approach requires manually applying each configuration:

```csharp
public class ApplicationDbContext : DbContext  
{  
    public DbSet<User> Users { get; set; }  
    public DbSet<Product> Products { get; set; }  
    public DbSet<Order> Orders { get; set; }  
    public DbSet<Category> Categories { get; set; }  
protected override void OnModelCreating(ModelBuilder modelBuilder)  
    {  
        base.OnModelCreating(modelBuilder);  
          
        // Apply configurations manually  
        modelBuilder.ApplyConfiguration(new UserConfiguration());  
        modelBuilder.ApplyConfiguration(new ProductConfiguration());  
        modelBuilder.ApplyConfiguration(new OrderConfiguration());  
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());  
        // ... many more configurations  
    }  
}
```

# Automatic Configuration Discovery

The superior approach uses assembly scanning to automatically discover and apply all configurations:

```csharp
public class ApplicationDbContext : DbContext  
{  
    public DbSet<User> Users { get; set; }  
    public DbSet<Product> Products { get; set; }  
    public DbSet<Order> Orders { get; set; }  
    public DbSet<Category> Categories { get; set; }  
public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)   
        : base(options)  
    {  
    }  
    protected override void OnModelCreating(ModelBuilder modelBuilder)  
    {  
        base.OnModelCreating(modelBuilder);  
          
        // Automatically apply all IEntityTypeConfiguration implementations  
        // from the current assembly  
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);  
          
        // Alternative: Apply from a specific configuration assembly  
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);  
    }  
}
```

# Advanced Configuration Patterns

# Configuration Extension Methods

Create extension methods to encapsulate common configuration patterns:

```csharp
public static class ModelBuilderExtensions  
{  
    public static void ConfigureCommonProperties<T>(this EntityTypeBuilder<T> builder)  
        where T : class, IBaseEntity  
    {  
        builder.HasKey(e => e.Id);  
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");  
    }  
public static void ConfigureAuditProperties<T>(this EntityTypeBuilder<T> builder)  
        where T : class, IAuditableEntity  
    {  
        builder.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);  
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");  
        builder.Property(e => e.ModifiedBy).HasMaxLength(100);  
        builder.Property(e => e.ModifiedAt);  
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);  
        builder.HasQueryFilter(e => !e.IsDeleted);  
    }  
    public static void ConfigureSoftDelete<T>(this EntityTypeBuilder<T> builder)  
        where T : class, ISoftDeletableEntity  
    {  
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);  
        builder.HasQueryFilter(e => !e.IsDeleted);  
        builder.HasIndex(e => e.IsDeleted).HasDatabaseName($"IX_{typeof(T).Name}_IsDeleted");  
    }  
    public static PropertyBuilder<string> ConfigureRequiredString(        this PropertyBuilder<string> propertyBuilder,  
        int maxLength)  
    {  
        return propertyBuilder  
            .IsRequired()  
            .HasMaxLength(maxLength);  
    }  
    public static PropertyBuilder<decimal> ConfigureMoney(this PropertyBuilder<decimal> propertyBuilder)  
    {  
        return propertyBuilder  
            .HasColumnType("decimal(18,2)")  
            .IsRequired();  
    }  
}  
// Usage in configuration classes  
public class ProductConfiguration : IEntityTypeConfiguration<Product>  
{  
    public void Configure(EntityTypeBuilder<Product> builder)  
    {  
        builder.ToTable("Products", "Catalog");  
          
        builder.ConfigureCommonProperties();  
        builder.ConfigureAuditProperties();  
          
        builder.Property(e => e.Name).ConfigureRequiredString(200);  
        builder.Property(e => e.SKU).ConfigureRequiredString(50);  
        builder.Property(e => e.Price).ConfigureMoney();  
    }  
}
```

# Provider-Specific Configurations

Handle different database providers with conditional configuration:

```csharp
public abstract class BaseProductConfiguration : IEntityTypeConfiguration<Product>  
{  
    public virtual void Configure(EntityTypeBuilder<Product> builder)  
    {  
        builder.ToTable("Products", "Catalog");  
        builder.HasKey(e => e.Id);  
          
        builder.Property(e => e.Name)  
               .IsRequired()  
               .HasMaxLength(200);  
        // Provider-specific configuration will be handled by derived classes  
        ConfigureProviderSpecific(builder);  
    }  
    protected abstract void ConfigureProviderSpecific(EntityTypeBuilder<Product> builder);  
}  
public class SqlServerProductConfiguration : BaseProductConfiguration  
{  
    protected override void ConfigureProviderSpecific(EntityTypeBuilder<Product> builder)  
    {  
        builder.Property(e => e.CreatedAt)  
               .HasDefaultValueSql("GETUTCDATE()");  
                 
        builder.Property(e => e.Id)  
               .HasDefaultValueSql("NEWID()");  
                 
        builder.Property(e => e.Metadata)  
               .HasColumnType("nvarchar(max)");  
    }  
}  
public class PostgreSqlProductConfiguration : BaseProductConfiguration  
{  
    protected override void ConfigureProviderSpecific(EntityTypeBuilder<Product> builder)  
    {  
        builder.Property(e => e.CreatedAt)  
               .HasDefaultValueSql("now()");  
                 
        builder.Property(e => e.Id)  
               .HasDefaultValueSql("gen_random_uuid()");  
                 
        builder.Property(e => e.Metadata)  
               .HasColumnType("jsonb");  
    }  
}  
// Configuration selection in DbContext  
public class ApplicationDbContext : DbContext  
{  
    protected override void OnModelCreating(ModelBuilder modelBuilder)  
    {  
        base.OnModelCreating(modelBuilder);  
          
        // Apply provider-specific configurations  
        if (Database.IsSqlServer())  
        {  
            modelBuilder.ApplyConfigurationsFromAssembly(  
                typeof(SqlServerProductConfiguration).Assembly,  
                t => t.Name.Contains("SqlServer"));  
        }  
        else if (Database.IsNpgsql())  
        {  
            modelBuilder.ApplyConfigurationsFromAssembly(  
                typeof(PostgreSqlProductConfiguration).Assembly,  
                t => t.Name.Contains("PostgreSql"));  
        }  
    }  
}
```

# Configuration with Environment-Specific Settings

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>  
{  
    private readonly IConfiguration _configuration;  
    private readonly IWebHostEnvironment _environment;  
public ProductConfiguration(IConfiguration configuration, IWebHostEnvironment environment)  
    {  
        _configuration = configuration;  
        _environment = environment;  
    }  
    public void Configure(EntityTypeBuilder<Product> builder)  
    {  
        builder.ToTable("Products", "Catalog");  
        builder.HasKey(e => e.Id);  
          
        // Environment-specific configuration  
        if (_environment.IsDevelopment())  
        {  
            // Add additional indexes for development/debugging  
            builder.HasIndex(e => e.Name).HasDatabaseName("IX_Products_Name_Dev");  
        }  
          
        // Configuration-driven settings  
        var useStringIds = _configuration.GetValue<bool>("Database:UseStringIds");  
        if (useStringIds)  
        {  
            builder.Property(e => e.Id)  
                   .HasColumnType("varchar(36)")  
                   .HasDefaultValueSql("NEWID()");  
        }  
        else  
        {  
            builder.Property(e => e.Id)  
                   .HasDefaultValueSql("NEWID()");  
        }  
    }  
}
```

# Advanced Relationship Configuration

# Complex Many-to-Many Relationships

```csharp
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>  
{  
    public void Configure(EntityTypeBuilder<UserRole> builder)  
    {  
        // Configure the join table  
        builder.ToTable("UserRoles", "Identity");  
          
        // Composite primary key  
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });  
          
        // Additional properties on the join table  
        builder.Property(ur => ur.AssignedAt)  
               .IsRequired()  
               .HasDefaultValueSql("GETUTCDATE()");  
                 
        builder.Property(ur => ur.AssignedBy)  
               .IsRequired()  
               .HasMaxLength(100);  
                 
        builder.Property(ur => ur.ExpiresAt);  
          
        // Configure relationships  
        builder.HasOne(ur => ur.User)  
               .WithMany(u => u.UserRoles)  
               .HasForeignKey(ur => ur.UserId)  
               .OnDelete(DeleteBehavior.Cascade);  
                 
        builder.HasOne(ur => ur.Role)  
               .WithMany(r => r.UserRoles)  
               .HasForeignKey(ur => ur.RoleId)  
               .OnDelete(DeleteBehavior.Cascade);  
                 
        // Indexes  
        builder.HasIndex(ur => ur.AssignedAt)  
               .HasDatabaseName("IX_UserRoles_AssignedAt");  
                 
        builder.HasIndex(ur => ur.ExpiresAt)  
               .HasDatabaseName("IX_UserRoles_ExpiresAt")  
               .HasFilter("[ExpiresAt] IS NOT NULL");  
    }  
}  
// Configure the many-to-many relationship in User entity  
public class UserConfiguration : IEntityTypeConfiguration<User>  
{  
    public void Configure(EntityTypeBuilder<User> builder)  
    {  
        // Other configuration...  
          
        // Configure many-to-many through UserRole  
        builder.HasMany(u => u.Roles)  
               .WithMany(r => r.Users)  
               .UsingEntity<UserRole>(  
                   j => j.HasOne(ur => ur.Role)  
                         .WithMany()  
                         .HasForeignKey(ur => ur.RoleId),  
                   j => j.HasOne(ur => ur.User)  
                         .WithMany()  
                         .HasForeignKey(ur => ur.UserId));  
    }  
}
```

# Self-Referencing Relationships

```csharp
public class CategoryConfiguration : IEntityTypeConfiguration<Category>  
{  
    public void Configure(EntityTypeBuilder<Category> builder)  
    {  
        builder.ToTable("Categories", "Catalog");  
        builder.HasKey(c => c.Id);  
          
        builder.Property(c => c.Name)  
               .IsRequired()  
               .HasMaxLength(100);  
                 
        builder.Property(c => c.Description)  
               .HasMaxLength(500);  
          
        // Self-referencing relationship  
        builder.HasOne(c => c.ParentCategory)  
               .WithMany(c => c.SubCategories)  
               .HasForeignKey(c => c.ParentCategoryId)  
               .OnDelete(DeleteBehavior.Restrict);