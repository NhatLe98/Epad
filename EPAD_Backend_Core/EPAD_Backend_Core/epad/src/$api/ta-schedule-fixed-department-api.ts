import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_ScheduleFixedByDepartmentApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetShiftByCompanyIndex() {
		return this.get('GetShiftByCompanyIndex');
	}
	public GetScheduleFixedByDepartmentAtPage(data: ScheduleFixedDepartmentRequest) {
		return this.post('GetScheduleFixedByDepartmentAtPage', data);
	}

	public AddScheduleFixedByDepartment(data: TA_ScheduleFixedDepartmentDTO) {
		return this.post('AddScheduleFixedByDepartment', data);
	}

	public UpdateScheduleFixedByDepartment(data: TA_ScheduleFixedDepartmentDTO) {
		return this.post('UpdateScheduleFixedByDepartment', data);
	}
  public DeleteScheduleFixedByDepartment(IdList: Array<any>) {
    return this.delete('DeleteScheduleFixedByDepartment', { data: IdList });
  }
  public AddScheduleFixedByDepartmentFromExcel(data: any) {
		return this.post('AddScheduleFixedByDepartmentFromExcel', data);
	}

  public ExportInfoShift() {
		return this.get('ExportInfoShift');
	}
}

export interface TA_ScheduleFixedDepartmentDTO {
  Index?: number;
  DepartmentList: Array<number>;
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

export interface ScheduleFixedDepartmentRequest {
	departmentIndexes: Array<number>;
  fromDate: Date;
  page: number; 
  pageSize: number;
}

export const taScheduleFixedByDepartment = new TA_ScheduleFixedByDepartmentApi("TA_ScheduleFixedByDepartment");
