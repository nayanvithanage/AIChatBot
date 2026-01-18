# InEight AI Chatbot Implementation - MVP

## Phase 1: Planning & Design
- [ ] Review existing DMS implementation
- [x] Create comprehensive implementation plan
- [/] Get user approval on implementation approach

## Phase 2: Solution Setup & Foundation
- [ ] Create InEightAIChatbot solution structure
- [ ] Set up .NET 8 Web API project
- [ ] Set up Core and Infrastructure projects
- [ ] Install required NuGet packages

## Phase 3: Infrastructure Setup
- [ ] Set up PostgreSQL with pgvector (Docker)
- [ ] Create vector database schema
- [ ] Install and test Ollama locally
- [ ] Pull Ollama models (llama3, nomic-embed-text)

## Phase 4: Core Implementation
- [ ] Implement IAIProvider and IVectorSearchProvider interfaces
- [ ] Create OllamaProvider implementation
- [ ] Create PgVectorProvider implementation
- [ ] Implement MetadataSyncService (documents only)
- [ ] Implement RAGOrchestrator

## Phase 5: API Development
- [ ] Create ChatController with message endpoint
- [ ] Implement JWT authentication validation
- [ ] Configure Program.cs with provider DI
- [ ] Add basic error handling

## Phase 6: React Chat Widget
- [ ] Set up React + Vite project
- [ ] Create basic chat UI component
- [ ] Implement API client
- [ ] Create floating chat button
- [ ] Add document link rendering

## Phase 7: DMS Integration
- [ ] Create JWT helper in DMS
- [ ] Embed chat widget in DMS _Layout.cshtml
- [ ] Test end-to-end flow

## Phase 8: Testing & Verification
- [ ] Test metadata synchronization
- [ ] Test natural language queries
- [ ] Test user access control
- [ ] Performance benchmarking
- [ ] Create walkthrough documentation

## Future Enhancements (Phase 2)
- [ ] Add linked items support (RFIs, Transmittals)
- [ ] Add document actions tracking
- [ ] Add chat history/session management
- [ ] Add admin analytics dashboard
- [ ] Implement Azure provider upgrade path
