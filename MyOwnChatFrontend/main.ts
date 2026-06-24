import { authClient } from "./src/api/authClient";

// DOM取得
const emailInput = document.getElementById("email") as HTMLInputElement;
const passwordInput = document.getElementById("password") as HTMLInputElement;
const loginForm = document.getElementById("loginForm") as HTMLFormElement;
const message = document.getElementById("message") as HTMLParagraphElement;

loginForm.addEventListener("submit", async (e) => {
    e.preventDefault();

    const email = emailInput.value.trim();
    const password = passwordInput.value.trim();

    if (!email || !password) {
        message.textContent = "メールアドレスとパスワードを入力してください";
        message.className = "text-red-600 text-sm text-center mt-2";
        return;
    }

    try {
        const response = await authClient.login({ email, password });

        localStorage.setItem("accessToken", response.accessToken);
        message.textContent = "ログイン成功しました";
        message.className = "text-green-600 text-sm text-center mt-2"

        
        location.href = "/src/pages/chat/chat.html";
    }
    catch (error: any) {
        const errorMessage = error.response?.data?.message || "ログインに失敗しました";
        message.textContent = errorMessage;
        message.className = "text-red-600 text-sm text-center mt-2";
    }
});