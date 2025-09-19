import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_LeaveRegistrationApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetLeaveDateType() {
		return this.get('GetLeaveDateType');
	}
  public GetLeaveDurationType() {
		return this.get('GetLeaveDurationType');
	}
  public GetHaftLeaveType() {
		return this.get('GetHaftLeaveType');
	}
	public GetLeaveRegistration(param: LeaveRegistrationModel | any) {
		return this.post('GetLeaveRegistration', param );
	}
	public AddLeaveRegistration(data: LeaveRegistrationModel | any) {
		return this.post('AddLeaveRegistration', data);
	}
	public UpdateLeaveRegistration(data: LeaveRegistrationModel | any) {
		return this.put('UpdateLeaveRegistration', data);
	}
	public DeleteLeaveRegistration(index: Array<any>) {
		return this.delete('DeleteLeaveRegistration', { data: index });
	}
  public AddLeaveRegistrationFromExcel(data: any) {
		return this.post('AddLeaveRegistrationFromExcel', data);
	}
  public ExportTemplateLeaveRegistration() {
    return this.get("ExportTemplateLeaveRegistration");
}
}

export interface TA_LeaveRegistration {
  Index?: number;
  EmployeeATID: string;
  Description?: string;
  LeaveDate: Date;
  LeaveDateType: number;
  LeaveDurationType: number;
  HaftLeaveType?: number;
}

export interface TA_LeaveRegistrationDTO extends TA_LeaveRegistration {
  FullName?: string;
  EmployeeCode?: string;
  DepartmentIndex?: number;
  DepartmentName?: string;
  LeaveDateString?: string;
}

export interface LeaveRegistrationModel extends TA_LeaveRegistrationDTO {
  Page?: number;
  PageSize?: number;
  Filter?: string;
  FromDate?: Date;
  ToDate?: Date;
  FromDateString?: string;
  ToDateString?: string;
  ListDepartmentIndex?: Array<number>;
  ListEmployeeATID?: Array<string>;
  LeaveDateTypeName?: string;
  LeaveDurationTypeName?: string;
  TotalWork?: number;
  HaftLeaveTypeName?: string;
  FirstHaftLeave?: boolean;
  LastHaftLeave?: boolean;
}

export enum LeaveDurationType {
  LeaveAllShift = 1,
  LeaveHaftShift
}

export const taLeaveRegistrationApi = new TA_LeaveRegistrationApi("TA_LeaveRegistration");
