import { chatClient } from "../../api/chatClient";
import { authClient } from "../../api/authClient";
import type {
    ConversationSummaryDto,
    ConversationDto,
    Message
} from "../../types/chat";

// DOM要素
// Sidebar
const sidebar = document.getElementById("sidebar") as HTMLDivElement;
const toggleSidebar = document.getElementById("toggleSidebar") as HTMLButtonElement;
const newChatBtn = document.getElementById("newChatBtn") as HTMLButtonElement;
const logoutBtn = document.getElementById("logoutBtn") as HTMLButtonElement;
const conversationList = document.getElementById("conversationList") as HTMLDivElement;
// Chat
const chatWindow = document.getElementById("chatWindow") as HTMLDivElement;
const chatInput = document.getElementById("chatInput") as HTMLInputElement;
const chatForm = document.getElementById("chatForm") as HTMLFormElement;
const submitBtn = document.getElementById("submitBtn") as HTMLButtonElement;

// 状態
let currentConversationId: string | null = null;
let aiBuffer = "";
let aiElement: HTMLDivElement | null = null;

// Chat画面TailWindClass
const userMessageStyle = "p-3 rounded-lg bg-blue-500 text-base text-white max-w-[75%] ml-auto shadow-md";
const assistantMessageStyle = "assistant p-3 rounded-lg bg-gray-200 text-base max-w-[75%] mr-auto shadow-md";



// Sidebar
toggleSidebar.addEventListener("click", () => {
    if (sidebar.classList.contains("w-64")) {
        sidebar.classList.remove("w-64");
        sidebar.classList.add("w-12");
        newChatBtn.textContent = "+";
        logoutBtn.textContent = "X";
    } else {
        sidebar.classList.remove("w-12");
        sidebar.classList.add("w-64");
        newChatBtn.textContent = "新規会話";
        logoutBtn.textContent = "ログアウト";
    }
});

// 新規会話
newChatBtn.addEventListener("click", () => {
    currentConversationId = null;
    chatWindow.innerHTML = "";
    aiElement = null;
    aiBuffer = "";
});


// 会話一覧の読み込み
async function loadConversationList() {
    const list = await chatClient.getHistoryList();

    conversationList.innerHTML = "";

    list.forEach((c: ConversationSummaryDto) => {
        const conversationSummaryDiv = document.createElement("div");
        conversationSummaryDiv.className = "p-3 hover:bg-gray-700 cursor-pointer border-b border-gray-800";
        const conversationTitle = c.firstMessage.content;
        conversationSummaryDiv.textContent = conversationTitle.length >= 20 ? conversationTitle.slice(0, 20) + "..." : conversationTitle;


        // サイドバーの会話を押すとその会話に移行できる
        conversationSummaryDiv.addEventListener("click", () => {
            currentConversationId = c.conversationId;
            // 全ての selected を外す
            document.querySelectorAll("#conversationList > div").forEach(div => {
                div.classList.remove("selected");
            });

            // クリックした要素に selected を付ける
            conversationSummaryDiv.classList.add("selected");
            loadConversationDetail(currentConversationId);
        });

        conversationList.appendChild(conversationSummaryDiv);
    });
}

// 会話詳細の読み込み
async function loadConversationDetail(conversationId: string) {
    const data: ConversationDto = await chatClient.getHistoryDetail(conversationId);

    chatWindow.innerHTML = "";
    aiElement = null;
    aiBuffer = "";

    data.messages.forEach((m: Message) => {
        const chatMessage = document.createElement("div");
        if (m.role === "user") {
            chatMessage.className = userMessageStyle;
        }
        else {
            chatMessage.className = assistantMessageStyle;
        }


        chatMessage.textContent = m.content;
        chatWindow.appendChild(chatMessage);
    });

    chatWindow.scrollTop = chatWindow.scrollHeight;
}

// メッセージ送信SSE
chatForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    submitBtn.disabled = true;
    submitBtn.classList.remove("bg-blue-600", "hover:bg-blue-700");
    submitBtn.classList.add("bg-gray-700");


    const text = chatInput.value.trim();
    if (!text) {
        submitBtn.disabled = false;
        submitBtn.classList.add("bg-blue-600", "hover:bg-blue-700");
        submitBtn.classList.remove("bg-gray-700");
        return;
    }
    appendUserMessage(text);

    chatInput.value = "";

    const reader = await chatClient.streamWithFetch({
        conversationId: currentConversationId,
        content: text
    });
    const decoder = new TextDecoder();

    while (true) {
        const { value, done } = await reader.read();
        if (done) {
            break;
        }

        const chunk = decoder.decode(value);

        // SSEのデータをParse
        const lines = chunk.split("\n");
        for (const line of lines) {
            if (!line.startsWith("data:")) {
                continue;
            }

            const data = line.replace("data:", "").trim();

            if (data === "[DONE]") {
                await loadConversationList();

                break;
            }

            // 最初のdataはconversationId
            if (data.startsWith("{")) {
                const obj = JSON.parse(data);
                currentConversationId = obj.conversationId;
                continue;
            }

            appendAssistantDelta(data);
        }
    }

    submitBtn.disabled = false
    submitBtn.classList.add("bg-blue-600", "hover:bg-blue-700");
    submitBtn.classList.remove("bg-gray-700");
});

function appendUserMessage(text: string) {
    const div = document.createElement("div");
    div.className = userMessageStyle;
    div.textContent = text;
    chatWindow.appendChild(div);
    chatWindow.scrollTop = chatWindow.scrollHeight;
}

/*
function formatTimestamp(date:Date): string{
    const yyyy = date.getFullYear();
    const M = date.getMonth() + 1;
    const d = date.getDate();
    const h = date.getHours();
    const m = date.getMinutes();

    return `${yyyy}年${M}月${d}日 ${h}:${m}`;
}
*/


setInterval(() => {
    if (aiElement && aiBuffer.length > 0) {
        aiElement.textContent += aiBuffer;
        aiBuffer = "";
        chatWindow.scrollTop = chatWindow.scrollHeight;
    }
}, 30)
function appendAssistantDelta(delta: string) {
    aiBuffer += delta;
    // 最後のメッセージが assistant じゃなければ新規作成
    if (!aiElement) {
        aiElement = document.createElement("div");
        aiElement.className = assistantMessageStyle;
        aiElement.textContent = "";
        chatWindow.appendChild(aiElement);
    }
}

logoutBtn.addEventListener("click", async () => {
    try {
        await authClient.logout();
        localStorage.removeItem("accessToken");
        window.location.href = "/index.html";
    } catch (err) {
        console.error("ログアウト失敗", err);
        alert("ログアウトに失敗しました");
    }
});

// 初期ロード
loadConversationList();