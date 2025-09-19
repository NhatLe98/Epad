import { AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class LoginAccountApi extends BaseApi {
    public constructor(_module: string, config?: AxiosRequestConfig) {
        super(_module, config);
    }

    public CheckLogin(userName: string) {
        return this.get('CheckLogin',{
            params: { userName }
        });
    }
}

export interface AuthModel {
    UserName?: string;
    Password?: string;
}
export interface AuthResetModel {
    Username?: string;
    Code?: string;
    NewPassword?: string;
}

export const authApi = new LoginAccountApi("Login");