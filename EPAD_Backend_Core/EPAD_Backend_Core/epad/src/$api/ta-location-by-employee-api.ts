import {  AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class TA_LocationByEmployeeApi extends BaseApi {
  public constructor(_module: string, _config?: AxiosRequestConfig){
    super(_module, _config);
  }
  public GetLocationByEmployeeAtPage(page: number, limit: number, filter: string){
    return this.get("GetLocationByEmployeeAtPage", { params: { page, limit, filter } });
  }
	public AddLocationByEmployee(data: TA_LocationByEmployeeDTO) {
		return this.post('AddLocationByEmployee', data);
	}
	public UpdateLocationByEmployee(data: TA_LocationByEmployeeDTO) {
		return this.put('UpdateLocationByEmployee', data);
	}
  public DeleteLocationByEmployee(data: Array<any>) {
    return this.delete('DeleteLocationByEmployee', { data: data });
  }
  public AddLocationByEmployeeFromExcel(param : any) {
    return this.post("AddLocationByEmployeeFromExcel", param);
  }
}
export interface TA_LocationByEmployeeDTO {
  EmployeeATIDs?: Array<string>;
  EmployeeATID?: Array<string>;
  LocationIndex?: Array<number>;
  DepartmentList?: Array<number>;
  DepartmentIndex?: Array<number>;
}

export interface LocationByEmployeeRequest {
  page: number; 
  pageSize: number;
  filter: string;
}

export const taLocationByEmployee = new TA_LocationByEmployeeApi("TA_LocationByEmployee");