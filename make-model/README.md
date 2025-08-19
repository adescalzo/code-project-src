# Model Processor - AI-Powered Document Processing System

A modern .NET 9 application using C# 13 features for processing technical markdown documentation with RAG (Retrieval Augmented Generation) capabilities using PostgreSQL with pgvector.

## Features

- **Modern C# 13 Features**:
  - Primary constructors with field keyword
  - Collection expressions with spread operators
  - Source-generated regex for performance
  - Pattern matching enhancements
  - SearchValues for optimized text processing

- **AI/ML Capabilities**:
  - Document embedding generation using OpenAI
  - Vector similarity search with pgvector
  - RAG-based question answering
  - Hybrid search (vector + metadata filtering)

- **Database**:
  - PostgreSQL with pgvector extension
  - HNSW indexing for fast similarity search
  - JSONB storage for flexible metadata
  - Entity Framework Core 9 integration

## Prerequisites

1. **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **PostgreSQL 15+** with pgvector extension
3. **OpenAI API Key** for embeddings and chat completion

## Quick Start with Docker

### 1. Start PostgreSQL with pgvector

```bash
# Make the setup script executable and run it
chmod +x scripts/docker-setup.sh
./scripts/docker-setup.sh

# Or manually with docker-compose
docker-compose up -d
```

This will start:
- **PostgreSQL 16** with pgvector extension
- All required extensions and optimizations
- Ready for DBeaver connection

### 2. Configure Application

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=vectordb;Username=postgres;Password=postgres"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  }
}
```

Or use environment variables:

```bash
export ConnectionStrings__PostgreSQL="Host=localhost;Database=vectordb;Username=postgres;Password=postgres"
export OpenAI__ApiKey="your-api-key-here"
```

### 3. Build and Run

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

## Usage

The application provides an interactive console interface with the following options:

1. **📄 Process markdown documents** - Parse and embed markdown files
2. **🔍 Search documents** - Vector similarity search with optional filters
3. **💬 Ask a question (RAG)** - Get AI-powered answers from your documents
4. **📊 View statistics** - Database and document statistics
5. **🗑️ Clear database** - Remove all stored embeddings

### Processing Documents

Place your markdown files in the `examples` folder or specify a custom path. The processor will:
- Parse YAML frontmatter
- Extract code blocks
- Create content chunks
- Generate embeddings
- Store in PostgreSQL

### Document Format

Documents should follow this structure:

```yaml
---
title: Your Document Title
source: https://example.com
date_published: 2025-08-19
category: general
tags: [tag1, tag2]
technologies: [.NET, C#]
programming_languages: [C#]
summary: |
  Brief summary of the document
---

# Document Content

Your markdown content here...
```

## Architecture

### Models
- `DocumentMetadata` - C# 13 record with primary constructor
- `ProcessedChunk` - Document chunks with embeddings
- `DocumentEmbedding` - EF Core entity for database storage
- `SearchRequest` - Search parameters with params collections

### Services
- `MarkdownProcessor` - Parses and chunks markdown files
- `EmbeddingService` - Generates and stores embeddings
- `RagService` - Implements RAG pattern for Q&A
- `VectorDbContext` - EF Core context with pgvector support

### Key Technologies
- **.NET 9** with **C# 13** language features
- **Semantic Kernel** for AI orchestration
- **pgvector** for vector similarity search
- **Entity Framework Core 9** for data access
- **Spectre.Console** for rich console UI

## Performance Optimizations

- **SearchValues** for fast character searches
- **Source-generated regex** for improved pattern matching
- **HNSW indexing** for vector similarity
- **Batch processing** for API rate limits
- **Immutable collections** for thread safety
- **Parallel processing** for document parsing

## Database Schema

```sql
CREATE TABLE "DocumentEmbeddings" (
    "Id" uuid PRIMARY KEY,
    "DocumentId" text NOT NULL,
    "ChunkType" text NOT NULL,
    "Content" text NOT NULL,
    "Embedding" vector(1536) NOT NULL,
    "MetadataJson" jsonb NOT NULL,
    "Priority" integer NOT NULL,
    "CreatedAt" timestamp,
    "UpdatedAt" timestamp
);

-- Indexes
CREATE INDEX idx_document_id ON "DocumentEmbeddings"("DocumentId");
CREATE INDEX idx_chunk_type ON "DocumentEmbeddings"("ChunkType");
CREATE INDEX idx_embedding_hnsw ON "DocumentEmbeddings" 
  USING hnsw ("Embedding" vector_cosine_ops);
CREATE INDEX idx_metadata_gin ON "DocumentEmbeddings" 
  USING gin ("MetadataJson");
```

## Docker Commands

```bash
# Start PostgreSQL
./scripts/docker-setup.sh

# Stop container
./scripts/docker-setup.sh --down

# View logs
./scripts/docker-setup.sh --logs

# Clean everything (removes volumes!)
./scripts/docker-setup.sh --clean

# Test connection
./scripts/test-connection.sh
```

## Database Management

### DBeaver Connection
- **Host**: localhost
- **Port**: 5432
- **Database**: vectordb
- **Username**: postgres
- **Password**: postgres
- **Driver**: PostgreSQL

### Direct psql Access
```bash
docker exec -it postgres-vector-db psql -U postgres -d vectordb
```

### Connection String
```
Host=localhost;Port=5432;Database=vectordb;Username=postgres;Password=postgres
```

## Troubleshooting

### pgvector not found
The Docker setup automatically installs pgvector. If using manual installation:
```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

### Container won't start
```bash
# Check logs
docker-compose logs postgres-vector

# Reset everything
./scripts/docker-setup.sh --clean
./scripts/docker-setup.sh
```

### OpenAI rate limits
Adjust `BatchSize` and `RateLimitDelayMs` in `appsettings.json`

### Memory issues with large documents
Reduce chunk sizes in `MarkdownProcessor.CreateContentChunks()`

## License

MIT