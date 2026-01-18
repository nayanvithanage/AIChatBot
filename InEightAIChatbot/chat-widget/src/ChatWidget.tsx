import { useState, useRef, useEffect } from 'react';
import { chatAPI } from './api';
import { ChatMessage } from './types';
import './ChatWidget.css';

export default function ChatWidget() {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [input, setInput] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const messagesEndRef = useRef<HTMLDivElement>(null);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    };

    useEffect(() => {
        scrollToBottom();
    }, [messages]);

    const handleSend = async () => {
        if (!input.trim() || isLoading) return;

        const userMessage: ChatMessage = {
            id: Date.now().toString(),
            role: 'user',
            content: input,
            timestamp: new Date(),
        };

        setMessages(prev => [...prev, userMessage]);
        setInput('');
        setIsLoading(true);

        try {
            const response = await chatAPI.sendMessage(input);

            const assistantMessage: ChatMessage = {
                id: (Date.now() + 1).toString(),
                role: 'assistant',
                content: response.answer,
                timestamp: new Date(),
                links: response.links,
                confidence: response.confidence,
            };

            setMessages(prev => [...prev, assistantMessage]);
        } catch (error) {
            const errorMessage: ChatMessage = {
                id: (Date.now() + 1).toString(),
                role: 'assistant',
                content: 'Sorry, I encountered an error. Please try again.',
                timestamp: new Date(),
            };
            setMessages(prev => [...prev, errorMessage]);
            console.error('Chat error:', error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    };

    return (
        <div className="chat-widget">
            <div className="chat-header">
                <div className="header-content">
                    <div className="header-icon">💬</div>
                    <div>
                        <h2>InEight AI Assistant</h2>
                        <p>Ask me anything about your documents</p>
                    </div>
                </div>
            </div>

            <div className="chat-messages">
                {messages.length === 0 && (
                    <div className="empty-state">
                        <div className="empty-icon">🤖</div>
                        <h3>Welcome to InEight AI Assistant!</h3>
                        <p>Ask me questions about your documents, projects, or transmittals.</p>
                        <div className="suggestions">
                            <button onClick={() => setInput('Show me recent documents')}>
                                Recent documents
                            </button>
                            <button onClick={() => setInput('What transmittals are pending?')}>
                                Pending transmittals
                            </button>
                        </div>
                    </div>
                )}

                {messages.map((message) => (
                    <div key={message.id} className={`message ${message.role}`}>
                        <div className="message-avatar">
                            {message.role === 'user' ? '👤' : '🤖'}
                        </div>
                        <div className="message-content">
                            <div className="message-text">{message.content}</div>
                            {message.links && message.links.length > 0 && (
                                <div className="message-links">
                                    <div className="links-title">Related Documents:</div>
                                    {message.links.map((link, idx) => (
                                        <a
                                            key={idx}
                                            href={link.url}
                                            className="document-link"
                                            target="_blank"
                                            rel="noopener noreferrer"
                                        >
                                            📄 {link.title}
                                        </a>
                                    ))}
                                </div>
                            )}
                            {message.confidence !== undefined && (
                                <div className="confidence">
                                    Confidence: {(message.confidence * 100).toFixed(0)}%
                                </div>
                            )}
                            <div className="message-time">
                                {message.timestamp.toLocaleTimeString()}
                            </div>
                        </div>
                    </div>
                ))}

                {isLoading && (
                    <div className="message assistant">
                        <div className="message-avatar">🤖</div>
                        <div className="message-content">
                            <div className="typing-indicator">
                                <span></span>
                                <span></span>
                                <span></span>
                            </div>
                        </div>
                    </div>
                )}

                <div ref={messagesEndRef} />
            </div>

            <div className="chat-input">
                <textarea
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyPress={handleKeyPress}
                    placeholder="Ask a question about your documents..."
                    rows={1}
                    disabled={isLoading}
                />
                <button
                    onClick={handleSend}
                    disabled={!input.trim() || isLoading}
                    className="send-button"
                >
                    {isLoading ? '⏳' : '➤'}
                </button>
            </div>
        </div>
    );
}
