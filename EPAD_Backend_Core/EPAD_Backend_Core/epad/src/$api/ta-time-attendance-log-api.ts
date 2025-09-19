import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_TimeAttendanceLogApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

	public CaculateAttendance(data: TA_TimeAttendanceLogDTO) {
		return this.post('CaculateAttendance', data);
	}
  public GetCaculateAttendanceData(data: TA_TimeAttendanceLogParam): Promise<any> {
    return this.post('GetCaculateAttendanceData', data);
}
public GetSyntheticAttendanceData(data: TA_TimeAttendanceLogParam): Promise<any> {
  return this.post('GetSyntheticAttendanceData', data);
}
}
export interface TA_TimeAttendanceLogParam {
  Filter?: string;
  Page?: number;
  PageSize?: number;
  Departments?: Array<number>;
  EmployeeATIDs?: Array<string>;
  FromDate?: string;
  ToDate?: string;
  FilterByType?: number;
}
export interface TA_TimeAttendanceLogDTO {
  EmployeeATIDs: Array<string>;
  FromDate: string;
  ToDate: string;
  
}

export const taTimeAttendanceLogApi = new TA_TimeAttendanceLogApi("TA_TimeAttendanceLog");
