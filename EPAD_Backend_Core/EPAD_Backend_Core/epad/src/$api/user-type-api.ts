import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class UserTypeApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GelAllUserType(): Promise<BaseResponse> {
    return this.get("GetAllUserType");
  }

  public GetUserTypeTitle(userType: HR_UserType) {
    return this.post("UpdateUserType", userType);
  }

  public GetUserTypeAtPage(page: number, filter: string, limit): Promise<BaseResponse> {
    return this.getItemsWithPaging("GetUserTypeAtPage", { params: { page, filter, limit } });
  }

  public AddUserType(userType: HR_UserType) {
    return this.post("AddUserType", userType);
  }

  public UpdateUserType(userType: HR_UserType) {
    return this.post("UpdateUserType", userType);
  }

  public DeleteUserType(userType: Array<HR_UserType>) {
    return this.post("DeleteUserType", userType);
  }

}

export interface HR_UserType {
  Index?: number;
  Code?: string;
  Name?: string;
  EnglishName?: string;
  Description?: string;
  Order?: number;
  StatusId?: number;
  UserTypeId?: number;
}

export const userTypeApi = new UserTypeApi("UserType");
