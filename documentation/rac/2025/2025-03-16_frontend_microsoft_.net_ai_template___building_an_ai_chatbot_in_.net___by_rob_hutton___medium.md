```yaml
---
title: "Microsoft .NET AI Template — Building an AI Chatbot in .NET | by Rob Hutton | Medium"
source: https://medium.com/@robhutton8/microsoft-net-ai-template-building-an-ai-chatbot-in-net-94a28f018155
date_published: 2025-03-16T14:02:33.517Z
date_captured: 2025-08-06T17:52:05.386Z
domain: medium.com
author: Rob Hutton
category: frontend
technologies: [.NET 9, Microsoft.Extensions.AI.Templates, Azure OpenAI, OpenAI API, GitHub Copilot, Ollama, Blazor WebAssembly, Razor Pages, Azure AI Search, Visual Studio, GitHub]
programming_languages: [C#, Markdown, XML]
tags: [ai, chatbot, dotnet, rag, blazor, github-copilot, ai-templates, document-processing, vector-search, azure-ai]
key_concepts: [ai-powered-applications, chatbot-development, retrieval-augmented-generation, vector-search, document-ingestion, ai-embeddings, system-prompt, personal-access-token, user-secrets]
code_examples: true
difficulty_level: intermediate
summary: |
  This article introduces `Microsoft.Extensions.AI.Templates`, a new .NET project template designed to simplify the creation of AI-driven chatbots. It details how to set up an AI chatbot using this template, specifically leveraging GitHub Copilot models for AI capabilities. The guide covers key features like Retrieval-Augmented Generation (RAG) for document processing, support for various AI providers, and a pre-built Blazor WebAssembly UI. Step-by-step instructions are provided for project creation, configuration, and customization of the AI's behavior via system prompts. The template offers a scalable foundation for integrating AI into .NET applications, supporting both local and cloud deployments.
---
```

# Microsoft .NET AI Template — Building an AI Chatbot in .NET | by Rob Hutton | Medium

# Microsoft .NET AI Template — Building an AI Chatbot in .NET

## `Microsoft.Extensions.AI.Templates`

![Diagram showing the .NET AI Chat Web App Template with icons for .NET, a chat bubble, and a sparkle, indicating AI capabilities.](https://miro.medium.com/v2/resize:fit:700/1*JG81-kLD0c4BSiXFiiKmpQ.png)

The rise of **AI-powered applications** has transformed the way developers build intelligent solutions. Whether it’s **chatbots, virtual assistants, or AI-enhanced search**, integrating AI into applications is now more accessible than ever.

To simplify AI development in .NET, **Microsoft introduced** `**Microsoft.Extensions.AI.Templates**`, a powerful new project template that enables developers to quickly build AI-driven chatbots. This template comes with **pre-built AI capabilities**, document processing, and support for **multiple AI providers**, including **Azure OpenAI, OpenAI API, GitHub Copilot, and local AI models (Ollama)**.

In this guide, we’ll walk through setting up an **AI chatbot in .NET** using the **GitHub Copilot model**, exploring its **key features** and **step-by-step implementation**.

# Introducing the Microsoft.Extensions.AI.Templates

The `Microsoft.Extensions.AI.Templates` provides a ready-to-use chatbot that can do the following:

*   **Process custom documents** (PDFs, text) and extract meaningful information.
*   **Use AI models** such as Azure OpenAI, OpenAI API, GitHub Copilot, or local models.
*   **Retrieve contextual answers** with citations for accuracy.
*   **Be easily extended** for additional AI integrations, UI enhancements, and new features.

# Key Features

## AI-Powered Chatbot with Retrieval-Augmented Generation (RAG)

*   Retrieves relevant content from **PDFs** before sending queries to an AI model.
*   Uses **vector search** for enhanced accuracy.
*   Provides **citation-based responses** for transparency.

## Supports Multiple AI Providers

The chatbot works with various AI models. In this example, we’ll use **GitHub Copilot models**, as they offer a **quick and simple setup**.

*   **Azure OpenAI** (GPT-4, GPT-3.5)
*   **OpenAI API**
*   **GitHub Copilot models**
*   **Ollama** (for running AI models locally)

## Document Ingestion & Vector Storage

*   Converts **PDF content** into **searchable AI embeddings**.
*   Uses a **local vector store** by default, but can integrate with **Azure AI Search** for enterprise-level indexing.

## Pre-Built Chat UI

*   Comes with a **Blazor WebAssembly / Razor Pages UI**.
*   Provides a **chat interface** with **real-time AI interactions** and **follow-up suggestions**.

## Ready for Cloud & Local Deployment

*   Run **locally** with an **embedded vector database**.
*   Deploy to **Azure** with **AI Search** for scalable solutions.

# Step-by-Step Implementation: AI Chatbot using Microsoft.Extensions.AI.Templates & GitHub Copilot

Let’s walk through the process of **setting up an AI chatbot** using this new template and **GitHub Copilot models** as the AI provider.

## Step 1️⃣: Create a Personal Access Token (PAT) in GitHub

1.  If you don’t have a GitHub account, create one at [GitHub](https://github.com/).
2.  Navigate to **Profile → Settings → Developer Settings**.
3.  Under **Developer Settings**, click **Personal Access Tokens → Fine-grained Tokens**.
4.  Generate a new token and **store it securely** for your AI project.

## Step 2️⃣: Install the AI Template

Ensure you have **Visual Studio 17.12** or later installed to support **.NET 9.0**.

Install the AI template using the following command:

```bash
dotnet new install Microsoft.Extensions.AI.Templates
```

## Step 3️⃣: Create an AI Chatbot Project

In **Visual Studio**, choose **Create a new project** and select the **AI Chat Web App** template.

![Screenshot of Visual Studio's "Create a new project" dialog, highlighting the "AI Chat Web App" template.](https://miro.medium.com/v2/resize:fit:700/1*5HjkJrzAuctwBwISt5fPCQ.png)

Alternatively, use this command:

```bash
dotnet new aichatweb -o MyAIChatApp
```

When prompted for the **AI service provider**, select **GitHub Models**.

![Screenshot of Visual Studio's "Additional information" dialog for the AI Chat Web App, showing a dropdown list with "GitHub Models" selected as the AI service provider.](https://miro.medium.com/v2/resize:fit:700/1*d8wqWgsil8ZpcMtVRQrBmw.png)

## Step 4️⃣: Configure Your Personal Access Token as a Secret

Depending on your Visual Studio version, configure the **GitHub PAT (Personal Access Token)** as a **user secret**:

**Option 1: Using Visual Studio**

1.  Right-click on your project and select **Manage User Secrets**.
2.  Add the following JSON:

```json
{  
  "GitHubModels:Token":  "YOUR-TOKEN"  
}
```

**Option 2: Using the Command Line**

```bash
cd <<your-project-directory>>  
dotnet user-secrets set GitHubModels:Token YOUR-TOKEN
```

## Step 5️⃣: Run the Application

Next, we can run the application and see what we get out of the box. You can also add your own PDF files to the project **data** folder and **train** the AI with your own unique data (product, service, etc.)

![Screenshot of the running AI Chat Web App, showing a chat interface with a question "how do I survive?" and an AI assistant's response with multiple points and citations.](https://miro.medium.com/v2/resize:fit:700/1*aPVlguFcPsw_hoxihrwIyQ.png)

## Step 6️⃣: Add Custom Content

To fully experience the AI Chatbot’s capabilities, try adding your own PDF files to the data folder for custom knowledge retrieval.

![Screenshot of Visual Studio's Solution Explorer, highlighting the 'Data' folder within the project structure, showing existing PDF files and a new placeholder 'My-Product-Or-Service.pdf'.](https://miro.medium.com/v2/resize:fit:568/1*wfzWZ-W_P0rkBuZhuIRaAw.png)

Next, update the **Chat.razor** file to reference the document for retrieval.

![Screenshot of the Chat.razor file in Visual Studio, showing C# code that references a PDF document for AI retrieval.](https://miro.medium.com/v2/resize:fit:700/1*h_42N3E4-bCHRzWZJY5LXw.png)

## Step 7️⃣: Modify the System Prompt

The `Chat.razor` page includes a **SystemPrompt**, which acts as the AI’s **guiding instruction set**. This prompt allows you to **train** the AI by defining its behavior, setting boundaries, and ensuring it retrieves and cites information strictly from the provided PDF documents. The AI is instructed to **only answer questions based on retrieved data**, format responses using simple Markdown, and provide **exact citations** in a structured XML format.

```csharp
private const string SystemPrompt = @"  
    You are an assistant who answers questions about information you retrieve.  
    Do not answer questions about anything else.  
    Use only simple markdown to format your responses.  
  
    Use the search tool to find relevant information. When you do this, end your  
    reply with citations in the special XML format:  
  
    <citation filename='string' page\_number='number'\>exact quote here</citation>  
  
    Always include the citation in your response if there are results.  
  
    The quote must be max 5 words, taken word-for\-word from the search result, and is the basis for why the citation is relevant.  
    Don't refer to the presence of citations; just emit these tags right at the end, with no surrounding text.  
    ";
```

**Example of a Change to the Prompt:**

You could modify the prompt to **allow more natural explanations** while still requiring citations. For instance, if you want the chatbot to summarize content in addition to citing sources, you might adjust the prompt like this:

```csharp
private const string SystemPrompt = @"  
    You are an assistant specializing in answering questions based on retrieved information.    
    If relevant data is found, summarize it in your own words while keeping the meaning intact.    
    Use simple markdown for formatting.    
  
    Always include a citation in this XML format at the end of your response:    
  
    <citation filename='string' page\_number='number'>exact quote here</citation>    
  
    The quote must be word-for-word, up to 10 words long, taken directly from the search result.    
    Don't explicitly mention that you are using citations; just include them at the end.  
";
```

**Impact of This Adjustment**

*   **Allows AI to summarize content instead of just quoting directly.**
*   **Increases the quote length to 10 words** for better context.
*   **Keeps citations mandatory** while making responses feel more natural.

The **Microsoft.Extensions.AI.Templates** project provides a streamlined way to build AI-powered chat applications using .NET and Blazor. This template offers a **Blazor WebAssembly / Razor Pages UI**, making it easy to integrate interactive AI experiences into web applications.

At its core, the chatbot utilizes **Retrieval-Augmented Generation (RAG)** to improve response accuracy by referencing **PDF documents stored in the project’s** `**data**` **folder**. When a user submits a query, the system **extracts relevant information from these PDFs**, transforms it into **vector embeddings**, and retrieves the most contextually appropriate content before sending the query to an AI model like **GitHub Copilot**, **Azure OpenAI**, or **Ollama**. This ensures that responses are not only AI-generated but also grounded in real, source-backed data.

With built-in support for multiple AI providers, **cloud and local deployment**, and **a pre-configured chat UI**, this template serves as a **powerful foundation for developers** looking to integrate AI-driven document search, chatbot interactions, and knowledge retrieval into their .NET applications. Whether you’re building an internal knowledge assistant, customer support bot, or research tool, this project offers **a flexible and scalable** starting point.

By following the **step-by-step guide**, you now have a working AI chatbot running on **Blazor and .NET 9**, processing documents, and responding with contextual answers. From here, you can extend the project by **customizing the UI**, **adding additional document types**, or **integrating with external data sources** to enhance functionality even further.