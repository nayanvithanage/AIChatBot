# InEight AI Chatbot - MVP Scope

## ✅ Approved Technology Stack

- **AI Provider**: Ollama (Llama 3) - Free, self-hosted
- **Vector Database**: PostgreSQL + pgvector (Docker)
- **Backend**: .NET 8 Web API
- **Frontend**: React + TypeScript (Vite)
- **Integration**: JWT authentication with existing DMS

## 🎯 MVP Focus (Happy Path)

### What's Included in MVP:

1. **Core Document Search**
   - Natural language queries on document metadata (26+ fields)
   - Semantic search using RAG (Retrieval-Augmented Generation)
   - User security filtering (project-based access control)

2. **Basic Chat Interface**
   - Floating chat button in DMS
   - Simple message input/output
   - Document links in responses
   - Loading states

3. **Essential Services**
   - MetadataSyncService (documents only)
   - RAGOrchestrator (query processing)
   - OllamaProvider (AI operations)
   - PgVectorProvider (vector search)

4. **Security**
   - JWT authentication from DMS
   - User-filtered vector search
   - Project-based access control

### What's Moved to Phase 2:

- ❌ Linked items (RFIs, Transmittals, Forms, Tasks)
- ❌ Document actions tracking (Bluebeam, updates, reviews)
- ❌ Chat history/session management
- ❌ Admin analytics dashboard
- ❌ Failed query logging with KB links
- ❌ Azure provider implementation

## 📊 Simplified Architecture

```
┌─────────────────┐
│   DMS (MVC 5)   │
│  + Chat Widget  │
└────────┬────────┘
         │ JWT
         ↓
┌─────────────────┐
│  .NET 8 API     │
│  RAGOrchestrator│
└────┬──────┬─────┘
     │      │
     ↓      ↓
┌─────┐  ┌──────┐
│Ollama│  │PgVec │
│Llama3│  │-tor  │
└─────┘  └──────┘
```

## 🚀 Implementation Phases

### Phase 1: Infrastructure (1-2 days)
- Set up Docker PostgreSQL + pgvector
- Install Ollama + models
- Create .NET 8 solution structure

### Phase 2: Core Services (2-3 days)
- Implement provider interfaces
- Create Ollama & PgVector providers
- Build MetadataSyncService
- Build RAGOrchestrator

### Phase 3: API & Widget (2-3 days)
- Create ChatController
- Build React chat widget
- Configure JWT authentication

### Phase 4: Integration & Testing (1-2 days)
- Integrate widget into DMS
- Test end-to-end flow
- Performance benchmarking

**Total Estimated Time**: 6-10 days

## 📝 Success Criteria

MVP is complete when:

1. ✅ User can login to DMS
2. ✅ Chat button appears in DMS UI
3. ✅ User can ask: "Show me documents uploaded today"
4. ✅ Chatbot returns relevant documents (user-filtered)
5. ✅ User can click document link to open it
6. ✅ Response time is under 10 seconds
7. ✅ Different users see different results based on project access

## 🔄 Future Enhancements

After MVP is validated:

- Add linked items support
- Add document actions tracking
- Implement chat history
- Add admin analytics
- Upgrade to Azure providers (config-only switch)
