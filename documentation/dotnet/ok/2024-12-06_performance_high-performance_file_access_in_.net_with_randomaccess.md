```yaml
---
title: High-Performance File Access in .NET with RandomAccess
source: https://okyrylchuk.dev/blog/high-performance-file-access-in-dotnet-with-randomaccess/?utm_source=emailoctopus&utm_medium=email&utm_campaign=RandomAccess
date_published: 2024-12-06T13:50:00.000Z
date_captured: 2025-08-08T18:07:16.803Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: performance
technologies: [.NET 6, FileStream, RandomAccess, .NET 8, Entity Framework, Mapperly]
programming_languages: [C#]
tags: [file-io, performance, dotnet, csharp, random-access, filestream, scatter-gather-io, high-performance]
key_concepts: [file-io, random-access-api, filestream, scatter-gather-io, asynchronous-operations, performance-optimization, thread-safe-file-access]
code_examples: true
difficulty_level: intermediate
summary: |
  This article introduces .NET 6's `RandomAccess` API as a high-performance, thread-safe alternative to traditional `FileStream` for file I/O operations. It contrasts `RandomAccess`'s explicit offset-based operations with `FileStream`'s internal cursor, highlighting how the former enables truly thread-safe concurrent access. The post also details `RandomAccess`'s support for Scatter/Gather IO, which optimizes data transfer to and from multiple memory buffers. While `FileStream` remains suitable for sequential operations, `RandomAccess` is recommended for large files, frequent random access, and performance-critical scenarios. The author advises profiling and benchmarking before implementing optimizations.
---
```

# High-Performance File Access in .NET with RandomAccess

# High-Performance File Access in .NET with RandomAccess

File IO is a fundamental operation in software development, and .NET has continually evolved to provide developers with more efficient and flexible ways to handle file operations. With the introduction of **RandomAccess** in .NET 6, developers now have a powerful alternative to traditional file streams. This blog post will dive deep into the differences, advantages, and use cases of **RandomAccess** compared to conventional file streams.

## **Understanding Traditional File Streams**

Before we explore **RandomAccess**, let’s review how traditional file streams work in .NET:

```csharp
// Traditional stream reading
using FileStream stream = new FileStream("example.txt", FileMode.Open, FileAccess.Read);

// To read from a specific position, you must manually seek
stream.Seek(100, SeekOrigin.Begin);
// Or
stream.Position = 100;

byte[] buffer = new byte[100];
await stream.ReadExactlyAsync(buffer, 0, 100);
```

**Characteristics of File Streams**

*   Require explicit seeking to change reading/writing position
*   Maintain an internal position pointer
*   Relatively heavyweight in terms of resource management
*   Synchronous and asynchronous versions available
*   Require careful management of stream lifetime

## **What is RandomAccess?**

**RandomAccess** is a static class designed for **high-performance**, **thread-safe**, random access file I/O operations. Unlike **FileStream**, which is object-oriented and focuses on streams, **RandomAccess** offers a lower-level API for performing direct file operations.

Let’s see how we can use it by rewriting the previous example using **FileStream** with **RandomAccess**. 

```csharp
using SafeFileHandle handle = File.OpenHandle("example.txt", access: FileAccess.ReadWrite);

Memory<byte> buffer = new byte[100];
await RandomAccess.ReadAsync(handle, buffer, 100);
```

**FileStream Challenges**

In traditional file handling, when you read or write to a file, the file handle maintains an internal cursor or offset. This means:

*   Each read or write operation moves this cursor
*   Concurrent access requires complex synchronization
*   Threads can interfere with each other’s file position

**RandomAccess: A Paradigm Shift**

**RandomAccess** introduces a fundamentally different approach:

*   Every read/write operation explicitly specifies its file offset
*   The file handle’s internal cursor remains untouched
*   This enables truly thread-safe, non-interfering file access

To get the file size, use the **GetLength** method:

```csharp
long length = RandomAccess.GetLength(handle);
```

## **Scatter/Gather IO**

Another cool feature of **RandomAccess** is that it supports **Scatter/Gather IO**. 

**Scatter/Gather IO** transfers data between a file and multiple memory buffers (or vice versa) in a single operation. It is divided into two complementary operations:

**Scatter Input (Read):**

The data is split and written to multiple non-contiguous memory buffers when reading from a file.

**Gather Output (Write):**

Data from multiple non-contiguous memory buffers is combined and written as a contiguous block when writing to a file.

```csharp
Memory<byte> buffer1 = new byte[100];
Memory<byte> buffer2 = new byte[200];
Memory<byte> buffer3 = new byte[200];
await RandomAccess.ReadAsync(handle, [buffer1, buffer2, buffer3], 100);
```

## **When to Choose RandomAccess**

Prefer **RandomAccess** when:

*   Working with large files
*   Requiring frequent random access
*   Seeking thread-safe file access
*   Dealing with scenarios where stream overhead is problematic

Continue using traditional streams when:

*   Performing sequential reads/writes
*   Requiring more complex stream manipulations

The **FileStream** implementation also received extensive rework in .NET 6 and became faster. For most use cases, **FileStream** is the go-to tool.

**Remember**: before any optimizations, always profile your use cases and benchmark your code. 

## **Conclusion**

The **RandomAccess** type brings a new level of performance and flexibility to .NET file IO. It’s a game-changer for developers working on performance-critical applications or those dealing with non-sequential file access. That said, **FileStream** remains the go-to for everyday file operations due to its simplicity and rich API.