import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class LoginAccountApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetUserAccountAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
    return this.getItemsWithPaging("GetUserAccountAtPage", {
      params: { page, filter, limit }
    });
  }

  public AddUserAccount(useraccount: IC_UserAccount) {
    return this.post("AddUserAccount", useraccount);
  }

  public UpdateUserAccount(useraccount: IC_UserAccount) {
    return this.post("UpdateUserAccount", useraccount);
  }

  public DeleteUserAccount(useraccount: Array<IC_UserAccount>) {
    return this.post("DeleteUserAccount", useraccount);
  }
}

export interface IC_UserAccount {
  UserName?: string;
  Password?: string;
  Name?: string;
  ResetPasswordCode?: string;
  Disabled?: boolean;
  LockTo?: Date;
  CreatedDate?: Date;
  UpdatedDate?: Date;
  UpdatedUser?: Date;
  AccountPrivilege?: number;
  IsAccountPrivilege?: string;
  IsLockTo?: string;
  IsUpdatedDate?: string;
}

export const loginAccountApi = new LoginAccountApi("UserAccount");
