```yaml
---
title: "Building Three Pipelines to Select the Right LLMs for RAG, Multi-Agent Systems, and Vision | by Fareed Khan | Aug, 2025 | Level Up Coding"
source: https://levelup.gitconnected.com/building-three-pipelines-to-select-the-right-llms-for-rag-multi-agent-systems-and-vision-3e47e0563b76
date_published: 2025-08-22T02:38:46.931Z
date_captured: 2025-08-22T11:20:39.429Z
domain: levelup.gitconnected.com
author: Fareed Khan
category: frontend
technologies: [RAG, Multi-Agent Systems, Vision Models, Jupyter Notebook, OpenAI Python Library, pypdf, requests, pandas, pydantic, json, re, dataclasses, tqdm, BytesIO, IPython.display, Llama 3.1 8B, Llama 3.3 70B, DeepSeek-V3, Qwen3-4B-fast, Qwen3-235B-A22B, Qwen3-14B, Gemma 3 27B-IT, Nebius AI, Ollama, Together AI, GitHub]
programming_languages: [Python]
tags: [llm-evaluation, rag, multi-agent-systems, vision-ai, ai-pipelines, model-selection, cost-optimization, performance-metrics, agentic-ai, python]
key_concepts: [LLM evaluation, Agentic RAG, Multi-agent systems, Vision with reasoning, Role-based LLM selection, LLM-as-Judge, Tool-use (LLMs), Performance metrics (cost, latency, quality)]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores building and evaluating three distinct AI pipelines for selecting optimal LLMs: Agentic RAG for deep document analysis, a Multi-Agent System for scientific research, and Vision with Reasoning for processing poorly scanned forms. It advocates for a strategy of using multiple LLMs of varying sizes and specialties, assigning each to a specific role to optimize for cost and performance. The evaluation framework emphasizes role-specific testing, real-world scenarios, and holistic measurement, including qualitative scores (faithfulness, relevance) and operational metrics (cost, latency). Each pipeline demonstrates how to orchestrate different models for tasks like routing, synthesis, critique, safety checks, and OCR, with a strong focus on automated quality assurance and human-in-the-loop processes. The findings highlight that no single LLM is universally best, advocating for a "right model for the right task" approach to build efficient and reliable AI systems.
---
```

# Building Three Pipelines to Select the Right LLMs for RAG, Multi-Agent Systems, and Vision | by Fareed Khan | Aug, 2025 | Level Up Coding

# Building Three Pipelines to Select the Right LLMs for RAG, Multi-Agent Systems, and Vision

## Agentic RAG, Multi-Agent and Vision with Reasoning

![LLM Decider Pipeline](https://miro.medium.com/v2/resize:fit:6616/1*2eDMUUbuMEHxEENkWSRuaA.png)
*Description: A diagram illustrating the "LLM Decider Pipeline" which outlines the three main evaluation strategies: Agentic RAG, Multi-Agent System, and Vision with Reasoning, leading to evaluation metrics and selection of best-performing LLMs.*

In 2025, the most advanced systems no longer rely on a single large, general-purpose model. Instead, they use multiple LLMs of different sizes and specialties, each handling a specific task like a team of experts working together on a complex problem.

However, properly evaluating an LLM within its specific role in such an architecture is very challenging, **since it needs to be tested directly in the context where it is used**.

We are going to build three distinct, production-grade AI pipelines and use them as testbeds to measure how different models perform in their assigned roles. Our evaluation framework is built on three core principles:

1.  **Role-Specific Testing:** We evaluate models based on the specific task they are assigned within the pipeline, whether it’s a fast router, a deep reasoner, or a precise judge.
2.  **Real-World Scenarios:** Instead of stopping at abstract tests, we are going to run our models through complete, end-to-end workflows to see how they perform under realistic conditions.
3.  **Holistic Measurement:** We are going to measure everything that matters, from operational metrics like cost and latency to qualitative scores like faithfulness, relevance, and scientific validity.

All the code (theory + 3 notebooks) is available in my GitHub repository:

[
## GitHub - FareedKhan-dev/best-llm-finder-pipeline: Agentic RAG, Multi-Agent Systems, and Vision…

### Agentic RAG, Multi-Agent Systems, and Vision Reasoning are three pipelines to find the perfect LLM …

github.com

](https://github.com/FareedKhan-dev/best-llm-finder-pipeline?source=post_page-----3e47e0563b76---------------------------------------)

Our codebase is organized as follows:

```
best-llm-finder-pipeline/
├── 01_agentic_RAG.ipynb        # Agentic RAG
├── 02_multi_agent_system.ipynb # Multi-agent system
├── 03_vision_reasoning.ipynb   # Vision reasoning
├── README.md                   # Project docs
└── requirements.txt            # Dependencies
```

> Our evaluation strategy for choosing the right LLM for each component is divided into three real-world cases.

## Agentic RAG for Deep Document Analysis

*   [Setting up the RAG Environment](#setting-up-the-rag-environment)
*   [Preprocessing our Dataset](#preprocessing-our-dataset)
*   [Creating the Two-Pass Router Agent](#creating-the-two-pass-router-agent)
*   [Building the Recursive Navigator](#building-the-recursive-navigator)
*   [Executing the Full Navigation Process](#executing-the-full-navigation-process)
*   [Synthesizer Agent for Citable Answers](#synthesizer-agent-for-citable-answers)
*   [LLM-as-Judge (Faithfulness, Retrieval, Relevance) Evaluation](#llm-as-judge-faithfulness-retrieval-relevance-evaluation)
*   [Cost and Confidence Evaluation](#cost-and-confidence-evaluation)
*   [Evaluation with Confidence Score](#evaluation-with-confidence-score)

## Multi-Agent System for Scientific Research

*   [Configuring the Co-Scientist and Mocking Lab Tools](#configuring-the-co-scientist-and-mocking-lab-tools)
*   [Building the Core Agent Runner](#building-the-core-agent-runner)
*   [Running the Parallel Ideation Agents](#running-the-parallel-ideation-agents)
*   [Implementing the Tournament Ranking Agent](#implementing-the-tournament-ranking-agent)
*   [Deep Critique and Safety Check Agents](#deep-critique-and-safety-check-agents)
*   [Quality Scoring Evaluation](#quality-scoring-evaluation)
*   [Human in the Loop Reviewer Eval](#human-in-the-loop-reviewer-eval)
*   [Evaluation of Final Components](#evaluation-of-final-components)

## Vision with Reasoning of Poorly Scanned Forms

*   [Setting Up the Two-Stage Component](#setting-up-the-two-stage-component)
*   [High-Fidelity OCR with a Vision Model](#high-fidelity-ocr-with-a-vision-model)
*   [Refinement with a Tool-Using Reasoning Model](#refinement-with-a-tool-using-reasoning-model)
*   [Analyzing the Results](#analyzing-the-results)

## Analyzing Our Findings

## Key Takeaways

# Agentic RAG for Deep Document Analysis

RAG systems based on agents are the new popular LLM-based solution in AI, especially for tackling the challenge of making sense of massive, unstructured documents.

![Agentic RAG Architecture](https://miro.medium.com/v2/resize:fit:1000/1*0qpfiC_1KUziAF1X0Qcfbw.png)
*Description: A diagram showing the architecture of an Agentic RAG system, where LLMs are used for routing, retrieval, synthesis, and evaluation within a document analysis workflow.*

In this kind of architecture, LLMs are used in different parts to make the entire system work (**The correct way to evaluate LLMs**), from routing and retrieval to synthesis and evaluation.

> We’ll avoid pre-indexing (like embedding-based RAG) and instead let an agent read documents on the fly, testing the LLM’s depth and awareness.

![Agentic RAG Pipeline Flowchart](https://miro.medium.com/v2/resize:fit:1000/1*RFpPHhKtg_Omoz1yGmRMCA.png)
*Description: A flowchart illustrating the Agentic RAG Pipeline, starting with semantic chunking of a document, routing by an agent, recursive navigation, synthesis, and evaluation to produce a response.*

1.  We start by loading a huge document and splitting it into a small number of large, high-level chunks. This creates a top-level map for the agent to navigate, avoiding a time-consuming pre-embedding step.
2.  Then we create a **Router Agent**, based on a small and fast LLM, that skims these large chunks to identify the most relevant sections.
3.  The system repeatedly takes the selected sections, splits them into smaller sub-chunks, and re-routes. This **zooming in** process allows the agent to progressively focus on the most promising information.
4.  Once the most relevant paragraphs are isolated, we use a powerful **Synthesizer Agent** to craft a detailed answer, ensuring every statement includes a precise citation back to its source.
5.  Finally, we implement a **Evaluation Agent** that acts as an impartial judge, scoring the final answer for faithfulness and relevance to provide a quantitative confidence score along with other evaluation metrices.

Let’s start building this pipeline.

## Setting up the RAG Environment

Before we start coding, we need to import the necessary libraries that will be useful throughout this RAG pipeline.

```python
import os                      # Interact with the operating system
import json                    # Work with JSON data
import re                      # Regular expression operations
import time                    # Time-related functions
from io import BytesIO         # Handle in-memory binary streams
from openai import OpenAI      # The OpenAI client library
from pypdf import PdfReader    # For reading and extracting text from PDF files
from typing import List, Dict, Any # Type hints for code clarity
```

We will be using open-source LLMs through an OpenAI-compatible API provider. I am using [Nebius AI](https://nebius.ai/), but you can easily switch to a local provider like [Ollama](https://ollama.com/) or another cloud service like [Together AI](https://www.together.ai/).

Since I don’t have a powerful local GPU, a cloud provider is the best way to access a range of models (They do offer free credits). Let’s initialize our API client.

```python
# --- LLM Configuration ---
# Replace with your API key and the base URL of your LLM provider
API_KEY = "YOUR_API_KEY" # Replace with your actual key
BASE_URL = "https://api.studio.nebius.com/v1/" # Or your Ollama/local host

# Initialize the OpenAI client to connect to the custom endpoint
client = OpenAI(
    api_key=API_KEY,
    base_url=BASE_URL,
)

# We will store performance metrics here throughout the run
metrics_log = []
```

We have also initialized an empty list, `metrics_log`, which will be important for our evaluation phase, as it will store detailed performance data for every LLM call we make.

> A key part of our strategy is using different models for different tasks.

A generic agentic RAG pipeline is often composed of at least three AI-driven components that we can optimize for cost and performance by choosing the right model for each.

*   **Routing:** This task involves skimming large amounts of text to find relevant sections. A smaller, faster model (like an 8B model) is often sufficient and highly cost-effective here.
*   **Synthesis:** This task requires deep understanding and the ability to combine information from multiple sources into a coherent answer. A larger, more powerful model (like a 70B model) is a better choice.
*   **Evaluation:** To reliably judge the quality of other models, we need a highly capable “judge” model. Using a top-tier model for this final check ensures our quality scores are trustworthy.

```python
# Define the models to be used for different tasks
ROUTER_MODEL = "meta-llama/Meta-Llama-3.1-8B-Instruct"  # A fast model for routing
SYNTHESIS_MODEL = "meta-llama/Llama-3.3-70B-Instruct"   # A powerful model for synthesis
EVALUATION_MODEL = "deepseek-ai/DeepSeek-V3"       # A top-tier model for evaluation
```

Now that we have initialized the core components, we can move on to loading and preprocessing our data.

## Preprocessing our Dataset

To properly evaluate our models, we need a challenging dataset. For this, we will use the [**Trademark Trial and Appeal Board Manual of Procedure (TBMP)**](https://www.uspto.gov/sites/default/files/documents/tbmp-Master-June2024.pdf), a dense, 920-page legal document. It’s freely available to download, so let’s write a function to fetch it and extract its text.

![Pre-processing Step](https://miro.medium.com/v2/resize:fit:1000/1*cgAXqVblN5hfGw5rGgVXIg.png)
*Description: A diagram depicting the pre-processing step for the RAG pipeline, showing a PDF document being downloaded, text extracted, and then split into high-level chunks.*

```python
import requests # Import requests for downloading the PDF

# URL for the TBMP manual
tbmp_url = "https://www.uspto.gov/sites/default/files/documents/tbmp-Master-June2024.pdf"

# Maximum number of pages to process
max_pages = 920
document_text = ""

# Download the PDF file from the URL
response = requests.get(tbmp_url)
response.raise_for_status()  # Check if the download was successful

# Read the PDF content from the response
pdf_file = BytesIO(response.content)
pdf_reader = PdfReader(pdf_file)

# Determine the number of pages to process
num_pages_to_process = min(max_pages, len(pdf_reader.pages))

full_text = ""
# Loop through pages
for page in pdf_reader.pages[:num_pages_to_process]:
    # Extract text from the current page
    page_text = page.extract_text()
    if page_text:
        # Append page text to the full document string
        full_text += page_text + "\n"

document_text = full_text

# Mock count_tokens and sent_tokenize for demonstration if not defined elsewhere
def count_tokens(text):
    # Simple token count approximation for demonstration
    return len(text.split())

import nltk
from nltk.tokenize import sent_tokenize
try:
    nltk.data.find('tokenizers/punkt')
except nltk.downloader.DownloadError:
    nltk.download('punkt')

# Display document statistics
char_count = len(document_text)
token_count = count_tokens(document_text)
print(f"\nDocument loaded successfully.")
print(f"- Total Characters: {char_count:,}")
print(f"- Estimated Tokens: {token_count:,}")

# Show a preview of the extracted text
print("\n--- Document Preview (first 500 characters) ---")
print(document_text[:500])
```

The document is far too large to fit into a single model context. Instead of creating hundreds of small, independent chunks for a vector database, we will create a small number of large, high-level chunks (e.g., 20). This forms the top level of our document hierarchy.

```python
from tqdm import tqdm # Import tqdm for progress bars

def split_text_into_chunks(text: str, num_chunks: int = 20) -> List[Dict[str, Any]]:
    """
    Splits a long text into a specified number of chunks, respecting sentence boundaries.
    """
    # First, split the entire text into individual sentences
    sentences = sent_tokenize(text)
    if not sentences:
        return []

    # Calculate how many sentences should go into each chunk on average
    sentences_per_chunk = (len(sentences) + num_chunks - 1) // num_chunks

    chunks = [] # Initialize list to hold the chunks
    # Set progress bar description, only for long texts
    desc = "Creating chunks" if len(sentences) > 500 else None
    # Iterate over the sentences list in steps of sentences_per_chunk
    for i in tqdm(range(0, len(sentences), sentences_per_chunk), desc=desc):
        # Slice the sentences list to get the sentences for the current chunk
        chunk_sentences = sentences[i:i + sentences_per_chunk]
        # Join the sentences back into a single string
        chunk_text = " ".join(chunk_sentences)
        # Append the new chunk as a dictionary to the list
        chunks.append({
            "id": len(chunks),  # Assign a simple integer ID
            "text": chunk_text
        })

    # Print a confirmation message with the number of chunks created
    print(f"Document split into {len(chunks)} chunks.")
    return chunks
```

We are going for chunking that is designed to be sentence-aware, which means that it will try to avoid splitting sentences in the middle. This helps preserve the semantic integrity of the text.

Let’s run this chunking on our document and check the different chunks along with the total number of tokens.

```python
# Split the document text into a specified number of chunks.
document_chunks = split_text_into_chunks(document_text, num_chunks=20)

# Display stats for the first few chunks.
for chunk in document_chunks[:3]:
    # Count the tokens in the current chunk's text.
    chunk_token_count = count_tokens(chunk['text'])