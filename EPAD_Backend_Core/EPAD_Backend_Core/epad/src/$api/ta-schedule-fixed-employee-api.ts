import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_ScheduleFixedByEmployeeApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetShiftByCompanyIndex() {
		return this.get('GetShiftByCompanyIndex');
	}
	public GetScheduleFixedByEmployeeAtPage(data: ScheduleFixedEmployeeRequest) {
		return this.post('GetScheduleFixedByEmployeeAtPage', data);
	}

	public AddScheduleFixedByEmployee(data: TA_ScheduleFixedEmployeeDTO) {
		return this.post('AddScheduleFixedByEmployee', data);
	}

	public UpdateScheduleFixedByEmployee(data: TA_ScheduleFixedEmployeeDTO) {
		return this.post('UpdateScheduleFixedByEmployee', data);
	}
  public DeleteScheduleFixedByEmployee(IdList: Array<any>) {
    return this.delete('DeleteScheduleFixedByEmployee', { data: IdList });
  }
  public AddScheduleFixedByEmployeeFromExcel(data: any) {
		return this.post('AddScheduleFixedByEmployeeFromExcel', data);
	}

  public ExportInfoShift() {
		return this.get('ExportInfoShift');
	}
}

export interface TA_ScheduleFixedEmployeeDTO {
  Index?: number;
  DepartmentList: Array<number>;
  EmployeeATIDs: Array<string>;
  FromDate: Date;
  ToDate?: Date;
  Monday: number;
  Tuesday: number;
  Wednesday: number;
  Thursday: number;
  Friday: number;
  Saturday: number;
  Sunday: number;
}

export interface ScheduleFixedEmployeeRequest {
	departmentIndexes: Array<number>;
	employeeATIDs: Array<number>;
  fromDate: Date;
  page: number; 
  pageSize: number;
}

export const taScheduleFixedByEmployee = new TA_ScheduleFixedByEmployeeApi("TA_ScheduleFixedByEmployee");
