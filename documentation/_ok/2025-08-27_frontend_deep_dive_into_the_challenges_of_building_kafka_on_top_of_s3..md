```yaml
---
title: Deep dive into the challenges of building Kafka on top of S3.
source: https://www.automq.com/blog/deep-dive-into-the-challenges-of-building-kafka-on-top-of-s3?utm_source=nikki_automq_vs_kafka
date_published: unknown
date_captured: 2025-08-27T13:13:21.840Z
domain: www.automq.com
author: Unknown
category: frontend
technologies: [Kafka, Apache Kafka, Amazon S3, AutoMQ, WarpStream, Bufstream, Redpanda Cloud Topics, Confluent Freight Clusters, AWS, GCP, EBS, ZooKeeper, Kraft, Apache Iceberg]
programming_languages: []
tags: [kafka, object-storage, cloud-native, streaming, distributed-systems, performance, cost-optimization, s3, data-storage, messaging]
key_concepts: [distributed-messaging, object-storage, cloud-native-architecture, kafka-compatibility, write-ahead-log, data-locality, cache-management, cost-optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article delves into the architectural challenges of building Kafka-compatible solutions on object storage like Amazon S3, driven by the need for cost efficiency and independent scaling in cloud environments. It addresses critical issues such as high latency, IOPS costs, efficient cache management, and complex metadata handling. Using AutoMQ as a primary example, the author explains how this open-source solution re-architects Kafka's storage layer with a Write-Ahead Log (WAL) and S3 approach. AutoMQ's strategies include intelligent data batching, multi-tier caching, leveraging Kraft for metadata, and an innovative mechanism to reduce cross-Availability Zone (AZ) traffic costs, all while maintaining 100% Kafka protocol compatibility. The article provides a comprehensive overview of how AutoMQ tackles these intricate distributed systems problems.]
---
```

# Deep dive into the challenges of building Kafka on top of S3.

![Blog post header image: Deep dive into the challenges of building Kafka on top of S3.](https://static-file-demo.automq.com/6809c9c3aaa66b13a5498262/68901dd5b4f6a8f89f739bae_67480fef30f9df5f84f31d36%252F68901dd4dd50d27e007b7fe8_2glivy2m.png)

AutoMQ Team

•

May 27, 2025

It’s really tough

![Diagram illustrating Kafka's traditional architecture with tightly coupled compute and storage.](https://static-file-demo.automq.com/6809c9c3aaa66b13a5498262/685e63cb032bcb896d06607d_67480fef30f9df5f84f31d36%252F685e63bab05359450e148d6e_z95l.webp)

## Intro

Since its open-source release, Kafka has become the de facto standard for distributed messaging. It has gone from operating only on LinkedIn to meeting growing log processing demands, now serving many companies worldwide for various use cases, including messaging, log aggregation, and stream processing.

However, it was designed at a time when local data centers were more widely adopted than cloud resources. Thus, there are challenges when operating Kafka on the cloud. Compute and storage can’t scale independently, or cross-availability-zone transfer fees due to data replication.

When searching for “Kafka alternative,” you can easily find four to five solutions that all promise to make your Kafka deployment cheaper and reduce the operational overhead. They can do this or implement that to make their offer more attractive. However, one thing you might observe over and over again is that they all try to store Kafka data completely in object storage.

This article won’t explore Kafka’s internal workings or why it is so popular. Instead, we will try to understand the challenges of building Kafka on top of S3.

## Background

But before we go further, let’s ask a simple question: “Why do they want to offload data to S3?“

The answer is cost-efficient.

In Kafka, compute and storage are tightly coupled, which means that scaling storage requires adding more machines, often leading to inefficient resource usage.

Kafka’s design also relies on replication for data durability. After storing messages, a leader must replicate data to followers. Because of the tightly coupled architecture, any change in cluster membership forces data to shift from one machine to another.

![Diagram showing Kafka's replication model, where a leader replicates data to followers, highlighting the tightly coupled nature.](https://static-file-demo.automq.com/6809c9c3aaa66b13a5498262/685e63cb032bcb896d0660a7_67480fef30f9df5f84f31d36%252F685e63bba37c4346fb897b8e_zcGt.png)

Another problem is cross-Availability Zone (AZ) transfer fees. Cloud vendors like AWS or GCP charge fees when we issue requests to different zones. Because producers can only produce messages to the partition leader, when deploying Kafka on the cloud, the producers must write to a leader in a different zone approximately two-thirds of the time (given a setup with three brokers). Kafka setup on the cloud can also incur significant cross-Availability Zone (AZ) transfer fees because the leader must replicate messages to followers in other zones.

![Diagram illustrating cross-Availability Zone (AZ) data transfer in Kafka, showing producers writing to a leader and the leader replicating to followers across AZs.](https://static-file-demo.automq.com/6809c9c3aaa66b13a5498262/685e63cb032bcb896d06606c_67480fef30f9df5f84f31d36%252F685e63bbb05359450e148e7f_60bS.webp)

Imagine you offload all the data to object storage like S3, you can:

*   Save storage money because object storage is cheaper than disk media.
*   Scale computing and storage independently.
*   Avoid data replication because the object storage will ensure data durability and availability.
*   Allow any broker to serve read and write
*   …

The trend of building a Kafka-compatible solution on object storage is emerging. At least five vendors have introduced a solution like that since 2023. We had WarpStream and AutoMQ in 2023, Confluent Freight Clusters, Bufstream, or Redpanda Cloud Topics in 2024.

Besides all the hype, I am curious about the challenges of building such a solution that uses S3 for the storage layer. To support this research, I chose [AutoMQ](https://github.com/AutoMQ/automq) because it’s the only open-source version. This allows me to dive deeper into understanding the challenges and solutions.

## Brief introduction of AutoMQ

AutoMQ is a 100% Kafka-compatible alternative solution. It is designed to run Kafka efficiently on the cloud by leveraging Kafka’s codebase for the protocol and rewriting the storage layer so it can effectively offload data to object storage with the introduction of the Write Ahead Log. For more details on AutoMQ, you can check [my previous article](https://open.substack.com/pub/vutr/p/how-do-we-run-kafka-100-on-the-object?r=2rj6sg&utm_campaign=post&utm_medium=web&showWelcomeOnShare=false).

Next, we will discuss the potential challenges of building Kafka on object storage and then see how AutoMQ overcomes them.