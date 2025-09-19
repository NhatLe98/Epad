import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class DepartmentAccessedGroupApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	public GetDepartmentAccessedGroup(filter: string, pageIndex: number, pageSize: number) {
		return this.get('GetDepartmentAccessedGroup', { params: { filter, pageIndex, pageSize } });
	}
	public GetDepartmentAccessedGroupByFilter(request: any) {
		return this.post('GetDepartmentAccessedGroupByFilter', request );
	}
	public GetDepartmentAccessedGroupAll() {
		return this.get('GetDepartmentAccessedGroupAll');
	}
	public AddDepartmentAccessedGroup(data: any) {
		return this.post('AddDepartmentAccessedGroup', data);
	}
	public UpdateDepartmentAccessedGroup(data: any) {
		return this.put('UpdateDepartmentAccessedGroup', data);
	}
	public async DeleteDepartmentAccessedGroup(arrDepartmentAccessedGroup: Array<any> | Array<number>): Promise<any> {
		return this.delete('DeleteDepartmentAccessedGroup', { data: arrDepartmentAccessedGroup });
	}
	public ImportDepartmentAccessedGroup(data: any) {
		return this.post('ImportDepartmentAccessedGroup', data);
	}
}

export interface DepartmentAccessedGroupModel {
	Index: number;
	DepartmentIndex?: number;
	DepartmentIDs: Array<number>;
	FromDate: Date;
	ToDate: Date;
	AccessedGroupIndex?: number;
}

export const departmentAccessedGroupApi = new DepartmentAccessedGroupApi('GC_Department_AccessedGroup');
