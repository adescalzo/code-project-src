```yaml
---
title: Getting Started with Azure Service Bus Emulator
source: https://okyrylchuk.dev/blog/getting-started-with-azure-service-bus-emulator/
date_published: 2025-03-28T17:02:09.000Z
date_captured: 2025-08-11T16:15:43.073Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: devops
technologies: [Azure Service Bus Emulator, Azure Service Bus, Docker, Azure SQL Edge, .NET, TestContainers, Uno Platform, Microsoft]
programming_languages: [YAML, JSON, PowerShell, SQL, C#]
tags: [azure, service-bus, emulator, docker, local-development, messaging, queues, topics, .net, testing]
key_concepts: [local development, message queues, message topics, event-driven architecture, containerization, development workflow, connection strings, environment variables]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the newly released Azure Service Bus Emulator, a crucial tool for local development and testing of applications that interact with Azure Service Bus. It emphasizes the benefits of using the emulator, such as eliminating cloud costs and accelerating iteration cycles, by enabling developers to simulate Service Bus functionality locally via a Docker container. The guide provides clear, step-by-step instructions on how to set up and run the emulator using configuration files and Docker Compose, or an automated script. It also details the necessary connection string for local access and outlines the current limitations of the emulator. This tool significantly streamlines the development and debugging process for .NET applications leveraging Azure Service Bus.]
---
```

# Getting Started with Azure Service Bus Emulator

# Getting Started with Azure Service Bus Emulator

The Azure Service Bus Emulator is a game-changing tool that allows developers to test and debug their applications in a completely local environment, eliminating the need for an internet connection, cloud costs, or slow iteration cycles. If you’ve ever struggled with setting up mock environments or relied on third-party alternatives to simulate Service Bus behavior, this release is fresh air. Let’s dive into what the Azure Service Bus Emulator brings and how you can use it today!

## What is the Azure Service Bus Emulator?

The [Azure Service Bus Emulator](https://learn.microsoft.com/en-us/azure/service-bus-messaging/overview-emulator) is a local development tool that simulates Azure Service Bus functionality. It enables developers to send and receive messages, work with queues and topics, and test their event-driven architectures — all without needing a live Azure subscription.

Microsoft provides the emulator as a **Docker container**, making it easy to run on Windows, macOS, or Linux. This local-first approach significantly improves development speed and debugging capabilities while reducing cloud dependency.

## How to Run Azure Service Bus Emulator

### Docker

To start the emulator, you have to create three files:

*   **config.json**
*   **docker-compose.yaml**
*   **.env**

You define your Service Bus entities in the **config.json** file. The sample file is available on [GitHub](https://github.com/Azure/azure-service-bus-emulator-installer/blob/main/ServiceBus-Emulator/Config/Config.json).

```json
{
    "UserConfig": {
      "Namespaces": [
        {
          "Name": "sbemulatorns",
          "Queues": [
            {
              "Name": "queue.1",
              "Properties": {
                "DeadLetteringOnMessageExpiration": false,
                "DefaultMessageTimeToLive": "PT1H",
                "DuplicateDetectionHistoryTimeWindow": "PT20S",
                "ForwardDeadLetteredMessagesTo": "",
                "ForwardTo": "",
                "LockDuration": "PT1M",
                "MaxDeliveryCount": 3,
                "RequiresDuplicateDetection": false,
                "RequiresSession": false
              }
            }
          ],
   
          "Topics": [
            {
              "Name": "topic.1",
              "Properties": {
                "DefaultMessageTimeToLive": "PT1H",
                "DuplicateDetectionHistoryTimeWindow": "PT20S",
                "RequiresDuplicateDetection": false
              },
              "Subscriptions": [
                {
                  "Name": "subscription.1",
                  "Properties": {
                    "DeadLetteringOnMessageExpiration": false,
                    "DefaultMessageTimeToLive": "PT1H",
                    "LockDuration": "PT1M",
                    "MaxDeliveryCount": 3,
                    "ForwardDeadLetteredMessagesTo": "",
                    "ForwardTo": "",
                    "RequiresSession": false
                  },
                  "Rules": [
                    {
                      "Name": "app-prop-filter-1",
                      "Properties": {
                        "FilterType": "Correlation",
                        "CorrelationFilter": {
                       "ContentType": "application/text",
                       "CorrelationId": "id1",
                       "Label": "subject1",
                       "MessageId": "msgid1",
                       "ReplyTo": "someQueue",
                       "ReplyToSessionId": "sessionId",
                       "SessionId": "session1",
                       "To": "xyz"
                     }
                      }
                    }
                  ]
                },
                {
                  "Name": "subscription.2",
                  "Properties": {
                    "DeadLetteringOnMessageExpiration": false,
                    "DefaultMessageTimeToLive": "PT1H",
                    "LockDuration": "PT1M",
                    "MaxDeliveryCount": 3,
                    "ForwardDeadLetteredMessagesTo": "",
                    "ForwardTo": "",
                    "RequiresSession": false
                  },
                  "Rules": [
                    {
                      "Name": "user-prop-filter-1",
                      "Properties": {
                        "FilterType": "Correlation",
                        "CorrelationFilter": {
                          "Properties": {
                            "prop1": "value1"
                          }
                        }
                      }
                    }
                  ]
                }
              ]
            }
          ]
        }
      ],
      "Logging": {
        "Type": "File"
      }
    }
}
```

To spin up containers for the Service Bus emulator, save the following .yaml file as _**docker-compose.yaml**_:

```yaml
name: microsoft-azure-servicebus-emulator
services:
  emulator:
    container_name: "servicebus-emulator"
    image: mcr.microsoft.com/azure-messaging/servicebus-emulator:latest
    pull_policy: always
    volumes:
      - "${CONFIG_PATH}:/ServiceBus_Emulator/ConfigFiles/Config.json"
    ports:
      - "5672:5672"
      - "5300:5300"
    environment:
      SQL_SERVER: sqledge
      MSSQL_SA_PASSWORD: "${SQL_PASSWORD}"  # Password should be same as what is set for SQL Edge  
      ACCEPT_EULA: ${ACCEPT_EULA}
      SQL_WAIT_INTERVAL: ${SQL_WAIT_INTERVAL} # Optional: Time in seconds to wait for SQL to be ready (default is 15 seconds)
    depends_on:
      - sqledge
    networks:
      sb-emulator:
        aliases:
          - "sb-emulator"
  sqledge:
        container_name: "sqledge"
        image: "mcr.microsoft.com/azure-sql-edge:latest"
        networks:
          sb-emulator:
            aliases:
              - "sqledge"
        environment:
          ACCEPT_EULA: ${ACCEPT_EULA}
          MSSQL_SA_PASSWORD: "${SQL_PASSWORD}"

networks:
  sb-emulator:
```

As you can see, it spins up two containers:

*   servicebus-emulator
*   azure-sql-edge

The Emulator’s default port is 5672. If you want to change it, make sure you change it everywhere in the .yaml file.

The .yaml file contains three variables. Let’s create a **.env** file to define those environment variables:

1.  **CONFIG_PATH** is a path to your **config.json** file.
2.  **ACCEPT_EULA** – put **Y** to accept license terms.
3.  **MSSQL_SA_PASSWORD –** the password for **azure-sql-edge** container. Create your password following [these policies](https://learn.microsoft.com/en-us/sql/relational-databases/security/strong-passwords?view=sql-server-linux-ver16).

The .env file must be in the same folder as a docker-compose file.

```
# Environment file for user defined variables in docker-compose.yml

# 1. CONFIG_PATH: Path to Config.json file
CONFIG_PATH="C:\\Config\\Config.json"

# 2. ACCEPT_EULA: Pass 'Y' to accept license terms for Azure SQL Edge and Azure Service Bus emulator.
# Service Bus emulator EULA : https://github.com/Azure/azure-service-bus-emulator-installer/blob/main/EMULATOR_EULA.txt
# SQL Edge EULA : https://go.microsoft.com/fwlink/?linkid=2139274
ACCEPT_EULA="Y"

# 3. MSSQL_SA_PASSWORD to be filled by user as per policy
MSSQL_SA_PASSWORD="YourPasswordHere!"
```

Then, you need to run a docker-compose command:

```bash
docker compose -f <pathtodockercomposefile> up -d
```

And you’ll see the running containers:

![](https://okyrylchuk.dev/wp-content/uploads/2025/03/docker-1024x166.avif "Screenshot of Docker Desktop showing running containers: microsoft-azure-servicebus-emulator, sqledge, and servicebus-emulator, along with their status and resource usage.")

### Automated script

There is a second option when you don’t need to create files. You can run the automated script to spin up the service bus emulator.

Clone the [Service Bus Emulator Installer](https://github.com/Azure/azure-service-bus-emulator-installer) repository.

Go to the [Scripts](https://github.com/Azure/azure-service-bus-emulator-installer/tree/main/ServiceBus-Emulator/Scripts) folder, where you can find automated scripts for different operating systems.

For example, for Windows run **./Launchemulator.ps1** script.

You’ll be asked to accept the End User License Agreement and provide the SQL password.

## Connection String

To connect to the Service Bus Emulator on the local machine, use the following connection string:

```
"Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"
```

The important is the **UseDevelopmentEmulator=true** setting.

## Limitations

Of course, the Service Bus Emulator is not a real Service Bus service. It has some limitations:

*   It can’t stream messages by using the JMS protocol.
*   Partitioned entities aren’t compatible with Emulator.
*   It doesn’t support on-the-fly management operations through a client-side SDK.
*   It doesn’t support Cloud features like autoscale or geo-disaster recovery capabilities, etc.
*   It has a limit of 1 namespace and 50 queues/topics.

More limitations can be found in the [Overview of the Azure Service Bus Emulator](https://learn.microsoft.com/en-us/azure/service-bus-messaging/overview-emulator#known-limitations).

## Conclusions

Microsoft has finally delivered a tool that .NET developers have been requesting for years. The Azure Service Bus Emulator **eliminates unnecessary cloud costs, speeds up development, and makes local testing easier than ever**.

By the way, if you use TestContainers, there is already [Azure Service Bus module](https://testcontainers.com/modules/azure-servicebus/?language=dotnet) there.

So, what are you waiting for? **Pull the image, start the emulator, and level up your .NET development workflow today!** 🚀