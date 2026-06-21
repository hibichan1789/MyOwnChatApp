import { authClient } from "../../api/authClient";

// DOM要素取得
const status = document.getElementById("status") as HTMLParagraphElement;
const loading = document.getElementById("loading") as HTMLDivElement;
const loginLink = document.getElementById("loginLink") as HTMLParagraphElement;

const params = new URLSearchParams(window.location.search);
const token = params.get("token");

await verifyEmail(token);

async function verifyEmail(token: string | null) {
    if (!token) {
        status.textContent = "トークンが無効です";
        status.className = "text-red-600 text-base";
        loading.classList.add("hidden");
        return;
    }

    try {
        await authClient.verifyEmail(token);

        status.textContent = "メール認証が完了しました,ログイン可能です";
        status.className = "text-green-600 text-base";
        loginLink.classList.remove("hidden");
    }
    catch (error: any) {
        const msg = error.response?.data?.message || "認証に失敗しました。";
        status.textContent = msg;
        status.className = "text-red-600 text-base";
    }
    finally{
        loading.classList.add("hidden");
    }
}