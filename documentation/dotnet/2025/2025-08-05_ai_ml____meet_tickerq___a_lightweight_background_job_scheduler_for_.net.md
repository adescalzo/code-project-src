```yaml
---
title: 🚀 Meet TickerQ – A Lightweight Background Job Scheduler for .NET
source: https://newsletter.kanaiyakatarmal.com/p/meet-tickerq-a-lightweight-background
date_published: 2025-08-05T08:42:58.000Z
date_captured: 2025-08-06T11:43:57.491Z
domain: newsletter.kanaiyakatarmal.com
author: Kanaiya Katarmal
category: ai_ml
technologies: [TickerQ, .NET, Hangfire, Quartz.NET, Entity Framework Core, SignalR, Tailwind CSS, Vue.js, NuGet, PostgreSQL, Docker, Redis]
programming_languages: [C#, SQL, JavaScript]
tags: [background-jobs, job-scheduler, dotnet, ef-core, cron, performance, real-time, dashboard, source-generators, in-memory]
key_concepts: [Background Job Scheduling, Cron Expressions, Source Generators, Reflection-free architecture, Job Persistence, Real-time Monitoring, Dependency Injection, Performance Optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces TickerQ, a lightweight, in-memory background job scheduler for .NET, positioned as a dependency-free alternative to Hangfire and Quartz.NET. It highlights TickerQ's core features, including reflection-free job discovery through source generators, support for both time-based and cron-based tasks, and seamless integration with Entity Framework Core for job persistence. The piece also showcases the new real-time dashboard in TickerQ 2.0.0, built with SignalR, Vue.js, and Tailwind CSS, providing live monitoring capabilities. Practical code examples for installation, configuration, and job definition are included, emphasizing TickerQ's focus on performance, clarity, and developer experience for modern .NET applications.
---
```

# 🚀 Meet TickerQ – A Lightweight Background Job Scheduler for .NET

# 🚀 Meet TickerQ – A Lightweight Background Job Scheduler for .NET

### A simple, dependency-free alternative to Hangfire and Quartz with Cron support

When building .NET applications that require background jobs—like sending daily reports, cleaning up logs, or syncing external services—developers often reach for **Hangfire** or **Quartz.NET**. While these tools are powerful, they can be overkill for smaller apps or services that just need scheduled task execution with minimal overhead.

Introducing **TickerQ** – a lightweight, in-memory job scheduler for .NET with built-in Cron expression support and async/await compatibility.

**TickerQ** is a high-performance background task scheduler for .NET that eliminates the need for reflection by using source generators. It supports both cron-based and time-based executions, integrates seamlessly with Entity Framework Core for persisting jobs and their states, and includes a real-time dashboard UI with minimal overhead.

### 🔑 Key Highlights:

*   Reflection-free job discovery using **source generators**
*   Supports **TimeTickers** (one-time tasks) and **CronTickers** (recurring tasks)
*   Built-in **EF Core** integration for job persistence, status tracking, and history
*   **Real-time dashboard** powered by SignalR, Tailwind CSS, and Vue.js
*   Advanced features like **retry policies**, **throttling**, and **cooldowns**
*   **Distributed job coordination**, **priority handling**, and **full DI support**

## 🚀 Why I Built TickerQ

I’ve always appreciated clean code, minimal abstractions, and tools that just work — especially in fast-paced development environments. But when it came to scheduling background tasks in .NET, I kept running into the same pain points:

*   Job schedulers that rely heavily on runtime reflection
*   Long startup times, especially in Docker environments
*   Over-engineered APIs that hide logic behind too much "magic"
*   Poor support for async flows and EF Core integration
*   Complex licensing models or steep learning curves just to scale

So I set out to build something better — **TickerQ**. What began as a weekend experiment has now evolved into a powerful job scheduling framework that focuses on performance, clarity, and developer experience.

## 🆕 What’s New in TickerQ 2.0.0?

### ⚙️ 1. Source Generators = Zero Reflection

Reflection might be powerful, but it's also slow and hard to debug. With **TickerQ 2.0**, we've eliminated all runtime reflection. Instead, we use **source generators** to handle job discovery and wiring at compile time.

**Why this matters:**

*   Significantly faster cold start times
*   More predictable behavior (especially in production)
*   Richer IntelliSense and compile-time error checking
*   Cleaner, more traceable stack traces

Your jobs are now fully wired at build time — no surprises at runtime.

### 📈 2. Live Dashboard (Finally!)

One of the most requested features is here: a built-in **real-time dashboard**.

Now you can monitor everything your background tasks are doing — live. See job statuses like `Pending`, `Running`, or `Completed`, along with performance metrics and thread activity — all from your browser.

**Built with:**

*   **SignalR** for real-time updates
*   **Vue.js + Tailwind CSS** for a clean UI
*   Easy to plug into your service

It’s like a lightweight Task Manager for your TickerQ jobs — ideal for debugging and demos.

### 🔥 3. Fully Static Runtime (No Dynamic Calls)

With 2.0, TickerQ’s runtime is now **100% static** — meaning no dynamic method invocation or runtime type scanning.

**Benefits:**

*   Less memory allocation
*   Predictable performance in containers and cloud runtimes
*   Fewer edge-case bugs caused by reflection or dynamic logic
*   Easier to maintain and debug

If you care about speed, stability, or cloud deployment costs — this change alone makes upgrading worth it.

### **🚀 Installing & Configuring**

Use the following NuGet packages. While only the core `TickerQ` package is required, in this example we will also set up optional packages for EF Core persistence and the real-time dashboard:

```
dotnet add package TickerQ
dotnet add package TickerQ.EntityFrameworkCore
dotnet add package TickerQ.Dashboard
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### **In** `Program.cs`

```csharp
builder.Services.AddTickerQ(options =>
{
    options.SetMaxConcurrency(4);
    options.SetExceptionHandler<MyExceptionHandler>();
    options.AddOperationalStore<MyDbContext>(efOpt =>
    {
        efOpt.UseModelCustomizerForMigrations(); // Design-time migrations only
        efOpt.CancelMissedTickersOnApplicationRestart();
    });
    options.AddDashboard(basePath: "/tickerq-dashboard");
    options.AddDashboardBasicAuth();
});

app.UseTickerQ();
```

## **🎯 Defining and Scheduling Jobs**

TickerQ uses a `[TickerFunction]` attribute and compile‑time source generation. You can define multiple scheduled functions with different triggers:

```csharp
[TickerFunction("CleanupLogs")]
public static async Task CleanupLogs(TickerRequest req, CancellationToken ct)
{
    // Do cleanup work...
}

[TickerFunction("CleanupTempFiles","0 */6 * * *")]
public static Task CleanupTempFiles(TickerRequest req, CancellationToken ct)
{
    Console.WriteLine("Cleaning temporary files...");
    return Task.CompletedTask;
}
```

### **▶️ TimeTicker example (fire-and-forget)**

```csharp
await _timeTickerManager.AddAsync(new TimeTicker
 {
     Function = "SendWelcome",
     ExecutionTime = DateTime.UtcNow,
     Request = TickerHelper.CreateTickerRequest<string>("User123"),
     Retries = 3,
     RetryIntervals = new[] { 30, 60, 120 }, // Retry after 30s, 60s, then 2min

 });
```

### **▶️ TimeTicker example (one‑off or delayed)**

```csharp
await _timeTickerManager.AddAsync(new TimeTicker
 {
     Function = "SendWelcome",
     ExecutionTime = DateTime.UtcNow.AddSeconds(30),
     Request = TickerHelper.CreateTickerRequest<string>("User123"),
     Retries = 3,
     RetryIntervals = new[] { 30, 60, 120 }, // Retry after 30s, 60s, then 2min

 });
```

### **🔁 CronTicker example (recurring)**

```csharp
await _cronTickerManager.AddAsync(new CronTicker
 {
     Function = "CleanupLogs",
     Expression = "0 */6 * * *",
     Retries = 2,
     RetryIntervals = new[] { 60, 300 }
 });
```

## 🗄️ Persistence with EF Core

To enable job persistence in TickerQ, you’ll need to set up the required database tables using **EF Core migrations**.

TickerQ’s EF Core integration handles the storage of tickers, job execution history, and job state—all seamlessly within your existing `DbContext`.

There are two ways to integrate:

### 🔧 Option 1: `UseModelCustomizerForMigrations()`

Ideal for keeping your runtime model clean, this method adds the TickerQ schema **only during design-time** (like when running `dotnet ef migrations add`). It won’t affect your `DbContext` at runtime, making it perfect for projects that prefer separation of concerns.

### ✍️ Option 2: Manual Configuration

If you need full control or want the TickerQ tables to be part of your runtime model, you can manually configure them in your `OnModelCreating` method. This gives you full visibility and customization over how TickerQ’s data is mapped and stored.

Both methods are supported—choose the one that fits your project’s architecture and preferences.

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    builder.ApplyConfiguration(new TimeTickerConfigurations());
    builder.ApplyConfiguration(new CronTickerConfigurations());
    builder.ApplyConfiguration(new CronTickerOccurrenceConfigurations());
}
```

**Apply migrations:**

```bash
dotnet ef migrations add AddTickerQTables
dotnet ef database update
```

## **🌐 Dashboard UI & Real‑Time Monitoring**

![](https://substackcdn.com/image/fetch/$s_!ZUlY!,w_1456,c_limit,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F8d8c7d52-e2b1-4d9f-988d-6aaf12a1b735_1882x896.png)

The TickerQ Dashboard is a modern Vue.js-based web UI powered by SignalR and Tailwind CSS. It provides live updates and comprehensive control over job scheduling and monitoring.

### **Setup**

```csharp
builder.Services.AddTickerQ(options =>
{
    options.AddOperationalStore<MyDbContext>(opt =>
    {
        opt.UseModelCustomizerForMigrations();
        opt.CancelMissedTickersOnApplicationRestart();
    });
    options.AddDashboard(basePath: "/tickerq");
    options.AddDashboardBasicAuth();
});


app.UseTickerQ();
```

Add credentials in `appsettings.json`:

```json
"TickerQBasicAuth": {
  "Username": "admin",
  "Password": "admin"
}
```

### **Features**

*   **System Overview**: throughput, active nodes, concurrency.
*   **Job Status Breakdown**: Done, DueDone, Failed, Pending.
*   **CronTickers View**: timeline, manual trigger, duplication, deletion.
*   **TimeTickers View**: execution history, live retry/cancel/edit/delete options.
*   **Add/Edit Job**: intuitive UI to define function, payload, retry policy.

# **🔍 TickerQ vs Hangfire vs Quartz.NET**

This page compares **TickerQ**, **Hangfire**, and **Quartz.NET** based on functionality, extensibility, performance, and developer experience. While all three are widely used, **TickerQ** provides the most modern, performant, and developer-friendly background task scheduling experience for .NET applications.

### **⚙️ Feature Comparison**

![](https://substackcdn.com/image/fetch/$s_!0oRg!,w_1456,c_limit,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F7647e8f4-f213-470e-9aa1-cdbb8a72c1e2_653x842.png)

## **✅ When to Choose TickerQ**

Choose TickerQ if you:

*   Prefer **compile-time safety** over runtime reflection
*   Need a **lightweight, fast scheduler**
*   Want **cron and time jobs** with retries/cooldowns
*   Use EF Core or need **distributed coordination**
*   Value a **first-party, real-time dashboard**

### **Limitations to Consider**

*   No Redis or distributed cache persistence (yet)
*   No node-specific routing or tag-based job targeting
*   Throttling and job chaining features are in progress

---

## **🔺 Conclusion**

TickerQ offers a **modern, efficient, and developer-friendly** background scheduler featuring:

*   Reflection-free architecture via source generation
*   Cron and time-based scheduling
*   Robust EF Core persistence
*   Real-time dashboard with live updates

### 👉 Full working code available at:

[Download Source Code](https://sourcecode.kanaiyakatarmal.com/tickerq)