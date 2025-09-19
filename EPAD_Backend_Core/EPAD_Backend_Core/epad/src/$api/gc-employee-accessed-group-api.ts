import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class EmployeeAccessedGroupApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetEmployeeAccessedGroup(filter: string, pageIndex: number, pageSize: number) {
		return this.get('GetEmployeeAccessedGroup', { params: { filter, pageIndex, pageSize } });
	}
	public GetEmployeeAccessedGroupByFilter(request: any) {
		return this.post('GetEmployeeAccessedGroupByFilter', request );
	}
	public GetEmployeeAccessedGroupAll() {
		return this.get('GetEmployeeAccessedGroupAll');
	}
	public AddEmployeeAccessedGroup(data: any) {
		return this.post('AddEmployeeAccessedGroup', data);
	}
	public UpdateEmployeeAccessedGroup(data: any) {
		return this.put('UpdateEmployeeAccessedGroup', data);
	}
	public async DeleteEmployeeAccessedGroup(arrEmployeeAccessedGroup: Array<any> | Array<number>): Promise<any> {
		return this.delete('DeleteEmployeeAccessedGroup', { data: arrEmployeeAccessedGroup });
	}
	public ImportEmployeeAccessedGroup(data: any) {
		return this.post('ImportEmployeeAccessedGroup', data);
	}
}

export interface EmployeeAccessedGroupModel {
	Index: number;
	EmployeeATID: string;
	EmployeeATIDs: Array<string>;
	DepartmentIndex: number;
	DepartmentIDs: Array<number>;
	FromDate: Date;
	ToDate: Date;
	AccessedGroupIndex?: number;
}

export const employeeAccessedGroupApi = new EmployeeAccessedGroupApi('GC_Employee_AccessedGroup');
