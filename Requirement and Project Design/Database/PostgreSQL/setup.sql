-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Document embeddings table
CREATE TABLE document_embeddings (
    id SERIAL PRIMARY KEY,
    document_id INT NOT NULL UNIQUE,
    chunk_text TEXT NOT NULL,
    embedding vector(384), -- nomic-embed-text dimensions
    metadata JSONB,
    user_access_list INT[],
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Create index for fast similarity search
CREATE INDEX document_embeddings_embedding_idx 
ON document_embeddings 
USING ivfflat (embedding vector_cosine_ops)
WITH (lists = 100);

-- Index for user access filtering
CREATE INDEX document_embeddings_access_idx 
ON document_embeddings 
USING GIN (user_access_list);

-- Chat sessions (for future Phase 2)
CREATE TABLE chat_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Chat messages (for future Phase 2)
CREATE TABLE chat_messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id UUID NOT NULL REFERENCES chat_sessions(id) ON DELETE CASCADE,
    role VARCHAR(20) NOT NULL, -- 'user' or 'assistant'
    content TEXT NOT NULL,
    metadata JSONB,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX chat_messages_session_idx ON chat_messages(session_id);
