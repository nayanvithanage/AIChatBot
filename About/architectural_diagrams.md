# InEight AI Chatbot - Architectural Diagrams

This document provides visual representations of the system architecture at various levels of detail.

---

## 1. High-Level System Architecture

```mermaid
graph TB
    subgraph "User Layer"
        User["👤 DMS User"]
    end
    
    subgraph "Presentation Layer"
        DMS["InEight DMS<br/>(ASP.NET MVC 5)"]
        Widget["React Chat Widget<br/>(TypeScript + Vite)"]
    end
    
    subgraph "Application Layer"
        API["Chatbot API<br/>(.NET 8 Web API)"]
        Sync["MetadataSync<br/>Service"]
        RAG["RAG<br/>Orchestrator"]
    end
    
    subgraph "AI Layer"
        Ollama["Ollama Service<br/>LLM: llama3.2:1b<br/>Embedding: nomic-embed-text"]
    end
    
    subgraph "Data Layer"
        DMSDB[("DMS Database<br/>(SQL Server)")]
        VectorDB[("Vector Database<br/>(PostgreSQL + pgvector)")]
    end
    
    User -->|Login & Browse| DMS
    DMS -->|Embed Widget| Widget
    DMS -->|Generate JWT| Widget
    Widget -->|Chat Query + JWT| API
    API -->|Process Query| RAG
    RAG -->|Generate Embedding| Ollama
    RAG -->|Vector Search| VectorDB
    RAG -->|Generate Response| Ollama
    Sync -->|Read Metadata| DMSDB
    Sync -->|Generate Embeddings| Ollama
    Sync -->|Store Vectors| VectorDB
    
    style User fill:#e1f5ff
    style DMS fill:#fff4e6
    style Widget fill:#e8f5e9
    style API fill:#f3e5f5
    style Ollama fill:#fce4ec
    style VectorDB fill:#e0f2f1
    style DMSDB fill:#e0f2f1
```

---

## 2. Component Architecture

### 2.1 Solution Structure

```mermaid
graph LR
    subgraph "InEightAIChatbot Solution"
        Core["InEightAIChatbot.Core<br/>(Interfaces & Models)"]
        Infra["InEightAIChatbot.Infrastructure<br/>(Provider Implementations)"]
        Service["InEightAIChatbot.Service<br/>(API & Services)"]
        Widget["chat-widget<br/>(React + TypeScript)"]
    end
    
    Service -->|References| Core
    Service -->|References| Infra
    Infra -->|References| Core
    
    style Core fill:#e3f2fd
    style Infra fill:#f3e5f5
    style Service fill:#e8f5e9
    style Widget fill:#fff3e0
```

### 2.2 Core Layer Components

```mermaid
classDiagram
    class IAIProvider {
        <<interface>>
        +GenerateCompletionAsync()
        +GenerateEmbeddingAsync()
        +ProviderName
        +EmbeddingDimensions
    }
    
    class IVectorSearchProvider {
        <<interface>>
        +IndexDocumentAsync()
        +SearchAsync()
        +DeleteDocumentAsync()
        +ProviderName
    }
    
    class ChatRequest {
        +Query: string
        +ProjectId: int?
        +SessionId: string?
    }
    
    class ChatResponse {
        +Answer: string
        +Links: ChatLink[]
        +Confidence: float
        +FallbackKBLink: string?
    }
    
    class SearchResult {
        +DocumentId: int
        +Text: string
        +Metadata: Dictionary
        +Distance: float
    }
```

### 2.3 Infrastructure Layer Components

```mermaid
classDiagram
    class OllamaProvider {
        +GenerateCompletionAsync()
        +GenerateEmbeddingAsync()
        -HttpClient
        -OllamaSettings
    }
    
    class PgVectorProvider {
        +IndexDocumentAsync()
        +SearchAsync()
        +DeleteDocumentAsync()
        -NpgsqlDataSource
    }
    
    class AzureOpenAIProvider {
        +GenerateCompletionAsync()
        +GenerateEmbeddingAsync()
        -OpenAIClient
    }
    
    class AzureAISearchProvider {
        +IndexDocumentAsync()
        +SearchAsync()
        +DeleteDocumentAsync()
        -SearchClient
    }
    
    IAIProvider <|.. OllamaProvider
    IAIProvider <|.. AzureOpenAIProvider
    IVectorSearchProvider <|.. PgVectorProvider
    IVectorSearchProvider <|.. AzureAISearchProvider
    
    OllamaProvider : MVP Implementation
    PgVectorProvider : MVP Implementation
    AzureOpenAIProvider : Future Phase 2
    AzureAISearchProvider : Future Phase 2
```

---

## 3. Data Flow Diagrams

### 3.1 Document Synchronization Flow

```mermaid
sequenceDiagram
    participant Sync as MetadataSync Service
    participant DMS as DMS Database
    participant Ollama as Ollama
    participant PG as PostgreSQL

    Note over Sync: Background Service<br/>Runs every 4 hours
    
    Sync->>DMS: Query Documents<br/>(SELECT with JOINs)
    DMS-->>Sync: Document Metadata
    
    loop For each document
        Sync->>Sync: Format Metadata<br/>to Text
        Sync->>DMS: Get Accessible<br/>User IDs
        DMS-->>Sync: User ID List
        Sync->>Ollama: Generate Embedding<br/>(nomic-embed-text)
        Ollama-->>Sync: 768-dim Vector
        Sync->>PG: UPSERT Document<br/>(embedding + metadata)
        PG-->>Sync: Success
    end
    
    Note over Sync: Sync Complete<br/>3 documents indexed
```

### 3.2 Chat Query Processing Flow

```mermaid
sequenceDiagram
    participant User as User
    participant Widget as Chat Widget
    participant API as Chatbot API
    participant RAG as RAG Orchestrator
    participant Ollama as Ollama
    participant PG as PostgreSQL

    User->>Widget: Type Query
    Widget->>API: POST /api/chat/message<br/>(Query + JWT)
    API->>API: Validate JWT<br/>Extract User ID
    API->>RAG: ProcessQueryAsync<br/>(Query, User ID)
    
    RAG->>Ollama: Generate Query<br/>Embedding
    Ollama-->>RAG: 768-dim Vector
    
    RAG->>PG: Vector Similarity<br/>Search (user-filtered)
    PG-->>RAG: Top 10 Documents
    
    alt Documents Found
        RAG->>RAG: Build Context<br/>from Results
        RAG->>Ollama: Generate Response<br/>(System + User Prompt)
        Ollama-->>RAG: AI Answer
        RAG->>RAG: Calculate<br/>Confidence
        RAG->>RAG: Build Document<br/>Links
        RAG-->>API: ChatResponse
    else No Documents
        RAG-->>API: "No documents found"<br/>+ Fallback Link
    end
    
    API-->>Widget: JSON Response
    Widget-->>User: Display Answer<br/>+ Document Links
```

---

## 4. Deployment Architecture

### 4.1 Development Environment

```mermaid
graph TB
    subgraph "Developer Machine"
        subgraph "Docker Desktop"
            PG["PostgreSQL<br/>Port 5433"]
        end
        
        subgraph "Local Services"
            Ollama["Ollama<br/>Port 11434"]
            API["Chatbot API<br/>Port 5169"]
        end
        
        subgraph "Visual Studio"
            DMS["DMS Web<br/>Port 52291"]
        end
        
        subgraph "VS Code / Terminal"
            Widget["React Widget<br/>Port 5173<br/>(Dev Mode)"]
        end
        
        LocalDB[("SQL Server<br/>LocalDB")]
    end
    
    API -->|Read Metadata| LocalDB
    API -->|Store Vectors| PG
    API -->|AI Requests| Ollama
    DMS -->|Embed| Widget
    Widget -->|API Calls| API
    
    style PG fill:#e0f2f1
    style Ollama fill:#fce4ec
    style API fill:#f3e5f5
    style DMS fill:#fff4e6
    style Widget fill:#e8f5e9
```

### 4.2 Production Architecture (Future)

```mermaid
graph TB
    subgraph "Azure Cloud"
        subgraph "App Services"
            API["Chatbot API<br/>(Azure App Service)"]
            DMS["DMS Web<br/>(Azure App Service)"]
        end
        
        subgraph "AI Services"
            OpenAI["Azure OpenAI<br/>(GPT-4o)"]
            Search["Azure AI Search<br/>(Vector Search)"]
        end
        
        subgraph "Data Services"
            SQL["Azure SQL<br/>Database"]
            KeyVault["Azure Key Vault<br/>(Secrets)"]
        end
        
        subgraph "Monitoring"
            AppInsights["Application<br/>Insights"]
            LogAnalytics["Log Analytics"]
        end
    end
    
    Users["👥 Users"] -->|HTTPS| DMS
    DMS -->|Embed Widget| DMS
    DMS -->|API Calls| API
    API -->|Read Metadata| SQL
    API -->|Vector Search| Search
    API -->|AI Requests| OpenAI
    API -->|Get Secrets| KeyVault
    API -->|Telemetry| AppInsights
    DMS -->|Telemetry| AppInsights
    AppInsights -->|Logs| LogAnalytics
    
    style API fill:#f3e5f5
    style DMS fill:#fff4e6
    style OpenAI fill:#fce4ec
    style Search fill:#e1f5ff
    style SQL fill:#e0f2f1
```

---

## 5. Security Architecture

### 5.1 Authentication Flow

```mermaid
sequenceDiagram
    participant User as User
    participant DMS as DMS
    participant JWT as JwtHelper
    participant Widget as Chat Widget
    participant API as Chatbot API

    User->>DMS: Login<br/>(Username + Password)
    DMS->>DMS: Validate Credentials<br/>(ASP.NET Identity)
    DMS->>DMS: Create Session
    
    Note over DMS,JWT: Generate JWT Token
    DMS->>JWT: GenerateToken<br/>(UserID, Name, Role)
    JWT->>JWT: Create Claims
    JWT->>JWT: Sign with Secret
    JWT-->>DMS: JWT Token
    
    DMS->>Widget: Embed Widget<br/>+ JWT Token
    Widget->>Widget: Store Token<br/>(window.config)
    
    User->>Widget: Send Chat Query
    Widget->>API: POST /api/chat/message<br/>Authorization: Bearer {JWT}
    
    API->>API: Validate Token<br/>(Signature + Expiry)
    API->>API: Extract User ID<br/>from Claims
    API->>API: Process Query<br/>(User-Filtered)
    API-->>Widget: Response
    Widget-->>User: Display Answer
```

### 5.2 Access Control Model

```mermaid
graph TB
    subgraph "User Access Control"
        User["User<br/>(ID: 2)"]
        Projects["Assigned Projects<br/>(Project 1, 2)"]
        Docs["Accessible Documents<br/>(Docs in Projects 1, 2)"]
    end
    
    subgraph "Database Layer"
        UserTable[("AspNetUsers<br/>Table")]
        ProjectUsers[("ProjectUsers<br/>Table")]
        Documents[("Documents<br/>Table")]
        Embeddings[("document_embeddings<br/>user_access_list")]
    end
    
    User -->|Member Of| Projects
    Projects -->|Contains| Docs
    
    UserTable -->|user_id| ProjectUsers
    ProjectUsers -->|project_id| Documents
    Documents -->|document_id| Embeddings
    Embeddings -->|Filters by| User
    
    style User fill:#e1f5ff
    style Projects fill:#fff4e6
    style Docs fill:#e8f5e9
    style Embeddings fill:#f3e5f5
```

---

## 6. Database Schema Diagrams

### 6.1 PostgreSQL Vector Database

```mermaid
erDiagram
    DOCUMENT_EMBEDDINGS {
        int document_id PK
        text chunk_text
        vector_768 embedding
        jsonb metadata
        int_array user_access_list
        timestamp updated_at
    }
    
    CHAT_SESSIONS {
        uuid id PK
        int user_id
        text title
        timestamp created_at
    }
    
    CHAT_MESSAGES {
        uuid id PK
        uuid session_id FK
        text role
        text content
        jsonb metadata
        timestamp timestamp
    }
    
    CHAT_SESSIONS ||--o{ CHAT_MESSAGES : contains
```

### 6.2 DMS Database (Relevant Tables)

```mermaid
erDiagram
    ASPNETUSERS {
        int Id PK
        string Name
        string Email
        int Role
    }
    
    PROJECTS {
        int Id PK
        string Name
        int ManagerId FK
    }
    
    DOCUMENTS {
        int Id PK
        string Name
        string Description
        int ProjectId FK
        int UploadedById FK
        int Status
    }
    
    PROJECTUSERS {
        int ProjectId FK
        int UserId FK
    }
    
    ASPNETUSERS ||--o{ PROJECTS : manages
    ASPNETUSERS ||--o{ DOCUMENTS : uploads
    ASPNETUSERS ||--o{ PROJECTUSERS : assigned_to
    PROJECTS ||--o{ DOCUMENTS : contains
    PROJECTS ||--o{ PROJECTUSERS : has_members
```

---

## 7. Component Interaction Diagrams

### 7.1 RAG Pipeline Components

```mermaid
graph LR
    subgraph "RAG Orchestrator"
        Input["User Query"]
        Embed["Embedding<br/>Generation"]
        Search["Vector<br/>Search"]
        Context["Context<br/>Building"]
        LLM["LLM<br/>Generation"]
        Response["Response<br/>Assembly"]
    end
    
    Input --> Embed
    Embed --> Search
    Search --> Context
    Context --> LLM
    LLM --> Response
    
    Embed -.->|Uses| OllamaEmbed["Ollama<br/>Embedding API"]
    Search -.->|Queries| PG["PostgreSQL<br/>pgvector"]
    LLM -.->|Uses| OllamaLLM["Ollama<br/>Generate API"]
    
    style Input fill:#e1f5ff
    style Response fill:#e8f5e9
    style Embed fill:#fff3e0
    style Search fill:#f3e5f5
    style Context fill:#fce4ec
    style LLM fill:#e0f2f1
```

### 7.2 Provider Abstraction Pattern

```mermaid
graph TB
    subgraph "Application Layer"
        RAG["RAG Orchestrator"]
        Sync["Metadata Sync"]
    end
    
    subgraph "Abstraction Layer"
        IAI["IAIProvider<br/>Interface"]
        IVector["IVectorSearchProvider<br/>Interface"]
    end
    
    subgraph "MVP Implementations"
        Ollama["OllamaProvider"]
        PgVector["PgVectorProvider"]
    end
    
    subgraph "Future Implementations"
        Azure["AzureOpenAIProvider"]
        AISearch["AzureAISearchProvider"]
    end
    
    RAG -->|Uses| IAI
    RAG -->|Uses| IVector
    Sync -->|Uses| IAI
    Sync -->|Uses| IVector
    
    IAI -.->|Implements| Ollama
    IAI -.->|Implements| Azure
    IVector -.->|Implements| PgVector
    IVector -.->|Implements| AISearch
    
    style RAG fill:#e8f5e9
    style Sync fill:#e8f5e9
    style IAI fill:#e1f5ff
    style IVector fill:#e1f5ff
    style Ollama fill:#fff3e0
    style PgVector fill:#fff3e0
    style Azure fill:#f3e5f5
    style AISearch fill:#f3e5f5
```

---

## 8. Network Architecture

### 8.1 Development Network Topology

```mermaid
graph TB
    subgraph "localhost"
        Browser["Web Browser"]
        
        subgraph "Port 52291"
            DMS["DMS Web<br/>(IIS Express)"]
        end
        
        subgraph "Port 5169"
            API["Chatbot API<br/>(.NET 8)"]
        end
        
        subgraph "Port 5433"
            PG["PostgreSQL<br/>(Docker)"]
        end
        
        subgraph "Port 11434"
            Ollama["Ollama<br/>(Service)"]
        end
        
        LocalDB[("LocalDB<br/>(SQL Server)")]
    end
    
    Browser -->|HTTP| DMS
    Browser -->|HTTP| API
    API -->|TCP| PG
    API -->|HTTP| Ollama
    API -->|TCP| LocalDB
    DMS -->|TCP| LocalDB
    
    style Browser fill:#e1f5ff
    style DMS fill:#fff4e6
    style API fill:#f3e5f5
    style PG fill:#e0f2f1
    style Ollama fill:#fce4ec
```

### 8.2 CORS Configuration

```mermaid
graph LR
    subgraph "Allowed Origins"
        Dev1["localhost:5173<br/>(React Dev)"]
        Dev2["localhost:5174"]
        Dev3["localhost:5175"]
        DMS1["localhost:52291<br/>(DMS HTTP)"]
        DMS2["localhost:44300<br/>(DMS HTTPS)"]
    end
    
    subgraph "Chatbot API"
        CORS["CORS Policy<br/>'DMS'"]
        API["API Endpoints"]
    end
    
    Dev1 -->|Allowed| CORS
    Dev2 -->|Allowed| CORS
    Dev3 -->|Allowed| CORS
    DMS1 -->|Allowed| CORS
    DMS2 -->|Allowed| CORS
    
    CORS --> API
    
    style CORS fill:#e8f5e9
    style API fill:#f3e5f5
```

---

## 9. State Diagrams

### 9.1 Chat Widget State Machine

```mermaid
stateDiagram-v2
    [*] --> Closed: Page Load
    Closed --> Open: Click FAB
    Open --> Closed: Click Close
    
    state Open {
        [*] --> Empty: No Messages
        Empty --> Typing: User Types
        Typing --> Sending: Press Enter
        Sending --> Loading: API Call
        Loading --> DisplayResponse: Response Received
        DisplayResponse --> Typing: User Types Again
        Loading --> Error: API Error
        Error --> Typing: Retry
    }
    
    Open --> [*]: Page Unload
```

### 9.2 Document Sync State Machine

```mermaid
stateDiagram-v2
    [*] --> Idle: Service Start
    Idle --> Syncing: Timer Trigger<br/>(4 hours)
    Syncing --> ReadingDMS: Start Sync
    ReadingDMS --> GeneratingEmbeddings: Documents Retrieved
    GeneratingEmbeddings --> StoringVectors: Embeddings Generated
    StoringVectors --> Idle: Sync Complete
    
    ReadingDMS --> Error: DB Error
    GeneratingEmbeddings --> Error: Ollama Error
    StoringVectors --> Error: PostgreSQL Error
    Error --> Idle: Log Error
```

---

## 10. Technology Stack Diagram

```mermaid
graph TB
    subgraph "Frontend"
        React["React 18"]
        TS["TypeScript"]
        Vite["Vite"]
        CSS["Modern CSS"]
    end
    
    subgraph "Backend"
        NET8[".NET 8"]
        ASPNET["ASP.NET Core"]
        EF["Entity Framework"]
    end
    
    subgraph "AI & ML"
        Llama["Llama 3.2:1b"]
        Nomic["nomic-embed-text"]
        OllamaEngine["Ollama Engine"]
    end
    
    subgraph "Databases"
        PG["PostgreSQL 16"]
        PGVector["pgvector Extension"]
        MSSQL["SQL Server"]
    end
    
    subgraph "Infrastructure"
        Docker["Docker Desktop"]
        IIS["IIS Express"]
    end
    
    React --> TS
    React --> Vite
    React --> CSS
    
    NET8 --> ASPNET
    ASPNET --> EF
    
    Llama --> OllamaEngine
    Nomic --> OllamaEngine
    
    PG --> PGVector
    
    Docker --> PG
    IIS --> ASPNET
    
    style React fill:#61dafb
    style NET8 fill:#512bd4
    style Llama fill:#ff6b6b
    style PG fill:#336791
    style Docker fill:#2496ed
```

---

## Summary

This architectural documentation provides:

1. **High-Level Architecture** - Overall system structure and component relationships
2. **Component Architecture** - Detailed breakdown of solution structure and classes
3. **Data Flow Diagrams** - Visual representation of document sync and chat query flows
4. **Deployment Architecture** - Development and production environment layouts
5. **Security Architecture** - Authentication and access control mechanisms
6. **Database Schemas** - Entity relationships and table structures
7. **Component Interactions** - RAG pipeline and provider abstraction patterns
8. **Network Architecture** - Port configurations and CORS policies
9. **State Diagrams** - Widget and sync service state machines
10. **Technology Stack** - Complete technology overview

These diagrams serve as comprehensive reference for understanding, maintaining, and extending the InEight AI Chatbot system.
