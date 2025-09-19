import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class HR_NannyInfoApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetAllNanny(): Promise<any> {
    return this.get('Get_HR_NannyInfos');
  }

  public GetNanny(employeeATID: string): Promise<any> {
    return this.get('Get_HR_NannyInfo/' + employeeATID);
  }
}

export const hrNannyInfoApi = new HR_NannyInfoApi("HR_NannyInfo");
