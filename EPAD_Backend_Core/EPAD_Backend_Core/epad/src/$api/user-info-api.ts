import { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class UserInfo extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetUserMachineInfo(addedParams:  Array<AddedParam>) {
      return this.post("GetUserMachineInfo", addedParams);
    }
    public ExportUserMachineInfo(addedParams: Array<AddedParam>) {
        return this.post("ExportToExcel", addedParams);
    }
    public DeleteUserInfo(listDevice: Array<string>) {
        return this.post("DeleteUserInfo", { listDevice });
    }
    public UpdateUserPrivilege(addedParams: Array<AddedParam>) {
        return this.post("UpdateUserPrivilege", addedParams );
    }
}
export interface AddedParam {
    Key: string;
    Value: any | object;
}
export const userInfoApi = new UserInfo("UserInfo");
