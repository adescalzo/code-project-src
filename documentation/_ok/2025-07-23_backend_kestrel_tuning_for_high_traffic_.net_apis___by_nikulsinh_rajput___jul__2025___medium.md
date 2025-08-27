```yaml
---
title: "Kestrel Tuning for High Traffic .NET APIs | by Nikulsinh Rajput | Jul, 2025 | Medium"
source: https://medium.com/@hadiyolworld007/kestrel-tuning-for-high-traffic-net-apis-fd3c999b1a73
date_published: 2025-07-23T19:31:50.346Z
date_captured: 2025-08-22T12:27:13.421Z
domain: medium.com
author: Nikulsinh Rajput
category: backend
technologies: [Kestrel, .NET, ASP.NET Core, dotnet-counters, Application Insights, Prometheus, dotnet-trace, NGINX, YARP, BenchmarkDotNet, Redis, Libuv]
programming_languages: [C#, JSON]
tags: [kestrel, .net, asp.net-core, performance, optimization, high-traffic, web-server, api, garbage-collection, thread-pool]
key_concepts: [threadpool-tuning, connection-queue-limits, garbage-collection-tuning, server-gc, low-latency-gc, socket-transport, performance-monitoring, reverse-proxy]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to optimizing Kestrel, the web server for ASP.NET Core, to handle high-traffic .NET APIs. It details how to tune the .NET ThreadPool by increasing minimum threads to prevent starvation under load. The author also explains configuring Kestrel's request queue limits to manage concurrent connections and requests effectively. Furthermore, the guide covers taming the .NET Garbage Collector using Server GC and Low Latency mode for improved memory management. Optional optimizations like using Socket Transport and essential performance monitoring with `dotnet-counters` are also discussed, culminating in a checklist for high-traffic settings.]
---
```

# Kestrel Tuning for High Traffic .NET APIs | by Nikulsinh Rajput | Jul, 2025 | Medium

# Kestrel Tuning for High Traffic .NET APIs

## Thread pool tweaks, connection queues, and GC tuning — how I scaled my .NET backend to handle insane traffic with Kestrel.

![Conceptual diagram showing .NET Backend Performance Optimization with Kestrel, Thread Pool, Connection Queues, and GC Optimization. The image features a stylized Kestrel bird, gears representing the thread pool, a speedometer for connection queues, and a molecular structure for GC optimization, all against a gradient purple background.](https://miro.medium.com/v2/resize:fit:700/1*As_hMcHRIVca-3Bzk9Ot0w.png)

Learn how to tune Kestrel in .NET for high traffic: optimize thread pools, queue limits, and garbage collection for blazing-fast APIs.

# ⚠️ The Problem: Your .NET API Is Fast — Until It Isn’t

Kestrel is the powerful web server that runs behind ASP.NET Core. It’s fast, lightweight, and production-ready. But by default, it’s not tuned for high-concurrency environments.

Maybe this sounds familiar:

*   Under load, requests queue up and response times spike
*   CPU usage is low, but throughput is terrible
*   Logs are clean, but the API “feels” slow at scale

> _The issue isn’t your code — it’s_ **_Kestrel defaults_**_._

In this guide, I’ll show you how to unlock serious performance by tuning Kestrel like a racecar.

# 🔧 Why Kestrel Tuning Matters

Kestrel is optimized for **general workloads** — but once you go from 1K to 100K requests/min, you need to:

*   Increase thread parallelism
*   Raise connection queue thresholds
*   Control .NET’s memory management

Let’s walk through it.

# 🧠 1. Configure ThreadPool to Match Your Load

.NET uses a thread pool to handle incoming requests. Under high traffic, it may take too long to spin up new threads.

# ✅ Fix: Boost the minimum threads

In `Program.cs` or `Startup.cs`, add:

```csharp
using System.Threading;  
  
ThreadPool.SetMinThreads(workerThreads: 200, completionPortThreads: 200);
```

This reduces thread starvation under spikes and improves cold-start behavior.

📌 **Tip:** Monitor with `dotnet-counters` to watch thread pool saturation in real time.

# ⏳ 2. Increase Kestrel’s Request Queue Limits

By default, Kestrel allows a limited number of concurrent requests and queued connections.

# ✅ Fix: Raise connection and request queue thresholds

In your `appsettings.json`:

```json
"Kestrel": {  
  "Limits": {  
    "MaxConcurrentConnections": 10000,  
    "MaxConcurrentUpgradedConnections": 5000,  
    "MaxRequestBodySize": 104857600  
  },  
  "EndpointDefaults": {  
    "Protocols": "Http1AndHttp2"  
  }  
}
```

You can also configure programmatically:

```csharp
webBuilder.ConfigureKestrel(serverOptions =>  
{  
    serverOptions.Limits.MaxConcurrentConnections = 10000;  
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 5000;  
});
```

📌 **Tip:** Keep an eye on queue lengths and reject thresholds using Application Insights or Prometheus exporters.

# 🧹 3. Tame the Garbage Collector (GC)

.NET’s GC can pause your app if not configured for the workload.

# ✅ Fix: Use Server GC + Low Latency Mode

In `csproj` or `runtimeconfig.json`, set:

```json
{  
  "runtimeOptions": {  
    "configProperties": {  
      "System.GC.Server": true,  
      "System.GC.Concurrent": true  
    }  
  }  
}
```

And optionally use LowLatency mode around high-throughput operations:

```csharp
using (new LatencyModeScope(GCLatencyMode.LowLatency))  
{  
    // Time-sensitive work  
}
```

📌 **Tip:** Monitor GC stats using `dotnet-trace` to spot collections during request spikes.

# 🛠️ 4. Use Socket Transport for Lower Latency (Optional)

If you need every microsecond:

```csharp
webBuilder.UseSockets(); // instead of default Libuv transport
```

This reduces system overhead for raw socket throughput — but test under load before switching.

# 📊 5. Measure Everything with dotnet-counters

Use the built-in `dotnet-counters` tool to track:

```bash
dotnet-counters monitor --process-id <your_pid>
```

Key metrics:

*   `ThreadPool Thread Count`
*   `Requests Queued`
*   `GC Pause Duration`
*   `Connection Queue Length`

# ✅ Summary: High-Traffic Settings Checklist

![Table summarizing high-traffic settings recommendations for .NET Kestrel. It lists settings such as ThreadPool Min Threads (ThreadPool.SetMinThreads(200, 200)), Max Concurrent Connections (10,000+), GC Mode (Server GC, Concurrent true), Transport (UseSockets() optional), and GC Monitoring (dotnet-counters, dotnet-trace).](https://miro.medium.com/v2/resize:fit:615/1*vXrZyxRpJ5aL09cAKdVhbw.png)

# 💭 Final Thoughts: Scaling Without Crashing

With these tweaks, we handled:

*   💥 120K requests/min
*   🚫 0 dropped connections
*   ⚙️ Under 200ms P95 latency

And most importantly, we did it **without touching our business logic**.

Kestrel is fast out of the box — but with tuning, it becomes a monster.

# 👇 Try This Next

*   Set up NGINX or YARP as a reverse proxy in front of Kestrel
*   Use [BenchmarkDotNet](https://benchmarkdotnet.org/) for controlled performance profiling
*   Pair with Redis or in-memory caching to reduce GC pressure

dropping your API tuning tips in the comments.Let’s build better backends — faster. ⚙️🚀