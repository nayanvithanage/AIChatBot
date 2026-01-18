import { ChatRequest, ChatResponse } from './types';

const API_BASE_URL = 'http://localhost:5169/api';

export class ChatAPI {
    private token: string | null = null;

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
