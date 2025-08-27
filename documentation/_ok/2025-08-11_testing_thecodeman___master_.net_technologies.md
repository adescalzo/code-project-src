```yaml
---
title: "TheCodeMan | Master .NET Technologies"
source: https://thecodeman.net/posts/simplifying-integration-with-adapter-pattern?utm_source=newsletter&utm_medium=email&utm_campaign=TheCodeMan%20Newsletter%20-%20%20Simplifying%20Integration%20with%20the%20Adapter%20Pattern%20%28copy%29
date_published: unknown
date_captured: 2025-08-11T15:07:40.904Z
domain: thecodeman.net
author: Unknown
category: testing
technologies: [.NET, C#, REST APIs, SOAP, Amazon S3, Azure Blob Storage, Google Cloud Storage, AWS SDK, Azure.Storage.Blobs, Google.Cloud.Storage.V1, Dependency Injection, gRPC, Splunk, ElasticSearch, Application Insights, Neon]
programming_languages: [C#, C++]
tags: [design-patterns, adapter-pattern, software-architecture, csharp, .net, integration, cloud-storage, legacy-systems, object-oriented-programming, dependency-injection]
key_concepts: [Adapter Pattern, Object Adapter Pattern, Class Adapter Pattern, Composition over Inheritance, Dependency Injection, API Integration, Decoupling, Structural Design Pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive explanation of the Adapter Design Pattern, a structural pattern that enables incompatible interfaces to work together seamlessly. It uses a relatable real-world analogy and offers practical C# code examples for integrating legacy payment systems and various cloud storage providers. The content thoroughly details both the Object Adapter and Class Adapter patterns, outlining their distinct characteristics and appropriate use cases. Furthermore, it offers clear guidance on when to apply this pattern and when to avoid it, emphasizing its role in fostering clean, maintainable, and flexible software architectures.
---
```

# TheCodeMan | Master .NET Technologies

## Simplifying Integration with the Adapter Pattern

Jan 27 2025

### The Problem: A Real-World Story

Imagine you’ve just moved to a new country. You’re excited to set up your home and plug in your laptop, only to realize the power outlets are completely different from the ones back home.

Your laptop’s plug doesn’t fit into the wall socket. Panic sets in - your laptop is essential for work!

What can you do?

You visit a local store and discover the solution: a power adapter. The adapter lets your laptop’s plug fit seamlessly into the foreign socket. It doesn’t change the wall socket or your laptop plug; it simply acts as a bridge, translating one interface into another.

Problem solved!

In the software world, you often encounter mismatched systems. For instance, you’re building a modern application that needs to integrate with a legacy payment gateway.

Your application works with REST APIs, while the payment gateway only supports SOAP-based services. They speak different “languages” and can’t communicate directly.

If you modify the legacy system to support REST APIs, it’s a costly and risky endeavor. Similarly, rewriting your application to support SOAP is time-consuming and unnecessary.

How do you bridge the gap?

### The Solution: The Adapter Pattern

The Adapter pattern solves this problem by acting as a translator. It allows two incompatible interfaces to work together without changing their existing code.

Here’s how it works:

1.  Create an Adapter: Build a new class that implements the interface your application expects (e.g., REST APIs).

2.  Delegate to the Legacy System: Inside the adapter, translate the REST API calls into the SOAP requests understood by the payment gateway.

3.  Return Results in the Expected Format: The adapter translates SOAP responses back into REST API responses for your application.

#### Implementing the Adapter Pattern in Code

Let’s see how this works in C#.

Problem Setup: You have a IPaymentProcessor interface your application uses:

```csharp
public interface IPaymentProcessor
{
    void ProcessPayment(decimal amount);
}
```

Your modern application uses this interface, but the legacy payment system only exposes a _LegacyPaymentService_ with the following method:

```csharp
public class LegacyPaymentService
{
    public void MakePayment(string amount)
    {
        Console.WriteLine($"Processing payment of {amount} via legacy system.");
    }
}
```

The Adapter:

Here’s how you create an adapter:

```csharp
public class PaymentAdapter(LegacyPaymentService legacyService) : IPaymentProcessor
{
    public void ProcessPayment(decimal amount)
    {
        // Convert the amount to a string and delegate to the legacy service
        string amountString = amount.ToString("F2");

        legacyService.MakePayment(amountString);
    }
}
```

Using the Adapter:

Now, you can seamlessly integrate the legacy system without modifying its code:

```csharp
internal class Program
{
    static void Main(string[] args)
    {
        LegacyPaymentService legacyService = new();

        IPaymentProcessor paymentProcessor = new PaymentAdapter(legacyService);

        // Your application code uses the modern IPaymentProcessor interface
        paymentProcessor.ProcessPayment(123.4567868m);
    }
}
```

### Definition

The **Adapter Design Pattern** is a structural design pattern **that allows two incompatible interfaces to work together by acting as a bridge.** It converts the interface of a class into another interface that the client expects, enabling integration without changing the existing code of the involved classes.

### UML Diagram

#### Object Adapter Pattern

![UML diagram illustrating the Object Adapter Pattern, showing a Client interacting with a Target interface, which is implemented by an Adapter. The Adapter holds an instance of the Adaptee Service and delegates calls to it.](images/blog/posts/simplifying-integration-with-adapter-pattern/object-adapter-pattern.jpg)

#### Class Adapter Pattern

![UML diagram illustrating the Class Adapter Pattern, showing a Client interacting with a Target Class. An Adapter class inherits from both the Target Class and the Adaptee Service, allowing it to directly call inherited methods.](images/blog/posts/simplifying-integration-with-adapter-pattern/class-adapter-pattern.jpg)

### Object Adapter Pattern

##### **Definition**

The **Object Adapter Pattern** uses **composition** to adapt one interface to another. The adapter contains an instance of the class it is adapting and delegates calls to it.

\*When to Use

*   When you cannot or should not modify the adaptee class (e.g., third-party libraries or legacy code).

*   When you want to use the adapter with multiple instances of the adaptee.

Example Scenario: Legacy Printer Integration

Suppose you have a modern application that prints documents using an **_IPrinter_** interface. However, you need to integrate a legacy LegacyPrinter class that has a different method signature.

Legacy Code:

```csharp
public class LegacyPrinter
{
    public void Print(string text)
    {
        Console.WriteLine($"Legacy Printer: {text}");
    }
}
```

Target Interface:

```csharp
public interface IPrinter
{
    void PrintDocument(string content);
}
```

Object Adapter Implementation:

```csharp
public class PrinterAdapter(LegacyPrinter legacyPrinter) : IPrinter
{
    public void PrintDocument(string content)
    {
        // Delegate the call to the adaptee (LegacyPrinter)
        legacyPrinter.Print(content);
    }
}
```

Usage:

```csharp
internal class Program
{
    static void Main(string[] args)
    {
        LegacyPrinter legacyPrinter = new();
        IPrinter printerAdapter = new PrinterAdapter(legacyPrinter);

        printerAdapter.PrintDocument("Hello, Object Adapter!");
    }
}
```

### Class Adapter Pattern

Definition:

The Class Adapter Pattern uses inheritance to adapt one interface to another. The adapter extends the adaptee class and implements the target interface.

When to Use:

*   When you can inherit from the adaptee class.

*   When the adaptee class is not sealed and does not require composition for flexibility.

*   When multiple inheritance (from the target interface and adaptee class) is acceptable.

Example

Class Adapter Implementation:

```csharp
public class PrinterAdapter : LegacyPrinter, IPrinter
{
    public void PrintDocument(string content)
    {
        // Directly call the inherited method from LegacyPrinter
        Print(content);
    }
}
```

Usage:

```csharp
internal class Program
{
    static void Main(string[] args)
    {
        IPrinter printerAdapter = new PrinterAdapter();

        printerAdapter.PrintDocument("Hello, Class Adapter!");
    }
}
```

### Choosing Between Object and Class Adapter

Use Object Adapter when:

*   Adaptee class is already implemented, and you can’t modify it.

*   Flexibility and reusability are important.

*   The language doesn’t support multiple inheritance (e.g., C#).

Use Class Adapter when:

*   Inheritance is appropriate and acceptable.

*   The language supports multiple inheritance (e.g., C++ or through interfaces in C#).

*   Performance is a concern (fewer indirections compared to composition).

### Cloud Providers Integration with Adapter Pattern

#### Adapter for Cloud Storage Providers

Scenario

You’re building an application that needs to upload and download files from different cloud storage providers, such as:

*   Amazon S3

*   Azure Blob Storage

*   Google Cloud Storage

Each cloud provider has its own SDK and API methods, which makes the integration process cumbersome and hard to maintain. For instance:

*   Amazon S3 uses the Amazon.S3.IAmazonS3 interface.

*   Azure Blob Storage uses the Azure.Storage.Blobs library.

*   Google Cloud Storage uses the Google.Cloud.Storage.V1.StorageClient.

You need to create a unified interface so your application can interact with any of these providers without changing the main codebase.

#### Solution: Adapter Pattern

The Adapter Pattern can be used to standardize the interaction with these cloud providers. You define a common interface for your application, and each provider gets its own adapter implementation.

#### Step-by-Step Implementation

##### 1. Define a Common Interface

Create an interface that abstracts cloud storage operations like uploading, downloading, and deleting files:

```csharp
public interface ICloudStorage
{
    Task UploadFileAsync(string containerName, string fileName, Stream fileStream);
    Task<Stream> DownloadFileAsync(string containerName, string fileName);
    Task DeleteFileAsync(string containerName, string fileName);
}
```

##### 2. Implement Adapters for Each Cloud Provider: Amazon S3 Adapter

```csharp
public class S3StorageAdapter : ICloudStorage
{
    private readonly IAmazonS3 _s3Client;

    public S3StorageAdapter(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task UploadFileAsync(string containerName, string fileName, Stream fileStream)
    {
        var request = new PutObjectRequest
        {
            BucketName = containerName,
            Key = fileName,
            InputStream = fileStream
        };

        await _s3Client.PutObjectAsync(request);
    }

    public async Task<Stream> DownloadFileAsync(string containerName, string fileName)
    {
        var request = new GetObjectRequest
        {
            BucketName = containerName,
            Key = fileName
        };
        var response = await _s3Client.GetObjectAsync(request);

        return response.ResponseStream;
    }

    public async Task DeleteFileAsync(string containerName, string fileName)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = containerName,
            Key = fileName
        };

        await _s3Client.DeleteObjectAsync(request);
    }
}
```

#### Azure Blob Storage Adapter

Use the Azure.Storage.Blobs library for Azure integration:

#### Google Cloud Storage Adapter

Use the Google.Cloud.Storage.V1 library for Google Cloud integration:

```csharp
public class GoogleCloudStorageAdapter : ICloudStorage
{
    private readonly StorageClient _storageClient;

    public GoogleCloudStorageAdapter(StorageClient storageClient)
    {
        _storageClient = storageClient;
    }

    public async Task UploadFileAsync(string containerName, string fileName, Stream fileStream)
    {
        await _storageClient.UploadObjectAsync(containerName, fileName, null, fileStream);
    }

    public async Task<Stream> DownloadFileAsync(string containerName, string fileName)
    {
        MemoryStream memoryStream = new();

        await _storageClient.DownloadObjectAsync(containerName, fileName, memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        return memoryStream;
    }

    public async Task DeleteFileAsync(string containerName, string fileName)
    {
        await _storageClient.DeleteObjectAsync(containerName, fileName);
    }
}
```

##### 3. Registering with DI

Create a factory or dependency injection setup to inject the appropriate adapter based on the cloud provider.

```csharp
builder.Services.AddSingleton(new BlobServiceClient("YourAzureConnectionString")); // Azure Blob Storage
builder.Services.AddSingleton(StorageClient.Create()); // Google Cloud Storage
builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client()); // Amazon S3

// Register Adapters
builder.Services.AddTransient<AzureBlobStorageAdapter>();
builder.Services.AddTransient<GoogleCloudStorageAdapter>();
builder.Services.AddTransient<S3StorageAdapter>();

// Register Factory
builder.Services.AddTransient<Func<string, ICloudStorage>>(sp => provider =>
{
    return provider switch
    {
        "Azure" => sp.GetRequiredService<AzureBlobStorageAdapter>(),
        "Google" => sp.GetRequiredService<GoogleCloudStorageAdapter>(),
        "AWS" => sp.GetRequiredService<S3StorageAdapter>(),
        _ => throw new ArgumentException("Unsupported cloud provider")
    };
});

// Register FileService
builder.Services.AddTransient<FileService>();
```

#### Usage in the Application:

```csharp
public class FileService
{
    private readonly ICloudStorage _cloudStorage;

    public FileService(ICloudStorage cloudStorage)
    {
        _cloudStorage = cloudStorage;
    }

    public async Task UploadFile(string containerName, string fileName, Stream fileStream)
    {
        await _cloudStorage.UploadFileAsync(containerName, fileName, fileStream);
    }

    public async Task<Stream> DownloadFile(string containerName, string fileName)
    {
        return await _cloudStorage.DownloadFileAsync(containerName, fileName);
    }

    public async Task DeleteFile(string containerName, string fileName)
    {
        await _cloudStorage.DeleteFileAsync(containerName, fileName);
    }
}
```

##### Explanation

1.  Target Interface:

*   ICloudStorage is the target interface that the application expects. It defines three standard methods: UploadFileAsync, DownloadFileAsync, and DeleteFileAsync.

*   The application works only with this interface and doesn’t know about specific cloud provider SDKs.

2.  Adaptee:

*   Each cloud provider (Azure Blob Storage, Google Cloud Storage, Amazon S3) has its own SDK with unique method signatures and functionality.

*   These SDKs are adaptees that need to be adapted to the ICloudStorage interface.

3.  Adapter:

*   The adapters (AzureBlobStorageAdapter, GoogleCloudStorageAdapter, S3StorageAdapter) implement ICloudStorage and translate calls from the ICloudStorage interface to the specific methods provided by each SDK.

4.  Client:

*   The client is your application, which interacts with the ICloudStorage interface (via the FileService) without worrying about the underlying implementation details.

##### How the Adapter Pattern is Used

*   The adapters bridge the gap between the ICloudStorage interface and the specific cloud SDKs.

*   This decouples the application logic from the specific SDKs, making it easy to switch providers or integrate new ones without changing the main application code.

#### Azure Blob Storage Adapter

```csharp
//This is adapter that trying to adapt Azure Blob Storage to my Cloud Storage
public class AzureBlobStorageAdapter : ICloudStorage
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageAdapter(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task UploadFileAsync(string containerName, string fileName, Stream fileStream)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(fileStream, overwrite: true);
    }

    public async Task<Stream> DownloadFileAsync(string containerName, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        var response = await blobClient.DownloadAsync();

        return response.Value.Content;
    }

    public async Task DeleteFileAsync(string containerName, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync();
    }
}
```

### Where to/NOT to Use the Adapter Pattern?

##### **Where to use the Adapter Pattern?**

1.  When Integrating Legacy Systems

*   Use the Adapter Pattern to modernize old APIs or integrate with newer systems.

*   Example: Connecting a SOAP-based service with a RESTful API.

2.  When Standardizing Interfaces

*   Use adapters to unify multiple incompatible implementations under a common interface.

*   Example: Supporting multiple logging providers (e.g., Splunk, ElasticSearch, Application Insights).

3.  When Bridging Different Protocols

*   Useful for translating between two incompatible communication protocols.

*   Example: Adapting between gRPC and REST for cross-service communication.

4.  When Adding a Third-Party Library

*   Use an adapter to wrap a third-party library, preventing it from leaking into the rest of your system.

*   Example: Wrapping a payment gateway SDK to conform to your application's interface.

5.  When Switching Implementations

*   Use adapters to simplify switching between different libraries or frameworks.

*   Example: Migrating from Amazon S3 to Azure Blob Storage with minimal client code changes.

##### **Where NOT to Use the Adapter Pattern**

Inappropriate Use Cases

1.  When Interfaces Are Already Compatible

*   Avoid unnecessary adapters if classes can work together without translation.

*   Example: Wrapping a List as IEnumerable.

2.  When Modifying the Adaptee is Possible

*   If you own the adaptee’s code and can modify it, consider making it conform to the target interface directly.

*   Example: Refactoring a legacy library to support the new system.

3.  For Simple Transformations

*   If the required translation logic is minimal, use inline conversion or a utility method instead.

*   Example: Converting a date string format without a dedicated adapter.

4.  When Performance is Critical

*   Avoid adapters in scenarios where even slight performance overhead is unacceptable.

*   Example: Real-time video streaming where every millisecond counts.

5.  When Over-Abstraction is a Concern

*   Avoid the Adapter Pattern if it introduces unnecessary complexity or abstraction for small, straightforward tasks.

*   Example: Adapting a basic settings reader library with only one method.

### Wrapping Up

The Adapter Pattern is a must-have for .NET developers, enabling smooth integration between incompatible systems while keeping code clean and maintainable.

Whether you're standardizing APIs, integrating legacy systems, or unifying third-party libraries, this pattern ensures flexibility and scalability without overhauling your codebase.

By keeping adapters focused and lightweight, you can tackle integration challenges with confidence.

Mastering the Adapter Pattern equips you to create robust, adaptable applications ready to meet both current and future needs.