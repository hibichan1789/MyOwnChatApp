import { authClient } from "../../api/authClient";

// DOM要素取得
const userNameInput = document.getElementById("userName") as HTMLInputElement;
const emailInput = document.getElementById("email") as HTMLInputElement;
const passwordInput = document.getElementById("password") as HTMLInputElement;
const registerForm = document.getElementById("registerForm") as HTMLFormElement;
const message = document.getElementById("message") as HTMLParagraphElement;

registerForm.addEventListener("submit", async (e)=>{
    e.preventDefault();

    const userName = userNameInput.value.trim();
    const email = emailInput.value.trim();
    const password = passwordInput.value.trim();

    if(!userName || !email || !password){
        message.textContent = "すべての項目を入力してください";
        message.className = "text-red-600 text-sm text-center mt-2";
        return;
    }

    try{
        await authClient.register({userName, email, password});

        message.textContent = "仮登録が完了しました,メールをご確認ください";
        message.className = "text-green-600 text-sm text-center mt-2";

        userNameInput.value = "";
        emailInput.value = "";
        passwordInput.value = "";
    }
    catch(error:any){
        const errorMessage = error.response?.data?.message || "エラーが発生しました";
        message.textContent = errorMessage;
        message.className = "text-red-600 text-sm text-center mt-2"
    }
});