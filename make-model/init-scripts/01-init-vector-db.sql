-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
CREATE EXTENSION IF NOT EXISTS pg_trgm; -- For text similarity search
CREATE EXTENSION IF NOT EXISTS btree_gin; -- For composite GIN indexes
CREATE EXTENSION IF NOT EXISTS btree_gist; -- For composite GIST indexes

-- Create schema for better organization
CREATE SCHEMA IF NOT EXISTS embeddings;

-- Set default search path
SET search_path TO embeddings, public;

-- Create optimized table for document embeddings
CREATE TABLE IF NOT EXISTS embeddings."DocumentEmbeddings" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DocumentId" TEXT NOT NULL,
    "ChunkType" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "Embedding" vector(1536) NOT NULL, -- OpenAI embeddings dimension
    "MetadataJson" JSONB NOT NULL,
    "Priority" INTEGER NOT NULL DEFAULT 2,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    "SimilarityScore" REAL -- Used for query results
);

-- Create indexes for optimal performance
-- HNSW index for vector similarity search (best for production)
CREATE INDEX IF NOT EXISTS idx_embedding_hnsw 
ON embeddings."DocumentEmbeddings" 
USING hnsw ("Embedding" vector_cosine_ops)
WITH (m = 16, ef_construction = 64);

-- Alternative IVFFlat index (commented out, use if HNSW is not available)
-- CREATE INDEX IF NOT EXISTS idx_embedding_ivfflat 
-- ON embeddings."DocumentEmbeddings" 
-- USING ivfflat ("Embedding" vector_cosine_ops)
-- WITH (lists = 100);

-- B-tree indexes for filtering
CREATE INDEX IF NOT EXISTS idx_document_id 
ON embeddings."DocumentEmbeddings"("DocumentId");

CREATE INDEX IF NOT EXISTS idx_chunk_type 
ON embeddings."DocumentEmbeddings"("ChunkType");

CREATE INDEX IF NOT EXISTS idx_priority 
ON embeddings."DocumentEmbeddings"("Priority");

CREATE INDEX IF NOT EXISTS idx_created_at 
ON embeddings."DocumentEmbeddings"("CreatedAt" DESC);

-- GIN index for JSONB queries
CREATE INDEX IF NOT EXISTS idx_metadata_gin 
ON embeddings."DocumentEmbeddings" 
USING gin ("MetadataJson");

-- Composite index for common query patterns
CREATE INDEX IF NOT EXISTS idx_type_priority_created 
ON embeddings."DocumentEmbeddings"("ChunkType", "Priority", "CreatedAt" DESC);

-- Text search index on content
CREATE INDEX IF NOT EXISTS idx_content_trgm 
ON embeddings."DocumentEmbeddings" 
USING gin ("Content" gin_trgm_ops);

-- Function for similarity search with metadata filtering
CREATE OR REPLACE FUNCTION embeddings.search_similar_documents(
    query_embedding vector(1536),
    max_results INTEGER DEFAULT 10,
    min_similarity REAL DEFAULT 0.7,
    filter_category TEXT DEFAULT NULL,
    filter_tags TEXT[] DEFAULT NULL
)
RETURNS TABLE (
    id UUID,
    document_id TEXT,
    chunk_type TEXT,
    content TEXT,
    metadata_json JSONB,
    similarity REAL
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        d."Id",
        d."DocumentId",
        d."ChunkType",
        d."Content",
        d."MetadataJson",
        1 - (d."Embedding" <=> query_embedding) AS similarity
    FROM embeddings."DocumentEmbeddings" d
    WHERE 
        1 - (d."Embedding" <=> query_embedding) >= min_similarity
        AND (filter_category IS NULL OR d."MetadataJson"->>'category' = filter_category)
        AND (filter_tags IS NULL OR d."MetadataJson"->'tags' ?| filter_tags)
    ORDER BY d."Embedding" <=> query_embedding
    LIMIT max_results;
END;
$$;

-- Function for hybrid search (vector + text)
CREATE OR REPLACE FUNCTION embeddings.hybrid_search(
    query_embedding vector(1536),
    query_text TEXT,
    max_results INTEGER DEFAULT 10,
    vector_weight REAL DEFAULT 0.7,
    text_weight REAL DEFAULT 0.3
)
RETURNS TABLE (
    id UUID,
    document_id TEXT,
    chunk_type TEXT,
    content TEXT,
    metadata_json JSONB,
    combined_score REAL
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    WITH vector_search AS (
        SELECT 
            d."Id",
            1 - (d."Embedding" <=> query_embedding) AS vector_score
        FROM embeddings."DocumentEmbeddings" d
        ORDER BY d."Embedding" <=> query_embedding
        LIMIT max_results * 2
    ),
    text_search AS (
        SELECT 
            d."Id",
            similarity(d."Content", query_text) AS text_score
        FROM embeddings."DocumentEmbeddings" d
        WHERE d."Content" % query_text -- Trigram similarity
        ORDER BY text_score DESC
        LIMIT max_results * 2
    ),
    combined AS (
        SELECT 
            COALESCE(v."Id", t."Id") AS id,
            COALESCE(v.vector_score * vector_weight, 0) + 
            COALESCE(t.text_score * text_weight, 0) AS score
        FROM vector_search v
        FULL OUTER JOIN text_search t ON v."Id" = t."Id"
    )
    SELECT 
        d."Id",
        d."DocumentId",
        d."ChunkType",
        d."Content",
        d."MetadataJson",
        c.score AS combined_score
    FROM combined c
    JOIN embeddings."DocumentEmbeddings" d ON d."Id" = c.id
    ORDER BY c.score DESC
    LIMIT max_results;
END;
$$;

-- Create statistics table for monitoring
CREATE TABLE IF NOT EXISTS embeddings."Statistics" (
    "Id" SERIAL PRIMARY KEY,
    "TotalDocuments" INTEGER DEFAULT 0,
    "TotalEmbeddings" INTEGER DEFAULT 0,
    "LastProcessedAt" TIMESTAMP WITH TIME ZONE,
    "AverageEmbeddingTime" REAL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Initial statistics row
INSERT INTO embeddings."Statistics" ("TotalDocuments", "TotalEmbeddings")
VALUES (0, 0)
ON CONFLICT DO NOTHING;

-- Trigger to update statistics
CREATE OR REPLACE FUNCTION embeddings.update_statistics()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE embeddings."Statistics"
    SET 
        "TotalEmbeddings" = (SELECT COUNT(*) FROM embeddings."DocumentEmbeddings"),
        "TotalDocuments" = (SELECT COUNT(DISTINCT "DocumentId") FROM embeddings."DocumentEmbeddings"),
        "UpdatedAt" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_statistics
AFTER INSERT OR DELETE ON embeddings."DocumentEmbeddings"
FOR EACH STATEMENT
EXECUTE FUNCTION embeddings.update_statistics();

-- Grant permissions (adjust as needed)
GRANT ALL ON SCHEMA embeddings TO postgres;
GRANT ALL ON ALL TABLES IN SCHEMA embeddings TO postgres;
GRANT ALL ON ALL SEQUENCES IN SCHEMA embeddings TO postgres;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA embeddings TO postgres;

-- Optimize PostgreSQL settings for vector operations
ALTER SYSTEM SET max_parallel_workers_per_gather = 4;
ALTER SYSTEM SET max_parallel_workers = 8;
ALTER SYSTEM SET shared_buffers = '256MB';
ALTER SYSTEM SET effective_cache_size = '1GB';
ALTER SYSTEM SET maintenance_work_mem = '128MB';
ALTER SYSTEM SET work_mem = '8MB';

-- Reload configuration
SELECT pg_reload_conf();

-- Verify extensions are installed
DO $$
BEGIN
    RAISE NOTICE 'Installed extensions:';
    RAISE NOTICE 'vector version: %', (SELECT extversion FROM pg_extension WHERE extname = 'vector');
    RAISE NOTICE 'Database initialization complete!';
END $$;