```yaml
---
title: "Observability and Distributed Tracing Terminology Guide | Michael's Coding Spot"
source: https://michaelscodingspot.com/observability-dictionary/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=what-s-new-in-c-13&_bhlid=a5556f5b3933b297f84c37d1296adb0855971751
date_published: unknown
date_captured: 2025-08-08T16:17:40.901Z
domain: michaelscodingspot.com
author: Unknown
category: frontend
technologies: [Datadog, New Relic, AppDynamics, Dynatrace, Azure Monitor, Splunk, Loki, Elastic Stack, Coralogix, Obics, Prometheus, OpenTelemetry, OpenTracing, OpenCensus, Jaeger, Zipkin, Grafana, Elasticsearch, InfluxDB, BigQuery, Redshift, Kubernetes, Pingdom, HTTP, JSON]
programming_languages: [SQL]
tags: [observability, distributed-tracing, telemetry, monitoring, logging, metrics, apm, performance, cloud-native, system-health]
key_concepts: [telemetry, observability, application-performance-monitoring, distributed-tracing, logs-and-metrics, open-standards, sampling, monitoring-strategies]
code_examples: false
difficulty_level: intermediate
summary: |
  This article serves as a comprehensive terminology guide for observability and distributed tracing, aimed at newcomers to the field. It defines essential concepts such as telemetry, logs, metrics, traces, and spans, explaining their roles in understanding the internal state of deployed systems. The guide also introduces various Application Performance Monitoring (APM) tools and open-source standards like OpenTelemetry, clarifying their functions. Furthermore, it details different types of logs, monitoring approaches like Real User Monitoring (RUM) and Synthetic Monitoring, and key performance indicators including SLIs, SLOs, and SLAs. The resource effectively demystifies the complex language associated with modern system monitoring.
---
```

# Observability and Distributed Tracing Terminology Guide | Michael's Coding Spot

# Observability and Distributed Tracing Terminology Guide

[Observability](../categories/observability) | [Logging](../tags/logging) , [Performance](../tags/performance) / Nov 20, 2024 ![](../img/posts2024/observability-tracing-dictionary.jpg "A stylized illustration of a closed book with a dark green cover and a red spine. The cover features a vibrant scene with a telescope on a tripod, looking up at a starry sky with a moon, against a background of colorful horizontal stripes representing a horizon. The book has a red bookmark ribbon.")

If you’re new to the observability space, it’s easy to get lost in a sea of terms. What are APMs, traces, spans, telemetry, and metrics? Are Jaeger and Zipkin actual words? What’s the difference between OpenTelemetry and OpenTracing? Let’s try to make sense of all this language. Let me present a complete dictionary of observability and distributed tracing terminology.

*   **Telemetry** refers to the collection of signals that your applications, services, and infrastructure send to observability backends. Telemetry might include logs (e.g., “MyFunction started”), metrics (e.g., “CPU is at 90%” or “Database transaction finished in 5ms”), traces (e.g., request and transaction lifecycles), profiling information (e.g., CPU stacks), or events (e.g., a user clicked a button).
*   **Observability** is an umbrella term for understanding what happens inside deployed services and applications. It’s a methodology for using telemetry signals to get insights about your app. For example, looking at log files to find recent crashes on your server. Or looking at metrics to figure out request durations. It can also mean _automatic_ insights, like AI-driven anomaly detection and alerts when something goes wrong.
*   **Application Performance Monitoring (APM)** refers to observability suites that help engineers understand the health of deployed systems. They use telemetry signals to analyze and visualize application and infrastructure performance. They are the tools that enable _observability_ for your system. For example, an APM can display hosts and services in a flowchart showing interactions between them. It can also show how many requests are sent from Service A to Service B, the latency of those requests, and their error rate. APM tools monitor not only applications and services but also databases, message brokers, containers, Kubernetes clusters, operating systems, and essentially any deployed system. In addition to the traditional APM role of monitoring system health, modern tools offer log management, security monitoring, and more. Some popular APMs are [Datadog](https://datadoghq.com/) , [New Relic](https://newrelic.com/) , [AppDynamics](https://www.appdynamics.com/) , [Dynatrace](https://www.dynatrace.com/) , and [Azure Monitor](https://azure.microsoft.com/en-us/products/monitor) .
*   **Logs** are time-stamped records that a system sends, usually to a file or a centralized backend. Logs might be **structured** (a set of fields in JSON format) or **unstructured** (plain text). Logs come from various sources, including:
    *   **Application Logs** are messages that software developers include (aka “instrument”) in applications or services for debugging purposes.
    *   **System Logs** are generated by the operating system and include events such as hardware changes.
    *   **Access Logs** are generated by web servers and include requests made to the server, such as IP addresses, for example.
    *   **Audit Logs** record user activity and data changes, which might be necessary for compliance and forensic analysis.
    *   **Authentication Logs** include login attempts and access control events.
    *   **Database Logs** contain information on transactions and queries from databases.

Logs are often sent by **agents in a sidecar pattern**, where there’s a process (agent) deployed alongside your service, and its job is to gather telemetry and send it to a backend.

Log monitoring solutions usually provide centralized data storage for logs and allow users to find, query, and visualize log data. Some notable ones are [Splunk](https://www.splunk.com/) , [Loki](https://github.com/grafana/loki) , [Elastic Stack](https://www.elastic.co/observability/log-monitoring) , [Coralogix](https://coralogix.com/) , and [Obics](https://obics.io/) .

*   **Metrics** are measurements that track the performance and health of systems. These might include CPU utilization, memory consumption, I/O usage, request rates, etc. A popular tool for gathering and querying metrics is [Prometheus](https://prometheus.io/) .
    
*   **Distributed Tracing** is a method for tracking and observing requests as they flow through multiple services in a distributed architecture. Distributed tracing usually consists of records of **traces** and **spans**. The most popular distributed tracing protocol is **OpenTelemetry**, though many APM vendors use their own proprietary protocols.
    
*   In Distributed Tracing, **Traces** are records of the execution lifecycle of a request or a transaction across a distributed system. A trace consists of multiple **spans**.
    
*   **Span** is a single unit of work within a trace. It represents an operation, like an HTTP request or a database query in a specific service. Each trace consists of multiple spans.
    
*   **Parent Span** or **root span** refers to the main span of a trace, representing the initial operation that triggers other operations (spans) within the trace.
    
*   A **child span** is a span that originates from another span.
    
*   **Trace ID**: A unique identifier for a trace that allows it to be tracked across services. All spans within the same trace share this ID.
    
*   **Span ID**: A unique identifier for each span within a trace.
    
*   **OpenTelemetry** is an open-source standard and a set of tools to collect telemetry data (traces, metrics, and logs) from applications and distributed systems. Above all, it is a protocol that defines how telemetry data should be sent. It also includes **instrumentation libraries** for different languages and runtimes, as well as **deployable agents** that can be installed on host machines to collect, process, and send telemetry data to an observability backend.
    
*   **OpenTracing** is a deprecated distributed tracing protocol. It was merged into **OpenTelemetry** along with **OpenCensus**. Instead of two competing protocols, they merged into one. This decision paid off for the industry, as OpenTelemetry has gained significant popularity since then.
    
*   **Jaeger** is an open-source observability platform that visualizes and aids in the analysis of distributed tracing data. As of 2022, it relies on OpenTelemetry collections as its tracing input and is maintained by the Cloud Native Computing Foundation (CNCF).
    
*   **Zipkin** is another system for analyzing and visualizing distributed tracing data, similar to Jaeger, though simpler and with fewer features. It supports OpenTelemetry data points but still maintains its own set of instrumentation libraries.
    
*   **Prometheus** is an open-source toolkit for collecting and storing metrics telemetry from cloud environments. Its primary function is to gather telemetry from applications and services in pull mode. Applications and services expose an API for Prometheus to access metrics. Prometheus stores metrics as time-series data, which may include request counts, CPU utilization, memory usage, or custom metrics you can instrument and send. Prometheus includes a query language to retrieve data, and it is often combined with Grafana for data visualization. It also has exporters to forward metrics to other backends, such as APM vendors. Prometheus is frequently used with Kubernetes due to its ability to automatically discover K8s containers, pods, and services.
    
*   **Grafana** is a popular open-source data visualization tool. It allows users to create custom dashboards with a wide variety of charts, graphs, heatmaps, and tables. It works well with Elasticsearch, Prometheus, Azure Monitor, InfluxDB, and nearly any notable data source.
    
*   **Instrumentation** is the process of adding code that produces telemetry for observability. It might be manual or automatic.
    
*   **Error Rate** is the frequency of errors in a system, often represented as a percentage of total requests.
    
*   **Root Cause Analysis** is the process of identifying the core reason for a problem.
    
*   **Time Series Data** refers to data points recorded at successive time intervals to observe changes over time.
    
*   **Event Correlation**: Analyzing and linking related events to understand complex system behaviors. Most observability platforms don’t offer OLAP capabilities that support correlation using an SQL-like JOIN (Azure Monitor and Obics.io are exceptions). Instead, companies can export telemetry to a data warehouse (e.g., BigQuery or Redshift) for event correlation.
    
*   **Mean Time to Repair (MTTR)** is the average time to repair a failed component.
    
*   **Mean Time Between Failures (MTBF)** is the predicted time between failures of a system during operation.
    
*   **Availability** is the proportion of time a system is operational and accessible when needed.
    
*   **Reliability** is the probability that a system will function without failure for a specified period.
    
*   **Service Level Indicators (SLIs)** are metrics that quantify aspects of the service level, such as latency, throughput, or error rates.
    
*   **Service Level Objectives (SLOs)** are target values or ranges for SLIs that define acceptable service performance.
    
*   **Service Level Agreements (SLAs)** are contracts between service providers and customers outlining expected performance levels (and consequences for failing to meet them).
    
*   **Sampling** is the process of collecting a subset of data (such as traces or logs) to reduce overhead while still providing statistical insights. In distributed systems, there are two main sampling strategies: **head-based sampling**, where the sampling decision is made at the initial request (or root), and **tail-based sampling**, where the decision is made after the request has completed. Head-based sampling is generally more efficient and widely used, while tail-based sampling allows for intelligent decisions, such as recording all errors or requests with higher latency.
    
*   **Context Propagation** is the process of passing context through different services in distributed systems to maintain observability. For example, a request passing through multiple services will include a Trace ID in the request headers to correlate logs and metrics with the same request.
    
*   **Ingesting** is the process of sending data to a data store. In observability, the data consists of telemetry signals, and the data store could be an APM backend like Datadog, a log management solution like Elastic Stack, or a traditional database.
    
*   **SIEM system**, or **Security Information and Event Management**, is a type of cybersecurity solution that works with telemetry signals. A SIEM system collects, analyzes, and correlates log and telemetry events, providing real-time threat detection, incident response, and security analytics.
    
*   **Synthetic monitoring** is a proactive observability technique that uses automated scripts to simulate user interactions with an application or service. It tests uptime, performance, and functionality by mimicking user behavior across servers, or websites. These scripts might include simple pings to a website to make sure it is up, API calls to measure request latency and correctness, or more complex user interactions that might include logging into a site and interacting with a page. Popular synthetics platforms include New Relic Synthetics, Dynatrace Synthetic Monitoring, Datadog Synthetics, Pingdom, and Grafana Synthetic Monitoring.
    
*   **Real User Monitoring (RUM)** is an observability technique that tracks real users' interactions with an application or website. It collects performance data such as user navigation, clicks, load times, and errors to provide insights into the actual user experience. This helps identify issues under real-world conditions.
    
*   **Session replay** is a service that records and visually reproduces real user interactions with a website or application, including clicks, scrolls, and navigation. It helps teams understand user behavior, debug issues, and improve the user experience by providing a detailed view of how users interact with the application. In the larger umbrella of observability it falls under the category of **Real User Monitoring (RUM)**.
    

---

Can you think of any other terms that might be relevant? Post a comment and I’ll add it. Cheers.

 [![](../img/share/twitter-share.png)](https://twitter.com/intent/tweet?text=Observability%20and%20Distributed%20Tracing%20Terminology%20Guide&url=https%3a%2f%2fmichaelscodingspot.com%2fobservability-dictionary%2f)[![](../img/share/facebook-share.png) ](https://www.facebook.com/sharer/sharer.php?u=https%3a%2f%2fmichaelscodingspot.com%2fobservability-dictionary%2f)[![](../img/share/linkedin-share.png)](https://www.linkedin.com/sharing/share-offsite/?url=https%3a%2f%2fmichaelscodingspot.com%2fobservability-dictionary%2f)

[My book "Practical Debugging for .NET Developers" is now Free! →](../free-book/)

Lock Thread

Login

Add Comment

Comment anonymously

**M ↓**   Markdown

UpvotesNewestOldest

[Commento](https://commento.io)