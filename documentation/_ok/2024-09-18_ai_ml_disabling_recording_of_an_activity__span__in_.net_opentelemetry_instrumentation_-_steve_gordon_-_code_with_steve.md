```yaml
---
title: Disabling Recording of an Activity (span) in .NET OpenTelemetry Instrumentation - Steve Gordon - Code with Steve
source: https://www.stevejgordon.co.uk/disabling-recording-of-an-activity-span-in-dotnet-opentelemetry-instrumentation?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=implementing-blocked-floyd-warshall-algorithm&_bhlid=f0bee2c32bfa316148a14486207d3f55ff910f83
date_published: 2024-09-18T11:30:07.000Z
date_captured: 2025-08-08T13:46:22.295Z
domain: www.stevejgordon.co.uk
author: Steve Gordon
category: ai_ml
technologies: [.NET, OpenTelemetry, ASP.NET Core, System.Diagnostics, Elastic, W3C]
programming_languages: [C#]
tags: [opentelemetry, dotnet, observability, tracing, instrumentation, span, activity, middleware, performance, sampling]
key_concepts: [OpenTelemetry, Distributed-Tracing, Activity-span, Sampling, Instrumentation, ASP.NET-Core-Middleware, ActivityTraceFlags, IsAllDataRequested]
code_examples: false
difficulty_level: intermediate
summary: |
  This article details how to programmatically disable the recording and export of an `Activity` (span) within .NET OpenTelemetry instrumentation. The author presents a practical scenario involving an ASP.NET Core middleware where invalid requests should not generate trace data to avoid unnecessary costs. The core solution involves directly manipulating the `Activity.Current` object's `ActivityTraceFlags` and `IsAllDataRequested` properties. This method allows for early termination of tracing for specific requests, preventing further overhead from activity enrichment or child span creation.
---
```

# Disabling Recording of an Activity (span) in .NET OpenTelemetry Instrumentation - Steve Gordon - Code with Steve

![Header image for an article on disabling recording of an Activity (span) in .NET OpenTelemetry Instrumentation, featuring the text ".NET" and the article title on a teal to green gradient background.](https://www.stevejgordon.co.uk/wp-content/uploads/2024/09/Disabling-Recording-of-an-Activity-span-in-.NET-OpenTelemetry-Instrumentation-750x410.png)

# Disabling Recording of an Activity (span) in .NET OpenTelemetry Instrumentation

I’ve recently been building some hobby code to dogfood the various observability tooling we develop at Elastic. Additionally, I’ve been interested in identifying the pain points of using our products as well as the .NET instrumentation libraries (from System.Diagnostics) used to instrument code in an OpenTelemetry-compatible way.

## Recording Activities in .NET

In today’s short post, I’d like to share a solution I’ve applied to programmatically disable the recording (and, therefore, exporting) of an `Activity`. In .NET, the `Activity` type is built into the .NET framework and mainly represents a span within an OpenTelemetry observability trace. In principle, this is a pretty simple requirement, and the code needed is very straightforward. However, the solution may not be immediately apparent. Hence, I wanted to document an approach others may want to leverage in their code.

Early in the pipeline, a decision is made whether a parent operation (such as handing an incoming HTTP request in ASP.NET Core) will be recorded and sampled (exported). This may occur through head sampling, which commonly uses a non-deterministic, ratio-based choice for whether a trace should be sampled. This is usually done by defining some approximate percentage of root spans (therefore, traces) that should be sampled.

While we ideally capture everything, sampling all traces may increase costs for high-volume services. If sampling is used, optimisations can occur after that decision is made. In .NET, the diagnostics library and our instrumented code can avoid some performance overheads when an upfront sampling decision has determined that something will not be recorded.

However, for traces that are sampled (which will be all requests by default when using the OpenTelemetry SDK), an `Activity` will be created for the incoming request, with the potential for child activities to be created for sub-operations during the request, such as outgoing HTTP requests or database activity.

I’ve oversimplified this summary since additional considerations exist, such as whether a dependent service has made a sampling decision for its own trace, which will be communicated through the W3C tracing headers and potentially override a sampling decision. There are also subtle distinctions between recording a trace (capturing spans) and sampling it for export. For the purpose of this post, we can focus on the fact that a decision is made early in the tracing process as to whether something will end up being eligible for export (recording).

## My Requirement

I recently had a situation in an application where I wanted to control whether a span (`Activity`) should be exported programmatically. I was developing an ASP.NET Core middleware component to handle requests for a particular callback URL in the application. Requests to the callback URL were expected from a third-party service with required specific query string parameters and should occur from a known IP allowlist. In my particular case, it would not be valid if a request were received without those parameters or from an unknown IP address. Further, since no human user and only a specific and single third-party automated service was expected to call the endpoint, any invalid request would likely be a mistake or, worse, potentially malicious.

I decided to exclude recording traces for these invalid requests. I reasoned that I didn’t want to incur any costs for storing the trace data when the request did not meet some early requirements validation. This could then avoid the subsequent overhead of enriching the `Activity` or creating any child activities (spans).

## Implementation

The implementation for disabling recording after an Activity has been marked to be recorded by a sampling decision is only two lines of code. Still, it may not be immediately apparent without first peeking into the implementation of the Activity type and understanding how the OpenTelemetry SDK (and other observability vendor agents) decide what to export.

The `Activity` class includes a `Recorded` property exporters access to determine what to send to a backend observability tool. The type of this property is `ActivityTraceFlags` and its implementation is as follows:

```csharp
[Flags]
public enum ActivityTraceFlags
{
    None = 0b_0_0000000,
    Recorded = 0b_0_0000001, // The Activity (or more likely its parents) has been marked as useful to record
}
```

A property holding the trace flags is also present on the `Activity` type.

```csharp
public ActivityTraceFlags ActivityTraceFlags
{
    get
    {
        if (!W3CIdFlagsSet)
        {
            TrySetTraceFlagsFromParent();
        }
        return (ActivityTraceFlags)((~ActivityTraceFlagsIsSet) & _w3CIdFlags);
    }
    set
    {
        _w3CIdFlags = (byte)(ActivityTraceFlagsIsSet | (byte)value);
    }
}
```

Without diving into this too deeply, it may be initialised with a default value or through propagation from a parent `Activity` (span). Therefore, the framework and/or instrumentation library usually set this value when a sampling decision occurs.

A second property of the `Activity` class is used during activity enrichment of instrumented code.

```csharp
public bool IsAllDataRequested { get; set; }
```

This property indicates whether propagation information and enrichment through activity (span) links, tags (attributes), and events are required based on the earlier recording and sampling decision. This allows instrumentation code to avoid the overhead of computing, allocating, and adding such information for an `Activity` that will ultimately not be exported.

## Preventing Recording for a Specific Activity

With this groundwork laid, we can now consider how to prevent the export of an activity. There are a few reasonable places to consider doing this. In my case, I wanted to prevent any incurred overhead as early as possible and could implement my code directly in my middleware class.

If, after validating the incoming request, I decided it was not valid and shouldn’t be recorded externally, I would need to modify two of the properties on the `Activity`.

```csharp
var activity = Activity.Current;
if (activity is not null)
{
    activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
    activity.IsAllDataRequested = false;
}
Activity.Current = null;
```

In the above code, we access the current activity, and if it is not null, we proceed to ensure that it is not recorded. The code modifies the `ActivityTraceFlags` property of the `Activity` object using a bitwise AND assignment operation to clear the Recorded flag. The expression `~ActivityTraceFlags.Recorded` inverts the bits of the Recorded flag, creating a mask where all bits are set to one except for the bit corresponding to Recorded, which is set to zero. When this mask is ANDed with the current value of `ActivityTraceFlags`, it ensures that the Recorded flag is turned off (set to zero).

In my scenario, I also wanted to avoid any further overhead introduced by enriching the activity. This is achieved by setting `IsAllDataRequested` to false. Finally, the code sets the value of `Activity.Current` to null to prevent any child spans from having this unrecorded activity as their parent.

## Alternative Designs

An alternative solution would be to use a `Processor` in the OpenTelemetry SDK. In fact, such a processor already exists, and it is named `MyFilteringProcessor`. Using this approach, in our instrumentation code, we could add a custom property to the activity that the filter delegate (passed to the constructor of `MyFilteringProcessor`) can access when deciding which activities to unset for recording.

This is viable; however, if enriching the `Activity` in the instrumentation code is resource-intensive, it is less efficient, as we only disable recording when the span ends (i.e., the `Activity` is disposed of). I slightly prefer the early decision for my scenario to avoid all possible overhead that malicious requests could trigger.

## Conclusion

That’s it for this post. The code needed to prevent an `Activity` from being recorded is pretty trivial but not particularly obvious. Hopefully, this post helps if you find yourself with a similar requirement.