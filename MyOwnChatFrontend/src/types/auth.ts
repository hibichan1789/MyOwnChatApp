export interface RegisterRequest{
    userName:string;
    email:string;
    password:string;
}
export interface LoginRequest{
    email:string;
    password:string;
}
export interface LoginResponse{
    accessToken:string;
    accessTokenExpiresAt:string;
}