```yaml
---
title: How to Perform Dynamic Code Execution in .NET with C# Eval Expression
source: https://antondevtips.com/blog/how-to-perform-dynamic-code-execution-in-dotnet-with-csharp-eval-expression
date_published: 2025-03-04T08:45:08.896Z
date_captured: 2025-08-06T17:36:37.752Z
domain: antondevtips.com
author: Anton Martyniuk
category: backend
technologies: [C# Eval Expression, .NET, .NET Core, .NET Framework, .NET 5+, LINQ, Entity Framework Core, SQL Server, SQLite, ASP.NET Core, NuGet, DotNetFiddle, ZZZ Code AI]
programming_languages: [C#, SQL]
tags: [dynamic-code-execution, dotnet, csharp, linq, ef-core, templating, runtime-compilation, expression-evaluation, code-injection, web-api]
key_concepts: [dynamic-code-execution, expression-evaluation, runtime-compilation, dynamic-templating, dynamic-linq-queries, code-injection, eval-context, minimal-api]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces C# Eval Expression, a powerful library for dynamic code execution in .NET applications. It demonstrates its use for dynamic email templating, allowing C# code to be embedded within string templates for personalized content generation. The post also illustrates how to construct and execute dynamic LINQ queries for both in-memory collections and Entity Framework Core, showcasing the translation of string-based expressions into SQL. Additionally, it covers using `EvalContext` for safe, dynamic C# code injection to modify application behavior at runtime without redeployment, highlighting its utility for advanced customization and plugin architectures.]
---
```

# How to Perform Dynamic Code Execution in .NET with C# Eval Expression

![Cover image for the article titled "How to Perform Dynamic Code Execution in .NET with C# Eval Expression" with a "dev tips" logo.](https://antondevtips.com/media/covers/dotnet/cover_dotnet_csharp_eval_expression.png)

# How to Perform Dynamic Code Execution in .NET with C# Eval Expression

Mar 4, 2025

[Download source code](/source-code/how-to-perform-dynamic-code-execution-in-dotnet-with-csharp-eval-expression)

7 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Have you ever worked on an email template with placeholders that need to be replaced dynamically based on a set of values? Or perhaps you've had to build a dynamic C# or LINQ expression for EF Core to filter and sort based on any attributes of the entity?

Today I want to introduce you to a library that I find incredibly helpful for these scenarios.

This library is [C# Eval Expression](https://eval-expression.net/?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website), one of the most powerful and flexible expression evaluators you can find.

Let's dive in and see how it can simplify your development workflow.

## Dynamic Email Text Templating with C# Eval Expression

To get started with [C# Eval Expression](https://eval-expression.net/?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website), install the following Nuget package:

```bash
dotnet add package Z.Expressions.Eval
```

**C# Eval Expression** is compatible with .NET, .NET Core and .NET Framework:

*   .NET 5+
*   .NET Framework 4.0+

C# Eval Expression allows you to evaluate and execute C# code on the fly.

A common use case is generating personalized email content by replacing placeholders with model values in a template. With C# Eval Expression, you can easily parse and render templates at runtime.

Let's explore an example. Consider the following model definitions:

```csharp
public record Customer
{
    public string Name { get; init; } = null!;
    public List<OrderItem> Items { get; init; } = [];
}

public record OrderItem
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
}
```

Here is a basic email template:

```csharp
public string GetEmailTemplate()
{
    return @"
Hello {{Name}},

Thank you for your order.

Best regards,
The Team
";
}
```

Using `Eval.Execute`, you can pass a `Customer` object to dynamically generate the email text:

```csharp
var emailTemplate = GetEmailTemplate();
var emailText = Eval.Execute<string>($"return $$'''{emailTemplate}'''", customer);

Console.WriteLine(emailText);
```

This produces the following output:

```
Hello Jermaine,

Thank you for your order.

Best regards,
The Team
```

Now, let's enhance the template to include a table of order items. C# Eval allows embedding C# code (as string) to build an HTML table dynamically:

```csharp
var emailTemplate = GetEmailTemplate();
var emailText = Eval.Execute<string>($"return $$'''{emailTemplate}'''", customer);

public string GetEmailTemplate()
{
    return @"
Dear {{Name}},

Thank you for your recent order with us! We are currently processing the following items:

<table>
<tr><td>Product Name</td><td>Price ($)</td><td>Quantity</td></tr>
{{
    var sb = new StringBuilder();
    foreach(var item in Items) {
        sb.AppendLine($$""""""<tr><td>{{item.Name}}</td><td>${{item.Price}}</td><td>{{item.Quantity}}</td></tr>"""""");
    }
    return sb.ToString();
}}
</table>

We will notify you once your order is shipped.

Best regards,
The Customer Service Team
";
}
```

In this template we are using a string with a C# code that creates a `StringBuilder` to append a table with order items. When executed, the template will produce the following content:

```csharp
Dear Arnold,

Thank you for your recent order with us! We are currently processing the following items:

<table>
<tr><td>Product Name</td><td>Price ($)</td><td>Quantity</td></tr>
<tr><td>Tasty Plastic Cheese</td><td>$41,99</td><td>1</td></tr>
<tr><td>Awesome Fresh Computer</td><td>$402,75</td><td>8</td></tr>
<tr><td>Ergonomic Concrete Shirt</td><td>$344,47</td><td>5</td></tr>
<tr><td>Rustic Plastic Gloves</td><td>$905,35</td><td>9</td></tr>
<tr><td>Ergonomic Steel Salad</td><td>$410,66</td><td>5</td></tr>

</table>

We will notify you once your order is shipped.

Best regards,
The Customer Service Team
```

It looks fantastic, right? You can experiment with a similar example on [DotNetFiddle](https://dotnetfiddle.net/Mf3CnA).

The C# Eval Expression library supports nearly everything from basic keywords to more advanced usage of the C# language, including:

*   Anonymous Type
*   Extension Method
*   Lambda Expression
*   LINQ Methods
*   Method Overloads

## Executing LINQ Dynamically

[Dynamic LINQ](https://eval-expression.net/my-first-linq-dynamic?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) queries are another powerful use-case for C# Eval Expression.

Imagine a scenario where you need to filter data based on user input at runtime. For example, consider filtering a list of products based on price.

We will define a filtering condition in a string `"products.Where(x => x.Price > greaterThan)"`:

```csharp
var products = new List<Product>
{
    new(1, "Laptop", 1200m),
    new(2, "Smartphone", 800m),
    new(3, "Tablet", 400m)
};

var greaterThan = 500m;

var filteredProducts = Eval.Execute<List<Product>>("products.Where(x => x.Price > greaterThan)",
    new { products, greaterThan });

foreach (var product in filteredProducts)
{
    Console.WriteLine(product);
}
```

With `Eval.Execute` we can dynamically execute this LINQ expression. The result will return 2 products that have price bigger than 500m:

```csharp
Product { Id = 1, Name = Laptop, Price = 1200 }
Product { Id = 2, Name = Smartphone, Price = 800 }
```

By constructing your LINQ queries dynamically, you gain flexibility in building data-intensive applications, such as those using EF Core.

## Executing Dynamic LINQ Expressions on EF Core

Consider the following `Author` and the `Book` entities:

```csharp
public class Author
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<Book> Books { get; set; } = [];
}

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    
    public Guid AuthorId { get; set; }
    public Author Author { get; set; }
}
```

And a corresponding DbContext:

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

I use it with SQLite database:

```csharp
var connectionString = builder.Configuration.GetConnectionString("Sqlite");

builder.Services.AddDbContext<ApplicationDbContext>(x => x.EnableSensitiveDataLogging()
    .UseSqlite(connectionString));
```

Now, let's build a dynamic LINQ query for EF Core. For instance, filtering books with the title "Mouse" and sorting by the author's name in descending order can be done as follows:

```csharp
var result = await dbContext.Books
    .Include(x => x.Author)
    .WhereDynamic(x => "x.Title == \"Mouse\")
    .OrderByDescending(x => "x.Author.Name")
    .ToListAsync();
    
result = await dbContext.Books
    .Include(x => x.Author)
    .WhereDynamic("x => x.Title == \"Mouse\")
    .OrderByDescendingDynamic("x => x.Author.Name")
    .ToListAsync();
```

Here I specify a `WhereDynamic` and `OrderByDescendingDynamic` LINQ statements and passing expressions as strings inside.

Notice that two types of syntax are supported: where you specify the part of the lambda and the full lambda as string expression.

You can also create a more advanced filters like getting all Books of year 2020 and newer, that have `Chips` in their `Title`:

```csharp
var result = await dbContext.Books
    .WhereDynamic(x => "x.Year >= 2020 && x.Title.Contains('Chips')")
    .ToListAsync();
```

We can expand this example further by introducing a Minimal API endpoint that accepts a dynamic filtering and sorting expressions from the frontend:

```csharp
app.MapPost("/api/books", async (
    ApplicationDbContext dbContext,
    BookFilterRequest request) =>
{
    var query = dbContext.Books
        .Include(x => x.Author)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(request.Filter))
    {
        query = query.WhereDynamic(request.Filter);
    }

    if (!string.IsNullOrWhiteSpace(request.Sort))
    {
        query = query.OrderByDynamic(request.Sort);
    }

    var books = await query.ToListAsync();
    return Results.Ok(books);
});
```

Let's send the following request:

```json
{
  "filter": "x => x.Title == \"Mouse\" && x.Year >= 2020",
  "sort": "x => x.Author.Name"
}
```

And pause the debugger:

![Screenshot from a debugger showing a C# Minimal API endpoint processing a dynamic filter and sort request, with the generated SQL query visible below.](https://antondevtips.com/media/code_screenshots/dotnet/csharp-eval-expression/img_1.png)

As you can see, these string dynamic expressions have "magically" been translated into a valid SQL query. It's truly fascinating how these dynamic strings are converted under the hood.

Feel free to play around with a similar example on [DotNetFiddle](https://dotnetfiddle.net/5flzOc).

## Injecting C# Code Dynamically To Modify Application Behaviour

[C# Eval](https://eval-expression.net/eval-context?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) can be used to safely inject and execute C# custom code into your application at runtime. This is particularly useful in scenarios where you want to adjust UI elements, change business logic, or enable advanced configuration options without redeploying your application.

The key is the [EvalContext](https://eval-expression.net/eval-context?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) class that requires you to register any types, methods, or variables that the injected code might use.

Consider the following model definitions:

```csharp
public class PluginModel
{
    public User User { get; set; }
    public string Title { get; set; }
    public string H1 { get; set; }
    public string Footer { get; set; }
    public TextBox TextBox1 { get; set; } = new();
    public TextBox TextBox2 { get; set; } = new();
}

public class User
{
    public string Name { get; set; }
    public bool IsAdmin { get; set; }
}

public class TextBox
{
    public string Label { get; set; }
    public int MaxLength { get; set; }
}
```

The following script customizes the `PluginModel` dynamically:

```csharp
public static string GetCustomizationScript()
{
    return """
           // Set title and header based on the user's name
           model.Title = $"Dynamic Dashboard for {model.User.Name}";
           model.H1 = model.Title;

           // Customize TextBox controls based on role
           model.TextBox1.Label = "Enter your query:";
           model.TextBox1.MaxLength = 100;

           model.TextBox2.Label = "Details:";
           model.TextBox2.MaxLength = 1000;

           // Additional customization if the user is an admin
           if(model.User.IsAdmin)
           {
               model.TextBox1.MaxLength = int.MaxValue;
               model.TextBox2.MaxLength = int.MaxValue;
               // Set an admin-specific footer
               model.Footer = "Admin Dashboard - All rights reserved.";
           }
           else
           {
               model.Footer = "User Dashboard";
           }
           """;
}
```

The injected script sets up titles, labels, and even a conditional footer, illustrating how you can inject custom logic dynamically into your application.

And here's how you execute the dynamic script using an `EvalContext`:

```csharp
var model = new PluginModel
{
    User = new User
    {
        Name = "Anton",
        IsAdmin = false
    }
};

var context = new EvalContext();
// Clear all existing registrations to start fresh
context.UnregisterAll();

// Limit iterations to prevent infinite loops
context.MaxLoopIteration = 5;

// Enable safe mode for restricted code execution
context.SafeMode = true;

// Register default aliases and the necessary types
context.RegisterDefaultAliasSafe();
context.RegisterType(typeof(PluginModel));
context.RegisterType(typeof(User));
context.RegisterType(typeof(TextBox));

context.Execute(GetCustomizationScript(), new { model });

Console.WriteLine(
    JsonSerializer.Serialize(model,
    new JsonSerializerOptions { WriteIndented = true }
));
```

This produces the following output:

```json
{
  "User": {
    "Name": "Anton",
    "IsAdmin": false
  },
  "Title": "Dynamic Dashboard for Anton",
  "H1": "Dynamic Dashboard for Anton",
  "Footer": "User Dashboard",
  "TextBox1": {
    "Label": "Enter your query:",
    "MaxLength": 100
  },
  "TextBox2": {
    "Label": "Details:",
    "MaxLength": 1000
  }
}
```

**How this works:**

1.  A new `EvalContext` instance is created, and options like `MaxLoopIteration` and `SafeMode` are set to secure the evaluation process.
    
2.  Before executing any dynamic code, register the types (`PluginModel`, `User`, and `TextBox`) so their properties are accessible during execution.
    
3.  The dynamic code returned by `GetCustomizationScript()` is executed in the context of the provided model. The script customizes the model by setting titles, adjusting text box properties, and conditionally adding a footer based on the user's role.
    

By using C# Eval Expression in this way, you gain the ability to modify your application's behavior at runtime without redeployment. This method of code injection, when used responsibly with proper safety measures, opens up exciting possibilities for dynamic application customization.

For further exploration, try a similar example on [DotNetFiddle](https://dotnetfiddle.net/gE6EAh).

A real example where it works in the real project? You can explore the [ZZZ Code AI](https://zzzcode.ai/), which allows you to load custom logic that generates forms, handles business rules, or modifies workflows dynamically.

![Screenshot of the ZZZ Code AI website, an AI code generator interface with fields for language, description, and output options, demonstrating a real-world application of dynamic code injection.](https://antondevtips.com/media/code_screenshots/dotnet/csharp-eval-expression/img_4.png)

This website uses dynamic code injection to configure multiple forms dynamically through a text file. The text file contains the code run on the website.

## Summary

**C# Eval Expression** allows you to execute dynamic C# code at runtime, offering innovative approaches to solving runtime challenges.

With it, you can:

*   **Execute Dynamic Code:** evaluate and run C# code on the fly for flexible application behavior.
*   **Create Dynamic Templates:** use raw string literals to build personalized email templates that adapt to your data.
*   **Construct Dynamic LINQ Queries:** build and execute LINQ queries dynamically to filter and sort data with string expressions for IEnumerable and IQueryable.
*   **Inject Custom Logic:** modify your application's behavior dynamically through safe, customizable code injection using EvalContext.

Start building dynamic apps now with [C# Eval Expression](https://eval-expression.net/?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website).

Disclaimer: this blog post is sponsored by ZZZ Projects.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-perform-dynamic-code-execution-in-dotnet-with-csharp-eval-expression)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-perform-dynamic-code-execution-in-dotnet-with-csharp-eval-expression&title=How%20to%20Perform%20Dynamic%20Code%20Execution%20in%20.NET%20with%20C%23%20Eval%20Expression)[X](https://twitter.com/intent/tweet?text=How%20to%20Perform%20Dynamic%20Code%20Execution%20in%20.NET%20with%20C%23%20Eval%20Expression&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-perform-dynamic-code-execution-in-dotnet%2Fhow-to-perform-dynamic-code-execution-in-dotnet-with-csharp-eval-expression)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-perform-dynamic-code-execution-in-dotnet-with-csharp-eval-expression)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.