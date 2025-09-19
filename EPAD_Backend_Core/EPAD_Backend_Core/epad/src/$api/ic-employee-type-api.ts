import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class EmployeeTypeApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetEmployeeTypes(page: number, filter: string, pageSize: number) {
		return this.get('GetEmployeeTypes', { params: { page, filter, pageSize } });
	}
	public GetUsingEmployeeType() {
		return this.get('GetUsingEmployeeType');
    }
	public GetEmployeeTypesAll() {
		return this.get('GetEmployeeTypesAll');
    }
	public AddEmployeeType(data: EmployeeTypeModel) {
		return this.post('AddEmployeeType', data);
	}
	public UpdateEmployeeType(data: EmployeeTypeModel) {
		return this.put('UpdateEmployeeType', data);
	}
	public DeleteEmployeeTypes(arrEmployeeTypes: Array<any>) {
		return this.delete('DeleteEmployeeType', { data: arrEmployeeTypes });
	}
}

export interface EmployeeTypeModel {
	Index: number;
	Code: string;
	Name: string;
	NameInEng: string;
	IsUsing: boolean;
	Description: string;
}
// export interface EmployeeTypeTree {
//     ID: string;
//     Code: string;
//     Name: string;
//     Type: string;
//     ParentIndex: number;

//     ListChildren: EmployeeTypeTree[];
// }
export const employeeTypesApi = new EmployeeTypeApi('IC_EmployeeType');
