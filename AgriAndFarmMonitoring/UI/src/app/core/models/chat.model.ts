export interface ChatRequestDto {
    sessionId?: string;
    message: string;
    farmId: number;
}

export interface ChatResponseDto {
    sessionId: string;
    message: string;
    timestamp: Date;
}

export interface ChatMessage {
    id: string;
    text: string;
    html?: string;
    sender: 'user' | 'bot';
    timestamp: Date;
}
