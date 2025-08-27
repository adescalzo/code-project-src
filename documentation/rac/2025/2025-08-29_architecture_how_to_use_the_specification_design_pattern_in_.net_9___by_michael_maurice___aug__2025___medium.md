```yaml
---
title: "How To Use The Specification Design Pattern in .net 9 | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/how-to-use-the-specification-design-pattern-in-net-9-3ac3fe889d87
date_published: 2025-08-29T17:01:42.390Z
date_captured: 2025-09-06T17:17:42.140Z
domain: medium.com
author: Michael Maurice
category: architecture
technologies: [.NET 9, Entity Framework Core, ASP.NET Core, Ardalis.Specification, Ardalis.Specification.EntityFrameworkCore, SQL Server, Xunit, FluentAssertions, Swagger, OpenAPI, Microsoft.Extensions.Logging]
programming_languages: [C#, SQL]
tags: [specification-pattern, dotnet, entity-framework-core, clean-architecture, design-patterns, data-access, repository-pattern, query-logic, unit-testing, web-api]
key_concepts: [Specification Design Pattern, Clean Architecture, Repository Pattern, Unit Testing, Dependency Injection, Expression Trees, Query Optimization, Domain-Driven Design]
code_examples: false
difficulty_level: intermediate
summary: |
  This comprehensive guide demonstrates the implementation of the Specification Design Pattern in .NET 9, showcasing its power in encapsulating query logic and business rules. It details how to build a custom specification infrastructure, including composable specifications with boolean logic, and integrate it seamlessly with Entity Framework Core and a generic repository pattern. The article further illustrates the pattern's application within service and API layers, covers dependency injection, and provides examples of unit testing. Additionally, it introduces the Ardalis.Specification library as a robust alternative and offers best practices for performance optimization and code maintainability.
---
```

# How To Use The Specification Design Pattern in .net 9 | by Michael Maurice | Aug, 2025 | Medium

# How To Use The Specification Design Pattern in .net 9

![A diagram illustrating the Specification Design Pattern in .NET 9. Multiple "Specification" blocks, each representing a specific query criterion (e.g., "User By Email", "Order Over Amount", "Product In Category"), connect to a central "NET 9" component. This component then links to a "Satisfies" block, which in turn filters and outputs multiple "Object" blocks, symbolizing the entities that meet the combined specifications. The background features a glowing circuit board pattern, reinforcing the technical context.](https://miro.medium.com/v2/resize:fit:700/1*ti1CKTCBDAAcSGQZHwp81g.png)

**If you want the full source code, download it from this link:** [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)

## How To Use The Specification Design Pattern in .NET 9

The Specification Design Pattern is a powerful technique for encapsulating query logic and business rules in a clean, testable, and reusable manner. In .NET 9, this pattern becomes even more valuable when combined with Entity Framework Core and Clean Architecture principles. This comprehensive guide shows you how to implement and use the Specification pattern effectively.

## Understanding the Specification Pattern

## What is the Specification Pattern?

The Specification pattern encapsulates query logic in its own class and promotes reuse of common queries. It allows you to build complex queries by combining smaller, focused specifications using boolean logic.

Core Benefits:

*   Reusable Query Logic: Write once, use everywhere
*   Testable in Isolation: Each specification can be unit tested independently
*   Domain-Focused: Keep business rules in the domain layer
*   Composable: Combine specifications using AND, OR, NOT operations

## The Problem Without Specifications

Consider this common scenario where query logic proliferates throughout your codebase:

```csharp
// Service methods become unwieldy  
public class ProductService  
{  
    public async Task<int> GetActiveProductCountAsync() =>   
        await _context.Products.CountAsync(p => p.IsActive);  
    public async Task<int> GetExpensiveProductCountAsync() =>   
        await _context.Products.CountAsync(p => p.Price > 1000);  
    public async Task<int> GetActiveExpensiveProductCountAsync() =>   
        await _context.Products.CountAsync(p => p.IsActive && p.Price > 1000);  
    public async Task<int> GetInactiveExpensiveProductCountAsync() =>   
        await _context.Products.CountAsync(p => !p.IsActive && p.Price > 1000);  
    // This grows exponentially with more criteria...  
}
```

Problems:

*   Query Proliferation: Endless combinations of query methods
*   Code Duplication: Same logic repeated in multiple places
*   Maintenance Nightmare: Changes require updating multiple locations
*   Testing Challenges: Difficult to test query logic in isolation

## Implementing Custom Specification Pattern in .NET 9

## Base Specification Infrastructure

Create a robust foundation for your specification system:

```csharp
// Specifications/ISpecification.cs  
using System.Linq.Expressions;  
namespace SpecificationPattern.Specifications;  
public interface ISpecification<T>  
{  
    Expression<Func<T, bool>>? Criteria { get; }  
    List<Expression<Func<T, object>>> Includes { get; }  
    List<string> IncludeStrings { get; }  
    Expression<Func<T, object>>? OrderBy { get; }  
    Expression<Func<T, object>>? OrderByDescending { get; }  
    Expression<Func<T, object>>? GroupBy { get; }  
      
    int Take { get; }  
    int Skip { get; }  
    bool IsPagingEnabled { get; }  
    bool IsSplitQuery { get; }  
      
    bool IsSatisfiedBy(T entity);  
}  
// Base implementation  
public abstract class BaseSpecification<T> : ISpecification<T>  
{  
    public Expression<Func<T, bool>>? Criteria { get; private set; }  
    public List<Expression<Func<T, object>>> Includes { get; } = [];  
    public List<string> IncludeStrings { get; } = [];  
    public Expression<Func<T, object>>? OrderBy { get; private set; }  
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }  
    public Expression<Func<T, object>>? GroupBy { get; private set; }  
    public int Take { get; private set; }  
    public int Skip { get; private set; }  
    public bool IsPagingEnabled { get; private set; }  
    public bool IsSplitQuery { get; private set; }  
    protected BaseSpecification() { }  
    protected BaseSpecification(Expression<Func<T, bool>> criteria)  
    {  
        Criteria = criteria;  
    }  
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)  
    {  
        Includes.Add(includeExpression);  
    }  
    protected virtual void AddInclude(string includeString)  
    {  
        IncludeStrings.Add(includeString);  
    }  
    protected virtual void ApplyPaging(int skip, int take)  
    {  
        Skip = skip;  
        Take = take;  
        IsPagingEnabled = true;  
    }  
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)  
    {  
        OrderBy = orderByExpression;  
    }  
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)  
    {  
        OrderByDescending = orderByDescendingExpression;  
    }  
    protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)  
    {  
        GroupBy = groupByExpression;  
    }  
    protected virtual void ApplySplitQuery()  
    {  
        IsSplitQuery = true;  
    }  
    public virtual bool IsSatisfiedBy(T entity)  
    {  
        if (Criteria == null) return true;  
          
        var compiledExpression = Criteria.Compile();  
        return compiledExpression(entity);  
    }  
}
```

## Specification Evaluator for Entity Framework Core

Create an evaluator to convert specifications into EF Core queries:

```csharp
// Specifications/SpecificationEvaluator.cs  
using Microsoft.EntityFrameworkCore;  
namespace SpecificationPattern.Specifications;  
public static class SpecificationEvaluator  
{  
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification)   
        where T : class  
    {  
        var query = inputQuery;  
        // Apply criteria (Where clause)  
        if (specification.Criteria != null)  
        {  
            query = query.Where(specification.Criteria);  
        }  
        // Apply includes  
        query = specification.Includes  
            .Aggregate(query, (current, include) => current.Include(include));  
        // Apply string-based includes  
        query = specification.IncludeStrings  
            .Aggregate(query, (current, include) => current.Include(include));  
        // Apply ordering  
        if (specification.OrderBy != null)  
        {  
            query = query.OrderBy(specification.OrderBy);  
        }  
        else if (specification.OrderByDescending != null)  
        {  
            query = query.OrderByDescending(specification.OrderByDescending);  
        }  
        // Apply grouping  
        if (specification.GroupBy != null)  
        {  
            query = query.GroupBy(specification.GroupBy).SelectMany(x => x);  
        }  
        // Apply split query if specified  
        if (specification.IsSplitQuery)  
        {  
            query = query.AsSplitQuery();  
        }  
        // Apply paging (should be last)  
        if (specification.IsPagingEnabled)  
        {  
            query = query.Skip(specification.Skip).Take(specification.Take);  
        }  
        return query;  
    }  
}
```

## Composable Specifications with Boolean Logic

Implement advanced composition capabilities:

```csharp
// Specifications/CompositeSpecifications.cs  
using System.Linq.Expressions;  
namespace SpecificationPattern.Specifications;  
public class AndSpecification<T> : BaseSpecification<T>  
{  
    private readonly ISpecification<T> _left;  
    private readonly ISpecification<T> _right;  
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)  
    {  
        _left = left;  
        _right = right;  
        if (_left.Criteria != null && _right.Criteria != null)  
        {  
            var leftExpression = _left.Criteria;  
            var rightExpression = _right.Criteria;  
              
            var parameter = Expression.Parameter(typeof(T));  
            var leftBody = ParameterReplacer.Replace(leftExpression.Body, leftExpression.Parameters, parameter);  
            var rightBody = ParameterReplacer.Replace(rightExpression.Body, rightExpression.Parameters, parameter);  
              
            var andExpression = Expression.AndAlso(leftBody, rightBody);  
            Criteria = Expression.Lambda<Func<T, bool>>(andExpression, parameter);  
        }  
        else  
        {  
            Criteria = _left.Criteria ?? _right.Criteria;  
        }  
    }  
    public override bool IsSatisfiedBy(T entity)  
    {  
        return _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);  
    }  
}  
public class OrSpecification<T> : BaseSpecification<T>  
{  
    private readonly ISpecification<T> _left;  
    private readonly ISpecification<T> _right;  
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)  
    {  
        _left = left;  
        _right = right;  
        if (_left.Criteria != null && _right.Criteria != null)  
        {  
            var leftExpression = _left.Criteria;  
            var rightExpression = _right.Criteria;  
              
            var parameter = Expression.Parameter(typeof(T));  
            var leftBody = ParameterReplacer.Replace(leftExpression.Body, leftExpression.Parameters, parameter);  
            var rightBody = ParameterReplacer.Replace(rightExpression.Body, rightExpression.Parameters, parameter);  
              
            var orExpression = Expression.OrElse(leftBody, rightBody);  
            Criteria = Expression.Lambda<Func<T, bool>>(orExpression, parameter);  
        }  
        else  
        {  
            Criteria = _left.Criteria ?? _right.Criteria;  
        }  
    }  
    public override bool IsSatisfiedBy(T entity)  
    {  
        return _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);  
    }  
}  
public class NotSpecification<T> : BaseSpecification<T>  
{  
    private readonly ISpecification<T> _inner;  
    public NotSpecification(ISpecification<T> inner)  
    {  
        _inner = inner;  
        if (_inner.Criteria != null)  
        {  
            var expression = _inner.Criteria;  
            var parameter = expression.Parameters;  
            var body = Expression.Not(expression.Body);  
            Criteria = Expression.Lambda<Func<T, bool>>(body, parameter);  
        }  
    }  
    public override bool IsSatisfiedBy(T entity)  
    {  
        return !_inner.IsSatisfiedBy(entity);  
    }  
}  
// Helper class for parameter replacement in expressions  
internal class ParameterReplacer : ExpressionVisitor  
{  
    private readonly ParameterExpression _source;  
    private readonly ParameterExpression _target;  
    private ParameterReplacer(ParameterExpression source, ParameterExpression target)  
    {  
        _source = source;  
        _target = target;  
    }  
    public static Expression Replace(Expression expression, ParameterExpression source, ParameterExpression target)  
    {  
        return new ParameterReplacer(source, target).Visit(expression);  
    }  
    protected override Expression VisitParameter(ParameterExpression node)  
    {  
        return node == _source ? _target : base.VisitParameter(node);  
    }  
}  
// Extension methods for fluent composition  
public static class SpecificationExtensions  
{  
    public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right)  
    {  
        return new AndSpecification<T>(left, right);  
    }  
    public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right)  
    {  
        return new OrSpecification<T>(left, right);  
    }  
    public static ISpecification<T> Not<T>(this ISpecification<T> specification)  
    {  
        return new NotSpecification<T>(specification);  
    }  
}
```

## Domain-Specific Specifications

## Product Entity and Specifications

Create focused, business-meaningful specifications:

```csharp
// Domain/Entities/Product.cs  
namespace SpecificationPattern.Domain.Entities;  
public class Product  
{  
    public int Id { get; set; }  
    public string Name { get; set; } = string.Empty;  
    public decimal Price { get; set; }  
    public bool IsActive { get; set; }  
    public int CategoryId { get; set; }  
    public Category Category { get; set; } = null!;  
    public DateTime CreatedDate { get; set; }  
    public int StockQuantity { get; set; }  
    public string? Description { get; set; }  
    public bool IsFeatured { get; set; }  
    public decimal? DiscountPercentage { get; set; }  
}  
public class Category  
{  
    public int Id { get; set; }  
    public string Name { get; set; } = string.Empty;  
    public bool IsActive { get; set; }  
    public List<Product> Products { get; set; } = [];  
}  
// Specifications/ProductSpecifications.cs  
namespace SpecificationPattern.Specifications.Products;  
public class ActiveProductsSpecification : BaseSpecification<Product>  
{  
    public ActiveProductsSpecification() : base(product => product.IsActive)  
    {  
        ApplyOrderBy(p => p.Name);  
    }  
}  
public class ProductsInPriceRangeSpecification : BaseSpecification<Product>  
{  
    public ProductsInPriceRangeSpecification(decimal minPrice, decimal maxPrice)   
        : base(product => product.Price >= minPrice && product.Price <= maxPrice)  
    {  
        ApplyOrderBy(p => p.Price);  
    }  
}  
public class ProductsByCategorySpecification : BaseSpecification<Product>  
{  
    public ProductsByCategorySpecification(int categoryId)   
        : base(product => product.CategoryId == categoryId)  
    {  
        AddInclude(p => p.Category);  
        ApplyOrderBy(p => p.Name);  
    }  
}  
public class FeaturedProductsSpecification : BaseSpecification<Product>  
{  
    public FeaturedProductsSpecification() : base(product => product.IsFeatured)  
    {  
        AddInclude(p => p.Category);  
        ApplyOrderBy(p => p.CreatedDate);  
    }  
}  
public class ProductsWithLowStockSpecification : BaseSpecification<Product>  
{  
    public ProductsWithLowStockSpecification(int threshold = 10)   
        : base(product => product.StockQuantity <= threshold && product.IsActive)  
    {  
        ApplyOrderBy(p => p.StockQuantity);  
    }  
}  
public class ProductsOnSaleSpecification : BaseSpecification<Product>  
{  
    public ProductsOnSaleSpecification()   
        : base(product => product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)  
    {  
        ApplyOrderByDescending(p => p.DiscountPercentage);  
    }  
}  
public class RecentProductsSpecification : BaseSpecification<Product>  
{  
    public RecentProductsSpecification(int days = 30)   
        : base(product => product.CreatedDate >= DateTime.UtcNow.AddDays(-days))  
    {  
        ApplyOrderByDescending(p => p.CreatedDate);  
    }  
}  
// Complex specification with multiple criteria  
public class PremiumProductsSpecification : BaseSpecification<Product>  
{  
    public PremiumProductsSpecification()   
        : base(product =>   
            product.IsActive &&   
            product.Price > 500 &&   
            product.IsFeatured &&   
            product.StockQuantity > 0)  
    {  
        AddInclude(p => p.Category);  
        ApplyOrderByDescending(p => p.Price);  
    }  
}  
// Parameterized specification for flexible filtering  
public class ProductFilterSpecification : BaseSpecification<Product>  
{  
    public ProductFilterSpecification(ProductFilter filter)  
    {  
        var criteria = BuildCriteria(filter);  
        if (criteria != null)  
        {  
            Criteria = criteria;  
        }  
        if (filter.IncludeCategory)  
        {  
            AddInclude(p => p.Category);  
        }  
        if (filter.PageSize > 0)  
        {  
            ApplyPaging(filter.Skip, filter.PageSize);  
        }  
        ApplySorting(filter);  
    }  
    private static Expression<Func<Product, bool>>? BuildCriteria(ProductFilter filter)  
    {  
        Expression<Func<Product, bool>>? criteria = null;  
        if (filter.IsActive.HasValue)  
        {  
            var activeCriteria = (Expression<Func<Product, bool>>)(p => p.IsActive == filter.IsActive.Value);  
            criteria = criteria == null ? activeCriteria : CombineAnd(criteria, activeCriteria);  
        }  
        if (filter.MinPrice.HasValue)  
        {  
            var minPriceCriteria = (Expression<Func<Product, bool>>)(p => p.Price >= filter.MinPrice.Value);  
            criteria = criteria == null ? minPriceCriteria : CombineAnd(criteria, minPriceCriteria);  
        }  
        if (filter.MaxPrice.HasValue)  
        {  
            var maxPriceCriteria = (Expression<Func<Product, bool>>)(p => p.Price <= filter.MaxPrice.Value);  
            criteria = criteria == null ? maxPriceCriteria : CombineAnd(criteria, maxPriceCriteria);  
        }  
        if (filter.CategoryId.HasValue)  
        {  
            var categoryCriteria = (Expression<Func<Product, bool>>)(p => p.CategoryId == filter.CategoryId.Value);  
            criteria = criteria == null ? categoryCriteria : CombineAnd(criteria, categoryCriteria);  
        }  
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))  
        {  
            var searchTerm = filter.SearchTerm.ToLower();  
            var searchCriteria = (Expression<Func<Product, bool>>)(p =>   
                p.Name.ToLower().Contains(searchTerm) ||   
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)));  
            criteria = criteria == null ? searchCriteria : CombineAnd(criteria, searchCriteria);  
        }  
        return criteria;  
    }  
    private static Expression<Func<T, bool>> CombineAnd<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)  
    {  
        var parameter = Expression.Parameter(typeof(T));  
        var leftBody = ParameterReplacer.Replace(left.Body, left.Parameters, parameter);  
        var rightBody = ParameterReplacer.Replace(right.Body, right.Parameters, parameter);  
        var andExpression = Expression.AndAlso(leftBody, rightBody);  
        return Expression.Lambda<Func<T, bool>>(andExpression, parameter);  
    }  
    private void ApplySorting(ProductFilter filter)  
    {  
        switch (filter.SortBy?.ToLower())  
        {  
            case "name":  
                if (filter.SortDescending)  
                    ApplyOrderByDescending(p => p.Name);  
                else  
                    ApplyOrderBy(p => p.Name);  
                break;  
            case "price":  
                if (filter.SortDescending)  
                    ApplyOrderByDescending(p => p.Price);  
                else  
                    ApplyOrderBy(p => p.Price);  
                break;  
            case "created":  
                if (filter.SortDescending)  
                    ApplyOrderByDescending(p => p.CreatedDate);  
                else  
                    ApplyOrderBy(p => p.CreatedDate);  
                break;  
            default:  
                ApplyOrderBy(p => p.Name);  
                break;  
        }  
    }  
}  
// Filter model for parameterized specifications  
public class ProductFilter  
{  
    public bool? IsActive { get; set; }  
    public decimal? MinPrice { get; set; }  
    public decimal? MaxPrice { get; set; }  
    public int? CategoryId { get; set; }  
    public string? SearchTerm { get; set; }  
    public bool IncludeCategory { get; set; } = true;  
    public string? SortBy { get; set; } = "name";  
    public bool SortDescending { get; set; }  
    public int Skip { get; set; }  
    public int PageSize { get; set; } = 20;  
}
```

## Generic Repository with Specifications

## Repository Implementation

Create a generic repository that works seamlessly with specifications:

```csharp
// Repositories/IRepository.cs  
namespace SpecificationPattern.Repositories;  
public interface IRepository<T> where T : class  
{  
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;  
    Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);  
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);  
    Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);  
    Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);  
    Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);  
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);  
    Task<List<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);  
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);  
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);  
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);  
}  
// Repositories/Repository.cs  
using Microsoft.EntityFrameworkCore;  
using SpecificationPattern.Data;  
using SpecificationPattern.Specifications;  
public class Repository<T> : IRepository<T> where T : class  
{  
    private readonly ApplicationDbContext _context;  
    private readonly DbSet<T> _dbSet;  
    public Repository(ApplicationDbContext context)  
    {  
        _context = context;  
        _dbSet = context.Set<T>();  
    }  
    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull  
    {  
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);  
    }  
    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)  
    {  
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);  
    }  
    public async Task<List<T>> ListAsync(CancellationToken cancellationToken = default)  
    {  
        return await _dbSet.ToListAsync(cancellationToken);  
    }  
    public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)  
    {  
        return await ApplySpecification(specification).ToListAsync(cancellationToken);  
    }  
    public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)  
    {  
        return await ApplySpecification(specification, true).CountAsync(cancellationToken);  
    }  
    public async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)  
    {  
        return await ApplySpecification(specification, true).AnyAsync(cancellationToken);  
    }  
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)  
    {  
        await _dbSet.AddAsync(entity, cancellationToken);  
        await _context.SaveChangesAsync(cancellationToken);  
        return entity;  
    }  
    public async Task<List<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)  
    {  
        var entityList = entities.ToList();  
        await _dbSet.AddRangeAsync(entityList, cancellationToken);  
        await _context.SaveChangesAsync(cancellationToken);  
        return entityList;  
    }  
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)  
    {  
        _dbSet.Update(entity);  
        await _context.SaveChangesAsync(cancellationToken);  
    }  
    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)  
    {  
        _dbSet.Remove(entity);  
        await _context.SaveChangesAsync(cancellationToken);  
    }  
    public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)  
    {  
        _dbSet.RemoveRange(entities);  
        await _context.SaveChangesAsync(cancellationToken);  
    }  
    private IQueryable<T> ApplySpecification(ISpecification<T> specification, bool includeOnlyCountableProperties = false)  
    {  
        return SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), specification, includeOnlyCountableProperties);  
    }  
}  
// Enhanced SpecificationEvaluator with count optimization  
public static class SpecificationEvaluator  
{  
    public static IQueryable<T> GetQuery<T>(        IQueryable<T> inputQuery,   
        ISpecification<T> specification,  
        bool includeOnlyCountableProperties = false) where T : class  
    {  
        var query = inputQuery;  
        // Apply criteria (Where clause)  
        if (specification.Criteria != null)  
        {  
            query = query.Where(specification.Criteria);  
        }  
        // For count operations, skip expensive operations  
        if (includeOnlyCountableProperties)  
        {  
            return query;  
        }  
        // Apply includes  
        query = specification.Includes  
            .Aggregate(query, (current, include) => current.Include(include));  
        query = specification.IncludeStrings  
            .Aggregate(query, (current, include) => current.Include(include));  
        // Apply ordering  
        if (specification.OrderBy != null)  
        {  
            query = query.OrderBy(specification.OrderBy);  
        }  
        else if (specification.OrderByDescending != null)  
        {  
            query = query.OrderByDescending(specification.OrderByDescending);  
        }  
        // Apply grouping  
        if (specification.GroupBy != null)  
        {  
            query = query.GroupBy(specification.GroupBy).SelectMany(x => x);  
        }  
        // Apply split query  
        if (specification.IsSplitQuery)  
        {  
            query = query.AsSplitQuery();  
        }  
        // Apply paging  
        if (specification.IsPagingEnabled)  
        {  
            query = query.Skip(specification.Skip).Take(specification.Take);  
        }  
        return query;  
    }  
}
```

## Service Layer Implementation

## Product Service with Specifications

Demonstrate clean service implementation using specifications:

```csharp
// Services/IProductService.cs  
using SpecificationPattern.Domain.Entities;  
using SpecificationPattern.Specifications.Products;  
namespace SpecificationPattern.Services;  
public interface IProductService  
{  
    Task<List<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);  
    Task<List<Product>> GetProductsInPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default);  
    Task<List<Product>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);  
    Task<List<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default);  
    Task<List<Product>> GetProductsWithLowStockAsync(int threshold = 10, CancellationToken cancellationToken = default);  
    Task<List<Product>> GetProductsOnSaleAsync(CancellationToken cancellationToken = default);  
    Task<List<Product>> GetRecentProductsAsync(int days = 30, CancellationToken cancellationToken = default);  
    Task<List<Product>> GetPremiumProductsAsync(CancellationToken cancellationToken = default);  
    Task<List<Product>> GetFilteredProductsAsync(ProductFilter filter, CancellationToken cancellationToken = default);  
    Task<int> GetProductCountAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default);  
    Task<bool> HasProductsAsync(ISpecification<Product> specification, CancellationToken cancellation