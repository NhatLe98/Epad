import { AxiosRequestConfig } from 'axios';
import { BaseApi } from '@/$core/base-api';

class ListViolationApi extends BaseApi {
	public constructor(_module: string, config?: AxiosRequestConfig) {
		super(_module, config);
	}
	// public GetByFilter(model: ListViolationModel) {
	// 	return this.post('GetByFilter', model);
	// }
	public GetViolationByShift(model: Array<string>, isCustomer: boolean) {
		const request: ViolationShiftModel = { EmployeeATIDs: model, IsCustomer: isCustomer }; 
		return this.post('GetViolationByShift', request);
	}
	
}
export interface ListViolationModel {
	ObjectAccessType: string;
	FromDate: Date;
	ToDate: Date;
	Status: Array<number>;
	Page: number;
	PageSize: number;
}
export interface ViolationShiftModel {
	EmployeeATIDs: Array<string>;
	IsCustomer: boolean;
}
export const listViolationApi = new ListViolationApi('GC_Violation');
