import { axiosClient } from "./axiosClient";
import type {
    ChatRequestDto,
    ConversationDto,
    ConversationSummaryDto
} from "../types/chat";
import { authClient } from "./authClient";


const basePath = "/chat"

export const chatClient = {
    // 会話一覧
    async getHistoryList(): Promise<ConversationSummaryDto[]> {
        const response = await axiosClient.get(`${basePath}/history`);
        return response.data;
    },

    // 会話詳細
    async getHistoryDetail(conversationId: string): Promise<ConversationDto> {
        const response = await axiosClient.get(`${basePath}/history/${conversationId}`);
        return response.data;
    },

    // fetch + ReadableStream版
    async streamWithFetch(request: ChatRequestDto): Promise<ReadableStreamDefaultReader<Uint8Array>> {
        const response = await fetchWithAuth(
            `${axiosClient.defaults.baseURL}${basePath}/stream`, 
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(request)
            }
        );

        if (!response.body) {
            throw new Error("ストリームがサポートされていません");
        }

        return response.body.getReader();
    }
}

async function fetchWithAuth(input: RequestInfo, init: RequestInit): Promise<Response> {
    const accessToken = localStorage.getItem("accessToken");

    const response = await fetch(input, {
        ...init,
        headers: {
            ...(init.headers || {}),
            "Authorization": `Bearer ${accessToken}`
        }
    });

    // Unauthorized → Refresh → Retry
    if (response.status === 401) {
        try {
            const refreshResult = await authClient.refresh();
            const newToken = refreshResult.accessToken;

            localStorage.setItem("accessToken", newToken);

            // Retry
            const retryResponse = await fetch(input, {
                ...init,
                headers: {
                    ...(init.headers || {}),
                    "Authorization": `Bearer ${newToken}`
                }
            });

            return retryResponse;
        }
        catch(err){
            localStorage.removeItem("accessToken");
            throw err;
        }
    }

    return response;
}