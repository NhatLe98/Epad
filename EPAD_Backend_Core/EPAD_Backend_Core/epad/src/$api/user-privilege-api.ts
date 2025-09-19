import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class UserPrivilegeApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetUserPrivilegeAtPage(page: number, filter: string, limit): Promise<any> {
    return this.get("GetUserPrivilegeAtPage", { params: { page, filter, limit } });
  }

  public GetAllUserPrivilege(): Promise<BaseResponse> {
    return this.get("GetAllUserPrivilege");
  }

  public GetUserPrivilege(): Promise<BaseResponse> {
    return this.get("GetUserPrivilege");
  }

  public AddUserPrivilege(userPrivilege: IC_UserPrivilege) {
    return this.post("AddUserPrivilege", userPrivilege);
  }

  public UpdateUserPrivilege(userPrivilege: IC_UserPrivilege) {
    return this.post("UpdateUserPrivilege", userPrivilege);
  }

  public DeleteUserPrivilege(userPrivilege: Array<IC_UserPrivilege>) {
    return this.post("DeleteUserPrivilege", userPrivilege);
  }
  public GetDeviceAll() {
    return this.get("GetDeviceAll");
  }
}
export interface IC_UserPrivilege {
    Index?:number;
    Name?:string;
    UseForDefault?:boolean;
    IsUseForDefault?:string;
    IsAdmin?:boolean;
    IsAdminName?:string;
    Note?:string;
    UpdatedDate?:string;
    UpdatedUser?:string
}

export const userPrivilegeApi = new UserPrivilegeApi("UserPrivilege");
