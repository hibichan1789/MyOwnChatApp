export interface Message{
    role: "user"|"assistant";
    content: string;
}

export interface ConversationSummaryDto{
    conversationId: string;
    firstMessage: Message;
}

export interface ConversationDto{
    conversationId: string;
    messages: Message[];
}

export interface ChatRequestDto{
    conversationId: string|null;
    content:string;
}

export interface ChatResponseDto{
    conversationId: string;
    reply: string;
}