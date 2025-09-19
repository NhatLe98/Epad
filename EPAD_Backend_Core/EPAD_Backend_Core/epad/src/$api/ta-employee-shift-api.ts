import {  AxiosRequestConfig } from "axios";
import { BaseApi } from "@/$core/base-api";

class TA_EmployeeShiftApi extends BaseApi {
  public constructor(_module: string, config?: AxiosRequestConfig) {
    super(_module, config);
  }

	public GetEmployeeShiftByFilter(data: EmployeeShiftRequest) {
		return this.post('GetEmployeeShiftByFilter', data);
	}
	public AddEmployeeShift(data: Array<EmployeeShiftModel>) {
		return this.post('AddEmployeeShift', data);
    }

	public ImportExcelEmployeeShift(data: any) {
		return this.post('ImportExcelEmployeeShift', data);
    }

	public ExportDataIntoTemplateImport(data: EmployeeShiftRequest): Promise<any> {
		return this.post('ExportDataIntoTemplateImport', data, {responseType: 'blob'});
	}
}

export interface EmployeeShiftRequest {
	FromDate: Date;
	ToDate: Date;
	EmployeeATIDs: Array<string>;
	DepartmentIDs: Array<string>;
	page: number;
	pageSize: number;
}

export interface EmployeeShiftModel {
	EmployeeATID: string;
	Date: Date;
	DateStr: string;
	ShiftIndex: number;
	DepartmentIndex: number;
}
export const taEmployeeShiftApi = new TA_EmployeeShiftApi("TA_EmployeeShift");
