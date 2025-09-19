import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";
import { isNullOrUndefined } from "util";

class UserAccountApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetUserAccountInfo(userName?: string) {
    if (isNullOrUndefined(userName)) {
      return this.get("GetUserAccountInfo");
    } else {
      return this.get("GetUserAccountInfo", {
        params: { pUserName: userName }
      });
    }
  }

  public GetAllUserAccount() {
    return this.get("GetAllUserAccount");
  }

  public CheckValidPassword(username: string, password: string) {
    return this.get("CheckValidPassword", { params: { username, password } });
  }

  public ChangePassword(data: IPasswordInfo) {
    return this.post("ChangePassword", data);
  }
  public SendResetPasswordCode(data: IPasswordInfo) {
    return this.post("SendResetPasswordCode", data);
  }

  public ResetPassword(data: IResetPassword) {
    return this.post("ResetPassword", data);
  }
}

export interface IPasswordInfo {
  UserName?: string;
  Password?: string;
  NewPassword?: string;
  ConfirmPassword?: string;
  Email?: string;
  ServiceId?: number;
}

export interface IResetPassword {
  UserName?: string;
  Code?: string;
  NewPassword?: string;
  ConfirmNewPassword?: string;
}

export const userAccountApi = new UserAccountApi("UserAccount");
