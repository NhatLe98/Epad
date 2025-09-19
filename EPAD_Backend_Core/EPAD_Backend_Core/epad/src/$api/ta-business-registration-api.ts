import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_BusinessRegistrationApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

  public GetBusinessType() {
		return this.get('GetBusinessType');
	}
	public GetBusinessRegistration(param: BusinessRegistrationModel | any) {
		return this.post('GetBusinessRegistration', param );
	}
	public AddBusinessRegistration(data: BusinessRegistrationModel | any) {
		return this.post('AddBusinessRegistration', data);
	}
	public UpdateBusinessRegistration(data: BusinessRegistrationModel | any) {
		return this.put('UpdateBusinessRegistration', data);
	}
	public DeleteBusinessRegistration(index: Array<any>) {
		return this.delete('DeleteBusinessRegistration', { data: index });
	}
  public AddBusinessRegistrationFromExcel(data: any) {
		return this.post('AddBusinessRegistrationFromExcel', data);
	}
}

export interface TA_BusinessRegistration {
  Index?: number;
  EmployeeATID: string;
  Description?: string;
  BusinessDate: Date;
  BusinessType: number;
  WorkPlace?: string;
  FromTime: Date;
  ToTime: Date;
}

export interface TA_BusinessRegistrationDTO extends TA_BusinessRegistration {
  FullName?: string;
  EmployeeCode?: string;
  DepartmentIndex?: number;
  DepartmentName?: string;
  BusinessDateString?: string;
  FromTimeString?: string;
  ToTimeString?: string;
}

export interface BusinessRegistrationModel extends TA_BusinessRegistrationDTO {
  Page?: number;
  PageSize?: number;
  Filter?: string;
  FromDate?: Date;
  ToDate?: Date;
  FromDateString?: string;
  ToDateString?: string;
  ListDepartmentIndex?: Array<number>;
  ListEmployeeATID?: Array<string>;
  BusinessTypeName?: string;
  TotalWork?: number;
}

export enum BusinessType {
  BusinessAllShift = 1,
  BusinessFromToTime
}

export const taBusinessRegistrationApi = new TA_BusinessRegistrationApi("TA_BusinessRegistration");
