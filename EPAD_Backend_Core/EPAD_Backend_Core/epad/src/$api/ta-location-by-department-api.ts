import {  AxiosRequestConfig } from "axios";
import { BaseApi, BaseResponse } from "@/$core/base-api";

class TA_LocationByDepartmentApi extends BaseApi {
  public constructor(_module: string, _config?: AxiosRequestConfig){
    super(_module, _config);
  }
  public GetLocationByDepartmentAtPage(page: number, limit: number, filter: string): Promise<BaseResponse> {
    return this.get("GetLocationByDepartmentAtPage", { params: { page, limit, filter } });
  }
	public AddLocationByDepartment(data: TA_LocationByDepartmentDTO) {
		return this.post('AddLocationByDepartment', data);
	}
	public UpdateLocationByDepartment(data: TA_LocationByDepartmentDTO) {
		return this.put('UpdateLocationByDepartment', data);
	}
  public DeleteLocationByDepartment(data: Array<any>) {
    return this.delete('DeleteLocationByDepartment', { data: data });
  }
}
export interface TA_LocationByDepartmentDTO {
  DepartmentList?: Array<number>;
  LocationIndex?: Array<number>;
  DepartmentIndex?: number;
}

export interface LocationByDepartmentRequest {
  page: number; 
  pageSize: number;
  filter: string;
}

export const taLocationByDepartment = new TA_LocationByDepartmentApi("TA_LocationByDepartment");