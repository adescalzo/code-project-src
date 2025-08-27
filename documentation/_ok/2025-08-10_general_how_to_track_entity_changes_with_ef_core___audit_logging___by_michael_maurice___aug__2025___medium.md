```yaml
---
title: "How To Track Entity Changes With EF Core | Audit Logging | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/how-to-track-entity-changes-with-ef-core-audit-logging-b1190a88ce93
date_published: 2025-08-10T17:01:55.749Z
date_captured: 2025-08-12T11:12:56.634Z
domain: medium.com
author: Michael Maurice
category: general
technologies: [EF Core, ASP.NET Core, SQL Server, PostgreSQL, .NET, JWT, Swagger, Xunit, Microsoft.Extensions.Logging]
programming_languages: [C#, SQL]
tags: [ef-core, audit-logging, change-tracking, dotnet, database, web-api, data-access, security, compliance, soft-delete]
key_concepts: [entity-framework-core, change-tracker, audit-trail, interceptors, dependency-injection, soft-delete, data-integrity, unit-testing]
code_examples: false
difficulty_level: intermediate
summary: |
  This comprehensive guide explores implementing robust entity change tracking and audit logging systems using Entity Framework Core. It details three main approaches: basic audit properties with an `IAuditable` interface, comprehensive audit trails by overriding `DbContext.SaveChanges`, and advanced auditing using EF Core interceptors. The article provides extensive C# code examples for each method, including handling current user context, soft deletes, and detailed log storage. It also covers critical aspects like performance optimization through indexing, data archival, security considerations for log integrity, and unit testing audit functionality, emphasizing their importance for transparency, compliance, and data integrity in modern applications.
---
```

# How To Track Entity Changes With EF Core | Audit Logging | by Michael Maurice | Aug, 2025 | Medium

# How To Track Entity Changes With EF Core | Audit Logging

![](https://miro.medium.com/v2/resize:fit:700/1*KtPDxUM7cZFhv6JGw9t66A.png)
*Image: A flow chart titled "EFCore Audit Flow" illustrating the steps involved in EF Core auditing. The steps are presented as vertical colored bars: "Detect state" (blue), "Find entities" (red), "Set fields" (green), "Prep entries" (grey-blue), "Save logs" (yellow), "Intercept opt" (red), and "Query history" (brown).*

Entity auditing is one of the most critical features in modern enterprise applications, providing transparency, compliance, and security through comprehensive change tracking. Entity Framework Core offers powerful built-in mechanisms for implementing robust audit logging systems that automatically capture who changed what data and when. This comprehensive guide explores multiple approaches to implementing entity change tracking with EF Core, from simple audit properties to advanced interceptor-based solutions that provide complete audit trails for regulatory compliance and data integrity.

# Understanding Entity Auditing in EF Core

# EF Core Change Tracking Fundamentals

EF Core’s **ChangeTracker** automatically monitors entity state changes throughout the DbContext lifecycle. Understanding how change tracking works is essential for implementing effective audit logging:[learn.microsoft](https://learn.microsoft.com/en-us/ef/core/change-tracking/)

```csharp
// Entity states tracked by EF Core  
public enum EntityState  
{  
    Detached,    // Not tracked by context  
    Unchanged,   // Tracked but no changes detected  
    Added,       // New entity to be inserted  
    Modified,    // Existing entity with changes  
    Deleted      // Entity marked for deletion  
}
```

The ChangeTracker provides access to:

*   **Current values**: Entity property values after changes
*   **Original values**: Property values when entity was first loaded
*   **Property modification status**: Which specific properties changed
*   **Entity metadata**: Information about entity type and structure

# Approach 1: Basic Audit Properties with IAuditable Interface

# Creating the Auditable Interface

The simplest audit implementation uses an interface to mark entities as auditable:youtube

```csharp
using System.ComponentModel.DataAnnotations;  
namespace AuditExample.Domain.Interfaces  
{  
    public interface IAuditableEntity  
    {  
        string? CreatedBy { get; set; }  
        DateTime CreatedAt { get; set; }  
        string? LastModifiedBy { get; set; }  
        DateTime? LastModifiedAt { get; set; }  
    }  
}  
// Enhanced interface for soft delete support  
public interface ISoftDeletableEntity : IAuditableEntity  
{  
    string? DeletedBy { get; set; }  
    DateTime? DeletedAt { get; set; }  
    bool IsDeleted { get; set; }  
}
```

# Base Entity Implementation

Create a base entity class that implements the auditable interface:

```csharp
using System.ComponentModel.DataAnnotations;  
namespace AuditExample.Domain.Entities  
{  
    public abstract class BaseAuditableEntity : IAuditableEntity  
    {  
        [Key]  
        public Guid Id { get; set; }  
        [MaxLength(100)]  
        public string? CreatedBy { get; set; }  
        public DateTime CreatedAt { get; set; }  
        [MaxLength(100)]  
        public string? LastModifiedBy { get; set; }  
        public DateTime? LastModifiedAt { get; set; }  
        // Constructor  
        protected BaseAuditableEntity()  
        {  
            Id = Guid.NewGuid();  
            CreatedAt = DateTime.UtcNow;  
        }  
    }  
    // Example domain entity  
    public class Product : BaseAuditableEntity  
    {  
        [Required]  
        [MaxLength(200)]  
        public string Name { get; set; } = string.Empty;  
        [MaxLength(1000)]  
        public string? Description { get; set; }  
        [Range(0.01, double.MaxValue)]  
        public decimal Price { get; set; }  
        [Range(0, int.MaxValue)]  
        public int StockQuantity { get; set; }  
        public bool IsActive { get; set; } = true;  
        // Business method that triggers audit  
        public void UpdatePrice(decimal newPrice, string modifiedBy)  
        {  
            if (newPrice <= 0)  
                throw new ArgumentException("Price must be greater than zero", nameof(newPrice));  
            Price = newPrice;  
            LastModifiedBy = modifiedBy;  
            LastModifiedAt = DateTime.UtcNow;  
        }  
    }  
}
```

# Current User Service

Implement a service to capture the current user for audit purposes:

```csharp
using System.Security.Claims;  
namespace AuditExample.Services  
{  
    public interface ICurrentUserService  
    {  
        string? UserId { get; }  
        string? UserName { get; }  
        string? Email { get; }  
    }  
    public class CurrentUserService : ICurrentUserService  
    {  
        private readonly IHttpContextAccessor _httpContextAccessor;  
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)  
        {  
            _httpContextAccessor = httpContextAccessor;  
        }  
        public string? UserId => _httpContextAccessor.HttpContext?.User  
            ?.FindFirstValue(ClaimTypes.NameIdentifier);  
        public string? UserName => _httpContextAccessor.HttpContext?.User  
            ?.FindFirstValue(ClaimTypes.Name);  
        public string? Email => _httpContextAccessor.HttpContext?.User  
            ?.FindFirstValue(ClaimTypes.Email);  
    }  
    // Alternative implementation for JWT tokens  
    public class JwtCurrentUserService : ICurrentUserService  
    {  
        private readonly IHttpContextAccessor _httpContextAccessor;  
        public JwtCurrentUserService(IHttpContextAccessor httpContextAccessor)  
        {  
            _httpContextAccessor = httpContextAccessor;  
        }  
        public string? UserId => _httpContextAccessor.HttpContext?.User  
            ?.FindFirstValue("sub") ?? _httpContextAccessor.HttpContext?.User  
            ?.FindFirstValue("userid");  
        public string? UserName => _httpContextAccessor.HttpContext?.User  
            ?.FindFirstValue("preferred_username");  
        public string? Email => _httpContextAccessor.HttpContext?.User  
            ?.FindFirstValue("email");  
    }  
}
```

# DbContext Implementation with Audit Support

Override `SaveChanges` methods to automatically populate audit fields:

```csharp
using Microsoft.EntityFrameworkCore;  
using AuditExample.Domain.Interfaces;  
using AuditExample.Services;  
namespace AuditExample.Data  
{  
    public class ApplicationDbContext : DbContext  
    {  
        private readonly ICurrentUserService _currentUserService;  
        public ApplicationDbContext(            DbContextOptions<ApplicationDbContext> options,  
            ICurrentUserService currentUserService) : base(options)  
        {  
            _currentUserService = currentUserService;  
        }  
        public DbSet<Product> Products { get; set; }  
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)  
        {  
            SetAuditProperties();  
            return await base.SaveChangesAsync(cancellationToken);  
        }  
        public override int SaveChanges()  
        {  
            SetAuditProperties();  
            return base.SaveChanges();  
        }  
        private void SetAuditProperties()  
        {  
            var currentUser = _currentUserService.UserId ?? "system";  
            var utcNow = DateTime.UtcNow;  
            var auditableEntities = ChangeTracker.Entries<IAuditableEntity>()  
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)  
                .ToList();  
            foreach (var entry in auditableEntities)  
            {  
                switch (entry.State)  
                {  
                    case EntityState.Added:  
                        entry.Entity.CreatedBy = currentUser;  
                        entry.Entity.CreatedAt = utcNow;  
                        entry.Entity.LastModifiedBy = currentUser;  
                        entry.Entity.LastModifiedAt = utcNow;  
                        break;  
                    case EntityState.Modified:  
                        // Prevent overwriting creation audit fields  
                        entry.Property(e => e.CreatedBy).IsModified = false;  
                        entry.Property(e => e.CreatedAt).IsModified = false;  
                          
                        entry.Entity.LastModifiedBy = currentUser;  
                        entry.Entity.LastModifiedAt = utcNow;  
                        break;  
                }  
            }  
            // Handle soft delete entities  
            var softDeletableEntities = ChangeTracker.Entries<ISoftDeletableEntity>()  
                .Where(e => e.State == EntityState.Deleted)  
                .ToList();  
            foreach (var entry in softDeletableEntities)  
            {  
                entry.State = EntityState.Modified;  
                entry.Entity.IsDeleted = true;  
                entry.Entity.DeletedBy = currentUser;  
                entry.Entity.DeletedAt = utcNow;  
            }  
        }  
        protected override void OnModelCreating(ModelBuilder modelBuilder)  
        {  
            // Configure audit properties  
            modelBuilder.Entity<Product>(entity =>  
            {  
                entity.HasKey(e => e.Id);  
                entity.HasIndex(e => e.CreatedAt);  
                entity.HasIndex(e => e.LastModifiedAt);  
                  
                // Configure soft delete filter  
                entity.HasQueryFilter(e => !((ISoftDeletableEntity)e).IsDeleted);  
            });  
            base.OnModelCreating(modelBuilder);  
        }  
    }  
}
```

# Approach 2: Comprehensive Audit Trail with Full Change History

# Audit Log Entity Model

Create entities to store detailed audit information:

```csharp
using System.ComponentModel.DataAnnotations;  
namespace AuditExample.Domain.Entities  
{  
    public class AuditLog  
    {  
        [Key]  
        public Guid Id { get; set; }  
        [Required]  
        [MaxLength(100)]  
        public string TableName { get; set; } = string.Empty;  
        [Required]  
        public string RecordId { get; set; } = string.Empty;  
        [Required]  
        [MaxLength(10)]  
        public string Operation { get; set; } = string.Empty; // INSERT, UPDATE, DELETE  
        public string? OldValues { get; set; } // JSON representation  
        public string? NewValues { get; set; } // JSON representation  
        public List<string> ChangedColumns { get; set; } = new();  
        [MaxLength(100)]  
        public string? ModifiedBy { get; set; }  
        public DateTime ModifiedAt { get; set; }  
        [MaxLength(45)]  
        public string? IpAddress { get; set; }  
        [MaxLength(500)]  
        public string? UserAgent { get; set; }  
        public AuditLog()  
        {  
            Id = Guid.NewGuid();  
            ModifiedAt = DateTime.UtcNow;  
        }  
    }  
    public enum AuditType  
    {  
        Create = 1,  
        Update = 2,  
        Delete = 3  
    }  
}
```

# Audit Entry Helper Class

Create a helper class to capture entity changes:

```csharp
using Microsoft.EntityFrameworkCore;  
using Microsoft.EntityFrameworkCore.ChangeTracking;  
using System.Text.Json;  
namespace AuditExample.Data.Audit  
{  
    public class AuditEntry  
    {  
        public EntityEntry Entry { get; }  
        public AuditType AuditType { get; set; }  
        public string EntityName { get; set; }  
        public Dictionary<string, object?> KeyValues { get; } = new();  
        public Dictionary<string, object?> OldValues { get; } = new();  
        public Dictionary<string, object?> NewValues { get; } = new();  
        public List<string> ChangedColumns { get; } = new();  
        public List<PropertyEntry> TemporaryProperties { get; } = new();  
        public bool HasTemporaryProperties => TemporaryProperties.Count > 0;  
        public AuditEntry(EntityEntry entry)  
        {  
            Entry = entry;  
            EntityName = entry.Entity.GetType().Name;  
            SetAuditType();  
        }  
        private void SetAuditType()  
        {  
            AuditType = Entry.State switch  
            {  
                EntityState.Added => AuditType.Create,  
                EntityState.Modified => AuditType.Update,  
                EntityState.Deleted => AuditType.Delete,  
                _ => throw new ArgumentException($"Unexpected EntityState: {Entry.State}")  
            };  
        }  
        public AuditLog ToAuditLog(string? userId, string? ipAddress, string? userAgent)  
        {  
            var auditLog = new AuditLog  
            {  
                TableName = EntityName,  
                RecordId = JsonSerializer.Serialize(KeyValues),  
                Operation = AuditType.ToString().ToUpper(),  
                ModifiedBy = userId,  
                IpAddress = ipAddress,  
                UserAgent = userAgent,  
                ChangedColumns = ChangedColumns  
            };  
            if (OldValues.Count > 0)  
                auditLog.OldValues = JsonSerializer.Serialize(OldValues, GetJsonOptions());  
            if (NewValues.Count > 0)  
                auditLog.NewValues = JsonSerializer.Serialize(NewValues, GetJsonOptions());  
            return auditLog;  
        }  
        private static JsonSerializerOptions GetJsonOptions()  
        {  
            return new JsonSerializerOptions  
            {  
                WriteIndented = false,  
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull  
            };  
        }  
    }  
}
```

# Enhanced DbContext with Full Audit Logging

```csharp
using Microsoft.EntityFrameworkCore;  
using AuditExample.Data.Audit;  
using AuditExample.Services;  
using System.Text.Json;  
namespace AuditExample.Data  
{  
    public class AuditableDbContext : DbContext  
    {  
        private readonly ICurrentUserService _currentUserService;  
        private readonly IHttpContextAccessor _httpContextAccessor;  
        public AuditableDbContext(            DbContextOptions<AuditableDbContext> options,  
            ICurrentUserService currentUserService,  
            IHttpContextAccessor httpContextAccessor) : base(options)  
        {  
            _currentUserService = currentUserService;  
            _httpContextAccessor = httpContextAccessor;  
        }  
        public DbSet<Product> Products { get; set; }  
        public DbSet<AuditLog> AuditLogs { get; set; }  
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)  
        {  
            // Set audit properties for IAuditable entities  
            SetAuditProperties();  
            // Capture audit entries before saving  
            var auditEntries = GetAuditEntries();  
            var result = await base.SaveChangesAsync(cancellationToken);  
            // Update audit entries with generated keys and save audit logs  
            await SaveAuditLogsAsync(auditEntries, cancellationToken);  
            return result;  
        }  
        public override int SaveChanges()  
        {  
            SetAuditProperties();  
            var auditEntries = GetAuditEntries();  
            var result = base.SaveChanges();  
            SaveAuditLogs(auditEntries);  
            return result;  
        }  
        private List<AuditEntry> GetAuditEntries()  
        {  
            ChangeTracker.DetectChanges();  
            var auditEntries = new List<AuditEntry>();  
            foreach (var entry in ChangeTracker.Entries())  
            {  
                // Skip audit log entries and unchanged entities  
                if (entry.Entity is AuditLog || entry.State == EntityState.Unchanged)  
                    continue;  
                var auditEntry = new AuditEntry(entry);  
                auditEntries.Add(auditEntry);  
                // Capture property changes  
                foreach (var property in entry.Properties)  
                {  
                    if (property.IsTemporary)  
                    {  
                        auditEntry.TemporaryProperties.Add(property);  
                        continue;  
                    }  
                    string propertyName = property.Metadata.Name;  
                    // Handle primary keys  
                    if (property.Metadata.IsPrimaryKey())  
                    {  
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;  
                        continue;  
                    }  
                    // Capture changes based on entity state  
                    switch (entry.State)  
                    {  
                        case EntityState.Added:  
                            auditEntry.NewValues[propertyName] = property.CurrentValue;  
                            break;  
                        case EntityState.Deleted:  
                            auditEntry.OldValues[propertyName] = property.OriginalValue;  
                            break;  
                        case EntityState.Modified:  
                            if (property.IsModified)  
                            {  
                                auditEntry.ChangedColumns.Add(propertyName);  
                                auditEntry.OldValues[propertyName] = property.OriginalValue;  
                                auditEntry.NewValues[propertyName] = property.CurrentValue;  
                            }  
                            break;  
                    }  
                }  
            }  
            return auditEntries.Where(ae => ae.HasTemporaryProperties ||   
                                           ae.OldValues.Count > 0 ||   
                                           ae.NewValues.Count > 0).ToList();  
        }  
        private async Task SaveAuditLogsAsync(List<AuditEntry> auditEntries, CancellationToken cancellationToken)  
        {  
            if (!auditEntries.Any()) return;  
            var userId = _currentUserService.UserId;  
            var ipAddress = GetClientIpAddress();  
            var userAgent = GetUserAgent();  
            foreach (var auditEntry in auditEntries)  
            {  
                // Handle temporary properties (like auto-generated IDs)  
                foreach (var prop in auditEntry.TemporaryProperties)  
                {  
                    if (prop.Metadata.IsPrimaryKey())  
                    {  
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;  
                    }  
                    else  
                    {  
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;  
                    }  
                }  
                var auditLog = auditEntry.ToAuditLog(userId, ipAddress, userAgent);  
                AuditLogs.Add(auditLog);  
            }  
            await base.SaveChangesAsync(cancellationToken);  
        }  
        private void SaveAuditLogs(List<AuditEntry> auditEntries)  
        {  
            var task = SaveAuditLogsAsync(auditEntries, CancellationToken.None);  
            task.Wait();  
        }  
        private void SetAuditProperties()  
        {  
            var currentUser = _currentUserService.UserId ?? "system";  
            var utcNow = DateTime.UtcNow;  
            var auditableEntries = ChangeTracker.Entries<IAuditableEntity>()  
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);  
            foreach (var entry in auditableEntries)  
            {  
                switch (entry.State)  
                {  
                    case EntityState.Added:  
                        entry.Entity.CreatedBy = currentUser;  
                        entry.Entity.CreatedAt = utcNow;  
                        entry.Entity.LastModifiedBy = currentUser;  
                        entry.Entity.LastModifiedAt = utcNow;  
                        break;  
                    case EntityState.Modified:  
                        entry.Property(e => e.CreatedBy).IsModified = false;  
                        entry.Property(e => e.CreatedAt).IsModified = false;  
                        entry.Entity.LastModifiedBy = currentUser;  
                        entry.Entity.LastModifiedAt = utcNow;  
                        break;  
                }  
            }  
        }  
        private string? GetClientIpAddress()  
        {  
            var httpContext = _httpContextAccessor.HttpContext;  
            if (httpContext == null) return null;  
            return httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').First().Trim() ??  
                   httpContext.Request.Headers["X-Real-IP"].FirstOrDefault() ??  
                   httpContext.Connection.RemoteIpAddress?.ToString();  
        }  
        private string? GetUserAgent()  
        {  
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();  
        }  
        protected override void OnModelCreating(ModelBuilder modelBuilder)  
        {  
            // Configure audit log entity  
            modelBuilder.Entity<AuditLog>(entity =>  
            {  
                entity.HasIndex(e => e.TableName);  
                entity.HasIndex(e => e.ModifiedAt);  
                entity.HasIndex(e => e.ModifiedBy);  
                entity.HasIndex(e => new { e.TableName, e.RecordId });  
                // Configure JSON columns for databases that support it (PostgreSQL, SQL Server 2016+)  
                entity.Property(e => e.ChangedColumns)  
                    .HasConversion(  
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),  
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());  
            });  
            base.OnModelCreating(modelBuilder);  
        }  
    }  
}
```

# Approach 3: EF Core Interceptors for Advanced Auditing

# SaveChanges Interceptor Implementation

EF Core interceptors provide the most powerful and flexible approach to audit logging:

```csharp
using Microsoft.EntityFrameworkCore;  
using Microsoft.EntityFrameworkCore.Diagnostics;  
using AuditExample.Services;  
using System.Text.Json;  
using Microsoft.Extensions.Logging; // Added for ILogger  
namespace AuditExample.Data.Interceptors  
{  
    public class AuditingInterceptor : SaveChangesInterceptor  
    {  
        private readonly ICurrentUserService _currentUserService;  
        private readonly ILogger<AuditingInterceptor> _logger;  
        private List<AuditEntry> _auditEntries = new();  
        public AuditingInterceptor(ICurrentUserService currentUserService, ILogger<AuditingInterceptor> logger)  
        {  
            _currentUserService = currentUserService;  
            _logger = logger;  
        }  
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)  
        {  
            _auditEntries = CreateAuditEntries(eventData.Context);  
            return result;  
        }  
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(  
            DbContextEventData eventData,  
            InterceptionResult<int> result,  
            CancellationToken cancellationToken = default)  
        {  
            _auditEntries = CreateAuditEntries(eventData.Context);  
            return ValueTask.FromResult(result);  
        }  
        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)  
        {  
            SaveAuditEntries(eventData.Context);  
            return result;  
        }  
        public override ValueTask<int> SavedChangesAsync(            SaveChangesCompletedEventData eventData,  
            int result,  
            CancellationToken cancellationToken = default)  
        {  
            SaveAuditEntries(eventData.Context);  
            return ValueTask.FromResult(result);  
        }  
        public override void SaveChangesFailed(DbContextErrorEventData eventData)  
        {  
            _logger.LogError(eventData.Exception, "SaveChanges failed, audit entries will not be saved");  
            _auditEntries.Clear();  
            base.SaveChangesFailed(eventData);  
        }  
        public override Task SaveChangesFailedAsync(            DbContextErrorEventData eventData,  
            CancellationToken cancellationToken = default)  
        {  
            _logger.LogError(eventData.Exception, "SaveChangesAsync failed, audit entries will not be saved");  
            _auditEntries.Clear();  
            return base.SaveChangesFailedAsync(eventData, cancellationToken);  
        }  
        private List<AuditEntry> CreateAuditEntries(DbContext? context)  
        {  
            if (context == null) return new List<AuditEntry>();  
            context.ChangeTracker.DetectChanges();  
            var auditEntries = new List<AuditEntry>();  
            foreach (var entry in context.ChangeTracker.Entries())  
            {  
                if (entry.Entity is AuditLog || entry.State == EntityState.Unchanged)  
                    continue;  
                var auditEntry = new AuditEntry(entry);  
                auditEntries.Add(auditEntry);  
                foreach (var property in entry.Properties)  
                {  
                    if (property.IsTemporary)  
                    {  
                        auditEntry.TemporaryProperties.Add(property);  
                        continue;  
                    }  
                    string propertyName = property.Metadata.Name;  
                    if (property.Metadata.IsPrimaryKey())  
                    {  
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;  
                        continue;  
                    }  
                    switch (entry.State)  
                    {  
                        case EntityState.Added:  
                            auditEntry.NewValues[propertyName] = property.CurrentValue;  
                            break;  
                        case EntityState.Deleted:  
                            auditEntry.OldValues[propertyName] = property.OriginalValue;  
                            break;  
                        case EntityState.Modified when property.IsModified:  
                            auditEntry.ChangedColumns.Add(propertyName);  
                            auditEntry.OldValues[propertyName] = property.OriginalValue;  
                            auditEntry.NewValues[propertyName] = property.CurrentValue;  
                            break;  
                    }  
                }  
            }  
            return auditEntries;  
        }  
        private void SaveAuditEntries(DbContext? context)  
        {  
            if (context == null || !_auditEntries.Any()) return;  
            var userId = _currentUserService.UserId;  
            var auditLogs = new List<AuditLog>();  
            foreach (var auditEntry in _auditEntries)  
            {  
                // Update temporary properties with generated values  
                foreach (var tempProperty in auditEntry.TemporaryProperties)  
                {  
                    if (tempProperty.Metadata.IsPrimaryKey())  
                    {  
                        auditEntry.KeyValues[tempProperty.Metadata.Name] = tempProperty.CurrentValue;  
                    }  
                    else  
                    {  
                        auditEntry.NewValues[tempProperty.Metadata.Name] = tempProperty.CurrentValue;  
                    }  
                }  
                var auditLog = auditEntry.ToAuditLog(userId, null, null);  
                auditLogs.Add(auditLog);  
            }  
            // Save audit logs using a separate context to avoid circular calls  
            using (var auditContext = CreateAuditContext(context))  
            {  
                auditContext.AuditLogs.AddRange(auditLogs);  
                auditContext.SaveChanges();  
            }  
            _auditEntries.Clear();  
        }  
        private DbContext CreateAuditContext(DbContext originalContext)  
        {  
            // Create a new context instance with the same configuration but without interceptors  
            var optionsBuilder = new DbContextOptionsBuilder<AuditableDbContext>();  
              
            if (originalContext.Database.IsSqlServer())  
            {  
                optionsBuilder.UseSqlServer(originalContext.Database.GetConnectionString());  
            }  
            else if (originalContext.Database.IsNpgsql())  
            {  
                optionsBuilder.UseNpgsql(originalContext.Database.GetConnectionString());  
            }  
              
            // Don't add the audit interceptor to prevent recursion  
            return new AuditableDbContext(optionsBuilder.Options, _currentUserService, null);  
        }  
    }  
}
```

# Configuring Interceptors in Program.cs

```csharp
using Microsoft.EntityFrameworkCore;  
using AuditExample.Data;  
using AuditExample.Data.Interceptors;  
using AuditExample.Services;  
var builder = WebApplication.CreateBuilder(args);  
// Register services  
builder.Services.AddHttpContextAccessor();  
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();  
builder.Services.AddScoped<AuditingInterceptor>();  
// Configure Entity Framework with audit interceptor  
builder.Services.AddDbContext<AuditableDbContext>((serviceProvider, options) =>  
{  
    var auditInterceptor = serviceProvider.GetRequiredService<AuditingInterceptor>();  
      
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))  
           .AddInterceptors(auditInterceptor)  
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())  
           .EnableDetailedErrors(builder.Environment.IsDevelopment());  
});  
builder.Services.AddControllers();  
builder.Services.AddEndpointsApiExplorer();  
builder.Services.AddSwaggerGen();  
var app = builder.Build();  
if (app.Environment.IsDevelopment())  
{  
    app.UseSwagger();  
    app.UseSwaggerUI();  
}  
app.UseHttpsRedirection();  
app.UseAuthentication();  
app.UseAuthorization();  
app.MapControllers();  
app.Run();
```

# Querying and Reporting Audit Data

# Audit History Service

Create a service to query audit history effectively:

```csharp
using Microsoft.EntityFrameworkCore;  
using AuditExample.Data;  
using System.Text.Json;  
namespace AuditExample.Services  
{  
    public interface IAuditService  
    {  
        Task<List<AuditLogDto>> GetEntityAuditHistoryAsync(string entityName, string recordId);  
        Task<List<AuditLogDto>> GetUserAuditHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);  
        Task<List<AuditLogDto>> GetAuditHistoryAsync(AuditFilterDto filter);  
    }  
    public class AuditService : IAuditService  
    {  
        private readonly AuditableDbContext _context;  
        public AuditService(AuditableDbContext context)  
        {  
            _context = context;  
        }  
        public async Task<List<AuditLogDto>> GetEntityAuditHistoryAsync(string entity