import { defineConfig } from "vite";
import tailwindCss from "@tailwindcss/vite";

export default defineConfig({
    plugins: [tailwindCss()],
    build: {
        rollupOptions: {
            input: {
                main: "index.html",
                chat: "src/pages/chat/chat.html",
                register: "src/pages/register/register.html",
                verify: "src/pages/verify/verify.html"
            }
        }
    }
});