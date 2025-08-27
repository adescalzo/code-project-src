```yaml
---
title: "Streaming API Data Without Burning Through Bandwidth | by Nikulsinh Rajput | Aug, 2025 | Medium"
source: https://medium.com/@hadiyolworld007/streaming-api-data-without-burning-through-bandwidth-b5090755af51
date_published: 2025-08-13T09:33:12.905Z
date_captured: 2025-08-13T11:13:35.420Z
domain: medium.com
author: Nikulsinh Rajput
category: backend
technologies: [HTTP, TLS, Gzip, Brotli, MessagePack, Protobuf, WebSocket, Server-Sent Events, Kafka, Redis Streams, Redis]
programming_languages: [Python]
tags: [streaming, api, bandwidth-optimization, real-time, data-pipeline, compression, batching, change-tracking, performance, websockets]
key_concepts: [Event-Delta Streaming, Change Tracking, Batching, Payload Compression, Real-time Data Delivery, Horizontal Scaling, Backpressure, Error Recovery]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article outlines a pattern for efficient API data streaming, focusing on reducing bandwidth and CPU usage. It addresses common pitfalls of naive streaming by introducing "Event-Delta Streaming," which centers on three core components: change tracking using timestamps or cursors, intelligent batching with micro-windows, and payload compression. Practical Python code examples illustrate these techniques. The piece also discusses architectural considerations like WebSocket and Server-Sent Events for delivery, alongside scaling strategies such as horizontal scaling and backpressure handling. This comprehensive approach aims to optimize real-time data pipelines for applications like IoT or financial feeds.]
---
```

# Streaming API Data Without Burning Through Bandwidth | by Nikulsinh Rajput | Aug, 2025 | Medium

# Streaming API Data Without Burning Through Bandwidth

## The pattern I use for scalable, real-time pipelines

Learn how to stream API data efficiently without ballooning bandwidth costs, with practical patterns for batching, compression, and change tracking.

### Image: Data Flow Optimization
![An abstract illustration depicting data flowing efficiently through a pipeline. An API source on the right connects to a curved pipe, through which glowing light-blue capsules (representing optimized data packets) travel. The pipe leads towards a cloud icon on the left, symbolizing a destination or service. The overall image suggests fast, contained, and optimized data transfer.](https://miro.medium.com/v2/resize:fit:700/1*dG7Lt4ZptvF1euc1TUamPg.png)

# The Problem with Naïve Streaming

Streaming data from APIs is appealing — you get **real-time insights** without waiting for batch jobs.
But most implementations I see have **two major problems**:

1.  **They fetch too much** — grabbing entire datasets repeatedly instead of only the changes
2.  **They send too often** — pushing tiny payloads that eat up bandwidth due to HTTP/TLS overhead

Result:

*   Higher bandwidth bills
*   More CPU spent on serialization/deserialization
*   Strained API quotas

If you’ve ever built a stock ticker, IoT sensor feed, or social media listener, you’ve probably hit these issues.

# The Core Idea: Event-Delta Streaming

Instead of streaming every single event in real time, the idea is to **stream only the differences** and **bundle them intelligently**.

Think of it as:

Naïve: Client ← Entire dataset every time
Optimized: Client ← Only the changes since last update

# Three Components of the Pattern

1.  **Change Tracking** — Know what’s new since the last push
2.  **Batching with Micro-Windows** — Group updates into short time windows
3.  **Payload Compression** — Reduce data size before transport

# 1. Change Tracking with Cursors or Timestamps

If the upstream API supports **incremental fetching**:

*   Use a `**last_updated**` **timestamp** or **cursor token**
*   Store the last-seen value in your pipeline
*   Query only for `WHERE updated_at > last_seen`

**Example:**

```python
import requests
from datetime import datetime

last_seen = datetime.utcnow().isoformat()
def fetch_changes():
    global last_seen
    resp = requests.get(
        "https://api.example.com/events",
        params={"since": last_seen}
    )
    data = resp.json()
    if data:
        last_seen = max(item["updated_at"] for item in data)
    return data
```

# 2. Batching with Micro-Windows

Instead of pushing each event instantly, collect events for a small **N-second buffer** and send them together.

Why?

*   TCP/HTTP handshake cost is amortized
*   Fewer packets, less header repetition
*   Better CPU utilization on both ends

**Example (async queue):**

```python
import asyncio

event_buffer = []
async def producer():
    while True:
        data = fetch_changes()
        event_buffer.extend(data)
        await asyncio.sleep(1)  # fetch every second
async def consumer():
    while True:
        if event_buffer:
            batch = event_buffer.copy()
            event_buffer.clear()
            send_to_client(batch)  # send in one payload
        await asyncio.sleep(5)  # send every 5 seconds
asyncio.run(asyncio.gather(producer(), consumer()))
```

# 3. Payload Compression

Even JSON compresses extremely well, especially if keys repeat (e.g., sensor data).
Options:

*   **Gzip** — Supported by most HTTP clients
*   **Brotli** — Better compression ratio, slightly slower
*   **Binary Formats** — e.g., MessagePack, Protobuf

**Example (gzip in Python):**

```python
import gzip
import json

def compress_payload(data):
    return gzip.compress(json.dumps(data).encode())
```

# Before vs After: Bandwidth Consumption

Here’s a real-world case from an IoT pipeline streaming **50 sensors** once per second:

| Strategy            | Payload Size (KB/sec) | Bandwidth per Day (GB) |
| :------------------ | :-------------------- | :--------------------- |
| Naïve (every event) | 200                   | 16.6                   |
| Batching (5 sec)    | 48                    | 3.9                    |
| Batch + Compression | 12                    | 1.0                    |

# Chart: Bandwidth Reduction

```
GB/day
16.6 | ██████████████████████████ Naïve
 3.9 | ████                      Batch
 1.0 | █                         Batch+Compress
 0.0 |___________________________
```

# Architecture Overview

**Flow:**

1.  **Ingest Layer** pulls changes using incremental API calls
2.  **Buffer Layer** holds data in memory for micro-windows
3.  **Processing Layer** optionally enriches/transforms
4.  **Compression Layer** reduces payload size
5.  **Delivery Layer** streams to clients via WebSocket or SSE

**ASCII Diagram:**

```
API Source → Change Tracker → Batch Buffer → Processor → Compressor → Client
```

# Implementation Variants

# a) WebSocket Push

*   Persistent connection, avoids HTTP overhead
*   Ideal for high-frequency updates
*   Pair with **ping/pong heartbeats** for reliability

# b) Server-Sent Events (SSE)

*   One-way push over HTTP
*   Lightweight, simple for browser clients

# c) Hybrid (Batch + Push + Store)

*   Push latest batch to active clients
*   Store batch in queue (Kafka, Redis Streams) for late consumers

# Scaling Considerations

1.  **Horizontal Scaling** — Use a shared cache (Redis) for `last_seen` cursor across workers
2.  **Backpressure** — Drop or coalesce old events if consumer falls behind
3.  **Error Recovery** — On failure, fetch a larger range from API to avoid missing events

# Example: Real-Time Crypto Price Feed

Instead of hitting an exchange API every second for all pairs:

*   Store last-seen trade timestamp per pair
*   Fetch only trades after that timestamp
*   Batch updates into 3-second windows
*   Send compressed payload to all WebSocket subscribers

**Result:**

*   **70% less bandwidth**
*   **Lower CPU load** on both producer and consumer sides

# Final Takeaways

*   **Always fetch incrementally** — cursors/timestamps are your friend
*   **Batch intelligently** — micro-windows can massively cut costs
*   **Compress payloads** — it’s low-hanging fruit for optimization
*   Combine all three to **scale real-time APIs without scaling bandwidth bills**