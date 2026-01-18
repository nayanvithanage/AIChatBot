import { ChatRequest, ChatResponse } from './types';

// Get config from DMS
const config = (window as any).INEIGHT_CHATBOT_CONFIG || {
    apiUrl: 'http://localhost:5169/api',
    jwtToken: null
};

const API_BASE_URL = config.apiUrl;

export class ChatAPI {
    private token: string | null = config.jwtToken;

    setToken(token: string) {
        this.token = token;
    }

    async sendMessage(query: string, projectId?: number): Promise<ChatResponse> {
        const request: ChatRequest = {
            query,
            projectId,
        };

        const response = await fetch(`${API_BASE_URL}/chat/message`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...(this.token && { 'Authorization': `Bearer ${this.token}` }),
            },
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            throw new Error(`API error: ${response.statusText}`);
        }

        return response.json();
    }
}

export const chatAPI = new ChatAPI();
