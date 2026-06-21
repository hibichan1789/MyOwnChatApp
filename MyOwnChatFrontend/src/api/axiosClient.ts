import axios from "axios";
import { authClient } from "./authClient";

const baseUrl:string = import.meta.env.VITE_API_BASE_URL;

export const axiosClient = axios.create({
    baseURL:baseUrl, // asp.net core apiのURL http://localhost:5062/api 環境変数ファイル.env,.env.productionで切り替えできる
    timeout: 30 * 1000, // タイムアウト30秒
    withCredentials:true // RefreshToken Cookieを送るために必須
});


// リクエスト前に割り込んでJWT AccessTokenを自動で付与する
axiosClient.interceptors.request.use(
    (config)=>{
        const accessToken = localStorage.getItem("accessToken");
        if(accessToken){
            config.headers.Authorization = `Bearer ${accessToken}`;
        }
        return config;
    },
    (error)=>{
        return Promise.reject(error);
    }
);

// 共通エラーハンドリング
// 自動リフレッシュ追加
let isRefreshing = false;
let pendingRequests: ((token:string)=>void)[] = [];

axiosClient.interceptors.response.use(
    (response)=>response,
    async (error)=>{
        const originalRequest = error.config;

        if(error.response?.status === 401 && !originalRequest._retry){
            originalRequest._retry = true;

            // すでにrefresh中ならばrefresh完了を待ってから再送
            if(isRefreshing){
                return new Promise((resolve)=>{
                    pendingRequests.push((newToken:string)=>{
                        originalRequest.headers.Authorization = `Bearer ${newToken}`;
                        resolve(axiosClient(originalRequest));
                    });
                });
            }

            isRefreshing = true;

            try{
                const response = await authClient.refresh();
                const newAccessToken = response.accessToken;

                // 新しいアクセストークンを保存
                localStorage.setItem("accessToken", newAccessToken);

                // 保留中のリクエストを再送
                pendingRequests.forEach((cb)=>cb(newAccessToken));
                pendingRequests = [];

                // 元のリクエストを再送
                originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                return axiosClient(originalRequest);
            }
            catch(refreshError){
                // Refresh失敗つまりログアウトする
                localStorage.removeItem("accessToken");
                pendingRequests = [];
                return Promise.reject(refreshError);
            }
            finally{
                isRefreshing = false;
            }
        }
        console.error("API Error: ", error.response?.data || error.message);
        return Promise.reject(error);
    }
);