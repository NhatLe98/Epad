import { AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class EmployeeStoppedApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }
  public GetEmployeeStopped(page: number, filter: string, limit): Promise<BaseResponse> {
    return this.get("GetEmployeeStopped", { params: { page, filter, limit } });
  }
  public AddEmployeeStopped(param) : Promise<any> {
    return this.post("AddEmployeeStopped", param);
  }
  public UpdateEmployeeStopped(param) : Promise<any> {
    return this.put("UpdateEmployeeStopped", param);
  }
  public DeleteEmployeeStopped(param: Array<any>) {
    return this.delete("DeleteEmployeeStopped", { data: param });
  }
  public AddEmployeeStoppedFromExcel(param) {
    return this.post("AddEmployeeStoppedFromExcel", param);
  }
}

export interface IC_EmployeeStoppedDTO {
  Index?: number;
  EmployeeATIDs?: Array<string>;
  StoppedDate: Date;
  StoppedDateString?: string;
  Reason?: string;
  EmployeeATID?: string;
}

export const employeeStoppedApi = new EmployeeStoppedApi("IC_EmployeeStopped");
