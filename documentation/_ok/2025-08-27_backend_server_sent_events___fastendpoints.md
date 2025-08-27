```yaml
---
title: "Server Sent Events | FastEndpoints"
source: https://fast-endpoints.com/docs/server-sent-events
date_published: unknown
date_captured: 2025-08-27T18:46:36.332Z
domain: fast-endpoints.com
author: Unknown
category: backend
technologies: [Server Sent Events, IAsyncEnumerable, EventSource, Kestrel, HTTP/2, FastEndpoints, .NET]
programming_languages: [C#, HTML, JavaScript]
tags: [server-sent-events, real-time, web-api, dotnet, async, streaming, frontend, backend, http2, data-push]
key_concepts: [Server-Sent Events, asynchronous-programming, IAsyncEnumerable, EventSource-API, real-time-communication, HTTP/2-multiplexing, data-streaming, event-driven]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces Server-Sent Events (SSE) as a method for pushing real-time data asynchronously to web browsers. It provides C# code examples demonstrating how to create an SSE endpoint using `IAsyncEnumerable` to stream data continuously. The content also illustrates how to consume these events on the client-side using the JavaScript `EventSource` object in an HTML page. Additionally, it explains how to send different data types within a single stream using a wrapper and emphasizes the benefit of enabling HTTP/2 for efficient multiplexing when managing multiple SSE streams.]
---
```

# Server Sent Events | FastEndpoints

# Server Sent Events

[Server Sent Events (SSE)](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events) can be used to push real-time data down to the web browser in an **async** manner without blocking threads using the **IAsyncEnumerable** interface like so:

### Endpoint.cs

```csharp
public class EventStream : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("event-stream");
        AllowAnonymous();
        Options(x => x.RequireCors(p => p.AllowAnyOrigin()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        //simply provide any IAsyncEnumerable<T> for the 2nd argument
        await Send.EventStreamAsync("my-event", GetDataStream(ct), ct);
    }

    private async IAsyncEnumerable<object> GetDataStream([EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000);
            yield return new { guid = Guid.NewGuid() };
        }
    }
}
```

In the browser, the event stream can be subscribed to and consumed using the **EventSource** object like so:

### Index.html

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8"/>
</head>
<body>
<script>
    const sse = new EventSource('http://localhost:8080/event-stream');
    sse.addEventListener('my-event', (e) => console.log(e.data));
</script>
</body>
</html>
```

---

The above example demonstrates sending a continuous stream of a single event model type. It is possible to send different types of data in a single stream with the use of the wrapper type **StreamItem** like so:

```csharp
public override async Task HandleAsync(CancellationToken c)
{
    await Send.EventStreamAsync(GetMultiDataStream(c), c);

    async IAsyncEnumerable<StreamItem> GetMultiDataStream([EnumeratorCancellation] CancellationToken ct)
    {
        long id = 0;

        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000);

            id++;

            if (DateTime.Now.Second % 2 == 1)
                yield return new StreamItem(id.ToString(), "odd-second", Guid.NewGuid()); //guide data
            else
                yield return new StreamItem(id.ToString(), "even-second", "hello!"); //string data
        }
    }
}
```

---

If you are planning to create more than a handful of server-sent-event streams, it's a good idea to enable **HTTP/2** in kestrel and all upstream servers such as reverse proxies and CDNs so that data can be multiplexed between the web server and client using a low number of tcp connections.

Here's a [good read](https://ordina-jworks.github.io/event-driven/2021/04/23/SSE-with-HTTP2.html) on the subject.

---

Previous [<- Remote Procedure Calls](/docs/remote-procedure-calls)

Next [Exception Handler ->](/docs/exception-handler)

© FastEndpoints 2025

[](/)