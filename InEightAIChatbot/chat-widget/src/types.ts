export type ChatRequest = {
  query: string;
  projectId?: number;
  sessionId?: string;
};

export type ChatLink = {
  type: string;
  id?: number;
  title: string;
  url: string;
};

export type ChatResponse = {
  answer: string;
  links: ChatLink[];
  confidence: number;
  fallbackKBLink?: string;
};

export type ChatMessage = {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
  links?: ChatLink[];
  confidence?: number;
};
