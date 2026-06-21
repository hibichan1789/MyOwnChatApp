import { axiosClient } from "./axiosClient";
import type {
    RegisterRequest,
    LoginRequest,
    LoginResponse
} from "../types/auth";

export const authClient = {
    register:async(data:RegisterRequest)=>{
        await axiosClient.post("/auth/register", data);
    },
    verifyEmail:async(token:string)=>{
        await axiosClient.get(`/auth/verify?token=${encodeURIComponent(token)}`);
    },
    login:async(data:LoginRequest):Promise<LoginResponse>=>{
        return (await axiosClient.post("/auth/login", data)).data;
    },
    refresh:async():Promise<LoginResponse>=>{
        return (await axiosClient.post("/auth/refresh")).data;
    },
    logout:async()=>{
        await axiosClient.post("/auth/logout");
    }
}