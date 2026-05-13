# 4. Use PostgreSQL with pgvector for Relational and AI Knowledge Storage

Date: 2026-05-12

## Status

Accepted

## Context

QuizArena requires storing highly structured, relational data (User Profiles, Match Histories, Quiz Scores) as well as unstructured/semantic data for the AI pipeline (the "Knowledge Base" generated from class transcripts). Introducing a polyglot persistence architecture (e.g., SQL Server for relational + Pinecone/Milvus for vector embeddings + MongoDB for raw text) increases operational cost, deployment complexity, and cognitive load for a monolithic MVP.

## Alternatives Considered

1. **Polyglot Persistence (SQL Server + Qdrant/ChromaDB):** 
   - *Description:* Using a relational DB for the game state and a dedicated open-source vector DB for AI.
   - *Rejection Reason:* Introduces heavy operational complexity (managing two database servers, backups, security) and requires complex application-level logic to keep relational data and vector data synchronized.
2. **MongoDB with Atlas Vector Search:**
   - *Description:* Using a NoSQL document database that recently added vector capabilities.
   - *Rejection Reason:* True vector search is locked behind their managed Cloud tier (Atlas), which violates our zero-cost MVP constraint. Additionally, a document database is not ideal for the strict ACID transactional requirements of competitive, millisecond-precise game scores.
3. **Elasticsearch / OpenSearch:**
   - *Description:* Enterprise search engines with k-NN vector support.
   - *Rejection Reason:* Extremely high RAM consumption (JVM-based) and overly complex for our specific MVP needs. They are also not optimized as primary relational transaction stores.
4. **SQLite + sqlite-vss:**
   - *Description:* Embedded file database with vector support.
   - *Rejection Reason:* Cannot handle the high concurrency writes expected from 50+ students sending SignalR WebSocket payloads simultaneously (leads to database lock errors).

## Decision

We will use **PostgreSQL** as the single primary database engine, specifically leveraging the **`pgvector`** extension. 
- Relational domains (`Identity`, `Assessment`, `LiveArena`) will use standard SQL tables.
- The `AIKnowledgeBase` domain will store chunked transcripts and their vector embeddings using `pgvector` columns, allowing us to perform similarity searches (RAG) locally.

## Consequences

- **Positive:** Simplifies infrastructure (only one database container needed).
- **Positive:** Zero cost compared to managed vector databases like Pinecone.
- **Positive:** Allows joining relational data (e.g., "Find all embeddings for Class ID 5") in a single SQL query.
- **Negative:** `pgvector` index building can be CPU-intensive on the database server compared to purpose-built scalable vector databases.
