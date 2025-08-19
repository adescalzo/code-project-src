#!/bin/bash

# Test PostgreSQL connection and pgvector extension

echo "Testing PostgreSQL connection..."

# Test connection
docker exec postgres-vector-db psql -U postgres -d vectordb -c "SELECT version();"

echo ""
echo "Checking pgvector extension..."
docker exec postgres-vector-db psql -U postgres -d vectordb -c "SELECT extname, extversion FROM pg_extension WHERE extname = 'vector';"

echo ""
echo "Testing vector operations..."
docker exec postgres-vector-db psql -U postgres -d vectordb -c "SELECT '[1,2,3]'::vector <-> '[4,5,6]'::vector as distance;"

echo ""
echo "Listing available schemas..."
docker exec postgres-vector-db psql -U postgres -d vectordb -c "\dn"

echo ""
echo "Checking embeddings tables..."
docker exec postgres-vector-db psql -U postgres -d vectordb -c "\dt embeddings.*"

echo ""
echo "Connection test complete!"