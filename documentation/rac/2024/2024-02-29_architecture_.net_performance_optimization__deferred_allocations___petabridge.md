```yaml
---
title: ".NET Performance Optimization: Deferred Allocations | Petabridge"
source: https://petabridge.com/blog/net-performance-deferred-allocation/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=unit-testing-clean-architecture-use-cases&_bhlid=4bcf5c04ecd79fdd02decb48154219bff08f35de
date_published: 2024-02-29T14:20:00.000Z
date_captured: 2025-08-12T18:14:15.218Z
domain: petabridge.com
author: Unknown
category: architecture
technologies: [Akka.NET, Phobos, OpenTelemetry.Api, System.Diagnostics.Activity, ASP.NET Core, .NET, Benchmark.NET, Microsoft.Extensions.Logging, Akka.Event]
programming_languages: [C#]
tags: [.net, performance-optimization, memory-management, akka.net, opentelemetry, observability, distributed-systems, hotpath, benchmarking, telemetry]
key_concepts: [Deferred Allocations, Hotpath Optimization, Memory Allocation, Throughput Optimization, OpenTelemetry Tracing, Asynchronous Processing, Value Types, Benchmarking]
code_examples: false
difficulty_level: intermediate
summary: |
  The article details how Petabridge achieved a 161% performance increase in Phobos, their OpenTelemetry add-on for Akka.NET, through "deferred allocations." It explains how moving memory-intensive operations, such as stringification, out of critical application hotpaths to an asynchronous telemetry export pipeline significantly reduces latency and boosts throughput. The post provides C# code examples demonstrating the transition from direct allocations to using `System.Diagnostics.Activity` and custom `struct`s with `IEnumerable` for deferred data expansion. Benchmark.NET results clearly illustrate the substantial reduction in memory allocation and the increase in message processing rates. The author concludes by discussing the benefits and trade-offs of this powerful optimization technique for observability in performance-critical distributed systems.
---
```

# .NET Performance Optimization: Deferred Allocations | Petabridge

# .NET Performance Optimization: Deferred Allocations

### How We Accelerated Phobos 2.5's Throughput by 161%

17 minutes to read

*   [Problem Context](#problem-context)
*   [Phobos 2.3.1 Instrumentation Code and Performance Impact](#phobos-231-instrumentation-code-and-performance-impact)
*   [How Deferred Allocation Can Work](#how-deferred-allocation-can-work)
*   [Phobos 2.5.0 Instrumentation Code and Performance Impact](#phobos-250-instrumentation-code-and-performance-impact)
    *   [Phobos 2.3.1 Memory Allocation](#phobos-231-memory-allocation)
    *   [Phobos 2.5.0 Memory Allocation](#phobos-250-memory-allocation)
    *   [Phobos 2.5.0 Throughput](#phobos-250-throughput)
*   [Things to Bear in Mind](#things-to-bear-in-mind)

We just finished shipping [Phobos 2.5 and it’s a _massive_ performance upgrade over previous versions of Phobos](https://phobos.petabridge.com/articles/releases/RELEASE_NOTES.html#250httpssdkbincompublisherpetabridgeproductphobospackagesphobosactorversions250-february-13th-2024).

> For those that are not aware: [Phobos is our commerical OpenTelemetry add-on for Akka.NET](https://phobos.petabridge.com/).

This past summer we posted about [Phobos 2.4’s performance being 62% faster than Phobos 2.3.1](https://petabridge.com/blog/phobos-2.4-opentelemetry-optimizations/) Phobos 2.5 is 161% faster than Phobos 2.3.1 - and in this post we’re going to share the generalized .NET performance optimization technique we’ve been using to accomplish these improvements: **deferred allocations**.

## Problem Context

Imagine you have a performance-critical hotpath in your application, such as an [Akka.NET actor](https://getakka.net/) or an ASP.NET Controller - we ideally want to keep the latency in this critical path as low as possible in order to maximize responsiveness and per-process throughput.

_But, we are also given a secondary requirement_ - maybe we have to add logging or OpenTelemetry tracing here for observability purposes, or maybe we have to push some data points into an internal-facing analytics / reporting system for internal stakeholders.

![Critical processing pipeline with fully allocated telemetry](/images/2024/deferred-allocation/deferred-allocation-before.png)
*Image: A flowchart illustrating a "before" scenario where an Akka.NET Actor processes incoming messages, performs work, and emits fully allocated telemetry directly to an out-of-band Telemetry Pipeline. This synchronous allocation within the critical path impacts performance.*

Implementing that secondary requirement along the critical path is going to increase our processing time at the expense of our mission-critical processing and ultimately, our end-users. **Deferring allocations to outside the critical path is how we can avoid this problem.**

Let’s consider what we did with Phobos 2.5 and .NET’s OpenTelemetry pipeline as an example.

## Phobos 2.3.1 Instrumentation Code and Performance Impact

Phobos installs inside your `ActorSystem` and injects some of the following telemetry code into all of the actors therein:

```csharp
// Allocation 1: Span creation (unavoidable allocation)
TelemetrySpan sp = Tracer.StartActiveSpan(underlyingMsg.GetType().GetOperationName(),
	SpanKind.Consumer, parentContext: context.Value, InitialAttributes, startTime: startTime);

// Allocation 2: Adding attributes + stringifying data – 2 collection allocs + 2 strings
sp.SetAttribute(MsgSenderTagName, (Sender ?? ActorRefs.Nobody).Path.ToStringWithAddress())
	.SetAttribute(MessageTagName, underlyingMsg.GetType().ToCleanName());

if (startTimeUtcTicks != null)
{
	// Allocation 3: Adding 1 event – 1 collection alloc
	sp.AddEvent("waiting", startTime);
}

// Allocation 4: Adding 1 event – 1 collection alloc, 1 SpanAttribute alloc
var attributes = new SpanAttributes();

// Allocation 5: 1 MAJOR stringification operation
attributes.Add("content", underlyingMsg.ToString());
sp.AddEvent("message", attributes);
```

This code uses the `OpenTelemetry.Api` package to collect `TelemetrySpan`s each time an actor processes a message (although [this can be configured](https://phobos.petabridge.com/articles/performance.html#performance-optimization-best-practices)) and is the most expensive part of Phobos due to the large number of allocations and the `string`\-ification of `ActorPath`s and the messages themselves.

To give you a general idea of the peformance impact, here are the numbers when running Phobos 2.3.1:

| Metric                  | Units / s   | Max / s     | Average / s   | Min / s     | StdDev / s  |
| :---------------------- | :---------- | :---------- | :------------ | :---------- | :---------- |
| TotalCollections [Gen0] | collections | 42.11       | 38.22         | 36.28       | 2.32        |
| TotalCollections [Gen1] | collections | 2.59        | 2.12          | 1.81        | 0.31        |
| TotalCollections [Gen2] | collections | 1.11        | 0.79          | 0.73        | 0.11        |
| [Counter] MessageReceived | operations  | 1,263,277.84 | 1,145,856.34 | 1,088,397.58 | 70,098.66   |

By comparison, when Phobos isn’t installed this same sample runs at about **6.2m messages per second for a single actor**. So the default performance of an Akka.NET actor is roughly 1/6th as fast when Phobos is installed versus when it’s not - and the only discernible difference is whether or not telemetry is enabled.

## How Deferred Allocation Can Work

In both Phobos 2.4 and 2.5, we gradually improved performance in this hotpath through the extensive use of deferred allocation - what does that mean?

*   **Hotpath** - we have a hotpath, the actor’s message-handling routine. This code is performing latency-sensitive work and has to execute as quickly as possible for best results.
*   **Telemetry** - users _want and need_ telemetry to help understand how their software performs, and while this data is important _it’s not as crucial or urgent to the real-time operation of the software_.

That last sentence is key: “telemetry is not as crucial or urgent.” This means we can try to change _when_ the telemetry is fully allocated and expanded so it happens _asynchronously_ from our hotpath.

![Critical processing pipeline with asynchronously allocated telemetry](/images/2024/deferred-allocation/deferred-allocation-after.png)
*Image: A flowchart illustrating an "after" scenario where an Akka.NET Actor processes incoming messages, performs work, and emits unallocated telemetry. The Telemetry Pipeline then asynchronously allocates and expands the telemetry payload during export, moving the allocation out of the critical path.*

The allocations are still going to happen, but now they’ll happen in such a way that it’ll only impact the OpenTelemetry export pipeline - which is less urgent and important than the rest of our software.

## Phobos 2.5.0 Instrumentation Code and Performance Impact

We changed our telemetry code in Phobos 2.5.0 to look like the following, in order to displace allocations from inside our actors to the OpenTelemetry export pipeline instead (which runs out of band:)

```csharp
// Deferral 1: Lumps tags into struct, defers stringification. 0 allocs.
var tags = InitialTags.WithMessageTypeAndSender(underlyingMsg, Sender?.Path);

var perfSpan = ActivitySource.StartActivity(underlyingMsg.GetType().GetOperationName(),
	ActivityKind.Consumer, context ?? default, tags);

if(perfSpan == null) return default;

// ensure that we are the current span
Activity.Current = perfSpan;

if (startTimeUtcTicks != null)
{
	// Adds 1 node in linked list inside Activity
	perfSpan.AddEvent(new ActivityEvent("waiting", startTime));
}

// Deferral 2: Adds 1 node in linked list inside Activity + ActivityTagsCollection
// but IEnumerable is not expanded until export pipeline
perfSpan.AddEvent(new ActivityEvent("message", 
	tags: new ActivityTagsCollection(CreateSpanAttributes(underlyingMsg))));

return perfSpan;

static IEnumerable<KeyValuePair<string, object>> CreateSpanAttributes(object msg)
{
	// Deferral 3: message is not stringified until enumeration (export pipeline)
	yield return new KeyValuePair<string, object>("content", msg);
}
```

So what’s happening here?

For starters, we’ve switched from using the OpenTelemetery.Api package to System.Diagnostics.Activity - the former being a wrapper over the latter, in order to access some APIs that make deferred allocation possible.

The `ActivitySource.StartActivity` method accepts an `IEnumerable<KeyValuePair<string, object>>` argument for all of the “tags” we want to apply to this `Activity`. Our `tags` data type implements `IEnumerable<KeyValuePair<string, object>>` using a custom `struct` that we’ll show later. This is beneficial to performance for two reasons:

1.  The `IEnumerable` doesn’t get iterated over until the `Activity` is about to be exported - so we save on list / array allocations there;
2.  The `object` will _eventually_ get rendered into a `string` - which means we can defer the stringification of the tag’s value until the `Activity` is in the export pipeline.

Let’s take a look at the `struct` we use to do this:

```csharp
internal readonly struct BuiltInTags : IEnumerable<KeyValuePair<string, object>>
{ 
	public string ActorPath { get; }
	public string ActorType { get; } 
	public object MessageType { get; }
	public ActorPath MsgSender { get; }
	
	public IEnumerator<KeyValuePair<string, object>> GetEnumerator(){
		yield return new KeyValuePair<string, object>(ActorPathTagName, ActorPath);
		yield return new KeyValuePair<string, object>(ActorTypeTagName, ActorType);
	
		// defer stringifying the message's type until we hit export pipeline
		yield return new KeyValuePair<string, object>(MessageTagName, FormatMessageType(MessageType));
	
		// defer stringifying the message's sender until we hit export pipeline
		yield return new KeyValuePair<string, object>(MsgSenderTagName, FormatSenderPath(MsgSender));
	}
	
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
```

Eliminating a collection allocation by using this `readonly struct` saves us some allocations, but the real benefit is deferring the `object` to `string` conversion until this `Activity` is getting exported by the OpenTelemetry tracing pipeline.

The other major area where we benefit from deferred stringification is the `static` local function which emits an `IEnumerable<KeyValuePair<string, object>>` for our `message` event - this uses the exact same technique as the previous code we looked at.

```csharp
// Deferral 2: Adds 1 node in linked list inside Activity + ActivityTagsCollection
// but IEnumerable is not expanded until export pipeline
perfSpan.AddEvent(new ActivityEvent("message", 
	tags: new ActivityTagsCollection(CreateSpanAttributes(underlyingMsg))));

return perfSpan;

static IEnumerable<KeyValuePair<string, object>> CreateSpanAttributes(object msg)
{
	// Deferral 3: message is not stringified until enumeration (export pipeline)
	yield return new KeyValuePair<string, object>("content", msg);
}
```

Same idea - the attributes for this trace event don’t get rendered into `string` representations until we hit the trace export pipeline, therefore we can avoid that expensive stringification operation from occurring inside our hotpath.

Let’s take a look at some memory allocation data from Benchmark.NET to drive this point home:

### Phobos 2.3.1 Memory Allocation

| Method             | MessageKind | Mean      | Error     | StdDev    | Req/sec      | Gen0    | Allocated |
| :----------------- | :---------- | :-------- | :-------- | :-------- | :----------- | :------ | :-------- |
| **CreateNewRootSpan** | **Primitive** | **112.6 ns** | **2.18 ns** | **5.64 ns** | **8,880,473.05** | **0.0246** | **232 B** |
| CreateChildSpan    | Primitive   | 112.9 ns  | 2.24 ns   | 3.42 ns   | 8,858,208.46 | 0.0246  | 232 B     |
| CreateChildSpanWithBaggage | Primitive   | 113.0 ns  | 2.28 ns   | 2.14 ns   | 8,852,716.94 | 0.0246  | 232 B     |
| **CreateNewRootSpan** | **Class**   | **159.5 ns** | **3.21 ns** | **8.52 ns** | **6,270,690.97** | **0.0348** | **328 B** |
| CreateChildSpan    | Class       | 165.4 ns  | 3.54 ns   | 10.45 ns  | 6,044,475.13 | 0.0348  | 328 B     |
| CreateChildSpanWithBaggage | Class       | 162.2 ns  | 3.24 ns   | 6.31 ns   | 6,165,487.70 | 0.0348  | 328 B     |
| **CreateNewRootSpan** | **Record**  | **201.4 ns** | **2.40 ns** | **2.13 ns** | **4,965,186.73** | **0.0730** | **688 B** |
| CreateChildSpan    | Record      | 211.1 ns  | 4.00 ns   | 3.74 ns   | 4,737,334.16 | 0.0730  | 688 B     |
| CreateChildSpanWithBaggage | Record      | 208.1 ns  | 3.60 ns   | 3.37 ns   | 4,805,978.43 | 0.0730  | 688 B     |

### Phobos 2.5.0 Memory Allocation

| Method             | MessageKind | Mean      | Error     | StdDev    | Req/sec      | Gen0    | Allocated |
| :----------------- | :---------- | :-------- | :-------- | :-------- | :----------- | :------ | :-------- |
| **CreateNewRootSpan** | **Primitive** | **59.97 ns** | **0.637 ns** | **0.565 ns** | **16,676,180.98** | **0.0050** | **48 B**  |
| CreateChildSpan    | Primitive   | 67.24 ns  | 0.943 ns  | 0.836 ns  | 14,871,155.56 | 0.0050  | 48 B      |
| CreateChildSpanWithBaggage | Primitive   | 77.87 ns  | 0.871 ns  | 0.814 ns  | 12,842,151.57 | 0.0050  | 48 B      |
| **CreateNewRootSpan** | **Class**   | **59.70 ns** | **0.695 ns** | **0.616 ns** | **16,749,521.86** | **0.0050** | **48 B**  |
| CreateChildSpan    | Class       | 63.69 ns  | 0.715 ns  | 0.669 ns  | 15,701,818.74 | 0.0050  | 48 B      |
| CreateChildSpanWithBaggage | Class       | 71.53 ns  | 0.650 ns  | 0.576 ns  | 13,979,808.57 | 0.0050  | 48 B      |
| **CreateNewRootSpan** | **Record**  | **56.55 ns** | **0.258 ns** | **0.229 ns** | **17,684,262.41** | **0.0051** | **48 B**  |
| CreateChildSpan    | Record      | 64.35 ns  | 0.369 ns  | 0.288 ns  | 15,538,845.63 | 0.0050  | 48 B      |
| CreateChildSpanWithBaggage | Record      | 72.41 ns  | 0.655 ns  | 0.613 ns  | 13,809,634.69 | 0.0050  | 48 B      |

Memory allocation figures changed quite a bit depending on how the underlying message type was implemented - `record` types, for instance, use a `StringBuilder` to pretty-print all of the properties and fields each time `object.ToString()` is called, hence the high memory footprint.

By deferring this `string`\-ification we’re able to normalize the memory allocation down from something as high as 688 bytes to a flat 48 bytes per message.

### Phobos 2.5.0 Throughput

From a throughput perspective, these deferred allocation techniques reduced Phobos’ latency / increased its throughput by 161%.

| Metric                  | Units / s   | Max / s     | Average / s   | Min / s     | StdDev / s  |
| :---------------------- | :---------- | :---------- | :------------ | :---------- | :---------- |
| TotalCollections [Gen0] | collections | 20.43       | 19.65         | 18.86       | 0.48        |
| TotalCollections [Gen1] | collections | 7.04        | 5.83          | 4.96        | 0.65        |
| TotalCollections [Gen2] | collections | 3.06        | 2.68          | 1.98        | 0.47        |
| [Counter] MessageReceived | operations  | 3,064,477.84 | 2,994,416.62 | 2,891,043.79 | 53,773.16   |

Roughly 3 million messages per second versus 1.1 million - this is all without changing the underlying data our telemetry collects. We’re just collecting it more efficiently.

## Things to Bear in Mind

Deferred allocation techniques, such as deferred enumeration and stringification, are powerful methods for making existing code run much faster without having to compromise on requirements. Our benchmark results demonstrate this quite clearly.

However, there are some important trade-offs to bear in mind with this technique:

1.  Deferred stringification relies on asychrony - if the output is critical to your hotpath, then you might not be able to use this technique effectively.
2.  There’s a chance that errors with object expansion / allocation might not get discovered until after it’s too late to fix it due to this technique - deferred allocation prioritizes availability over consistency, essentially. That trade needs to align with your requirements for the data being deferred.
3.  The underyling consumer of the data needs to support deferred allocation in order for this to work - OpenTelemetry, System.Diagnostics.Activity, Microsoft.Extensions.Logging, and Akka.Event are all examples of APIs that support deferred allocation techniques.

If you liked this post, you can [share it with your followers](https://twitter.com/intent/tweet?url=https://petabridge.com/blog/net-performance-deferred-allocation/&text=.NET+Performance+Optimization%3A+Deferred+Allocations&via=petabridge) or [follow us on Twitter](https://twitter.com/petabridge)!

Written by [Aaron Stannard](http://twitter.com/Aaronontheweb) on February 29, 2024

*   Read more about:
*   [Akka.NET](/blog/category/akkadotnet/)
*   [Case Studies](/blog/category/case-studies/)
*   [Videos](/blog/category/videos/)

### Observe and Monitor Your Akka.NET Applications with Phobos

![Phobos - instant OpenTelemetry observability for Akka.NET](/images/phobos/phobos_profile_icon.png)
*Image: A circular icon featuring a stylized blue mountain or 'P' shape, representing the Phobos product for Akka.NET observability.*

Did you know that [Phobos](https://phobos.petabridge.com/ "Phobos - instant observability for Akka.NET") can automatically instrument your Akka.NET applications with OpenTelemetry?

[Click here to learn more.](https://phobos.petabridge.com/)

## Get the Latest on Akka.NET

Subscribe to stay up to date on the latest happenings with Akka.NET and access to training + live events.

Subscribe

We won't send you spam. Unsubscribe at any time.