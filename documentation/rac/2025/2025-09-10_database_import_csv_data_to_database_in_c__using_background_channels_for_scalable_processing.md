```yaml
---
title: Import CSV Data to Database in C# Using Background Channels for Scalable Processing
source: https://newsletter.kanaiyakatarmal.com/p/import-csv-data-to-database-in-asp-net?utm_source=post-email-title&publication_id=5461735&post_id=173212733&utm_campaign=email-post-title&isFreemail=true&r=a97lu&triedRedirect=true&utm_medium=email
date_published: 2025-09-10T04:31:20.000Z
date_captured: 2025-09-10T11:19:14.556Z
domain: newsletter.kanaiyakatarmal.com
author: Kanaiya Katarmal
category: database
technologies: [System.Threading.Channels, ASP.NET Core, .NET, Entity Framework Core, CsvHelper, BackgroundService]
programming_languages: [C#, SQL]
tags: [csv-import, background-processing, asynchronous, dotnet, csharp, aspnet-core, data-import, scalability, performance, producer-consumer]
key_concepts: [Background Channels, Producer-Consumer Pattern, Asynchronous Processing, Dependency Injection, BackgroundService, Error Handling, Scalability, Backpressure Mechanism]
code_examples: false
difficulty_level: intermediate
summary: |
  This article presents a robust solution for importing large CSV files into a database using C# Background Channels. It addresses common issues with synchronous imports like high memory usage, blocking HTTP requests, and timeouts, by decoupling file upload from data processing. The proposed architecture leverages `System.Threading.Channels` for a scalable producer-consumer pattern, integrated with ASP.NET Core's `BackgroundService` and Entity Framework Core for database operations. Code examples illustrate the data model, request/response models, a worker class for CSV processing, the background service, and an API controller endpoint. The approach emphasizes asynchronous processing, scalability, resilience, and maintainability for production-ready applications.
---
```

# Import CSV Data to Database in C# Using Background Channels for Scalable Processing

# Import CSV Data to Database in C# Using Background Channels for Scalable Processing

### Effortlessly handle large CSV uploads and ensure reliable database imports using asynchronous background processing in C#

Importing large CSV files into a database is a very common task in business applications—whether you're uploading product catalogs, customer lists, or historical data dumps. However, doing this synchronously can block your API, cause performance bottlenecks, and even lead to timeouts when files are large.

![Architecture Diagram](https://substackcdn.com/image/fetch/$s_!7WfH!,w_1456,c_limit,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F10f91c49-bad2-4c6d-ad94-4630e0783fb0_1225x817.png)

In this article, I’ll show you a robust approach using **C# Background Channels** that decouples file upload from data processing. This results in a scalable, efficient, and maintainable solution.

## 🎯 The Problem with Simple CSV Imports

Many developers start by reading the entire file in-memory and inserting records in bulk like this:

```csharp
var records = csv.GetRecords<Product>().ToList();
await _dbContext.Products.AddRangeAsync(records);
await _dbContext.SaveChangesAsync();
```

While this works for small files, it fails when files grow large (e.g., hundreds of MBs or millions of rows):

*   📉 High memory usage
*   🚫 Blocking the HTTP request thread until processing completes
*   🕰 Long timeouts and poor UX
*   ❌ Hard to scale when multiple users upload large files concurrently

## ✅ Why Background Channels?

Background Channels are part of `System.Threading.Channels`. They offer a powerful producer-consumer pattern built into .NET.

### Key Benefits:

*   ✅ Fully asynchronous
*   ✅ Thread-safe, high-performance queue
*   ✅ Simple backpressure mechanism (prevents overwhelming the system)
*   ✅ Easy to combine with `BackgroundService` in ASP.NET Core

👉 This makes them ideal for handling file imports without blocking the main request thread.

## 🏗 Architecture Overview

Let’s visualize the flow:

1.  ✅ Client uploads a file via HTTP POST
2.  ✅ Each file becomes a `FileImportRequest` and is written to a **Channel**
3.  ✅ A **BackgroundService** (the `FileProcessor`) consumes the Channel in the background
4.  ✅ Each file is processed by reading CSV rows and saving them to the database
5.  ✅ Responses are immediately returned after scheduling (non-blocking upload)

## 💻 The Core Components

### 1️⃣ Data Model

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string SKU { get; set; }
}
```

### 2️⃣ **File Import Request & Response Models**

```csharp
public record FileImportRequest
{
    public string RequestId { get; init; }
    public MemoryStream FileData { get; init; }
}

public record FileImportResponseModel
{
    public string RequestId { get; init; }
    public string FileName { get; init; }
    public long FileSize { get; init; }
    public string Status { get; init; } = "Scheduled for Processing";
}
```

### 3️⃣ Worker Class – Import CSV to DB

This class handles actual CSV reading and saving to the database:

```csharp
public class FileImportWorker : IFileImportWorker
{
    private readonly ApplicationDbContext _dbContext;

    public FileImportWorker(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Import(FileImportRequest request)
    {
        request.FileData.Position = 0;

        using var reader = new StreamReader(request.FileData);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var products = csv.GetRecords<Product>().ToList();

        await _dbContext.Products.AddRangeAsync(products);
        await _dbContext.SaveChangesAsync();
    }
}
```

### 4️⃣ BackgroundService – Continuous Processing

```csharp
public class FileProcessor : BackgroundService
{
    private readonly Channel<FileImportRequest> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FileProcessor> _logger;

    public FileProcessor(Channel<FileImportRequest> channel, IServiceProvider serviceProvider, ILogger<FileProcessor> logger)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var worker = scope.ServiceProvider.GetRequiredService<IFileImportWorker>();
                await worker.Import(request);
                _logger.LogInformation("Successfully processed {RequestId}", request.RequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {RequestId}", request.RequestId);
                // Optionally add retry or dead-letter logic here
            }
            finally
            {
                request.FileData?.Dispose();
            }
        }
    }
}
```

5️⃣ API Controller – File Upload Endpoint

```csharp
app.MapPost("/FileUpload", async ([FromForm] IFormFileCollection files, Channel<FileImportRequest> _channel) =>
{
    var responses = new List<FileImportResponseModel>();

    foreach (var file in files)
    {
        var request = new FileImportRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            FileData = new MemoryStream()
        };

        await file.CopyToAsync(request.FileData);
        request.FileData.Position = 0;

        await _channel.Writer.WriteAsync(request);

        responses.Add(new FileImportResponseModel
        {
            RequestId = request.RequestId,
            FileName = file.FileName,
            FileSize = file.Length,
            Status = "Scheduled for Processing"
        });
    }

    return responses;
})
.Accepts<IFormFile[]>("multipart/form-data");
```

## 📚 Best Practices & Tips

*   ✅ Always dispose streams properly
*   ✅ Use DI scopes to avoid memory leaks
*   ✅ Log important events for traceability
*   ✅ Handle exceptions gracefully and consider dead-letter queue mechanisms for failed imports
*   ✅ Limit maximum file size at the API layer to avoid abuse

## 📊 Example CSV

```csv
Id,Name,Price,SKU
1,Laptop,55000.75,LAP-001
2,Smartphone,25000.0,PHN-002
3,Tablet,18000.5,TAB-003
```

## 🚀 Benefits of This Approach

*   ✅ Asynchronous Processing
*   ✅ Scalability
*   ✅ Resilience
*   ✅ Maintainable

## ✅ Conclusion

By combining **Background Channels**, **CsvHelper**, and **Entity Framework Core**, you build a highly scalable and efficient CSV import pipeline in C#. This architecture is production-ready and easily extensible for large applications.

👉 Feel free to fork this pattern, improve upon it with features like monitoring, progress tracking, or batch size control.

---

## **👉 Full working code available at:**

🔗[https://sourcecode.kanaiyakatarmal.com/ImportCSV](https://sourcecode.kanaiyakatarmal.com/ImportCSV)

---

I hope you found this guide helpful and informative.

Thanks for reading!